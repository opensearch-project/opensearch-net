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
		private readonly IMultiGetRequest _request;

		public MultiGetResponseBuilder(IMultiGetRequest request)
		{
			_request = request;
			Formatter = new MultiGetResponseFormatter(request);
		}

		private MultiGetResponseFormatter Formatter { get; }

		public override object DeserializeResponse(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream)
		{
			if (!response.Success)
				return new MultiGetResponse();

			// Under System.Text.Json the legacy CreateStateful path (which needs a Utf8Json formatter resolver) is not
			// available, so deserialize through a per-request MultiGetResponseConverter that carries the request's
			// document CLR types.
			if (StatefulSerializerExtensions.TryGetSystemTextJsonSerializer(builtInSerializer, out var stj))
				return stj.DeserializeWithConverter<MultiGetResponse>(new MultiGetResponseConverter(_request), stream);

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

			if (StatefulSerializerExtensions.TryGetSystemTextJsonSerializer(builtInSerializer, out var stj))
				return await stj.DeserializeWithConverterAsync<MultiGetResponse>(new MultiGetResponseConverter(_request), stream, ctx)
					.ConfigureAwait(false);

			return await builtInSerializer.CreateStateful(Formatter)
				.DeserializeAsync<MultiGetResponse>(stream, ctx)
				.ConfigureAwait(false);
		}
	}
}
