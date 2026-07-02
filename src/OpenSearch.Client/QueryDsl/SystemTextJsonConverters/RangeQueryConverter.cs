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
	/// Converter for <see cref="IRangeQuery"/>.
	/// JSON format: <c>{"field": {"gte": ..., "lte": ..., "boost": 1.0, "format": "...", "time_zone": "..."}}</c>
	///
	/// Range queries are polymorphic - they can be date ranges or numeric ranges.
	/// Discrimination is based on presence of date-specific properties (format, time_zone)
	/// or the value format of gte/lte/gt/lt.
	/// </summary>
	internal sealed class RangeQueryConverter : JsonConverter<IRangeQuery>
	{
		public override IRangeQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject token but got {reader.TokenType}");

			reader.Read(); // Move past StartObject

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected field name property in range query");

			var fieldName = reader.GetString();
			reader.Read(); // Move past PropertyName to StartObject of inner properties

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected StartObject for range query inner properties");

			// We need to buffer the inner object to determine the range type
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var isDate = root.TryGetProperty("format", out _) ||
						 root.TryGetProperty("time_zone", out _);

			if (!isDate)
			{
				// Check if any of the range values look like dates
				isDate = IsDateValue(root, "gte") || IsDateValue(root, "lte") ||
						 IsDateValue(root, "gt") || IsDateValue(root, "lt");
			}

			if (isDate)
			{
				var query = ReadDateRange(root, fieldName);
				reader.Read(); // Move past outer EndObject
				return query;
			}
			else
			{
				var query = ReadNumericRange(root, fieldName);
				reader.Read(); // Move past outer EndObject
				return query;
			}
		}

		public override void Write(Utf8JsonWriter writer, IRangeQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			var field = ((IFieldNameQuery)value).Field;
			var fieldName = field?.ToString() ?? throw new JsonException("Field name cannot be null for a range query");
			writer.WritePropertyName(fieldName);
			writer.WriteStartObject();

			switch (value)
			{
				case IDateRangeQuery dateRange:
					WriteDateRange(writer, dateRange);
					break;
				case INumericRangeQuery numericRange:
					WriteNumericRange(writer, numericRange);
					break;
				case ILongRangeQuery longRange:
					WriteLongRange(writer, longRange);
					break;
				default:
					// Fallback - write nothing specific
					break;
			}

			// Write common properties
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
			writer.WriteEndObject();
		}

		private static bool IsDateValue(JsonElement root, string propertyName)
		{
			if (!root.TryGetProperty(propertyName, out var element))
				return false;

			if (element.ValueKind == JsonValueKind.String)
			{
				var str = element.GetString();
				// Heuristic: if it contains date separators or date math operators
				return str != null && (str.Contains("||") || str.Contains("now") ||
					   str.Contains("-") && str.Length >= 10);
			}

			return false;
		}

		private static DateRangeQuery ReadDateRange(JsonElement root, string fieldName)
		{
			var query = new DateRangeQuery { Field = new Field(fieldName) };

			if (root.TryGetProperty("gte", out var gte) && gte.ValueKind == JsonValueKind.String)
				query.GreaterThanOrEqualTo = DateMath.FromString(gte.GetString());

			if (root.TryGetProperty("gt", out var gt) && gt.ValueKind == JsonValueKind.String)
				query.GreaterThan = DateMath.FromString(gt.GetString());

			if (root.TryGetProperty("lte", out var lte) && lte.ValueKind == JsonValueKind.String)
				query.LessThanOrEqualTo = DateMath.FromString(lte.GetString());

			if (root.TryGetProperty("lt", out var lt) && lt.ValueKind == JsonValueKind.String)
				query.LessThan = DateMath.FromString(lt.GetString());

			if (root.TryGetProperty("format", out var format))
				query.Format = format.GetString();

			if (root.TryGetProperty("time_zone", out var timeZone))
				query.TimeZone = timeZone.GetString();

			if (root.TryGetProperty("relation", out var relation))
			{
				var rel = relation.GetString();
				if (Enum.TryParse<RangeRelation>(rel, true, out var rangeRelation))
					query.Relation = rangeRelation;
			}

			if (root.TryGetProperty("boost", out var boost))
				query.Boost = boost.GetDouble();

			if (root.TryGetProperty("_name", out var name))
				query.Name = name.GetString();

			return query;
		}

		private static NumericRangeQuery ReadNumericRange(JsonElement root, string fieldName)
		{
			var query = new NumericRangeQuery { Field = new Field(fieldName) };

			if (root.TryGetProperty("gte", out var gte) && gte.ValueKind == JsonValueKind.Number)
				query.GreaterThanOrEqualTo = gte.GetDouble();

			if (root.TryGetProperty("gt", out var gt) && gt.ValueKind == JsonValueKind.Number)
				query.GreaterThan = gt.GetDouble();

			if (root.TryGetProperty("lte", out var lte) && lte.ValueKind == JsonValueKind.Number)
				query.LessThanOrEqualTo = lte.GetDouble();

			if (root.TryGetProperty("lt", out var lt) && lt.ValueKind == JsonValueKind.Number)
				query.LessThan = lt.GetDouble();

			if (root.TryGetProperty("relation", out var relation))
			{
				var rel = relation.GetString();
				if (Enum.TryParse<RangeRelation>(rel, true, out var rangeRelation))
					query.Relation = rangeRelation;
			}

			if (root.TryGetProperty("boost", out var boost))
				query.Boost = boost.GetDouble();

			if (root.TryGetProperty("_name", out var name))
				query.Name = name.GetString();

			return query;
		}

		private static void WriteDateRange(Utf8JsonWriter writer, IDateRangeQuery value)
		{
			if (value.GreaterThanOrEqualTo != null)
			{
				writer.WritePropertyName("gte");
				writer.WriteStringValue(value.GreaterThanOrEqualTo.ToString());
			}

			if (value.GreaterThan != null)
			{
				writer.WritePropertyName("gt");
				writer.WriteStringValue(value.GreaterThan.ToString());
			}

			if (value.LessThanOrEqualTo != null)
			{
				writer.WritePropertyName("lte");
				writer.WriteStringValue(value.LessThanOrEqualTo.ToString());
			}

			if (value.LessThan != null)
			{
				writer.WritePropertyName("lt");
				writer.WriteStringValue(value.LessThan.ToString());
			}

			if (value.Format != null)
			{
				writer.WritePropertyName("format");
				writer.WriteStringValue(value.Format);
			}

			if (value.TimeZone != null)
			{
				writer.WritePropertyName("time_zone");
				writer.WriteStringValue(value.TimeZone);
			}

			if (value.Relation.HasValue)
			{
				writer.WritePropertyName("relation");
				writer.WriteStringValue(value.Relation.Value.ToString().ToLowerInvariant());
			}
		}

		private static void WriteNumericRange(Utf8JsonWriter writer, INumericRangeQuery value)
		{
			if (value.GreaterThanOrEqualTo.HasValue)
			{
				writer.WritePropertyName("gte");
				writer.WriteNumberValue(value.GreaterThanOrEqualTo.Value);
			}

			if (value.GreaterThan.HasValue)
			{
				writer.WritePropertyName("gt");
				writer.WriteNumberValue(value.GreaterThan.Value);
			}

			if (value.LessThanOrEqualTo.HasValue)
			{
				writer.WritePropertyName("lte");
				writer.WriteNumberValue(value.LessThanOrEqualTo.Value);
			}

			if (value.LessThan.HasValue)
			{
				writer.WritePropertyName("lt");
				writer.WriteNumberValue(value.LessThan.Value);
			}

			if (value.Relation.HasValue)
			{
				writer.WritePropertyName("relation");
				writer.WriteStringValue(value.Relation.Value.ToString().ToLowerInvariant());
			}
		}

		private static void WriteLongRange(Utf8JsonWriter writer, ILongRangeQuery value)
		{
			if (value.GreaterThanOrEqualTo.HasValue)
			{
				writer.WritePropertyName("gte");
				writer.WriteNumberValue(value.GreaterThanOrEqualTo.Value);
			}

			if (value.GreaterThan.HasValue)
			{
				writer.WritePropertyName("gt");
				writer.WriteNumberValue(value.GreaterThan.Value);
			}

			if (value.LessThanOrEqualTo.HasValue)
			{
				writer.WritePropertyName("lte");
				writer.WriteNumberValue(value.LessThanOrEqualTo.Value);
			}

			if (value.LessThan.HasValue)
			{
				writer.WritePropertyName("lt");
				writer.WriteNumberValue(value.LessThan.Value);
			}

			if (value.Relation.HasValue)
			{
				writer.WritePropertyName("relation");
				writer.WriteStringValue(value.Relation.Value.ToString().ToLowerInvariant());
			}
		}
	}
}
