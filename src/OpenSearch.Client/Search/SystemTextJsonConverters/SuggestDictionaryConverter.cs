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

namespace OpenSearch.Client.Search.SystemTextJsonConverters;

/// <summary>
/// System.Text.Json converter factory for <see cref="ISuggestDictionary{T}"/>.
/// Response format: {"suggest_name": [{"text": "...", "options": [...]}], ...}
/// Dictionary where keys are suggest names, values are suggest response arrays.
/// </summary>
internal sealed class SuggestDictionaryConverterFactory : JsonConverterFactory
{
	public override bool CanConvert(Type typeToConvert)
	{
		if (!typeToConvert.IsGenericType)
			return false;

		var genericDef = typeToConvert.GetGenericTypeDefinition();
		return genericDef == typeof(ISuggestDictionary<>) || genericDef == typeof(SuggestDictionary<>);
	}

	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Type elementType;

		if (typeToConvert.IsGenericType)
		{
			elementType = typeToConvert.GetGenericArguments()[0];
		}
		else
		{
			throw new JsonException($"Cannot create converter for {typeToConvert}");
		}

		var converterType = typeof(SuggestDictionaryConverter<>).MakeGenericType(elementType);
		return (JsonConverter)Activator.CreateInstance(converterType)!;
	}
}

/// <summary>
/// System.Text.Json converter for <see cref="ISuggestDictionary{T}"/>.
/// Deserializes a JSON object where each property is a suggest name mapped to an array of suggest responses.
/// </summary>
internal sealed class SuggestDictionaryConverter<T> : JsonConverter<ISuggestDictionary<T>>
	where T : class
{
	public override ISuggestDictionary<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType != JsonTokenType.StartObject)
			throw new JsonException($"Expected StartObject for ISuggestDictionary<{typeof(T).Name}> but got {reader.TokenType}");

		var dictionary = new Dictionary<string, ISuggest<T>[]>();

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.EndObject)
				break;

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new JsonException("Expected PropertyName in suggest dictionary");

			var key = reader.GetString()!;
			reader.Read();

			// Sanitize typed_keys prefix (e.g., "term#my-suggest" -> "my-suggest")
			var hashIndex = key.IndexOf('#');
			var sanitizedKey = hashIndex > -1 ? key.Substring(hashIndex + 1) : key;

			var suggestArray = JsonSerializer.Deserialize<ISuggest<T>[]>(ref reader, options);
			if (suggestArray != null)
				dictionary[sanitizedKey] = suggestArray;
		}

		return new SuggestDictionary<T>(dictionary);
	}

	public override void Write(Utf8JsonWriter writer, ISuggestDictionary<T> value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		writer.WriteStartObject();

		foreach (var key in value.Keys)
		{
			writer.WritePropertyName(key);
			if (value.ContainsKey(key))
				JsonSerializer.Serialize(writer, value[key], options);
			else
				writer.WriteNullValue();
		}

		writer.WriteEndObject();
	}
}
