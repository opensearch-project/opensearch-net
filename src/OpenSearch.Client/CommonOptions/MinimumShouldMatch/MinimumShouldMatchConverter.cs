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
	/// A <see cref="System.Text.Json"/> converter for <see cref="MinimumShouldMatch"/> (a
	/// <c>Union&lt;int?, string&gt;</c>), replacing the vendored Utf8Json
	/// <c>MinimumShouldMatchFormatter</c> (#388): a fixed count is written as a number, a
	/// percentage/expression as a string.
	/// </summary>
	internal sealed class MinimumShouldMatchConverter : JsonConverter<MinimumShouldMatch>
	{
		public override MinimumShouldMatch Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					return new MinimumShouldMatch(reader.GetInt32());
				case JsonTokenType.String:
					return new MinimumShouldMatch(reader.GetString());
				default:
					throw new JsonException($"Cannot deserialize {nameof(MinimumShouldMatch)} from token {reader.TokenType}.");
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
