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
	/// System.Text.Json replacement for the legacy Utf8Json <c>DateMathTimeFormatter</c>. A
	/// <see cref="DateMathTime"/> is serialized as its string representation (e.g. <c>"5m"</c>), or
	/// as JSON <c>null</c> when the value is <c>null</c>. On read, a JSON string is parsed into a
	/// <see cref="DateMathTime"/> via the implicit <c>string</c> to <see cref="DateMathTime"/>
	/// conversion.
	/// </summary>
	internal class DateMathTimeConverter : JsonConverter<DateMathTime>
	{
		public override DateMathTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
				case JsonTokenType.Null:
					// Mirrors the legacy `return reader.ReadString();`. A null JSON token yields a
					// null string which the implicit string -> DateMathTime conversion passes to
					// the DateMathTime(string) constructor, throwing ArgumentNullException; a JSON
					// string is parsed into a DateMathTime the same way.
					return reader.GetString();
				default:
					throw new JsonException(
						$"Unexpected token '{reader.TokenType}' when reading {nameof(DateMathTime)}.");
			}
		}

		public override void Write(Utf8JsonWriter writer, DateMathTime value, JsonSerializerOptions options)
		{
			if (value is null) writer.WriteNullValue();
			else writer.WriteStringValue(value.ToString());
		}
	}
}
