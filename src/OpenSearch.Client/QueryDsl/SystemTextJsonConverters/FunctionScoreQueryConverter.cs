/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Collections.Generic;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IFunctionScoreQuery"/>.
	/// JSON format:
	/// <code>
	/// {"query": {...}, "functions": [...], "score_mode": "...", "boost_mode": "...",
	///  "max_boost": ..., "min_score": ...}
	/// </code>
	/// </summary>
	internal sealed class FunctionScoreQueryConverter : JsonConverter<IFunctionScoreQuery>
	{
		private static readonly ScoreFunctionConverter FunctionConverter = new ScoreFunctionConverter();

		public override IFunctionScoreQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IFunctionScoreQuery but got {reader.TokenType}");

			var query = new FunctionScoreQuery();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName");

				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "query":
						query.Query = JsonSerializer.Deserialize<QueryContainer>(ref reader, options);
						break;
					case "functions":
						query.Functions = ReadFunctions(ref reader, options);
						break;
					case "score_mode":
						query.ScoreMode = ParseScoreMode(reader.GetString());
						break;
					case "boost_mode":
						query.BoostMode = ParseBoostMode(reader.GetString());
						break;
					case "max_boost":
						query.MaxBoost = reader.GetDouble();
						break;
					case "min_score":
						query.MinScore = reader.GetDouble();
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

		public override void Write(Utf8JsonWriter writer, IFunctionScoreQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (value.Query != null)
			{
				writer.WritePropertyName("query");
				JsonSerializer.Serialize(writer, value.Query, options);
			}

			if (value.Functions != null)
			{
				writer.WritePropertyName("functions");
				writer.WriteStartArray();
				foreach (var fn in value.Functions)
					FunctionConverter.Write(writer, fn, options);
				writer.WriteEndArray();
			}

			if (value.ScoreMode.HasValue)
			{
				writer.WritePropertyName("score_mode");
				writer.WriteStringValue(GetScoreModeString(value.ScoreMode.Value));
			}

			if (value.BoostMode.HasValue)
			{
				writer.WritePropertyName("boost_mode");
				writer.WriteStringValue(GetBoostModeString(value.BoostMode.Value));
			}

			if (value.MaxBoost.HasValue)
			{
				writer.WritePropertyName("max_boost");
				writer.WriteNumberValue(value.MaxBoost.Value);
			}

			if (value.MinScore.HasValue)
			{
				writer.WritePropertyName("min_score");
				writer.WriteNumberValue(value.MinScore.Value);
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

		private IEnumerable<IScoreFunction> ReadFunctions(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartArray)
				throw new JsonException("Expected StartArray for functions");

			var list = new List<IScoreFunction>();
			while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
			{
				var fn = FunctionConverter.Read(ref reader, typeof(IScoreFunction), options);
				if (fn != null)
					list.Add(fn);
			}
			return list;
		}

		private static FunctionScoreMode? ParseScoreMode(string v) => v?.ToLowerInvariant() switch
		{
			"multiply" => FunctionScoreMode.Multiply,
			"sum" => FunctionScoreMode.Sum,
			"avg" => FunctionScoreMode.Average,
			"first" => FunctionScoreMode.First,
			"max" => FunctionScoreMode.Max,
			"min" => FunctionScoreMode.Min,
			_ => null
		};

		private static string GetScoreModeString(FunctionScoreMode v) => v switch
		{
			FunctionScoreMode.Multiply => "multiply",
			FunctionScoreMode.Sum => "sum",
			FunctionScoreMode.Average => "avg",
			FunctionScoreMode.First => "first",
			FunctionScoreMode.Max => "max",
			FunctionScoreMode.Min => "min",
			_ => v.ToString().ToLowerInvariant()
		};

		private static FunctionBoostMode? ParseBoostMode(string v) => v?.ToLowerInvariant() switch
		{
			"multiply" => FunctionBoostMode.Multiply,
			"replace" => FunctionBoostMode.Replace,
			"sum" => FunctionBoostMode.Sum,
			"avg" => FunctionBoostMode.Average,
			"max" => FunctionBoostMode.Max,
			"min" => FunctionBoostMode.Min,
			_ => null
		};

		private static string GetBoostModeString(FunctionBoostMode v) => v switch
		{
			FunctionBoostMode.Multiply => "multiply",
			FunctionBoostMode.Replace => "replace",
			FunctionBoostMode.Sum => "sum",
			FunctionBoostMode.Average => "avg",
			FunctionBoostMode.Max => "max",
			FunctionBoostMode.Min => "min",
			_ => v.ToString().ToLowerInvariant()
		};
	}
}
