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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	internal class MultiGetResponseBuilder : CustomResponseBuilderBase
	{
		public MultiGetResponseBuilder(IMultiGetRequest request)
		{
			_request = request;
			Formatter = new MultiGetResponseFormatter(request);
		}

		private readonly IMultiGetRequest _request;

		private MultiGetResponseFormatter Formatter { get; }

		public override object DeserializeResponse(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream)
		{
			if (!response.Success)
				return new MultiGetResponse();

			// The Utf8Json path uses a stateful formatter; the System.Text.Json serializer (see #388) does not
			// participate in that layer, so build the response directly from the stream instead.
			if (!BuiltInSerializerState.UsesUtf8JsonFormatter(builtInSerializer))
				return SystemTextJsonMultiResponseBuilder.BuildMultiGet(builtInSerializer, _request, ReadAllBytes(stream));

			return builtInSerializer.CreateStateful(Formatter).Deserialize<MultiGetResponse>(stream);
		}

		public override async Task<object> DeserializeResponseAsync(
			IOpenSearchSerializer builtInSerializer,
			IApiCallDetails response,
			Stream stream,
			CancellationToken ctx = default
		)
		{
			if (!response.Success)
				return new MultiGetResponse();

			if (!BuiltInSerializerState.UsesUtf8JsonFormatter(builtInSerializer))
				return SystemTextJsonMultiResponseBuilder.BuildMultiGet(builtInSerializer, _request,
					await ReadAllBytesAsync(stream, ctx).ConfigureAwait(false));

			return await builtInSerializer.CreateStateful(Formatter)
				.DeserializeAsync<MultiGetResponse>(stream, ctx)
				.ConfigureAwait(false);
		}

		private static byte[] ReadAllBytes(Stream stream)
		{
			if (stream is MemoryStream existing) return existing.ToArray();
			using var ms = new MemoryStream();
			stream.CopyTo(ms);
			return ms.ToArray();
		}

		private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken ctx)
		{
			if (stream is MemoryStream existing) return existing.ToArray();
			using var ms = new MemoryStream();
			await stream.CopyToAsync(ms, 81920, ctx).ConfigureAwait(false);
			return ms.ToArray();
		}
	}
}
