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
	/// System.Text.Json replacement for the legacy Utf8Json <c>DateTimeOffsetEpochMillisecondsFormatter</c>.
	/// On read, accepts an ISO-8601 string, a JSON number of milliseconds since the Unix epoch, or null
	/// (null yields <c>default(DateTimeOffset)</c>, matching the legacy base formatter). On write, emits the
	/// epoch milliseconds as a quoted string, e.g. <c>"1609459200000"</c>.
	/// </summary>
	internal class DateTimeOffsetEpochMillisecondsConverter : JsonConverter<DateTimeOffset>
	{
		public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return reader.GetDateTimeOffset();
				case JsonTokenType.Null:
					return default;
				case JsonTokenType.Number:
					var millisecondsSinceEpoch = reader.GetDouble();
					return DateTimeUtil.UnixEpoch.AddMilliseconds(millisecondsSinceEpoch);
				default:
					throw new JsonException($"Cannot deserialize {nameof(DateTimeOffset)} from token {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
			writer.WriteStringValue(value.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>NullableDateTimeOffsetEpochMillisecondsFormatter</c>.
	/// Behaves like <see cref="DateTimeOffsetEpochMillisecondsConverter"/> but maps a JSON null to a null value
	/// and writes null as a JSON null.
	/// </summary>
	internal class NullableDateTimeOffsetEpochMillisecondsConverter : JsonConverter<DateTimeOffset?>
	{
		public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return reader.GetDateTimeOffset();
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					var millisecondsSinceEpoch = reader.GetDouble();
					return DateTimeUtil.UnixEpoch.AddMilliseconds(millisecondsSinceEpoch);
				default:
					throw new JsonException($"Cannot deserialize {nameof(DateTimeOffset)} from token {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(value.Value.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture));
		}
	}
}
