/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>FuzzyQueryFormatter</c>.
	///
	/// <see cref="IFuzzyQuery"/> is polymorphic and, like the other field-name queries, is serialized as
	/// <c>{ "field": { "value": ..., "fuzziness": ..., ... } }</c>. The concrete type is inferred from the JSON type
	/// of the inner <c>value</c> field:
	/// <list type="bullet">
	/// <item><description>a string that parses as an ISO8601 date selects <see cref="FuzzyDateQuery"/>;</description></item>
	/// <item><description>any other string selects <see cref="FuzzyQuery"/> (the string variant);</description></item>
	/// <item><description>a number selects <see cref="FuzzyNumericQuery"/>.</description></item>
	/// </list>
	/// The <c>fuzziness</c> field is typed per concrete variant (<see cref="Fuzziness"/>, <see cref="Time"/> or a
	/// numeric ratio). System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound, so —
	/// unlike the Utf8Json version which buffered a byte segment for fuzziness until the value type was known — we
	/// parse the whole value into a <see cref="JsonDocument"/> and read the fields from the DOM in any order.
	/// Serialization writes by runtime type.
	/// </summary>
	internal class FuzzyQueryConverter : JsonConverter<IFuzzyQuery>
	{
		public override IFuzzyQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			foreach (var fieldProperty in root.EnumerateObject())
			{
				var field = fieldProperty.Name;
				var body = fieldProperty.Value;
				if (body.ValueKind != JsonValueKind.Object)
					continue;

				return ReadBody(field, body, options);
			}

			return null;
		}

		private static IFuzzyQuery ReadBody(string field, JsonElement body, JsonSerializerOptions options)
		{
			IFuzzyQuery query = null;

			// The concrete type is chosen from the "value" field's JSON type (mirrors the legacy formatter).
			if (body.TryGetProperty("value", out var valueElement))
			{
				switch (valueElement.ValueKind)
				{
					case JsonValueKind.String:
						var s = valueElement.GetString();
						if (IsDateTime(s, out var dateTime))
							query = new FuzzyDateQuery { Field = field, Value = dateTime };
						else
							query = new FuzzyQuery { Field = field, Value = s };
						break;
					case JsonValueKind.Number:
						query = new FuzzyNumericQuery { Field = field, Value = valueElement.GetDouble() };
						break;
				}
			}

			if (query == null)
				return null;

			if (body.TryGetProperty("fuzziness", out var fuzzinessElement))
				SetFuzziness(query, fuzzinessElement, options);

			if (body.TryGetProperty("prefix_length", out var prefixLength))
				query.PrefixLength = prefixLength.GetInt32();
			if (body.TryGetProperty("max_expansions", out var maxExpansions))
				query.MaxExpansions = maxExpansions.GetInt32();
			if (body.TryGetProperty("transpositions", out var transpositions))
				query.Transpositions = transpositions.GetBoolean();
			if (body.TryGetProperty("rewrite", out var rewrite))
				query.Rewrite = rewrite.Deserialize<MultiTermQueryRewrite>(options);
			if (body.TryGetProperty("_name", out var name))
				query.Name = name.GetString();
			if (body.TryGetProperty("boost", out var boost))
				query.Boost = boost.GetDouble();

			return query;
		}

		private static void SetFuzziness(IFuzzyQuery query, JsonElement fuzziness, JsonSerializerOptions options)
		{
			switch (query)
			{
				case FuzzyDateQuery fuzzyDateQuery:
					fuzzyDateQuery.Fuzziness = fuzziness.Deserialize<Time>(options);
					break;
				case FuzzyNumericQuery fuzzyNumericQuery:
					fuzzyNumericQuery.Fuzziness = fuzziness.GetDouble();
					break;
				case FuzzyQuery fuzzyQuery:
					fuzzyQuery.Fuzziness = fuzziness.Deserialize<IFuzziness>(options) as Fuzziness;
					break;
			}
		}

		public override void Write(Utf8JsonWriter writer, IFuzzyQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}

		private static bool IsDateTime(string value, out DateTime dateTime)
		{
			dateTime = default;
			return value != null &&
				DateTime.TryParse(value, CultureInfo.InvariantCulture,
					DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out dateTime);
		}
	}
}
