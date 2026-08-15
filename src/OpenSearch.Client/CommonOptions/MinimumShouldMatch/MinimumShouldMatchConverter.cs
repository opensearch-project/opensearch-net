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
	/// System.Text.Json replacement for the legacy Utf8Json <c>MinimumShouldMatchFormatter</c>. A
	/// <see cref="MinimumShouldMatch"/> is a union of <see cref="int"/> (JSON number) and
	/// <see cref="string"/> (JSON string, e.g. a percentage).
	/// </summary>
	internal class MinimumShouldMatchConverter : JsonConverter<MinimumShouldMatch>
	{
		public override MinimumShouldMatch Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return new MinimumShouldMatch(reader.GetString());
				case JsonTokenType.Number:
					return new MinimumShouldMatch(reader.GetInt32());
				default:
					throw new JsonException($"Expected {nameof(JsonTokenType.String)} or {nameof(JsonTokenType.Number)} but got {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, MinimumShouldMatch value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					writer.WriteNumberValue(value.Item1.Value);
					break;
				case 1:
					writer.WriteStringValue(value.Item2);
					break;
			}
		}
	}
}
