/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// A <see cref="JsonConverter{T}"/> for <see cref="Nullable{Int32}"/> that handles values
	/// which may arrive from OpenSearch as a JSON number, a string containing a number, or null.
	/// </summary>
	public class NullableStringIntConverter : JsonConverter<int?>
	{
		/// <inheritdoc />
		public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					return reader.GetInt32();
				case JsonTokenType.String:
					var s = reader.GetString();
					if (string.IsNullOrEmpty(s))
						return null;
					if (int.TryParse(s, out var result))
						return result;
					throw new JsonException($"Cannot parse int from string value: '{s}'.");
				default:
					throw new JsonException($"Unexpected token type {reader.TokenType} when reading nullable int.");
			}
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
		{
			if (value.HasValue)
				writer.WriteNumberValue(value.Value);
			else
				writer.WriteNullValue();
		}
	}
}
