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

namespace OpenSearch.Client.Search.SystemTextJsonConverters;

/// <summary>
/// System.Text.Json converter for <see cref="TotalHits"/>.
/// Reads both legacy number format and object format {"value": N, "relation": "eq"/"gte"}.
/// Always writes in object format.
/// </summary>
internal sealed class TotalHitsConverter : JsonConverter<TotalHits>
{
	public override TotalHits? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		// Legacy format: just a number
		if (reader.TokenType == JsonTokenType.Number)
		{
			return new TotalHits
			{
				Value = reader.GetInt64()
			};
		}

		// Object format: {"value": N, "relation": "eq"/"gte"}
		if (reader.TokenType == JsonTokenType.StartObject)
		{
			long value = 0;
			TotalHitsRelation? relation = null;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var propertyName = reader.GetString();
				reader.Read();

				switch (propertyName)
				{
					case "value":
						value = reader.GetInt64();
						break;
					case "relation":
						var relationStr = reader.GetString();
						relation = relationStr switch
						{
							"eq" => TotalHitsRelation.EqualTo,
							"gte" => TotalHitsRelation.GreaterThanOrEqualTo,
							_ => null
						};
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return new TotalHits
			{
				Value = value,
				Relation = relation
			};
		}

		throw new JsonException($"Cannot deserialize TotalHits from token {reader.TokenType}");
	}

	public override void Write(Utf8JsonWriter writer, TotalHits value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();
		writer.WriteNumber("value", value.Value);
		if (value.Relation.HasValue)
		{
			var relationStr = value.Relation.Value switch
			{
				TotalHitsRelation.EqualTo => "eq",
				TotalHitsRelation.GreaterThanOrEqualTo => "gte",
				_ => "eq"
			};
			writer.WriteString("relation", relationStr);
		}
		else
		{
			writer.WriteString("relation", "eq");
		}
		writer.WriteEndObject();
	}
}
