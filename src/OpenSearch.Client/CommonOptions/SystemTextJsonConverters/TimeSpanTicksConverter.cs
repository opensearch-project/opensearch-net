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

internal sealed class TimeSpanTicksConverter : JsonConverter<TimeSpan>
{
	private readonly IConnectionSettingsValues _settings;

	public TimeSpanTicksConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Number)
		{
			var ticks = reader.GetInt64();
			return TimeSpan.FromTicks(ticks);
		}

		throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading TimeSpan.");
	}

	public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
	{
		writer.WriteNumberValue(value.Ticks);
	}
}

internal sealed class NullableTimeSpanTicksConverter : JsonConverter<TimeSpan?>
{
	private readonly IConnectionSettingsValues _settings;

	public NullableTimeSpanTicksConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.Number)
		{
			var ticks = reader.GetInt64();
			return TimeSpan.FromTicks(ticks);
		}

		throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading TimeSpan?.");
	}

	public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
	{
		if (!value.HasValue)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteNumberValue(value.Value.Ticks);
	}
}
