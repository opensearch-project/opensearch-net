/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>MultiSearchResponseFormatter</c>.
	///
	/// REQUEST-STATEFUL CONVERTER. The wire body (<c>{ "took": N, "responses": [ &lt;response&gt;, ... ] }</c>)
	/// carries no per-response type discriminator, so the concrete document type of each inner search response cannot
	/// be recovered from the JSON alone. The legacy formatter zipped the <c>responses</c> array positionally against
	/// the originating request's operations and deserialized each entry as <c>SearchResponse&lt;operation.ClrType&gt;</c>,
	/// keyed by the operation's key:
	/// <list type="bullet">
	/// <item>for an <see cref="IMultiSearchRequest"/>, against <c>Operations</c> (a <c>string -&gt; ISearchRequest</c> map);</item>
	/// <item>for an <see cref="IMultiSearchTemplateRequest"/>, against <c>Operations</c> (a <c>string -&gt; ISearchTemplateRequest</c> map);</item>
	/// <item>any other request type threw <see cref="InvalidOperationException"/> — preserved here.</item>
	/// </list>
	///
	/// This per-operation key + <see cref="ITypedSearchRequest.ClrType"/> is runtime request state that is NOT
	/// available to a converter registered on the shared serializer, so — exactly as the legacy formatter was
	/// constructed per-request and installed via <c>CreateStateful</c> in <see cref="MultiSearchResponseBuilder"/> —
	/// this converter takes the request in its constructor and MUST likewise be created per-request (it cannot be
	/// registered globally on <c>SystemTextJsonHighLevelSerializer</c>).
	///
	/// Parity notes with the legacy formatter:
	/// <list type="bullet">
	/// <item>A null request yields <c>null</c> (legacy returned <c>null</c> when <c>_request == null</c>).</item>
	/// <item><c>took</c> is read into <see cref="MultiSearchResponse.Took"/>; other properties besides
	/// <c>responses</c>/<c>took</c> are skipped.</item>
	/// <item>If there is no <c>responses</c> array (or it is empty), a <see cref="MultiSearchResponse"/> with only
	/// <c>Took</c> populated is returned.</item>
	/// <item>Each response is deserialized as the closed generic <c>SearchResponse&lt;ClrType&gt;</c> using the
	/// operation's <see cref="ITypedSearchRequest.ClrType"/> (falling back to <c>object</c> when null), positionally
	/// zipped with the request operations and added under the operation's key (so extra responses beyond the operation
	/// count are dropped, matching <c>Zip</c>).</item>
	/// </list>
	///
	/// <see cref="Utf8JsonReader"/> is forward-only; the body is buffered into a <see cref="JsonDocument"/> so the
	/// <c>responses</c> array can be paired with the request operations.
	///
	/// Write: responses are only ever read from the server, never sent; the legacy write path delegated to a dynamic
	/// camelCase reflection serializer whose output was unused. This converter writes the canonical
	/// <c>{ "took": N, "responses": [ ... ] }</c> map (dispatching each response by its runtime type) so read/write is
	/// symmetric for tests — a deliberate, documented divergence from the legacy write path.
	/// </summary>
	internal class MultiSearchResponseConverter : JsonConverter<MultiSearchResponse>
	{
		private readonly IRequest _request;

		public MultiSearchResponseConverter(IRequest request) => _request = request;

		public override MultiSearchResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (_request == null)
				return null;

			var response = new MultiSearchResponse();

			if (reader.TokenType == JsonTokenType.Null)
				return response;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return response;

			if (root.TryGetProperty("took", out var tookElement)
				&& tookElement.ValueKind == JsonValueKind.Number
				&& tookElement.TryGetInt64(out var took))
				response.Took = took;

			if (!root.TryGetProperty("responses", out var responsesElement) || responsesElement.ValueKind != JsonValueKind.Array)
				return response;

			var descriptors = GetDescriptors(_request);

			using var responsesEnumerator = responsesElement.EnumerateArray().GetEnumerator();
			foreach (var descriptor in descriptors)
			{
				if (!responsesEnumerator.MoveNext())
					break; // fewer responses than operations — Zip stops at the shorter sequence

				var clrType = descriptor.Value?.ClrType ?? typeof(object);
				var responseType = typeof(SearchResponse<>).MakeGenericType(clrType);
				var searchResponse = (IResponse)responsesEnumerator.Current.Deserialize(responseType, options);
				response.Responses.Add(descriptor.Key, searchResponse);
			}

			return response;
		}

		public override void Write(Utf8JsonWriter writer, MultiSearchResponse value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WriteNumber("took", value.Took);
			writer.WritePropertyName("responses");
			writer.WriteStartArray();
			foreach (var kvp in value.Responses ?? new Dictionary<string, IResponse>())
			{
				if (kvp.Value == null)
				{
					writer.WriteNullValue();
					continue;
				}

				JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}

		private static IEnumerable<KeyValuePair<string, ITypedSearchRequest>> GetDescriptors(IRequest request)
		{
			switch (request)
			{
				case IMultiSearchRequest multiSearch:
					return multiSearch.Operations.Select(o =>
						new KeyValuePair<string, ITypedSearchRequest>(o.Key, o.Value));
				case IMultiSearchTemplateRequest multiSearchTemplate:
					return multiSearchTemplate.Operations.Select(o =>
						new KeyValuePair<string, ITypedSearchRequest>(o.Key, o.Value));
				default:
					throw new InvalidOperationException($"Request must be an instance of {nameof(IMultiSearchRequest)}"
						+ $" or {nameof(IMultiSearchTemplateRequest)}");
			}
		}
	}
}
