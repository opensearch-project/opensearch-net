/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// <see cref="System.Text.Json"/> converters for <see cref="DateTime"/>, <see cref="DateTimeOffset"/>
	/// and <see cref="TimeSpan"/> that reproduce, byte-for-byte, the output of the vendored Utf8Json
	/// formatters (<c>ISO8601DateTimeFormatter</c>, <c>ISO8601DateTimeOffsetFormatter</c>,
	/// <c>ISO8601TimeSpanFormatter</c>) and the <c>TimeSpanTicksFormatter</c> family, as part of the
	/// Utf8Json → System.Text.Json migration (#388). The read paths are faithful ports of the
	/// vendored lenient parsers.
	/// </summary>
	internal static class Iso8601
	{
		private static bool IsDigit(char c) => c >= '0' && c <= '9';

		// ---- DateTime write --------------------------------------------------------------------

		internal static string WriteDateTime(DateTime value)
		{
			var sb = new StringBuilder(33);
			AppendDateAndTime(sb, value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second,
				value.Ticks % TimeSpan.TicksPerSecond);

			switch (value.Kind)
			{
				case DateTimeKind.Local:
					// should not use `BaseUtcOffset`
					var localOffset = TimeZoneInfo.Local.GetUtcOffset(value);
					AppendOffset(sb, localOffset);
					break;
				case DateTimeKind.Utc:
					sb.Append('Z');
					break;
				case DateTimeKind.Unspecified:
				default:
					break;
			}

			return sb.ToString();
		}

		internal static string WriteDateTimeOffset(DateTimeOffset value)
		{
			var sb = new StringBuilder(33);
			AppendDateAndTime(sb, value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second,
				value.Ticks % TimeSpan.TicksPerSecond);
			AppendOffset(sb, value.Offset);
			return sb.ToString();
		}

		private static void AppendDateAndTime(StringBuilder sb, int year, int month, int day, int hour, int minute,
			int second, long nanosec)
		{
			sb.Append(year.ToString("D4", CultureInfo.InvariantCulture));
			sb.Append('-');
			sb.Append(month.ToString("D2", CultureInfo.InvariantCulture));
			sb.Append('-');
			sb.Append(day.ToString("D2", CultureInfo.InvariantCulture));
			sb.Append('T');
			sb.Append(hour.ToString("D2", CultureInfo.InvariantCulture));
			sb.Append(':');
			sb.Append(minute.ToString("D2", CultureInfo.InvariantCulture));
			sb.Append(':');
			sb.Append(second.ToString("D2", CultureInfo.InvariantCulture));

			if (nanosec != 0)
			{
				sb.Append('.');
				sb.Append(nanosec.ToString("D7", CultureInfo.InvariantCulture));
			}
		}

		private static void AppendOffset(StringBuilder sb, TimeSpan localOffset)
		{
			var minus = localOffset < TimeSpan.Zero;
			if (minus) localOffset = localOffset.Negate();
			sb.Append(minus ? '-' : '+');
			sb.Append(localOffset.Hours.ToString("D2", CultureInfo.InvariantCulture));
			sb.Append(':');
			sb.Append(localOffset.Minutes.ToString("D2", CultureInfo.InvariantCulture));
		}

		// ---- DateTime read (faithful port of ISO8601DateTimeFormatter.Deserialize) -------------

		internal static DateTime ReadDateTime(string str)
		{
			var i = 0;
			var len = str.Length;
			var to = len;

			char At(int idx) => idx < len ? str[idx] : '\0';

			// YYYY
			if (len == 4)
			{
				var y = (str[i++] - '0') * 1000 + (str[i++] - '0') * 100 + (str[i++] - '0') * 10 + (str[i++] - '0');
				return new DateTime(y, 1, 1);
			}

			// YYYY-MM
			if (len == 7)
			{
				var y = (str[i++] - '0') * 1000 + (str[i++] - '0') * 100 + (str[i++] - '0') * 10 + (str[i++] - '0');
				if (str[i++] != '-') goto ERROR;
				var m = (str[i++] - '0') * 10 + (str[i++] - '0');
				return new DateTime(y, m, 1);
			}

			// YYYY-MM-DD
			if (len == 10)
			{
				var y = (str[i++] - '0') * 1000 + (str[i++] - '0') * 100 + (str[i++] - '0') * 10 + (str[i++] - '0');
				if (str[i++] != '-') goto ERROR;
				var m = (str[i++] - '0') * 10 + (str[i++] - '0');
				if (str[i++] != '-') goto ERROR;
				var d = (str[i++] - '0') * 10 + (str[i++] - '0');
				return new DateTime(y, m, d);
			}

			// range-first section requires 19
			if (len < 19) goto ERROR;

			var year = (str[i++] - '0') * 1000 + (str[i++] - '0') * 100 + (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != '-') goto ERROR;
			var month = (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != '-') goto ERROR;
			var day = (str[i++] - '0') * 10 + (str[i++] - '0');

			if (str[i++] != 'T') goto ERROR;

			var hour = (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != ':') goto ERROR;
			var minute = (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != ':') goto ERROR;
			var second = (str[i++] - '0') * 10 + (str[i++] - '0');

			var ticks = 0;
			if (i < to && str[i] == '.')
			{
				i++;

				// *7.
				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 1000000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 100000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 10000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 1000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 100;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 10;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 1;
				i++;

				// others, lack of precision
				while (i < to && IsDigit(str[i]))
					i++;
			}

			END_TICKS:
			var kind = DateTimeKind.Unspecified;
			if (i < to && At(i) == 'Z')
				kind = DateTimeKind.Utc;
			else if (i < to && At(i) == '-' || At(i) == '+')
			{
				var offLen = to - i;
				if (offLen != 3 && offLen != 5 && offLen != 6) goto ERROR;
				var minus = str[i++] == '-';
				var h = (str[i++] - '0') * 10 + (str[i++] - '0');
				var m = 0;
				if (i < to)
				{
					if (offLen == 6)
					{
						if (str[i] != ':') goto ERROR;
						i++;
					}

					m = (str[i++] - '0') * 10 + (str[i++] - '0');
				}

				var offset = new TimeSpan(h, m, 0);
				if (minus) offset = offset.Negate();

				return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).AddTicks(ticks).Subtract(offset).ToLocalTime();
			}

			return new DateTime(year, month, day, hour, minute, second, kind).AddTicks(ticks);

			ERROR:
			throw new InvalidOperationException("invalid datetime format. value:" + str);
		}

		// ---- DateTimeOffset read (faithful port of ISO8601DateTimeOffsetFormatter.Deserialize) --

		internal static DateTimeOffset ReadDateTimeOffset(string str)
		{
			var i = 0;
			var len = str.Length;
			var to = len;

			char At(int idx) => idx < len ? str[idx] : '\0';

			// YYYY
			if (len == 4)
			{
				var y = (str[i++] - '0') * 1000 + (str[i++] - '0') * 100 + (str[i++] - '0') * 10 + (str[i++] - '0');
				return new DateTimeOffset(y, 1, 1, 0, 0, 0, TimeSpan.Zero);
			}

			// YYYY-MM
			if (len == 7)
			{
				var y = (str[i++] - '0') * 1000 + (str[i++] - '0') * 100 + (str[i++] - '0') * 10 + (str[i++] - '0');
				if (str[i++] != '-') goto ERROR;
				var m = (str[i++] - '0') * 10 + (str[i++] - '0');
				return new DateTimeOffset(y, m, 1, 0, 0, 0, TimeSpan.Zero);
			}

			// YYYY-MM-DD
			if (len == 10)
			{
				var y = (str[i++] - '0') * 1000 + (str[i++] - '0') * 100 + (str[i++] - '0') * 10 + (str[i++] - '0');
				if (str[i++] != '-') goto ERROR;
				var m = (str[i++] - '0') * 10 + (str[i++] - '0');
				if (str[i++] != '-') goto ERROR;
				var d = (str[i++] - '0') * 10 + (str[i++] - '0');
				return new DateTimeOffset(y, m, d, 0, 0, 0, TimeSpan.Zero);
			}

			// range-first section requires 19
			if (len < 19) goto ERROR;

			var year = (str[i++] - '0') * 1000 + (str[i++] - '0') * 100 + (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != '-') goto ERROR;
			var month = (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != '-') goto ERROR;
			var day = (str[i++] - '0') * 10 + (str[i++] - '0');

			if (str[i++] != 'T') goto ERROR;

			var hour = (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != ':') goto ERROR;
			var minute = (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != ':') goto ERROR;
			var second = (str[i++] - '0') * 10 + (str[i++] - '0');

			var ticks = 0;
			if (i < to && str[i] == '.')
			{
				i++;

				// *7.
				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 1000000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 100000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 10000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 1000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 100;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 10;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 1;
				i++;

				// others, lack of precision
				while (i < to && IsDigit(str[i]))
					i++;
			}

			END_TICKS:

			if (i < to && At(i) == '-' || At(i) == '+')
			{
				var offLen = to - i;
				if (offLen != 3 && offLen != 5 && offLen != 6) goto ERROR;
				var minus = str[i++] == '-';
				var h = (str[i++] - '0') * 10 + (str[i++] - '0');
				var m = 0;
				if (i < to)
				{
					if (offLen == 6)
					{
						if (str[i] != ':') goto ERROR;
						i++;
					}

					m = (str[i++] - '0') * 10 + (str[i++] - '0');
				}

				var offset = new TimeSpan(h, m, 0);
				if (minus) offset = offset.Negate();

				return new DateTimeOffset(year, month, day, hour, minute, second, offset).AddTicks(ticks);
			}

			return new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero).AddTicks(ticks);

			ERROR:
			throw new InvalidOperationException("invalid datetime format. value:" + str);
		}

		// ---- TimeSpan (string form) write (port of ISO8601TimeSpanFormatter.Serialize) ---------

		internal static string WriteStringTimeSpan(TimeSpan value)
		{
			// can not negate, use cache
			if (value == TimeSpan.MinValue)
				return TimeSpan.MinValue.ToString();

			var sb = new StringBuilder(26);

			var minus = value < TimeSpan.Zero;
			if (minus) value = value.Negate();
			var day = value.Days;
			var hour = value.Hours;
			var minute = value.Minutes;
			var second = value.Seconds;
			var nanosecond = value.Ticks % TimeSpan.TicksPerSecond;

			if (minus)
				sb.Append('-');

			if (day != 0)
			{
				sb.Append(day.ToString(CultureInfo.InvariantCulture));
				sb.Append('.');
			}

			sb.Append(hour.ToString("D2", CultureInfo.InvariantCulture));
			sb.Append(':');
			sb.Append(minute.ToString("D2", CultureInfo.InvariantCulture));
			sb.Append(':');
			sb.Append(second.ToString("D2", CultureInfo.InvariantCulture));

			if (nanosecond != 0)
			{
				sb.Append('.');
				sb.Append(nanosecond.ToString("D7", CultureInfo.InvariantCulture));
			}

			return sb.ToString();
		}

		// ---- TimeSpan (string form) read (port of ISO8601TimeSpanFormatter.Deserialize) --------

		internal static TimeSpan ReadStringTimeSpan(string str)
		{
			var i = 0;
			var len = str.Length;
			var to = len;

			// check day exists
			var hasDay = false;
			{
				var foundDot = false;
				var foundColon = false;
				for (var j = i; j < to; j++)
				{
					if (str[j] == '.')
					{
						if (foundColon)
							break;

						foundDot = true;
					}
					else if (str[j] == ':')
					{
						if (foundDot)
							hasDay = true;

						foundColon = true;
					}
				}
			}

			// check sign
			var minus = false;
			if (str[i] == '-')
			{
				minus = true;
				i++;
			}

			var day = 0;
			if (hasDay)
			{
				var start = i;
				for (; str[i] != '.'; i++) { }

				day = int.Parse(str.Substring(start, i - start), CultureInfo.InvariantCulture);
				i++; // skip '.'
			}

			var hour = (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != ':') goto ERROR;
			var minute = (str[i++] - '0') * 10 + (str[i++] - '0');
			if (str[i++] != ':') goto ERROR;
			var second = (str[i++] - '0') * 10 + (str[i++] - '0');

			var ticks = 0;
			if (i < to && str[i] == '.')
			{
				i++;

				// *7.
				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 1000000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 100000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 10000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 1000;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 100;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 10;
				i++;

				if (!(i < to) || !IsDigit(str[i])) goto END_TICKS;
				ticks += (str[i] - '0') * 1;
				i++;

				// others, lack of precision
				while (i < to && IsDigit(str[i]))
					i++;
			}

			END_TICKS:

			// be careful to overflow
			var ts = new TimeSpan(day, hour, minute, second);
			var tk = TimeSpan.FromTicks(ticks);
			return minus
				? ts.Negate().Subtract(tk)
				: ts.Add(tk);

			ERROR:
			throw new InvalidOperationException("invalid TimeSpan format. value:" + str);
		}
	}

	internal sealed class DateTimeConverter : JsonConverter<DateTime>
	{
		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
			writer.WriteStringValue(Iso8601.WriteDateTime(value));

		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			Iso8601.ReadDateTime(reader.GetString());
	}

	internal sealed class NullableDateTimeConverter : JsonConverter<DateTime?>
	{
		public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
		{
			if (value == null) writer.WriteNullValue();
			else writer.WriteStringValue(Iso8601.WriteDateTime(value.Value));
		}

		public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			return Iso8601.ReadDateTime(reader.GetString());
		}
	}

	internal sealed class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
	{
		public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
			writer.WriteStringValue(Iso8601.WriteDateTimeOffset(value));

		public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			// A numeric value is epoch milliseconds (e.g. nodes usage timestamp); otherwise an ISO string.
			reader.TokenType == JsonTokenType.Number
				? DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64())
				: Iso8601.ReadDateTimeOffset(reader.GetString());
	}

	internal sealed class NullableDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
	{
		public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
		{
			if (value == null) writer.WriteNullValue();
			else writer.WriteStringValue(Iso8601.WriteDateTimeOffset(value.Value));
		}

		public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			return reader.TokenType == JsonTokenType.Number
				? DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64())
				: Iso8601.ReadDateTimeOffset(reader.GetString());
		}
	}

	/// <summary>
	/// The default TimeSpan wire form: the <see cref="TimeSpan.Ticks"/> count as a JSON number,
	/// matching the vendored <c>TimeSpanTicksFormatter</c>.
	/// </summary>
	internal sealed class TimeSpanTicksConverter : JsonConverter<TimeSpan>
	{
		public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
			writer.WriteNumberValue(value.Ticks);

		public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Number:
					return TimeSpan.FromTicks(reader.GetInt64());
				case JsonTokenType.String:
					var s = reader.GetString();
					return long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var t)
						? TimeSpan.FromTicks(t)
						: TimeSpan.Parse(s, CultureInfo.InvariantCulture);
				default:
					throw new JsonException($"Cannot convert token of type {reader.TokenType} to {nameof(TimeSpan)}.");
			}
		}
	}

	internal sealed class NullableTimeSpanTicksConverter : JsonConverter<TimeSpan?>
	{
		public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
		{
			if (value == null) writer.WriteNullValue();
			else writer.WriteNumberValue(value.Value.Ticks);
		}

		public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					return TimeSpan.FromTicks(reader.GetInt64());
				case JsonTokenType.String:
					var s = reader.GetString();
					return long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var t)
						? TimeSpan.FromTicks(t)
						: TimeSpan.Parse(s, CultureInfo.InvariantCulture);
				default:
					throw new JsonException($"Cannot convert token of type {reader.TokenType} to {nameof(TimeSpan)}?.");
			}
		}
	}

	/// <summary>
	/// The ISO8601/string TimeSpan form used for members marked <c>[StringTimeSpan]</c>, matching the
	/// vendored <c>ISO8601TimeSpanFormatter</c>. Applied per-member.
	/// </summary>
	internal sealed class StringTimeSpanConverter : JsonConverter<TimeSpan>
	{
		public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
			writer.WriteStringValue(Iso8601.WriteStringTimeSpan(value));

		public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			Iso8601.ReadStringTimeSpan(reader.GetString());
	}

	internal sealed class NullableStringTimeSpanConverter : JsonConverter<TimeSpan?>
	{
		public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
		{
			if (value == null) writer.WriteNullValue();
			else writer.WriteStringValue(Iso8601.WriteStringTimeSpan(value.Value));
		}

		public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			return Iso8601.ReadStringTimeSpan(reader.GetString());
		}
	}
}
