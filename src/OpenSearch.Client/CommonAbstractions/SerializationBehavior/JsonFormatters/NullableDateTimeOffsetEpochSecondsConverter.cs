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
	/// System.Text.Json replacement for the legacy Utf8Json <c>NullableDateTimeOffsetEpochSecondsFormatter</c>.
	/// On read, only a JSON number is treated as a value (seconds since the Unix epoch, 1970-01-01T00:00:00Z);
	/// any other token (including string or null) is skipped and yields null. On write, emits the epoch seconds
	/// as a bare JSON number.
	/// </summary>
	internal class NullableDateTimeOffsetEpochSecondsConverter : JsonConverter<DateTimeOffset?>
	{
		public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.Number)
			{
				reader.Skip();
				return null;
			}

			var secondsSinceEpoch = reader.GetDouble();
			return DateTimeUtil.UnixEpoch.AddSeconds(secondsSinceEpoch);
		}

		public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var dateTimeOffsetDifference = (value.Value - DateTimeUtil.UnixEpoch).TotalSeconds;
			writer.WriteNumberValue((long)dateTimeOffsetDifference);
		}
	}
}
