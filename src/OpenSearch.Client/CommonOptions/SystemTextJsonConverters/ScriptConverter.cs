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

namespace OpenSearch.Client.CommonOptions.SystemTextJsonConverters;

internal sealed class ScriptConverter : JsonConverter<IScript>
{
	private readonly IConnectionSettingsValues _settings;

	public ScriptConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override IScript? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Unexpected token type '{reader.TokenType}' when reading IScript.");

		IScript? script = null;
		string? language = null;
		Dictionary<string, object>? parameters = null;

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				break;

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected property name in script object.");

			var propertyName = reader.GetString()!;
			reader.Read();

			switch (propertyName)
			{
				case "inline":
				case "source":
					script = new InlineScript(reader.GetString()!);
					break;
				case "id":
					script = new IndexedScript(reader.GetString()!);
					break;
				case "lang":
					language = reader.GetString();
					break;
				case "params":
					parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
					break;
				default:
					reader.Skip();
					break;
			}
		}

		if (script == null)
			return null;

		script.Lang = language;
		script.Params = parameters;
		return script;
	}

	public override void Write(Utf8JsonWriter writer, IScript value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();

		switch (value)
		{
			case IInlineScript inlineScript:
				writer.WriteString("source", inlineScript.Source);
				break;
			case IIndexedScript indexedScript:
				writer.WriteString("id", indexedScript.Id);
				break;
		}

		if (!string.IsNullOrEmpty(value.Lang))
			writer.WriteString("lang", value.Lang);

		if (value.Params != null && value.Params.Count > 0)
		{
			writer.WritePropertyName("params");
			JsonSerializer.Serialize(writer, value.Params, options);
		}

		writer.WriteEndObject();
	}
}
