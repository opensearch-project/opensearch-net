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
	/// Converter for <see cref="INestedQuery"/>.
	/// JSON format (standard object, no field-name wrapper):
	/// <code>
	/// {"path": "...", "query": {...}, "score_mode": "...", "inner_hits": {...}, "ignore_unmapped": true}
	/// </code>
	/// </summary>
	internal sealed class NestedQueryConverter : JsonConverter<INestedQuery>
	{
		public override INestedQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for INestedQuery but got {reader.TokenType}");

			var query = new NestedQuery();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName");

				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "path":
						query.Path = new Field(reader.GetString());
						break;
					case "query":
						query.Query = JsonSerializer.Deserialize<QueryContainer>(ref reader, options);
						break;
					case "score_mode":
						query.ScoreMode = ParseScoreMode(reader.GetString());
						break;
					case "inner_hits":
						query.InnerHits = JsonSerializer.Deserialize<IInnerHits>(ref reader, options);
						break;
					case "ignore_unmapped":
						query.IgnoreUnmapped = reader.GetBoolean();
						break;
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "_name":
						query.Name = reader.GetString();
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, INestedQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.Path != null)
			{
				writer.WritePropertyName("path");
				writer.WriteStringValue(value.Path.ToString());
			}

			if (value.Query != null)
			{
				writer.WritePropertyName("query");
				JsonSerializer.Serialize(writer, value.Query, options);
			}

			if (value.ScoreMode.HasValue)
			{
				writer.WritePropertyName("score_mode");
				writer.WriteStringValue(GetScoreModeString(value.ScoreMode.Value));
			}

			if (value.InnerHits != null)
			{
				writer.WritePropertyName("inner_hits");
				JsonSerializer.Serialize(writer, value.InnerHits, options);
			}

			if (value.IgnoreUnmapped.HasValue)
			{
				writer.WritePropertyName("ignore_unmapped");
				writer.WriteBooleanValue(value.IgnoreUnmapped.Value);
			}

			if (value.Boost.HasValue)
			{
				writer.WritePropertyName("boost");
				writer.WriteNumberValue(value.Boost.Value);
			}

			if (!string.IsNullOrEmpty(value.Name))
			{
				writer.WritePropertyName("_name");
				writer.WriteStringValue(value.Name);
			}

			writer.WriteEndObject();
		}

		private static NestedScoreMode? ParseScoreMode(string v) => v?.ToLowerInvariant() switch
		{
			"avg" => NestedScoreMode.Average,
			"sum" => NestedScoreMode.Sum,
			"min" => NestedScoreMode.Min,
			"max" => NestedScoreMode.Max,
			"none" => NestedScoreMode.None,
			_ => null
		};

		private static string GetScoreModeString(NestedScoreMode v) => v switch
		{
			NestedScoreMode.Average => "avg",
			NestedScoreMode.Sum => "sum",
			NestedScoreMode.Min => "min",
			NestedScoreMode.Max => "max",
			NestedScoreMode.None => "none",
			_ => v.ToString().ToLowerInvariant()
		};
	}
}
