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
	/// System.Text.Json replacement for the legacy Utf8Json <c>IntStringFormatter</c>. Deserializes
	/// an int into a string, and serializes a string into an int.
	/// </summary>
	internal class IntStringConverter : JsonConverter<string>
	{
		public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Number:
					return reader.GetInt32().ToString(CultureInfo.InvariantCulture);
				case JsonTokenType.String:
					return reader.GetString();
				default:
					throw new JsonException($"expected string or int but found {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
		{
			if (int.TryParse(value, out var i))
				writer.WriteNumberValue(i);
			else
				throw new InvalidOperationException($"expected a int string value, but found {value}");
		}
	}
}
