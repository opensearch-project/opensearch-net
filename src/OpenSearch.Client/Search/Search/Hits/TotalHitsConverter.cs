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
	/// System.Text.Json replacement for the legacy Utf8Json <c>TotalHitsFormatter</c>. A <see cref="TotalHits"/> is
	/// serialized either as a bare integral number (when <see cref="TotalHits.Relation"/> is <c>null</c>) or as a
	/// <c>{ "value": &lt;long&gt;, "relation": "eq"|"gte" }</c> object. On read, a JSON object populates both
	/// <see cref="TotalHits.Value"/> and <see cref="TotalHits.Relation"/>, a bare number populates only
	/// <see cref="TotalHits.Value"/> (defaulting <c>value</c> to <c>-1</c> when absent from an object), and any other
	/// token is skipped and yields <c>null</c>.
	/// </summary>
	internal class TotalHitsConverter : JsonConverter<TotalHits>
	{
		public override TotalHits Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.StartObject:
					long value = -1;
					TotalHitsRelation? relation = null;
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject)
							break;

						var propertyName = reader.GetString();
						reader.Read();
						if (propertyName == "value")
							value = reader.GetInt64();
						else if (propertyName == "relation")
							relation = JsonSerializer.Deserialize<TotalHitsRelation>(ref reader, options);
						else
							reader.Skip();
					}

					return new TotalHits { Value = value, Relation = relation };
				case JsonTokenType.Number:
					return new TotalHits { Value = reader.GetInt64() };
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
