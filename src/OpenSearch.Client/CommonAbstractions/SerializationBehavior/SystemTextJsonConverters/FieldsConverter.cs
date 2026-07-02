/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Collections.Generic;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.SystemTextJsonConverters;

internal sealed class FieldsConverter : JsonConverter<Fields>
{
	private readonly IConnectionSettingsValues _settings;

	public FieldsConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override Fields? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.String)
		{
			var singleField = reader.GetString();
			return singleField == null ? null : new Fields(new[] { new Field(singleField) });
		}

		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading Fields.");

		var fields = new List<Field>();
		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			var fieldName = reader.GetString();
			if (fieldName != null)
				fields.Add(new Field(fieldName));
		}

		return new Fields(fields);
	}

	public override void Write(Utf8JsonWriter writer, Fields value, JsonSerializerOptions options)
	{
		if (value?.ListOfFields == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartArray();
		foreach (var field in value.ListOfFields)
		{
			var fieldName = field.Name ?? _settings.Inferrer.Field(field);
			writer.WriteStringValue(fieldName);
		}
		writer.WriteEndArray();
	}
}
