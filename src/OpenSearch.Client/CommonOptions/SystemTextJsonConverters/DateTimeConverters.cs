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

internal sealed class EpochMillisecondsDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
	private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

	private readonly IConnectionSettingsValues _settings;

	public EpochMillisecondsDateTimeOffsetConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Number)
		{
			var milliseconds = reader.GetInt64();
			return Epoch.AddMilliseconds(milliseconds);
		}

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			if (long.TryParse(value, out var ms))
				return Epoch.AddMilliseconds(ms);

			return DateTimeOffset.Parse(value!);
		}

		throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading DateTimeOffset.");
	}

	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
	{
		var milliseconds = (long)(value - Epoch).TotalMilliseconds;
		writer.WriteNumberValue(milliseconds);
	}
}

internal sealed class NullableEpochMillisecondsDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
	private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

	private readonly IConnectionSettingsValues _settings;

	public NullableEpochMillisecondsDateTimeOffsetConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.Number)
		{
			var milliseconds = reader.GetInt64();
			return Epoch.AddMilliseconds(milliseconds);
		}

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			if (string.IsNullOrEmpty(value))
				return null;

			if (long.TryParse(value, out var ms))
				return Epoch.AddMilliseconds(ms);

			return DateTimeOffset.Parse(value);
		}

		throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading DateTimeOffset?.");
	}

	public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
	{
		if (!value.HasValue)
		{
			writer.WriteNullValue();
			return;
		}

		var milliseconds = (long)(value.Value - Epoch).TotalMilliseconds;
		writer.WriteNumberValue(milliseconds);
	}
}

internal sealed class EpochMillisecondsDateTimeConverter : JsonConverter<DateTime>
{
	private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	private readonly IConnectionSettingsValues _settings;

	public EpochMillisecondsDateTimeConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Number)
		{
			var milliseconds = reader.GetInt64();
			return Epoch.AddMilliseconds(milliseconds);
		}

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			if (long.TryParse(value, out var ms))
				return Epoch.AddMilliseconds(ms);

			return DateTime.Parse(value!);
		}

		throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading DateTime.");
	}

	public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
	{
		var utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
		var milliseconds = (long)(utcValue - Epoch).TotalMilliseconds;
		writer.WriteNumberValue(milliseconds);
	}
}

internal sealed class NullableEpochMillisecondsDateTimeConverter : JsonConverter<DateTime?>
{
	private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	private readonly IConnectionSettingsValues _settings;

	public NullableEpochMillisecondsDateTimeConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.Number)
		{
			var milliseconds = reader.GetInt64();
			return Epoch.AddMilliseconds(milliseconds);
		}

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			if (string.IsNullOrEmpty(value))
				return null;

			if (long.TryParse(value, out var ms))
				return Epoch.AddMilliseconds(ms);

			return DateTime.Parse(value);
		}

		throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading DateTime?.");
	}

	public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
	{
		if (!value.HasValue)
		{
			writer.WriteNullValue();
			return;
		}

		var utcValue = value.Value.Kind == DateTimeKind.Utc ? value.Value : value.Value.ToUniversalTime();
		var milliseconds = (long)(utcValue - Epoch).TotalMilliseconds;
		writer.WriteNumberValue(milliseconds);
	}
}
