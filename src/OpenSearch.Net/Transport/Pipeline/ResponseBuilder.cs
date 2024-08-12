/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.Extensions;

namespace OpenSearch.Net;

public static class ResponseBuilder
{
    public const int BufferSize = 81920;

    private static readonly Type[] SpecialTypes =
        { typeof(StringResponse), typeof(BytesResponse), typeof(VoidResponse), typeof(DynamicResponse) };

    public static TResponse ToResponse<TResponse>(
        RequestData requestData,
        Exception ex,
        int? statusCode,
        IEnumerable<string> warnings,
        Stream responseStream,
        string mimeType = RequestData.MimeType
    )
        where TResponse : class, IOpenSearchResponse, new()
    {
        responseStream.ThrowIfNull(nameof(responseStream));
        var details = Initialize(requestData, ex, statusCode, warnings, mimeType);
        //TODO take ex and (responseStream == Stream.Null) into account might not need to flow to SetBody in that case
        var response = SetBody<TResponse>(details, requestData, responseStream, mimeType) ?? new TResponse();
        response.ApiCall = details;
        return response;
    }

    public static async Task<TResponse> ToResponseAsync<TResponse>(
        RequestData requestData,
        Exception ex,
        int? statusCode,
        IEnumerable<string> warnings,
        Stream responseStream,
        string mimeType = RequestData.MimeType,
        CancellationToken cancellationToken = default
    )
        where TResponse : class, IOpenSearchResponse, new()
    {
        responseStream.ThrowIfNull(nameof(responseStream));
        var details = Initialize(requestData, ex, statusCode, warnings, mimeType);
        var response = await SetBodyAsync<TResponse>(details, requestData, responseStream, mimeType, cancellationToken).ConfigureAwait(false)
            ?? new TResponse();
        response.ApiCall = details;
        return response;
    }

    private static ApiCallDetails Initialize(
        RequestData requestData, Exception exception, int? statusCode, IEnumerable<string> warnings, string mimeType
    )
    {
        var success = false;
        var allowedStatusCodes = requestData.AllowedStatusCodes;
        if (statusCode.HasValue)
        {
            if (allowedStatusCodes.Contains(-1) || allowedStatusCodes.Contains(statusCode.Value))
                success = true;
            else
                success = requestData.ConnectionSettings
                    .StatusCodeToResponseSuccess(requestData.Method, statusCode.Value);
        }
        //mimeType can include charset information on .NET full framework
        if (!string.IsNullOrEmpty(mimeType) && !mimeType.StartsWith(requestData.Accept))
            success = false;

        var details = new ApiCallDetails
        {
            Success = success,
            OriginalException = exception,
            HttpStatusCode = statusCode,
            RequestBodyInBytes = requestData.PostData?.WrittenBytes,
            Uri = requestData.Uri,
            HttpMethod = requestData.Method,
            DeprecationWarnings = warnings ?? Enumerable.Empty<string>(),
            ResponseMimeType = mimeType,
            ConnectionConfiguration = requestData.ConnectionSettings
        };
        return details;
    }

    private static TResponse SetBody<TResponse>(ApiCallDetails details, RequestData requestData, Stream responseStream, string mimeType)
        where TResponse : class, IOpenSearchResponse, new()
    {
        byte[] bytes = null;
        var disableDirectStreaming = requestData.PostData?.DisableDirectStreaming ?? requestData.ConnectionSettings.DisableDirectStreaming;
        if (disableDirectStreaming || NeedsToEagerReadStream<TResponse>())
        {
            var inMemoryStream = requestData.MemoryStreamFactory.Create();
            responseStream.CopyTo(inMemoryStream, BufferSize);
            bytes = SwapStreams(ref responseStream, ref inMemoryStream);
            details.ResponseBodyInBytes = bytes;
        }

        using (responseStream)
        {
            if (SetSpecialTypes<TResponse>(mimeType, bytes, requestData.MemoryStreamFactory, out var r))
                return r;

            if (details.HttpStatusCode.HasValue && requestData.SkipDeserializationForStatusCodes.Contains(details.HttpStatusCode.Value))
                return null;

            var serializer = requestData.ConnectionSettings.RequestResponseSerializer;
            if (requestData.CustomResponseBuilder != null)
                return requestData.CustomResponseBuilder.DeserializeResponse(serializer, details, responseStream) as TResponse;

            return mimeType == null || !mimeType.StartsWith(requestData.Accept, StringComparison.Ordinal)
                ? null
                : serializer.Deserialize<TResponse>(responseStream);
        }
    }

    private static async Task<TResponse> SetBodyAsync<TResponse>(
        ApiCallDetails details, RequestData requestData, Stream responseStream, string mimeType, CancellationToken cancellationToken
    )
        where TResponse : class, IOpenSearchResponse, new()
    {
        byte[] bytes = null;
        var disableDirectStreaming = requestData.PostData?.DisableDirectStreaming ?? requestData.ConnectionSettings.DisableDirectStreaming;
        if (disableDirectStreaming || NeedsToEagerReadStream<TResponse>())
        {
            var inMemoryStream = requestData.MemoryStreamFactory.Create();
            await responseStream.CopyToAsync(inMemoryStream, BufferSize, cancellationToken).ConfigureAwait(false);
            bytes = SwapStreams(ref responseStream, ref inMemoryStream);
            details.ResponseBodyInBytes = bytes;
        }

        using (responseStream)
        {
            if (SetSpecialTypes<TResponse>(mimeType, bytes, requestData.MemoryStreamFactory, out var r)) return r;

            if (details.HttpStatusCode.HasValue && requestData.SkipDeserializationForStatusCodes.Contains(details.HttpStatusCode.Value))
                return null;

            var serializer = requestData.ConnectionSettings.RequestResponseSerializer;
            if (requestData.CustomResponseBuilder != null)
                return await requestData.CustomResponseBuilder.DeserializeResponseAsync(serializer, details, responseStream, cancellationToken).ConfigureAwait(false) as TResponse;

            return mimeType == null || !mimeType.StartsWith(requestData.Accept, StringComparison.Ordinal)
                ? null
                : await serializer
                    .DeserializeAsync<TResponse>(responseStream, cancellationToken)
                    .ConfigureAwait(false);
        }
    }

    private static bool SetSpecialTypes<TResponse>(string mimeType, byte[] bytes, IMemoryStreamFactory memoryStreamFactory, out TResponse cs)
        where TResponse : class, IOpenSearchResponse, new()
    {
        cs = null;
        var responseType = typeof(TResponse);
        if (!SpecialTypes.Contains(responseType)) return false;

        if (responseType == typeof(StringResponse))
            cs = new StringResponse(bytes.Utf8String()) as TResponse;
        else if (responseType == typeof(BytesResponse))
            cs = new BytesResponse(bytes) as TResponse;
        else if (responseType == typeof(VoidResponse))
            cs = new VoidResponse() as TResponse;
        else if (responseType == typeof(DynamicResponse))
        {
            //if not json store the result under "body"
            if (mimeType == null || !mimeType.StartsWith(RequestData.MimeType))
            {
                var dictionary = new DynamicDictionary
                {
                    ["body"] = new DynamicValue(bytes.Utf8String())
                };

                cs = new DynamicResponse(dictionary) as TResponse;
            }
            else
            {
                using var ms = memoryStreamFactory.Create(bytes);
                var body = LowLevelRequestResponseSerializer.Instance.Deserialize<DynamicDictionary>(ms);
                cs = new DynamicResponse(body) as TResponse;
            }
        }
        return cs != null;
    }

    private static bool NeedsToEagerReadStream<TResponse>()
        where TResponse : class, IOpenSearchResponse, new() =>
        typeof(TResponse) == typeof(StringResponse)
        || typeof(TResponse) == typeof(BytesResponse)
        || typeof(TResponse) == typeof(DynamicResponse);

    private static byte[] SwapStreams(ref Stream responseStream, ref MemoryStream ms)
    {
        var bytes = ms.ToArray();
        responseStream.Dispose();
        responseStream = ms;
        responseStream.Position = 0;
        return bytes;
    }
}
