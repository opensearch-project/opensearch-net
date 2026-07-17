/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonFormatterAttribute = OpenSearch.Net.Utf8Json.JsonFormatterAttribute;

namespace OpenSearch.Client
{
	/// <summary>
	/// Constructs the closed <see cref="SuggestDictionaryConverter{T}"/> for each <c>ISuggestDictionary&lt;T&gt;</c>
	/// at runtime. This is a <see cref="JsonConverterFactory"/> because <see cref="SuggestDictionaryConverter{T}"/> is
	/// an open generic that cannot be added to <c>JsonSerializerOptions.Converters</c> directly.
	///
	/// The type is discovered from the legacy <c>[JsonFormatter(typeof(SuggestDictionaryFormatter&lt;&gt;))]</c>
	/// attribute that already annotates <see cref="ISuggestDictionary{T}"/>, mirroring the discovery pattern used by
	/// <see cref="FieldNameQueryConverterFactory"/>. Reusing that attribute keeps the STJ path in lock-step with the
	/// legacy mapping and requires no per-document-type registration.
	///
	/// Unlike <c>PerFieldAnalyzer</c>, this dictionary's keys are plain <c>string</c>s, so no <c>Inferrer</c> is
	/// required and the converter is settings-independent (a plain <see cref="JsonConverterFactory"/>).
	/// </summary>
	internal class SuggestDictionaryConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) => IsSuggestDictionary(typeToConvert);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (!IsSuggestDictionary(typeToConvert))
				return null;

			// ISuggestDictionary<T> -> T is the suggested document type.
			var documentType = typeToConvert.GetGenericArguments()[0];
			var converterType = typeof(SuggestDictionaryConverter<>).MakeGenericType(documentType);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}

		// Only handle the closed ISuggestDictionary<T> interface whose legacy [JsonFormatter] points at the open
		// SuggestDictionaryFormatter<>.
		private static bool IsSuggestDictionary(Type typeToConvert)
		{
			if (!typeToConvert.IsGenericType || typeToConvert.GetGenericTypeDefinition() != typeof(ISuggestDictionary<>))
				return false;

			var attr = typeToConvert.GetCustomAttribute<JsonFormatterAttribute>();
			var formatterType = attr?.FormatterType;
			if (formatterType == null || !formatterType.IsGenericTypeDefinition)
				return false;

			return formatterType.GetGenericTypeDefinition() == typeof(SuggestDictionaryFormatter<>);
		}
	}

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>SuggestDictionaryFormatter&lt;T&gt;</c>. Reads/writes a
	/// JSON object mapping a suggestion name to an array of <see cref="ISuggest{T}"/> results. On read the raw string
	/// keys are handed to the <see cref="SuggestDictionary{T}"/> constructor, which sanitizes the
	/// <c>typed_keys=true</c> form (<c>&lt;type&gt;#&lt;name&gt;</c>) down to <c>&lt;name&gt;</c>. On write, entries
	/// whose value array is <c>null</c> are skipped, matching the legacy verbatim dictionary formatter.
	/// </summary>
	internal class SuggestDictionaryConverter<T> : JsonConverter<ISuggestDictionary<T>>
		where T : class
	{
		// A JSON null yields an empty dictionary (matching the legacy formatter). STJ skips the converter for a null
		// reference type unless HandleNull is true.
		public override bool HandleNull => true;

		public override ISuggestDictionary<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return new SuggestDictionary<T>(new Dictionary<string, ISuggest<T>[]>());

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject but found {reader.TokenType} when deserializing {typeof(SuggestDictionary<T>).Name}.");

			var dictionary = new Dictionary<string, ISuggest<T>[]>();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var key = reader.GetString();
				reader.Read();
				// Deserialize the CONCRETE Suggest<T>[] directly. The interface's [ReadAs(typeof(Suggest<>))] names an
				// OPEN generic, which ReadAsConverterFactory cannot close (it would try Suggest<> with an unbound arg);
				// closing Suggest<T> here sidesteps that and produces the same concrete instances.
				var concrete = JsonSerializer.Deserialize<Suggest<T>[]>(ref reader, options);
				dictionary[key] = concrete == null ? null : System.Array.ConvertAll(concrete, s => (ISuggest<T>)s);
			}

			return new SuggestDictionary<T>(dictionary);
		}

		public override void Write(Utf8JsonWriter writer, ISuggestDictionary<T> value, JsonSerializerOptions options)
		{
			var enumerable = value as IEnumerable<KeyValuePair<string, ISuggest<T>[]>>;
			if (enumerable == null)
			{
				writer.WriteNullValue();
				return;
			}

			// De-duplicate on the string key, preserving last-writer-wins, matching the legacy verbatim formatter.
			var seenEntries = new Dictionary<string, ISuggest<T>[]>();
			foreach (var entry in enumerable)
			{
				if (entry.Value == null) // legacy VerbatimDictionaryKeysBaseFormatter skips null values
					continue;

				if (entry.Key != null)
					seenEntries[entry.Key] = entry.Value;
			}

			writer.WriteStartObject();
			foreach (var entry in seenEntries)
			{
				writer.WritePropertyName(entry.Key);
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}
