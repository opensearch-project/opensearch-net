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

namespace OpenSearch.Client.CommonOptions.SystemTextJsonConverters;

internal sealed class MinimumShouldMatchConverter : JsonConverter<MinimumShouldMatch>
{
	private readonly IConnectionSettingsValues _settings;

	public MinimumShouldMatchConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override MinimumShouldMatch? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			return value == null ? null : new MinimumShouldMatch(value);
		}

		if (reader.TokenType == JsonTokenType.Number)
		{
			var value = reader.GetInt32();
			return new MinimumShouldMatch(value);
		}

		throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading MinimumShouldMatch.");
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
				if (value.Item1.HasValue)
					writer.WriteNumberValue(value.Item1.Value);
				else
					writer.WriteNullValue();
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
