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
using System.Text.Json;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// Writes the newline-delimited (NDJSON) <c>_msearch</c> / <c>_msearch/template</c> body for the
	/// <c>System.Text.Json</c> serializer (#388), replacing the vendored <c>MultiSearchFormatter</c> /
	/// <c>MultiSearchTemplateFormatter</c>. Each operation contributes a header line (index + per-request
	/// options resolved from the query string) followed by the operation body. NDJSON cannot go through a
	/// single-root <c>Utf8JsonWriter</c>, so each line is serialized independently and separated by <c>\n</c>.
	/// </summary>
	internal static class MultiSearchRequestJsonSerializer
	{
		private const byte Newline = (byte)'\n';

		public static void Write(IMultiSearchRequest value, Stream stream, IOpenSearchSerializer builtInSerializer)
		{
			if (value?.Operations == null) return;

			var settings = BuiltInSerializerState.GetConnectionSettings(builtInSerializer);
			var options = BuiltInSerializerState.GetOptions(builtInSerializer);

			foreach (var operation in value.Operations.Values)
				WriteOperation(stream, settings, options, operation, operation.RequestParameters, operation.Index, value.Index);
		}

		public static void WriteTemplate(IMultiSearchTemplateRequest value, Stream stream, IOpenSearchSerializer builtInSerializer)
		{
			if (value?.Operations == null) return;

			var settings = BuiltInSerializerState.GetConnectionSettings(builtInSerializer);
			var options = BuiltInSerializerState.GetOptions(builtInSerializer);

			foreach (var operation in value.Operations.Values)
				WriteOperation(stream, settings, options, operation, operation.RequestParameters, operation.Index, value.Index);
		}

		private static void WriteOperation(Stream stream, IConnectionSettingsValues settings, JsonSerializerOptions options,
			object operation, IRequestParameters parameters, Indices operationIndex, Indices requestIndex)
		{
			string GetString(string key) => parameters.GetResolvedQueryStringValue(key, settings);

			IUrlParameter indices = requestIndex == null || !requestIndex.Equals(operationIndex) ? operationIndex : null;

			var searchType = GetString("search_type");
			if (searchType == "query_then_fetch") searchType = null;

			var header = new
			{
				index = indices?.GetString(settings),
				search_type = searchType,
				preference = GetString("preference"),
				routing = GetString("routing"),
				ignore_unavailable = GetString("ignore_unavailable")
			};

			WriteLine(stream, JsonSerializer.SerializeToUtf8Bytes(header, header.GetType(), options));
			WriteLine(stream, JsonSerializer.SerializeToUtf8Bytes(operation, operation.GetType(), options));
		}

		private static void WriteLine(Stream stream, byte[] bytes)
		{
			stream.Write(bytes, 0, bytes.Length);
			stream.WriteByte(Newline);
		}
	}
}
