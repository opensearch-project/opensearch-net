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
	/// System.Text.Json replacement for the legacy Utf8Json <c>NullableDateTimeEpochMillisecondsFormatter</c>.
	/// Reads a <see cref="DateTime"/> that OpenSearch may send either as an ISO-8601 string or as a JSON number
	/// of milliseconds since the Unix epoch (1970-01-01T00:00:00Z), and writes it back as epoch milliseconds.
	/// </summary>
	internal class NullableDateTimeEpochMillisecondsConverter : JsonConverter<DateTime?>
	{
		public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return reader.GetDateTime();
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					var millisecondsSinceEpoch = reader.GetDouble();
					var dateTimeOffset = DateTimeUtil.UnixEpoch.AddMilliseconds(millisecondsSinceEpoch);
					return dateTimeOffset.DateTime;
				default:
					throw new JsonException($"Cannot deserialize {nameof(DateTime)} from token {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var dateTimeDifference = (value.Value - DateTimeUtil.UnixEpoch).TotalMilliseconds;
			writer.WriteNumberValue((long)dateTimeDifference);
		}
	}
}
