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
	/// System.Text.Json equivalent of the builtin TimeSpan handling the legacy engine applied to a member marked
	/// <see cref="StringTimeSpanAttribute"/>: the <see cref="TimeSpan"/> is written as its default string form
	/// (<c>[-][d.]hh:mm:ss[.fffffff]</c>) and parsed with <see cref="TimeSpan.Parse(string, IFormatProvider)"/> under
	/// the invariant culture. This is the member-level override of the ticks-number type-level default; it is bound
	/// via the [StringTimeSpan] mapping in <see cref="HighLevelContractResolver"/>.
	/// </summary>
	internal class StringTimeSpanConverter : JsonConverter<TimeSpan>
	{
		public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String: return TimeSpan.Parse(reader.GetString(), CultureInfo.InvariantCulture);
				case JsonTokenType.Number: return new TimeSpan(reader.GetInt64());
			}
			throw new JsonException($"Cannot convert token of type {reader.TokenType} to {nameof(TimeSpan)}.");
		}

		public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
			writer.WriteStringValue(value.ToString());
	}

	internal class NullableStringTimeSpanConverter : JsonConverter<TimeSpan?>
	{
		public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: return null;
				case JsonTokenType.String: return TimeSpan.Parse(reader.GetString(), CultureInfo.InvariantCulture);
				case JsonTokenType.Number: return new TimeSpan(reader.GetInt64());
			}
			throw new JsonException($"Cannot convert token of type {reader.TokenType} to {nameof(TimeSpan)}?.");
		}

		public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
		{
			if (value == null)
				writer.WriteNullValue();
			else
				writer.WriteStringValue(value.Value.ToString());
		}
	}
}
