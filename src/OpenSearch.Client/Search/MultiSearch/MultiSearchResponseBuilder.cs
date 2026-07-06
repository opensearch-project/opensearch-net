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
	internal class MultiSearchResponseBuilder : CustomResponseBuilderBase
	{
		public MultiSearchResponseBuilder(IRequest request)
		{
			_request = request;
			Formatter = new MultiSearchResponseFormatter(request);
		}

		private readonly IRequest _request;

		private MultiSearchResponseFormatter Formatter { get; }

		public override object DeserializeResponse(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream)
		{
			if (!response.Success)
				return new MultiSearchResponse();

			// The Utf8Json path uses a stateful formatter; the System.Text.Json serializer (see #388) does not
			// participate in that layer, so build the response directly from the stream instead.
			if (!BuiltInSerializerState.UsesUtf8JsonFormatter(builtInSerializer))
				return SystemTextJsonMultiResponseBuilder.BuildMultiSearch(builtInSerializer, _request, stream.ReadAllBytes());

			return builtInSerializer.CreateStateful(Formatter).Deserialize<MultiSearchResponse>(stream);
		}

		public override async Task<object> DeserializeResponseAsync(
			IOpenSearchSerializer builtInSerializer,
			IApiCallDetails response,
			Stream stream,
			CancellationToken ctx = default
		)
		{
			if (!response.Success)
				return new MultiSearchResponse();

			if (!BuiltInSerializerState.UsesUtf8JsonFormatter(builtInSerializer))
				return SystemTextJsonMultiResponseBuilder.BuildMultiSearch(builtInSerializer, _request,
					await stream.ReadAllBytesAsync(ctx).ConfigureAwait(false));

			return await builtInSerializer.CreateStateful(Formatter)
				.DeserializeAsync<MultiSearchResponse>(stream, ctx)
				.ConfigureAwait(false);
		}
	}
}
