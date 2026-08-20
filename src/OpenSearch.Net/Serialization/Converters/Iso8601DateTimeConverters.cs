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
	/// System.Text.Json replacements for the legacy Utf8Json <c>ISO8601DateTimeFormatter</c> /
	/// <c>ISO8601DateTimeOffsetFormatter</c> (the default DateTime/DateTimeOffset handling of the old engine).
	///
	/// System.Text.Json's built-in <c>Utf8JsonReader.GetDateTime()</c> only accepts a strict RFC 3339 / ISO 8601
	/// extended profile: it rejects the ISO 8601 "basic format" numeric offsets OpenSearch emits and accepts
	/// (<c>+1000</c>, <c>+10</c>) and it caps fractional seconds at 7 digits. The legacy formatter parsed those
	/// forms, so relying on the built-in reader regresses real payloads (see GitHub issue #4876) with
	/// "The JSON value is not in a supported DateTime format." This converter reproduces the legacy parser exactly.
	///
	/// Behaviour mirrored from the legacy formatter:
	///  - Truncated forms: "YYYY", "YYYY-MM", "YYYY-MM-DD".
	///  - Time to seconds precision, then optional '.' fraction of 1..7 significant digits (ticks); digits beyond the
	///    seventh are read and discarded ("lack of precision").
	///  - Zone: 'Z' (UTC), or a sign followed by an offset of length 3 (+hh), 5 (+hhmm) or 6 (+hh:mm). Any other
	///    offset length is an error.
	///  - DateTime with an explicit offset is normalised to UTC then converted to local time (matching the legacy
	///    <c>...DateTimeKind.Utc).AddTicks(ticks).Subtract(offset).ToLocalTime()</c>); a bare value keeps
	///    <c>DateTimeKind.Unspecified</c> (or Utc for a trailing 'Z').
	///  - Invalid input throws <see cref="InvalidOperationException"/> with the same message prefix as the legacy
	///    formatter, so callers that catch it (e.g. issue #4876's tests) keep working.
	/// </summary>
	public class Iso8601DateTimeConverter : JsonConverter<DateTime>
	{
		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var str = reader.GetString();
			return Iso8601DateParser.ParseDateTime(str);
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
			writer.WriteStringValue(Iso8601DateFormatter.FormatDateTime(value));
	}

	public class NullableIso8601DateTimeConverter : JsonConverter<DateTime?>
	{
		public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;
			return Iso8601DateParser.ParseDateTime(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
		{
			if (value.HasValue)
				writer.WriteStringValue(Iso8601DateFormatter.FormatDateTime(value.Value));
			else
				writer.WriteNullValue();
		}
	}

	public class Iso8601DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
	{
		public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			Iso8601DateParser.ParseDateTimeOffset(reader.GetString());

		public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
			writer.WriteStringValue(Iso8601DateFormatter.FormatDateTimeOffset(value));
	}

	public class NullableIso8601DateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
	{
		public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;
			return Iso8601DateParser.ParseDateTimeOffset(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
		{
			if (value.HasValue)
				writer.WriteStringValue(Iso8601DateFormatter.FormatDateTimeOffset(value.Value));
			else
				writer.WriteNullValue();
		}
	}

	internal static class Iso8601DateParser
	{
		private static bool IsDigit(char c) => c >= '0' && c <= '9';

		private static InvalidOperationException Error(string str) =>
			new InvalidOperationException("invalid datetime format. value:" + str);

		public static DateTime ParseDateTime(string str)
		{
			if (str == null) throw Error(null);
			var len = str.Length;
			var i = 0;
			var to = len;

			// YYYY
			if (len == 4)
				return new DateTime(Read4(str, ref i), 1, 1);

			// YYYY-MM
			if (len == 7)
			{
				var y = Read4(str, ref i);
				Expect(str, ref i, '-');
				return new DateTime(y, Read2(str, ref i), 1);
			}

			// YYYY-MM-DD
			if (len == 10)
			{
				var y = Read4(str, ref i);
				Expect(str, ref i, '-');
				var m = Read2(str, ref i);
				Expect(str, ref i, '-');
				return new DateTime(y, m, Read2(str, ref i));
			}

			if (len < 19) throw Error(str);

			var year = Read4(str, ref i);
			Expect(str, ref i, '-');
			var month = Read2(str, ref i);
			Expect(str, ref i, '-');
			var day = Read2(str, ref i);
			Expect(str, ref i, 'T');
			var hour = Read2(str, ref i);
			Expect(str, ref i, ':');
			var minute = Read2(str, ref i);
			Expect(str, ref i, ':');
			var second = Read2(str, ref i);

			var ticks = ReadFraction(str, ref i, to);

			// zone
			if (i < to && str[i] == 'Z')
				return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(ticks);

			// NOTE: precedence mirrors the legacy formatter exactly (`i < to && str[i] == '-' || str[i] == '+'`),
			// which relies on `str[i]` being in range on the RHS; guard the index to avoid an out-of-range read
			// while preserving the same accepted inputs.
			if (i < to && (str[i] == '-' || str[i] == '+'))
			{
				var offset = ReadOffset(str, ref i, to);
				return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc)
					.AddTicks(ticks).Subtract(offset).ToLocalTime();
			}

			return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified).AddTicks(ticks);
		}

		public static DateTimeOffset ParseDateTimeOffset(string str)
		{
			if (str == null) throw Error(null);
			var len = str.Length;
			var i = 0;
			var to = len;

			if (len == 4)
				return new DateTimeOffset(Read4(str, ref i), 1, 1, 0, 0, 0, TimeSpan.Zero);

			if (len == 7)
			{
				var y = Read4(str, ref i);
				Expect(str, ref i, '-');
				return new DateTimeOffset(y, Read2(str, ref i), 1, 0, 0, 0, TimeSpan.Zero);
			}

			if (len == 10)
			{
				var y = Read4(str, ref i);
				Expect(str, ref i, '-');
				var m = Read2(str, ref i);
				Expect(str, ref i, '-');
				return new DateTimeOffset(y, m, Read2(str, ref i), 0, 0, 0, TimeSpan.Zero);
			}

			if (len < 19) throw Error(str);

			var year = Read4(str, ref i);
			Expect(str, ref i, '-');
			var month = Read2(str, ref i);
			Expect(str, ref i, '-');
			var day = Read2(str, ref i);
			Expect(str, ref i, 'T');
			var hour = Read2(str, ref i);
			Expect(str, ref i, ':');
			var minute = Read2(str, ref i);
			Expect(str, ref i, ':');
			var second = Read2(str, ref i);

			var ticks = ReadFraction(str, ref i, to);

			if (i < to && (str[i] == '-' || str[i] == '+'))
			{
				var offset = ReadOffset(str, ref i, to);
				return new DateTimeOffset(year, month, day, hour, minute, second, offset).AddTicks(ticks);
			}

			return new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero).AddTicks(ticks);
		}

		// Reads the optional fractional-seconds component and returns the corresponding ticks (1 tick = 100ns).
		// Consumes the '.' and the digits; the first seven digits are significant, any beyond are discarded.
		private static int ReadFraction(string str, ref int i, int to)
		{
			var ticks = 0;
			if (!(i < to) || str[i] != '.')
				return ticks;

			i++;
			var scale = 1000000; // first fractional digit contributes 10^6 ticks
			var significant = 0;
			while (i < to && IsDigit(str[i]))
			{
				if (significant < 7)
				{
					ticks += (str[i] - '0') * scale;
					scale /= 10;
					significant++;
				}
				// digits beyond the seventh are read and discarded (lack of precision)
				i++;
			}
			return ticks;
		}

		// Reads a sign + offset of length 3 (+hh), 5 (+hhmm) or 6 (+hh:mm). Any other length is an error.
		private static TimeSpan ReadOffset(string str, ref int i, int to)
		{
			var offLen = to - i;
			if (offLen != 3 && offLen != 5 && offLen != 6) throw Error(str);

			var minus = str[i++] == '-';
			var h = Read2(str, ref i);
			var m = 0;
			if (i < to)
			{
				if (offLen == 6)
					Expect(str, ref i, ':');
				m = Read2(str, ref i);
			}

			var offset = new TimeSpan(h, m, 0);
			return minus ? offset.Negate() : offset;
		}

		private static void Expect(string str, ref int i, char c)
		{
			if (i >= str.Length || str[i] != c) throw Error(str);
			i++;
		}

		private static int Read2(string str, ref int i)
		{
			if (i + 2 > str.Length || !IsDigit(str[i]) || !IsDigit(str[i + 1])) throw Error(str);
			var v = (str[i] - '0') * 10 + (str[i + 1] - '0');
			i += 2;
			return v;
		}

		private static int Read4(string str, ref int i)
		{
			if (i + 4 > str.Length) throw Error(str);
			for (var k = 0; k < 4; k++)
				if (!IsDigit(str[i + k])) throw Error(str);
			var v = (str[i] - '0') * 1000 + (str[i + 1] - '0') * 100 + (str[i + 2] - '0') * 10 + (str[i + 3] - '0');
			i += 4;
			return v;
		}
	}

	internal static class Iso8601DateFormatter
	{
		// Mirrors the legacy ISO8601DateTimeFormatter.Serialize byte-for-byte:
		//   yyyy-MM-ddTHH:mm:ss[.fffffff] and then, by Kind, "" (Unspecified) / "Z" (Utc) / "+HH:mm" (Local).
		// Fractional seconds are written only when non-zero, with no trailing-zero trimming (7-digit ticks field).
		public static string FormatDateTime(DateTime value)
		{
			var sb = new System.Text.StringBuilder(33);
			AppendDateAndTime(sb, value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second,
				value.Ticks % TimeSpan.TicksPerSecond);

			switch (value.Kind)
			{
				case DateTimeKind.Local:
					AppendOffset(sb, TimeZoneInfo.Local.GetUtcOffset(value));
					break;
				case DateTimeKind.Utc:
					sb.Append('Z');
					break;
			}
			return sb.ToString();
		}

		public static string FormatDateTimeOffset(DateTimeOffset value)
		{
			var sb = new System.Text.StringBuilder(33);
			AppendDateAndTime(sb, value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second,
				value.Ticks % TimeSpan.TicksPerSecond);
			AppendOffset(sb, value.Offset);
			return sb.ToString();
		}

		private static void AppendDateAndTime(System.Text.StringBuilder sb, int year, int month, int day,
			int hour, int minute, int second, long nanosec)
		{
			sb.Append(year.ToString("D4", CultureInfo.InvariantCulture)).Append('-')
				.Append(month.ToString("D2", CultureInfo.InvariantCulture)).Append('-')
				.Append(day.ToString("D2", CultureInfo.InvariantCulture)).Append('T')
				.Append(hour.ToString("D2", CultureInfo.InvariantCulture)).Append(':')
				.Append(minute.ToString("D2", CultureInfo.InvariantCulture)).Append(':')
				.Append(second.ToString("D2", CultureInfo.InvariantCulture));

			if (nanosec != 0)
				sb.Append('.').Append(nanosec.ToString("D7", CultureInfo.InvariantCulture));
		}

		private static void AppendOffset(System.Text.StringBuilder sb, TimeSpan offset)
		{
			var minus = offset < TimeSpan.Zero;
			if (minus) offset = offset.Negate();
			sb.Append(minus ? '-' : '+')
				.Append(offset.Hours.ToString("D2", CultureInfo.InvariantCulture)).Append(':')
				.Append(offset.Minutes.ToString("D2", CultureInfo.InvariantCulture));
		}
	}
}
