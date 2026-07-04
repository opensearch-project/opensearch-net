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
using System.Text;
using System.Text.Json;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// Builds <see cref="MultiSearchResponse"/> and <see cref="MultiGetResponse"/> instances directly from a
	/// response stream when the built-in serializer is the <c>System.Text.Json</c> implementation
	/// (see GitHub issue #388).
	/// <para>
	/// The Utf8Json code path relies on a stateful formatter created via
	/// <see cref="StatefulSerializerExtensions.CreateStateful{T}"/>, which is only available for the
	/// vendored Utf8Json serializer (it requires <see cref="IInternalSerializer"/>). For the STJ serializer
	/// we instead parse the envelope, split it into its per-operation segments, and deserialize each segment
	/// into the concrete <c>SearchResponse&lt;T&gt;</c> / <c>MultiGetHit&lt;T&gt;</c> requested by the
	/// originating operation.
	/// </para>
	/// </summary>
	internal static class SystemTextJsonMultiResponseBuilder
	{
		public static MultiSearchResponse BuildMultiSearch(IOpenSearchSerializer serializer, IRequest request, byte[] bytes)
		{
			var response = new MultiSearchResponse();
			if (request == null || bytes == null || bytes.Length == 0)
				return response;

			using var document = JsonDocument.Parse(bytes);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object)
				return response;

			if (root.TryGetProperty("took", out var took) && took.ValueKind == JsonValueKind.Number)
				response.Took = took.GetInt64();

			if (!root.TryGetProperty("responses", out var responses) || responses.ValueKind != JsonValueKind.Array)
				return response;

			using var operations = GetSearchOperations(request).GetEnumerator();
			foreach (var element in responses.EnumerateArray())
			{
				if (!operations.MoveNext())
					break;

				var operation = operations.Current;
				var clrType = operation.Value.ClrType ?? typeof(object);
				var closedType = typeof(SearchResponse<>).MakeGenericType(clrType);
				var searchResponse = (IResponse)Deserialize(serializer, closedType, element);
				response.Responses[operation.Key] = searchResponse;
			}

			return response;
		}

		public static MultiGetResponse BuildMultiGet(IOpenSearchSerializer serializer, IMultiGetRequest request, byte[] bytes)
		{
			var response = new MultiGetResponse();
			if (request?.Documents == null || bytes == null || bytes.Length == 0)
				return response;

			using var document = JsonDocument.Parse(bytes);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object)
				return response;

			if (!root.TryGetProperty("docs", out var docs) || docs.ValueKind != JsonValueKind.Array)
				return response;

			using var operations = request.Documents.GetEnumerator();
			foreach (var element in docs.EnumerateArray())
			{
				if (!operations.MoveNext())
					break;

				var operation = operations.Current;
				var clrType = operation.ClrType ?? typeof(object);
				var closedType = typeof(MultiGetHit<>).MakeGenericType(clrType);
				var hit = (IMultiGetHit<object>)Deserialize(serializer, closedType, element);
				response.InternalHits.Add(hit);
			}

			return response;
		}

		private static object Deserialize(IOpenSearchSerializer serializer, Type type, JsonElement element)
		{
			var segment = Encoding.UTF8.GetBytes(element.GetRawText());
			using var stream = new MemoryStream(segment);
			return serializer.Deserialize(type, stream);
		}

		private static IEnumerable<KeyValuePair<string, ITypedSearchRequest>> GetSearchOperations(IRequest request)
		{
			switch (request)
			{
				case IMultiSearchRequest multiSearch:
					foreach (var operation in multiSearch.Operations)
						yield return new KeyValuePair<string, ITypedSearchRequest>(operation.Key, operation.Value);
					break;
				case IMultiSearchTemplateRequest multiSearchTemplate:
					foreach (var operation in multiSearchTemplate.Operations)
						yield return new KeyValuePair<string, ITypedSearchRequest>(operation.Key, operation.Value);
					break;
				default:
					throw new InvalidOperationException($"Request must be an instance of {nameof(IMultiSearchRequest)}"
						+ $" or {nameof(IMultiSearchTemplateRequest)}");
			}
		}
	}
}
