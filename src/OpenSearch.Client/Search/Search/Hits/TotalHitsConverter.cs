/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="TotalHits"/>, replacing the vendored
	/// Utf8Json <c>TotalHitsFormatter</c> as part of #388. Reads either a bare number (the value) or an
	/// object <c>{ "value": …, "relation": "eq"|"gte" }</c>; writes the object form when a relation is
	/// present, otherwise the bare number.
	/// </summary>
	internal sealed class TotalHitsConverter : JsonConverter<TotalHits>
	{
		public override TotalHits Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					return new TotalHits { Value = reader.GetInt64() };
				case JsonTokenType.StartObject:
				{
					using var document = JsonDocument.ParseValue(ref reader);
					var root = document.RootElement;
					var totalHits = new TotalHits { Value = -1 };
					if (root.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Number)
						totalHits.Value = value.GetInt64();
					if (root.TryGetProperty("relation", out var relation) && relation.ValueKind != JsonValueKind.Null)
						totalHits.Relation = relation.Deserialize<TotalHitsRelation>(options);
					return totalHits;
				}
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, TotalHits value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (value.Relation.HasValue)
			{
				writer.WriteStartObject();
				writer.WriteNumber("value", value.Value);
				writer.WritePropertyName("relation");
				JsonSerializer.Serialize(writer, value.Relation.Value, options);
				writer.WriteEndObject();
			}
			else
				writer.WriteNumberValue(value.Value);
		}
	}
}
