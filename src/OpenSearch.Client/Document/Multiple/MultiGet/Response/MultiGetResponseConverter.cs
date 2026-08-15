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
	/// System.Text.Json replacement for the legacy Utf8Json <c>MultiGetResponseFormatter</c> (which was
	/// declared in the file named <c>MultiGetHitJsonConverter.cs</c> — despite the file name, the legacy type was
	/// an <c>IJsonFormatter&lt;MultiGetResponse&gt;</c>, not a hit converter).
	///
	/// REQUEST-STATEFUL CONVERTER. The wire body (<c>{ "docs": [ &lt;hit&gt;, ... ] }</c>) carries no per-document
	/// type discriminator, so the concrete document type for each hit cannot be recovered from the JSON alone: the
	/// legacy formatter zipped the <c>docs</c> array positionally against the originating request's
	/// <see cref="IMultiGetRequest.Documents"/> and deserialized each hit as <c>MultiGetHit&lt;operation.ClrType&gt;</c>.
	/// That runtime request state is NOT available to a converter registered on the shared serializer, so — exactly as
	/// the legacy formatter was constructed per-request and installed via <c>CreateStateful</c> in
	/// <see cref="MultiGetResponseBuilder"/> — this converter takes the request in its constructor and MUST likewise be
	/// created per-request (it cannot be registered globally on <c>SystemTextJsonHighLevelSerializer</c>).
	///
	/// Parity notes with the legacy formatter:
	/// <list type="bullet">
	/// <item>A null request yields <c>null</c> (legacy returned <c>null</c> when <c>_request == null</c>).</item>
	/// <item>Properties other than <c>docs</c> are skipped; only the <c>docs</c> array is consumed.</item>
	/// <item>If there is no <c>docs</c> array (or it is empty), an empty <see cref="MultiGetResponse"/> is returned.</item>
	/// <item>Each hit is deserialized as the closed generic <c>MultiGetHit&lt;ClrType&gt;</c> using the operation's
	/// <see cref="IMultiGetOperation.ClrType"/>, positionally zipped with the request documents (so extra hits beyond
	/// the request document count are dropped, matching <c>Zip</c>).</item>
	/// </list>
	///
	/// <see cref="Utf8JsonReader"/> is forward-only; the body is buffered into a <see cref="JsonDocument"/> so the
	/// <c>docs</c> array can be paired with the request documents.
	///
	/// Write: responses are only ever read from the server, never sent; the legacy write path delegated to a dynamic
	/// camelCase reflection serializer whose output was unused and non-round-trippable. This converter writes the
	/// canonical <c>{ "docs": [ ... ] }</c> map (dispatching each hit by its runtime type) so read/write is symmetric
	/// for tests — a deliberate, documented divergence from the legacy write path.
	/// </summary>
	internal class MultiGetResponseConverter : JsonConverter<MultiGetResponse>
	{
		private readonly IMultiGetRequest _request;

		public MultiGetResponseConverter(IMultiGetRequest request) => _request = request;

		public override MultiGetResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (_request == null)
				return null;

			var response = new MultiGetResponse();

			if (reader.TokenType == JsonTokenType.Null)
				return response;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return response;

			if (!root.TryGetProperty("docs", out var docsElement) || docsElement.ValueKind != JsonValueKind.Array)
				return response;

			var docs = docsElement.EnumerateArray();
			using var docsEnumerator = docs.GetEnumerator();

			foreach (var operation in _request.Documents ?? Enumerable.Empty<IMultiGetOperation>())
			{
				if (!docsEnumerator.MoveNext())
					break; // fewer hits than request documents — Zip stops at the shorter sequence

				var clrType = operation.ClrType ?? typeof(object);
				var hitType = typeof(MultiGetHit<>).MakeGenericType(clrType);
				var hit = (IMultiGetHit<object>)docsEnumerator.Current.Deserialize(hitType, options);
				response.InternalHits.Add(hit);
			}

			return response;
		}

		public override void Write(Utf8JsonWriter writer, MultiGetResponse value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("docs");
			writer.WriteStartArray();
			foreach (var hit in value.InternalHits ?? new List<IMultiGetHit<object>>())
			{
				if (hit == null)
				{
					writer.WriteNullValue();
					continue;
				}

				JsonSerializer.Serialize(writer, hit, hit.GetType(), options);
			}
			writer.WriteEndArray();
			writer.WriteEndObject();
		}
	}
}
