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

internal sealed class DateMathConverter : JsonConverter<DateMath>
{
	private readonly IConnectionSettingsValues _settings;

	public DateMathConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override DateMath? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.String)
			return null;

		var value = reader.GetString();
		return value == null ? null : DateMath.FromString(value);
	}

	public override void Write(Utf8JsonWriter writer, DateMath value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.ToString());
	}
}

internal sealed class DateMathExpressionConverter : JsonConverter<DateMathExpression>
{
	private readonly IConnectionSettingsValues _settings;

	public DateMathExpressionConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override DateMathExpression? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.String)
			return null;

		var value = reader.GetString();
		if (value == null)
			return null;

		if (DateTime.TryParse(value, out var dateTime) && !DateMath.IsValidDateMathString(value))
			return new DateMathExpression(dateTime);

		return new DateMathExpression(value);
	}

	public override void Write(Utf8JsonWriter writer, DateMathExpression value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.ToString());
	}
}

internal sealed class DateMathTimeConverter : JsonConverter<DateMathTime>
{
	private readonly IConnectionSettingsValues _settings;

	public DateMathTimeConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override DateMathTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.String)
			return null;

		var value = reader.GetString();
		return value == null ? null : new DateMathTime(value);
	}

	public override void Write(Utf8JsonWriter writer, DateMathTime value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStringValue(value.ToString());
	}
}
