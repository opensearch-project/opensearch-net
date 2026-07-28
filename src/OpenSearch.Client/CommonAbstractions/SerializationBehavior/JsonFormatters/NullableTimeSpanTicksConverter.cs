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
	/// System.Text.Json replacement for the legacy Utf8Json <c>NullableTimeSpanTicksFormatter</c>. A
	/// nullable <see cref="TimeSpan"/> is serialized as its <see cref="TimeSpan.Ticks"/> (a JSON number)
	/// or JSON null, and may be deserialized from a JSON number of ticks, a JSON string parseable by
	/// <see cref="TimeSpan.Parse(string)"/>, or JSON null.
	/// </summary>
	internal class NullableTimeSpanTicksConverter : JsonConverter<TimeSpan?>
	{
		public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var token = reader.TokenType;
			switch (token)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String: return TimeSpan.Parse(reader.GetString());
				case JsonTokenType.Number: return new TimeSpan(reader.GetInt64());
			}
			throw new JsonException($"Cannot convert token of type {token} to {nameof(TimeSpan)}?.");
		}

		public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
		{
			if (value == null)
				writer.WriteNullValue();
			else
				writer.WriteNumberValue(value.Value.Ticks);
		}
	}
}
