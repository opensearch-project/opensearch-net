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
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="IRangeQuery"/>,
	/// replacing the vendored Utf8Json <c>RangeQueryFormatter</c> as part of #388.
	/// <para>
	/// A range query has no discriminator; the concrete type is inferred from the bound values inside
	/// the field-keyed body: <c>format</c>/<c>time_zone</c> or a date-like bound → date range; a
	/// non-integral number → numeric range; an integral number → long range; otherwise a term (string)
	/// range. Dispatch then delegates to the concrete type's field-name converter (which handles the
	/// <c>{ "&lt;field&gt;": { … } }</c> wrapping).
	/// </para>
	/// </summary>
	internal sealed class RangeQueryInterfaceConverter : JsonConverter<IRangeQuery>
	{
		public override void Write(Utf8JsonWriter writer, IRangeQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value)
			{
				case IDateRangeQuery date:
					JsonSerializer.Serialize(writer, date, options);
					break;
				case INumericRangeQuery numeric:
					JsonSerializer.Serialize(writer, numeric, options);
					break;
				case ILongRangeQuery @long:
					JsonSerializer.Serialize(writer, @long, options);
					break;
				case ITermRangeQuery term:
					JsonSerializer.Serialize(writer, term, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}

		public override IRangeQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			var target = typeof(ITermRangeQuery);

			foreach (var field in root.EnumerateObject())
			{
				if (field.Value.ValueKind != JsonValueKind.Object) continue;

				bool isDate = false, isDouble = false, isLong = false;
				foreach (var bound in field.Value.EnumerateObject())
				{
					switch (bound.Name)
					{
						case "format":
						case "time_zone":
							isDate = true;
							break;
						case "gt":
						case "gte":
						case "lt":
						case "lte":
							if (bound.Value.ValueKind == JsonValueKind.String)
							{
								if (IsDateLike(bound.Value.GetString())) isDate = true;
							}
							else if (bound.Value.ValueKind == JsonValueKind.Number)
							{
								var raw = bound.Value.GetRawText();
								if (raw.IndexOf('.') >= 0 || raw.IndexOf('e') >= 0 || raw.IndexOf('E') >= 0)
									isDouble = true;
								else
									isLong = true;
							}
							break;
					}

					if (isDate || isDouble) break;
				}

				target = isDate ? typeof(IDateRangeQuery)
					: isDouble ? typeof(INumericRangeQuery)
					: isLong ? typeof(ILongRangeQuery)
					: typeof(ITermRangeQuery);
				break;
			}

			return (IRangeQuery)root.Deserialize(target, options);
		}

		private static bool IsDateLike(string value)
		{
			if (string.IsNullOrEmpty(value)) return false;
			if (value.Contains("||") || value.Contains("now") || value.IndexOf('/') >= 0) return true;
			return DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
				System.Globalization.DateTimeStyles.RoundtripKind, out _);
		}
	}
}
