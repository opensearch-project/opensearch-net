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

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IMatchQuery"/>.
	/// JSON format: <c>{"field_name": {"query": "search text", "analyzer": "...", "boost": 1.0, ...}}</c>
	/// Also supports short form: <c>{"field_name": "search text"}</c>
	/// </summary>
	internal sealed class MatchQueryConverter : FieldNameQueryConverterBase<IMatchQuery, MatchQuery>
	{
		protected override void ReadInnerProperties(ref Utf8JsonReader reader, MatchQuery query, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected StartObject for match query inner properties");

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName token");

				var propertyName = reader.GetString();
				reader.Read(); // Move to value

				switch (propertyName)
				{
					case "query":
						query.Query = reader.GetString();
						break;
					case "analyzer":
						query.Analyzer = reader.GetString();
						break;
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "fuzziness":
						// Fuzziness can be a string like "AUTO" or a number
						query.Fuzziness = ReadFuzziness(ref reader);
						break;
					case "operator":
						var op = reader.GetString();
						if (Enum.TryParse<Operator>(op, true, out var operatorValue))
							query.Operator = operatorValue;
						break;
					case "minimum_should_match":
						query.MinimumShouldMatch = ReadMinimumShouldMatch(ref reader);
						break;
					case "lenient":
						query.Lenient = reader.GetBoolean();
						break;
					case "max_expansions":
						query.MaxExpansions = reader.GetInt32();
						break;
					case "prefix_length":
						query.PrefixLength = reader.GetInt32();
						break;
					case "fuzzy_transpositions":
						query.FuzzyTranspositions = reader.GetBoolean();
						break;
					case "auto_generate_synonyms_phrase_query":
						query.AutoGenerateSynonymsPhraseQuery = reader.GetBoolean();
						break;
					case "zero_terms_query":
						var ztq = reader.GetString();
						if (Enum.TryParse<ZeroTermsQuery>(ztq, true, out var zeroTerms))
							query.ZeroTermsQuery = zeroTerms;
						break;
					case "_name":
						query.Name = reader.GetString();
						break;
					default:
						// Skip unknown properties
						reader.Skip();
						break;
				}
			}
		}

		protected override void WriteInnerProperties(Utf8JsonWriter writer, IMatchQuery value, JsonSerializerOptions options)
		{
			if (value.Query != null)
			{
				writer.WritePropertyName("query");
				writer.WriteStringValue(value.Query);
			}

			if (value.Analyzer != null)
			{
				writer.WritePropertyName("analyzer");
				writer.WriteStringValue(value.Analyzer);
			}

			if (value.Fuzziness != null)
			{
				writer.WritePropertyName("fuzziness");
				WriteFuzziness(writer, value.Fuzziness);
			}

			if (value.Operator.HasValue)
			{
				writer.WritePropertyName("operator");
				writer.WriteStringValue(value.Operator.Value.ToString().ToLowerInvariant());
			}

			if (value.MinimumShouldMatch != null)
			{
				writer.WritePropertyName("minimum_should_match");
				WriteMinimumShouldMatch(writer, value.MinimumShouldMatch);
			}

			if (value.Lenient.HasValue)
			{
				writer.WritePropertyName("lenient");
				writer.WriteBooleanValue(value.Lenient.Value);
			}

			if (value.MaxExpansions.HasValue)
			{
				writer.WritePropertyName("max_expansions");
				writer.WriteNumberValue(value.MaxExpansions.Value);
			}

			if (value.PrefixLength.HasValue)
			{
				writer.WritePropertyName("prefix_length");
				writer.WriteNumberValue(value.PrefixLength.Value);
			}

			if (value.FuzzyTranspositions.HasValue)
			{
				writer.WritePropertyName("fuzzy_transpositions");
				writer.WriteBooleanValue(value.FuzzyTranspositions.Value);
			}

			if (value.AutoGenerateSynonymsPhraseQuery.HasValue)
			{
				writer.WritePropertyName("auto_generate_synonyms_phrase_query");
				writer.WriteBooleanValue(value.AutoGenerateSynonymsPhraseQuery.Value);
			}

			if (value.ZeroTermsQuery.HasValue)
			{
				writer.WritePropertyName("zero_terms_query");
				writer.WriteStringValue(value.ZeroTermsQuery.Value.ToString().ToLowerInvariant());
			}
		}

		protected override void ReadShortForm(ref Utf8JsonReader reader, MatchQuery query, JsonSerializerOptions options)
		{
			// Short form: {"field": "search text"}
			if (reader.TokenType == JsonTokenType.String)
				query.Query = reader.GetString();
			else
				throw new JsonException($"Unexpected token {reader.TokenType} in match query short form");
		}

		private static IFuzziness ReadFuzziness(ref Utf8JsonReader reader)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				var value = reader.GetString();
				if (string.Equals(value, "AUTO", StringComparison.OrdinalIgnoreCase))
					return Fuzziness.Auto;
				if (int.TryParse(value, out var intVal))
					return Fuzziness.EditDistance(intVal);
				return Fuzziness.Auto;
			}

			if (reader.TokenType == JsonTokenType.Number)
			{
				var editDistance = reader.GetInt32();
				return Fuzziness.EditDistance(editDistance);
			}

			return null;
		}

		private static void WriteFuzziness(Utf8JsonWriter writer, IFuzziness fuzziness)
		{
			// Fuzziness serializes as its string representation
			var value = fuzziness.ToString();
			if (int.TryParse(value, out var intVal))
				writer.WriteNumberValue(intVal);
			else
				writer.WriteStringValue(value);
		}

		private static MinimumShouldMatch ReadMinimumShouldMatch(ref Utf8JsonReader reader)
		{
			if (reader.TokenType == JsonTokenType.Number)
				return reader.GetInt32();

			if (reader.TokenType == JsonTokenType.String)
				return reader.GetString();

			return null;
		}

		private static void WriteMinimumShouldMatch(Utf8JsonWriter writer, MinimumShouldMatch msm)
		{
			if (msm == null)
			{
				writer.WriteNullValue();
				return;
			}

			var value = msm.ToString();
			if (int.TryParse(value, out var intVal))
				writer.WriteNumberValue(intVal);
			else
				writer.WriteStringValue(value);
		}
	}
}
