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

namespace OpenSearch.Client.SystemTextJsonConverters;

internal sealed class TimeConverter : JsonConverter<Time>
{
	public override Time? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.String)
		{
			var timeString = reader.GetString();
			return timeString == null ? null : new Time(timeString);
		}

		if (reader.TokenType == JsonTokenType.Number)
		{
			var milliseconds = reader.GetDouble();
			return new Time(milliseconds);
		}

		throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading Time.");
	}

	public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		var timeString = value.ToString();
		if (!string.IsNullOrEmpty(timeString) && !IsNumericOnly(timeString))
		{
			writer.WriteStringValue(timeString);
		}
		else if (value.Milliseconds.HasValue)
		{
			writer.WriteNumberValue(value.Milliseconds.Value);
		}
		else
		{
			writer.WriteNullValue();
		}
	}

	private static bool IsNumericOnly(string value)
	{
		foreach (var c in value)
		{
			if (!char.IsDigit(c) && c != '.' && c != '-')
				return false;
		}
		return true;
	}
}
