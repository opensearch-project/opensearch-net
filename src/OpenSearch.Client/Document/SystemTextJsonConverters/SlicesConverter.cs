/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.Document.SystemTextJsonConverters;

/// <summary>
/// Converts <see cref="Slices"/> which is a union type that can be:
/// - An integer (number of slices for parallel processing)
/// - The string "auto" (let the server determine the number of slices)
/// </summary>
internal sealed class SlicesConverter : JsonConverter<Slices>
{
	private readonly IConnectionSettingsValues _settings;

	public SlicesConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override Slices? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		switch (reader.TokenType)
		{
			case JsonTokenType.Number:
				return new Slices(reader.GetInt64());

			case JsonTokenType.String:
				var value = reader.GetString();
				if (value == null)
					return null;
				// "auto" or other string values
				return new Slices(value);

			default:
				throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading Slices.");
		}
	}

	public override void Write(Utf8JsonWriter writer, Slices value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		switch (value.Tag)
		{
			case 0:
				writer.WriteNumberValue(value.Item1);
				break;
			case 1:
				writer.WriteStringValue(value.Item2);
				break;
			default:
				writer.WriteNullValue();
				break;
		}
	}
}
