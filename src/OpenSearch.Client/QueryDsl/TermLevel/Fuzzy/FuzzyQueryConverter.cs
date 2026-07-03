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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IFuzzyQuery"/>, replacing the
	/// vendored Utf8Json <c>FuzzyQueryFormatter</c> as part of #388. It is a field-name-keyed query
	/// whose outer shape is <c>{ "&lt;field&gt;": { …body… } }</c>; the body flattens <c>value</c>,
	/// <c>fuzziness</c>, <c>prefix_length</c>, <c>max_expansions</c>, <c>transpositions</c>,
	/// <c>rewrite</c>, <c>_name</c> and <c>boost</c> alongside each other. Mirrors the formatter's
	/// logic for selecting the concrete fuzzy type (<see cref="FuzzyQuery"/> /
	/// <see cref="FuzzyDateQuery"/> / <see cref="FuzzyNumericQuery"/>) and for the per-type
	/// <c>value</c>/<c>fuzziness</c> shapes. Constructed with the connection settings for field-name
	/// inference.
	/// </summary>
	internal sealed class FuzzyQueryConverter : JsonConverter<IFuzzyQuery>
	{
		private readonly IConnectionSettingsValues _settings;

		public FuzzyQueryConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override void Write(Utf8JsonWriter writer, IFuzzyQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			var field = value.Field == null ? null : _settings.Inferrer.Field(value.Field);
			if (!string.IsNullOrEmpty(field))
			{
				writer.WritePropertyName(field);
				writer.WriteStartObject();

				// Member order mirrors the concrete fuzzy query interfaces' [DataMember] declaration
				// order, which the original formatter emitted via the dynamic resolver: fuzziness,
				// value, max_expansions, prefix_length, transpositions, rewrite, boost, _name.
				// value + fuzziness are typed per concrete fuzzy query; delegate through JsonSerializer
				// so the registered converters (IFuzziness, Time, …) are used.
				switch (value)
				{
					case IFuzzyStringQuery stringQuery:
						if (stringQuery.Fuzziness != null)
						{
							writer.WritePropertyName("fuzziness");
							JsonSerializer.Serialize(writer, (IFuzziness)stringQuery.Fuzziness, options);
						}
						if (stringQuery.Value != null)
						{
							writer.WritePropertyName("value");
							JsonSerializer.Serialize(writer, stringQuery.Value, options);
						}
						break;
					case IFuzzyDateQuery dateQuery:
						if (dateQuery.Fuzziness != null)
						{
							writer.WritePropertyName("fuzziness");
							JsonSerializer.Serialize(writer, dateQuery.Fuzziness, options);
						}
						if (dateQuery.Value.HasValue)
						{
							writer.WritePropertyName("value");
							JsonSerializer.Serialize(writer, dateQuery.Value.Value, options);
						}
						break;
					case IFuzzyNumericQuery numericQuery:
						if (numericQuery.Fuzziness.HasValue)
						{
							writer.WritePropertyName("fuzziness");
							JsonSerializer.Serialize(writer, numericQuery.Fuzziness.Value, options);
						}
						if (numericQuery.Value.HasValue)
						{
							writer.WritePropertyName("value");
							JsonSerializer.Serialize(writer, numericQuery.Value.Value, options);
						}
						break;
				}

				if (value.MaxExpansions.HasValue)
					writer.WriteNumber("max_expansions", value.MaxExpansions.Value);
				if (value.PrefixLength.HasValue)
					writer.WriteNumber("prefix_length", value.PrefixLength.Value);
				if (value.Transpositions.HasValue)
					writer.WriteBoolean("transpositions", value.Transpositions.Value);
				if (value.Rewrite != null)
				{
					writer.WritePropertyName("rewrite");
					JsonSerializer.Serialize(writer, value.Rewrite, options);
				}
				if (value.Boost.HasValue)
				{
					writer.WritePropertyName("boost");
					JsonSerializer.Serialize(writer, value.Boost.Value, options);
				}
				if (!string.IsNullOrEmpty(value.Name))
					writer.WriteString("_name", value.Name);

				writer.WriteEndObject();
			}

			writer.WriteEndObject();
		}

		public override IFuzzyQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			foreach (var member in root.EnumerateObject())
			{
				// The formatter nests the whole body (including _name/boost) under the field key;
				// the single object-valued member is that field body.
				if (member.Value.ValueKind != JsonValueKind.Object) continue;

				var query = ReadBody(member.Value, options);
				if (query != null)
					query.Field = member.Name;
				return query;
			}

			return null;
		}

		private static IFuzzyQuery ReadBody(JsonElement body, JsonSerializerOptions options)
		{
			// Choose the concrete fuzzy type by inspecting `value`, exactly like the formatter:
			// a string that parses as a date -> FuzzyDateQuery, any other string -> FuzzyQuery,
			// a number -> FuzzyNumericQuery.
			IFuzzyQuery query;
			if (body.TryGetProperty("value", out var valueElement))
			{
				switch (valueElement.ValueKind)
				{
					case JsonValueKind.Number:
						query = new FuzzyNumericQuery { Value = valueElement.GetDouble() };
						break;
					case JsonValueKind.String:
						if (valueElement.TryGetDateTime(out var dateTime))
							query = new FuzzyDateQuery { Value = dateTime };
						else
							query = new FuzzyQuery { Value = valueElement.GetString() };
						break;
					default:
						query = new FuzzyQuery();
						break;
				}
			}
			else
				query = new FuzzyQuery();

			// fuzziness is typed per concrete query; mirror SetFuzziness in the formatter.
			if (body.TryGetProperty("fuzziness", out var fuzzinessElement)
				&& fuzzinessElement.ValueKind != JsonValueKind.Null)
			{
				switch (query)
				{
					case FuzzyDateQuery dateQuery:
						dateQuery.Fuzziness = fuzzinessElement.Deserialize<Time>(options);
						break;
					case FuzzyNumericQuery numericQuery:
						numericQuery.Fuzziness = fuzzinessElement.GetDouble();
						break;
					case FuzzyQuery stringQuery:
						stringQuery.Fuzziness = (Fuzziness)fuzzinessElement.Deserialize<IFuzziness>(options);
						break;
				}
			}

			if (body.TryGetProperty("prefix_length", out var prefixLength)
				&& prefixLength.ValueKind == JsonValueKind.Number)
				query.PrefixLength = prefixLength.GetInt32();
			if (body.TryGetProperty("max_expansions", out var maxExpansions)
				&& maxExpansions.ValueKind == JsonValueKind.Number)
				query.MaxExpansions = maxExpansions.GetInt32();
			if (body.TryGetProperty("transpositions", out var transpositions)
				&& (transpositions.ValueKind == JsonValueKind.True || transpositions.ValueKind == JsonValueKind.False))
				query.Transpositions = transpositions.GetBoolean();
			if (body.TryGetProperty("rewrite", out var rewrite)
				&& rewrite.ValueKind != JsonValueKind.Null)
				query.Rewrite = rewrite.Deserialize<MultiTermQueryRewrite>(options);
			if (body.TryGetProperty("_name", out var name)
				&& name.ValueKind == JsonValueKind.String)
				query.Name = name.GetString();
			if (body.TryGetProperty("boost", out var boost)
				&& boost.ValueKind == JsonValueKind.Number)
				query.Boost = boost.GetDouble();

			return query;
		}
	}
}
