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
	/// System.Text.Json replacement for the legacy Utf8Json <c>RangeQueryFormatter</c>.
	///
	/// <see cref="IRangeQuery"/> is polymorphic: OpenSearch serializes every variant with the same
	/// <c>{ "field": { ...bounds... } }</c> shape, so the concrete type has to be inferred from the values of the
	/// inner bound fields (<c>gt</c>/<c>gte</c>/<c>lt</c>/<c>lte</c>) plus the presence of the date-only options
	/// <c>format</c>/<c>time_zone</c>:
	/// <list type="bullet">
	/// <item><description>a <c>format</c> or <c>time_zone</c> option, or a bound whose string value is an ISO8601
	/// date or a date-math expression, selects <see cref="DateRangeQuery"/>;</description></item>
	/// <item><description>a floating-point numeric bound selects <see cref="NumericRangeQuery"/>;</description></item>
	/// <item><description>an integral numeric bound selects <see cref="LongRangeQuery"/>;</description></item>
	/// <item><description>otherwise <see cref="TermRangeQuery"/>.</description></item>
	/// </list>
	/// System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound, so — unlike the Utf8Json
	/// version which peeked at a byte segment and re-read it — we buffer the value into a <see cref="JsonDocument"/>,
	/// inspect the DOM to choose the concrete type, then read the field name and bounds directly from the DOM.
	/// Serialization writes by runtime type.
	/// </summary>
	internal class RangeQueryConverter : JsonConverter<IRangeQuery>
	{
		public override IRangeQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

				return ReadBody(field, body);
			}

			return null;
		}

		private static IRangeQuery ReadBody(string field, JsonElement body)
		{
			var isLong = false;
			var isDate = false;
			var isDouble = false;

			foreach (var bound in body.EnumerateObject())
			{
				switch (bound.Name)
				{
					case "format":
					case "time_zone":
						isDate = true;
						break;
					case "gt":
					case "gte":
					case "lte":
					case "lt":
						switch (bound.Value.ValueKind)
						{
							case JsonValueKind.String:
							case JsonValueKind.Null:
								if (!isDate)
								{
									var s = bound.Value.ValueKind == JsonValueKind.String ? bound.Value.GetString() : null;
									isDate = IsDateTime(s) ||
										(ContainsDateMathSeparator(s) && DateMath.IsValidDateMathString(s));
								}
								break;
							case JsonValueKind.Number:
								if (!isDouble)
								{
									if (bound.Value.TryGetInt64(out _))
										isLong = true;
									else
										isDouble = true;
								}
								break;
						}
						break;
				}

				if (isDate || isDouble)
					break;
			}

			if (isDate)
				return ReadDate(field, body);
			if (isDouble)
				return ReadNumeric(field, body);
			if (isLong)
				return ReadLong(field, body);

			return ReadTerm(field, body);
		}

		private static IRangeQuery ReadDate(string field, JsonElement body)
		{
			var query = new DateRangeQuery { Field = field };
			if (body.TryGetProperty("gt", out var gt) && gt.ValueKind == JsonValueKind.String) query.GreaterThan = gt.GetString();
			if (body.TryGetProperty("gte", out var gte) && gte.ValueKind == JsonValueKind.String) query.GreaterThanOrEqualTo = gte.GetString();
			if (body.TryGetProperty("lt", out var lt) && lt.ValueKind == JsonValueKind.String) query.LessThan = lt.GetString();
			if (body.TryGetProperty("lte", out var lte) && lte.ValueKind == JsonValueKind.String) query.LessThanOrEqualTo = lte.GetString();
			if (body.TryGetProperty("format", out var format) && format.ValueKind == JsonValueKind.String) query.Format = format.GetString();
			if (body.TryGetProperty("time_zone", out var tz) && tz.ValueKind == JsonValueKind.String) query.TimeZone = tz.GetString();
			if (body.TryGetProperty("relation", out var relation)) query.Relation = ParseRelation(relation);
			ReadCommon(query, body);
			return query;
		}

		private static IRangeQuery ReadNumeric(string field, JsonElement body)
		{
			var query = new NumericRangeQuery { Field = field };
			if (body.TryGetProperty("gt", out var gt) && gt.ValueKind == JsonValueKind.Number) query.GreaterThan = gt.GetDouble();
			if (body.TryGetProperty("gte", out var gte) && gte.ValueKind == JsonValueKind.Number) query.GreaterThanOrEqualTo = gte.GetDouble();
			if (body.TryGetProperty("lt", out var lt) && lt.ValueKind == JsonValueKind.Number) query.LessThan = lt.GetDouble();
			if (body.TryGetProperty("lte", out var lte) && lte.ValueKind == JsonValueKind.Number) query.LessThanOrEqualTo = lte.GetDouble();
			if (body.TryGetProperty("relation", out var relation)) query.Relation = ParseRelation(relation);
			ReadCommon(query, body);
			return query;
		}

		private static IRangeQuery ReadLong(string field, JsonElement body)
		{
			var query = new LongRangeQuery { Field = field };
			if (body.TryGetProperty("gt", out var gt) && gt.ValueKind == JsonValueKind.Number) query.GreaterThan = gt.GetInt64();
			if (body.TryGetProperty("gte", out var gte) && gte.ValueKind == JsonValueKind.Number) query.GreaterThanOrEqualTo = gte.GetInt64();
			if (body.TryGetProperty("lt", out var lt) && lt.ValueKind == JsonValueKind.Number) query.LessThan = lt.GetInt64();
			if (body.TryGetProperty("lte", out var lte) && lte.ValueKind == JsonValueKind.Number) query.LessThanOrEqualTo = lte.GetInt64();
			if (body.TryGetProperty("relation", out var relation)) query.Relation = ParseRelation(relation);
			ReadCommon(query, body);
			return query;
		}

		private static IRangeQuery ReadTerm(string field, JsonElement body)
		{
			var query = new TermRangeQuery { Field = field };
			if (body.TryGetProperty("gt", out var gt) && gt.ValueKind == JsonValueKind.String) query.GreaterThan = gt.GetString();
			if (body.TryGetProperty("gte", out var gte) && gte.ValueKind == JsonValueKind.String) query.GreaterThanOrEqualTo = gte.GetString();
			if (body.TryGetProperty("lt", out var lt) && lt.ValueKind == JsonValueKind.String) query.LessThan = lt.GetString();
			if (body.TryGetProperty("lte", out var lte) && lte.ValueKind == JsonValueKind.String) query.LessThanOrEqualTo = lte.GetString();
			ReadCommon(query, body);
			return query;
		}

		// The query common options (_name / boost from QueryBase) apply to every range variant. The legacy formatter
		// picked these up by delegating to the full type formatter; here we read the bounds directly, so read them too.
		private static void ReadCommon(IRangeQuery query, JsonElement body)
		{
			if (body.TryGetProperty("_name", out var name) && name.ValueKind == JsonValueKind.String) query.Name = name.GetString();
			if (body.TryGetProperty("boost", out var boost) && boost.ValueKind == JsonValueKind.Number) query.Boost = boost.GetDouble();
		}

		private static RangeRelation? ParseRelation(JsonElement element)
		{
			if (element.ValueKind != JsonValueKind.String)
				return null;
			switch (element.GetString())
			{
				case "within": return RangeRelation.Within;
				case "contains": return RangeRelation.Contains;
				case "intersects": return RangeRelation.Intersects;
				default: return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, IRangeQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}

		private static bool IsDateTime(string value) =>
			value != null &&
			DateTime.TryParse(value, CultureInfo.InvariantCulture,
				DateTimeStyles.RoundtripKind | DateTimeStyles.AllowWhiteSpaces, out _);

		private static bool ContainsDateMathSeparator(string value) =>
			value != null && value.IndexOf("||", StringComparison.Ordinal) >= 0;
	}
}
