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

internal sealed class StringBooleanConverter : JsonConverter<bool?>
{
	private readonly IConnectionSettingsValues _settings;

	public StringBooleanConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.True)
			return true;

		if (reader.TokenType == JsonTokenType.False)
			return false;

		if (reader.TokenType == JsonTokenType.String)
		{
			var value = reader.GetString();
			if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
				return true;
			if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
				return false;

			return null;
		}

		if (reader.TokenType == JsonTokenType.Number)
		{
			var value = reader.GetInt32();
			return value != 0;
		}

		return null;
	}

	public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
	{
		if (!value.HasValue)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteBooleanValue(value.Value);
	}
}
