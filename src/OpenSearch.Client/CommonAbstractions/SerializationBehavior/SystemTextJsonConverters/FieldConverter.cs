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

internal sealed class FieldConverter : JsonConverter<Field>
{
	private readonly IConnectionSettingsValues _settings;

	public FieldConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override Field? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		var fieldName = reader.GetString();
		return fieldName == null ? null : new Field(fieldName);
	}

	public override void Write(Utf8JsonWriter writer, Field value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		var fieldName = value.Name ?? _settings.Inferrer.Field(value);
		writer.WriteStringValue(fieldName);
	}
}
