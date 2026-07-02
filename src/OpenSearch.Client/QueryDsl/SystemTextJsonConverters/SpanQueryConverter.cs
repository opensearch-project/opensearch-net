/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="ISpanQuery"/>.
	/// Polymorphic: The JSON property name is the discriminator.
	/// <code>
	/// {"span_term": {...}} or {"span_near": {"clauses": [...], "slop": 2}}
	/// </code>
	/// Supported: span_term, span_near, span_or, span_not, span_first,
	/// span_containing, span_within, span_multi, field_masking_span, span_gap
	/// </summary>
	internal sealed class SpanQueryConverter : JsonConverter<ISpanQuery>
	{
		public override ISpanQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for ISpanQuery but got {reader.TokenType}");

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var query = new SpanQuery();

			foreach (var prop in root.EnumerateObject())
			{
				var raw = prop.Value.GetRawText();
				switch (prop.Name)
				{
					case "span_term":
						query.SpanTerm = JsonSerializer.Deserialize<ISpanTermQuery>(raw, options);
						break;
					case "span_near":
						query.SpanNear = JsonSerializer.Deserialize<ISpanNearQuery>(raw, options);
						break;
					case "span_or":
						query.SpanOr = JsonSerializer.Deserialize<ISpanOrQuery>(raw, options);
						break;
					case "span_not":
						query.SpanNot = JsonSerializer.Deserialize<ISpanNotQuery>(raw, options);
						break;
					case "span_first":
						query.SpanFirst = JsonSerializer.Deserialize<ISpanFirstQuery>(raw, options);
						break;
					case "span_containing":
						query.SpanContaining = JsonSerializer.Deserialize<ISpanContainingQuery>(raw, options);
						break;
					case "span_within":
						query.SpanWithin = JsonSerializer.Deserialize<ISpanWithinQuery>(raw, options);
						break;
					case "span_multi":
						query.SpanMultiTerm = JsonSerializer.Deserialize<ISpanMultiTermQuery>(raw, options);
						break;
					case "field_masking_span":
						query.SpanFieldMasking = JsonSerializer.Deserialize<ISpanFieldMaskingQuery>(raw, options);
						break;
					case "span_gap":
						query.SpanGap = JsonSerializer.Deserialize<ISpanGapQuery>(raw, options);
						break;
					case "boost":
						((IQuery)query).Boost = prop.Value.GetDouble();
						break;
					case "_name":
						((IQuery)query).Name = prop.Value.GetString();
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, ISpanQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.SpanTerm != null)
			{
				writer.WritePropertyName("span_term");
				JsonSerializer.Serialize(writer, value.SpanTerm, options);
			}
			else if (value.SpanNear != null)
			{
				writer.WritePropertyName("span_near");
				JsonSerializer.Serialize(writer, value.SpanNear, options);
			}
			else if (value.SpanOr != null)
			{
				writer.WritePropertyName("span_or");
				JsonSerializer.Serialize(writer, value.SpanOr, options);
			}
			else if (value.SpanNot != null)
			{
				writer.WritePropertyName("span_not");
				JsonSerializer.Serialize(writer, value.SpanNot, options);
			}
			else if (value.SpanFirst != null)
			{
				writer.WritePropertyName("span_first");
				JsonSerializer.Serialize(writer, value.SpanFirst, options);
			}
			else if (value.SpanContaining != null)
			{
				writer.WritePropertyName("span_containing");
				JsonSerializer.Serialize(writer, value.SpanContaining, options);
			}
			else if (value.SpanWithin != null)
			{
				writer.WritePropertyName("span_within");
				JsonSerializer.Serialize(writer, value.SpanWithin, options);
			}
			else if (value.SpanMultiTerm != null)
			{
				writer.WritePropertyName("span_multi");
				JsonSerializer.Serialize(writer, value.SpanMultiTerm, options);
			}
			else if (value.SpanFieldMasking != null)
			{
				writer.WritePropertyName("field_masking_span");
				JsonSerializer.Serialize(writer, value.SpanFieldMasking, options);
			}
			else if (value.SpanGap != null)
			{
				writer.WritePropertyName("span_gap");
				JsonSerializer.Serialize(writer, value.SpanGap, options);
			}

			writer.WriteEndObject();
		}
	}
}
