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

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// Reproduces the legacy Utf8Json floating-point number format for <see cref="double"/>/<see cref="float"/>
	/// (and their nullable forms). Utf8Json's <c>DoubleToStringConverter</c> is configured with
	/// <c>EMIT_TRAILING_DECIMAL_POINT | EMIT_TRAILING_ZERO_AFTER_POINT</c>, so an integral value is written with a
	/// trailing <c>.0</c> (e.g. <c>10.0</c>, <c>0.0</c>) rather than System.Text.Json's default <c>10</c>. This
	/// difference makes otherwise-correct payloads fail exact-JSON comparisons across the client.
	///
	/// Non-integral values use .NET's shortest round-trippable representation (the same "shortest that round-trips"
	/// goal as Utf8Json's Grisu implementation), so only the integral case needs the explicit <c>.0</c> suffix.
	/// The value is written as a raw JSON number token via the writer's WriteRawValue.
	/// </summary>
	internal static class RealNumberFormat
	{
		public static void WriteDouble(Utf8JsonWriter writer, double value)
		{
			// Non-finite values have no valid JSON number form; defer to the default writer (throws like legacy).
			if (double.IsNaN(value) || double.IsInfinity(value))
			{
				writer.WriteNumberValue(value);
				return;
			}

			// Normalise negative zero to positive: the legacy Grisu formatter emitted "0.0" for -0.0, whereas
			// double.ToString("R") preserves the sign bit and yields "-0.0".
			if (value == 0.0) value = 0.0;

			var s = value.ToString("R", CultureInfo.InvariantCulture);
			writer.WriteRawValue(EnsureDecimal(s), skipInputValidation: true);
		}

		public static void WriteSingle(Utf8JsonWriter writer, float value)
		{
			if (float.IsNaN(value) || float.IsInfinity(value))
			{
				writer.WriteNumberValue(value);
				return;
			}

			if (value == 0.0f) value = 0.0f;

			var s = value.ToString("R", CultureInfo.InvariantCulture);
			writer.WriteRawValue(EnsureDecimal(s), skipInputValidation: true);
		}

		public static void WriteDecimal(Utf8JsonWriter writer, decimal value)
		{
			// Match the legacy DecimalFormatter's format string exactly. Its optional '#' placeholders drop trailing
			// zeros (3.10m -> "3.1"), whereas decimal.ToString() preserves the value's scale ("3.10"); the leading
			// "0.0" guarantees at least one integer and one fractional digit (0m -> "0.0", 3m -> "3.0"). EnsureDecimal
			// is then a no-op here (the point is always present) but is kept for symmetry with the double/float paths.
			var s = value.ToString("0.0###########################", CultureInfo.InvariantCulture);
			writer.WriteRawValue(EnsureDecimal(s), skipInputValidation: true);
		}

		// Appends ".0" when the shortest representation has no fractional part and no exponent, matching the legacy
		// EMIT_TRAILING_DECIMAL_POINT/ZERO behaviour. Values already carrying '.' or an exponent are left as-is.
		private static string EnsureDecimal(string s)
		{
			for (var i = 0; i < s.Length; i++)
			{
				var c = s[i];
				if (c == '.' || c == 'e' || c == 'E')
					return s;
			}
			return s + ".0";
		}
	}

	public class DoubleConverter : JsonConverter<double>
	{
		public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.GetDouble();

		public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options) =>
			RealNumberFormat.WriteDouble(writer, value);
	}

	public class NullableDoubleConverter : JsonConverter<double?>
	{
		public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Null ? (double?)null : reader.GetDouble();

		public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
		{
			if (value.HasValue)
				RealNumberFormat.WriteDouble(writer, value.Value);
			else
				writer.WriteNullValue();
		}
	}

	public class DecimalConverter : JsonConverter<decimal>
	{
		public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.GetDecimal();

		public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
			RealNumberFormat.WriteDecimal(writer, value);
	}

	public class NullableDecimalConverter : JsonConverter<decimal?>
	{
		public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Null ? (decimal?)null : reader.GetDecimal();

		public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
		{
			if (value.HasValue)
				RealNumberFormat.WriteDecimal(writer, value.Value);
			else
				writer.WriteNullValue();
		}
	}

	public class SingleConverter : JsonConverter<float>
	{
		public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.GetSingle();

		public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options) =>
			RealNumberFormat.WriteSingle(writer, value);
	}

	public class NullableSingleConverter : JsonConverter<float?>
	{
		public override float? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Null ? (float?)null : reader.GetSingle();

		public override void Write(Utf8JsonWriter writer, float? value, JsonSerializerOptions options)
		{
			if (value.HasValue)
				RealNumberFormat.WriteSingle(writer, value.Value);
			else
				writer.WriteNullValue();
		}
	}
}
