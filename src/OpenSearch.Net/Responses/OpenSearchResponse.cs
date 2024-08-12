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
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using OpenSearch.Net.Diagnostics;

namespace OpenSearch.Net;

/// <summary>
/// A response from OpenSearch including details about the request/response life cycle
/// </summary>
public abstract class OpenSearchResponseBase : IApiCallDetails, IOpenSearchResponse
{
    /// <inheritdoc />
    public IApiCallDetails ApiCall { get; set; }

    /// <inheritdoc cref="IApiCallDetails.TcpStats"/>
    public ReadOnlyDictionary<TcpState, int> TcpStats
    {
        get => ApiCall.TcpStats;
        set => ApiCall.TcpStats = value;
    }

    /// <inheritdoc cref="IApiCallDetails.DebugInformation"/>
    public string DebugInformation => ApiCall.DebugInformation;
    /// <inheritdoc cref="IApiCallDetails.HttpMethod"/>
    public HttpMethod HttpMethod => ApiCall.HttpMethod;
    /// <inheritdoc cref="IApiCallDetails.AuditTrail"/>
    public List<Audit> AuditTrail
    {
        get => ApiCall.AuditTrail;
        set => ApiCall.AuditTrail = value;
    }

    /// <inheritdoc cref="IApiCallDetails.ThreadPoolStats"/>
    public ReadOnlyDictionary<string, ThreadPoolStatistics> ThreadPoolStats
    {
        get => ApiCall.ThreadPoolStats;
        set => ApiCall.ThreadPoolStats = value;
    }

    /// <inheritdoc cref="IApiCallDetails.DeprecationWarnings"/>
    public IEnumerable<string> DeprecationWarnings => ApiCall.DeprecationWarnings;
    /// <inheritdoc cref="IApiCallDetails.SuccessOrKnownError"/>
    public bool SuccessOrKnownError => ApiCall.SuccessOrKnownError;
    /// <inheritdoc cref="IApiCallDetails.HttpStatusCode"/>
    public int? HttpStatusCode => ApiCall.HttpStatusCode;

    /// <inheritdoc cref="IApiCallDetails.Success"/>
    public bool Success => ApiCall.Success;
    /// <inheritdoc cref="IApiCallDetails.OriginalException"/>
    public Exception OriginalException => ApiCall.OriginalException;
    /// <inheritdoc cref="IApiCallDetails.ResponseMimeType"/>
    public string ResponseMimeType => ApiCall.ResponseMimeType;
    /// <inheritdoc cref="IApiCallDetails.Uri"/>
    public Uri Uri => ApiCall.Uri;

    /// <inheritdoc cref="IApiCallDetails.ConnectionConfiguration"/>
    public IConnectionConfigurationValues ConnectionConfiguration => ApiCall.ConnectionConfiguration;

    /// <inheritdoc cref="IApiCallDetails.ResponseBodyInBytes"/>
    public byte[] ResponseBodyInBytes => ApiCall.ResponseBodyInBytes;

    /// <inheritdoc cref="IApiCallDetails.RequestBodyInBytes"/>
    public byte[] RequestBodyInBytes => ApiCall.RequestBodyInBytes;

    bool IOpenSearchResponse.TryGetServerErrorReason(out string reason) => TryGetServerErrorReason(out reason);

    public virtual bool TryGetServerError(out ServerError serverError)
    {
        serverError = null;
        var bytes = ApiCall.ResponseBodyInBytes;
        if (bytes == null || ResponseMimeType != RequestData.MimeType)
            return false;

        using (var stream = ConnectionConfiguration.MemoryStreamFactory.Create(bytes))
            return ServerError.TryCreate(stream, out serverError);
    }

    protected virtual bool TryGetServerErrorReason(out string reason)
    {
        reason = null;
        if (!TryGetServerError(out var serverError)) return false;

        reason = serverError?.Error?.ToString();
        return !string.IsNullOrEmpty(reason);
    }

    public override string ToString() => ApiCall.ToString();
}

/// <summary>
/// A response from OpenSearch including details about the request/response life cycle. Base class for the built in low level response
/// types, <see cref="StringResponse"/>, <see cref="BytesResponse"/>, <see cref="DynamicResponse"/> and <see cref="VoidResponse"/>
/// </summary>
public abstract class OpenSearchResponse<T> : OpenSearchResponseBase
{
    public T Body { get; protected internal set; }
}
