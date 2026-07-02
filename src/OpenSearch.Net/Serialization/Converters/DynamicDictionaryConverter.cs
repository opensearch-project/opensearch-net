/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// A <see cref="JsonConverter{T}"/> for <see cref="DynamicDictionary"/> that serializes
	/// key-value pairs as a JSON object and deserializes JSON objects or arrays into
	/// a <see cref="DynamicDictionary"/>.
	/// </summary>
	public class DynamicDictionaryConverter : JsonConverter<DynamicDictionary>
	{
		/// <inheritdoc />
		public override DynamicDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType == JsonTokenType.StartObject)
				return ReadObject(ref reader, options);

			if (reader.TokenType == JsonTokenType.StartArray)
				return ReadArray(ref reader, options);

			throw new JsonException($"Unexpected token type {reader.TokenType} when deserializing DynamicDictionary.");
		}

		private static DynamicDictionary ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					return DynamicDictionary.Create(dict);

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var key = reader.GetString();
				reader.Read();
				dict[key] = ReadValue(ref reader, options);
			}

			throw new JsonException("Unexpected end of JSON when deserializing DynamicDictionary object.");
		}

		private static DynamicDictionary ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			var index = 0;

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray)
					return DynamicDictionary.Create(dict);

				dict[index.ToString()] = ReadValue(ref reader, options);
				index++;
			}

			throw new JsonException("Unexpected end of JSON when deserializing DynamicDictionary array.");
		}

		private static object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.True:
					return true;
				case JsonTokenType.False:
					return false;
				case JsonTokenType.Number:
					if (reader.TryGetInt64(out var longValue))
						return longValue;
					return reader.GetDouble();
				case JsonTokenType.String:
					return reader.GetString();
				case JsonTokenType.StartObject:
					return ReadNestedObject(ref reader, options);
				case JsonTokenType.StartArray:
					return ReadNestedArray(ref reader, options);
				default:
					throw new JsonException($"Unexpected token type {reader.TokenType} when reading value.");
			}
		}

		private static Dictionary<string, object> ReadNestedObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					return dict;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var key = reader.GetString();
				reader.Read();
				dict[key] = ReadValue(ref reader, options);
			}

			throw new JsonException("Unexpected end of JSON when reading nested object.");
		}

		private static List<object> ReadNestedArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			var list = new List<object>();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndArray)
					return list;

				list.Add(ReadValue(ref reader, options));
			}

			throw new JsonException("Unexpected end of JSON when reading nested array.");
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, DynamicDictionary value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			foreach (var kvp in (IDictionary<string, DynamicValue>)value)
			{
				writer.WritePropertyName(kvp.Key);
				WriteValue(writer, kvp.Value?.Value, options);
			}
			writer.WriteEndObject();
		}

		private static void WriteValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value)
			{
				case string s:
					writer.WriteStringValue(s);
					break;
				case bool b:
					writer.WriteBooleanValue(b);
					break;
				case int i:
					writer.WriteNumberValue(i);
					break;
				case long l:
					writer.WriteNumberValue(l);
					break;
				case double d:
					writer.WriteNumberValue(d);
					break;
				case float f:
					writer.WriteNumberValue(f);
					break;
				case decimal dec:
					writer.WriteNumberValue(dec);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}
	}
}
