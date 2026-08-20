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
	public class SourceRequestResponseBuilder<TDocument> : CustomResponseBuilderBase
	{
		public static SourceRequestResponseBuilder<TDocument> Instance { get; } = new SourceRequestResponseBuilder<TDocument>();

		public override object DeserializeResponse(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream)
		{
			if (response.Success)
			{
				var sourceSerializer = GetSourceSerializer(builtInSerializer);
				return new SourceResponse<TDocument>
				{
					Body = (sourceSerializer ?? builtInSerializer).Deserialize<TDocument>(stream)
				};
			}

			return new SourceResponse<TDocument>();
		}

		public override async Task<object> DeserializeResponseAsync(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream, CancellationToken ctx = default)
		{
			if (response.Success)
			{
				var sourceSerializer = GetSourceSerializer(builtInSerializer);
				return new SourceResponse<TDocument>
				{
					Body = await (sourceSerializer ?? builtInSerializer).DeserializeAsync<TDocument>(stream, ctx).ConfigureAwait(false)
				};
			}

			return new SourceResponse<TDocument>();
		}

		// The _source document body is (de)serialized by the connection's configured SourceSerializer (e.g. a
		// Newtonsoft JsonNetSerializer) rather than the built-in serializer. The legacy engine reached it via the
		// Utf8Json formatter resolver; under System.Text.Json unwrap the STJ serializer and read its Settings. Returns
		// null when neither engine exposes settings, in which case the caller falls back to the built-in serializer.
		private static IOpenSearchSerializer GetSourceSerializer(IOpenSearchSerializer builtInSerializer)
		{
			if (builtInSerializer is IInternalSerializer internalSerializer &&
				internalSerializer.TryGetJsonFormatter(out var formatter))
				return formatter.GetConnectionSettings().SourceSerializer;

			if (StatefulSerializerExtensions.TryGetSystemTextJsonSerializer(builtInSerializer, out var stj))
				return stj.Settings.SourceSerializer;

			return null;
		}
	}
}
