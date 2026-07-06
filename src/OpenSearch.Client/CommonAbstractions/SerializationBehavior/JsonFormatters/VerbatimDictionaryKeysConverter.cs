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
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>VerbatimDictionaryKeysFormatter</c>.
	/// Serializes an <see cref="IIsADictionary{TKey,TValue}"/> as a JSON object writing the keys "verbatim"
	/// and deserializes a JSON object back into the concrete <typeparamref name="TDictionary"/> implementation.
	///
	/// NOTE: The legacy formatter resolved keys of type <c>Field</c>, <c>PropertyName</c>, <c>IndexName</c> and
	/// <c>RelationName</c> through the connection-settings <c>Inferrer</c>. In the System.Text.Json engine that
	/// runtime configuration is not threaded through <see cref="JsonSerializerOptions"/> to converters, so this
	/// converter reproduces the settings-independent behaviour of the base formatter: <c>string</c> keys are
	/// written as-is and every other key type falls back to <see cref="Convert.ToString(object,IFormatProvider)"/>
	/// with <see cref="CultureInfo.InvariantCulture"/> (identical to the base formatter's <c>settings == null</c>
	/// branch). Wiring the inferrer through options is deferred to the full migration.
	/// </summary>
	internal class VerbatimDictionaryKeysConverter<TDictionary, TInterface, TKey, TValue> : JsonConverter<TInterface>
		where TDictionary : TInterface, IIsADictionary<TKey, TValue>
		where TInterface : IIsADictionary<TKey, TValue>
	{
		/// <summary>When <c>false</c>, entries with a <c>null</c> value are still written (matches the PreservingNull formatter).</summary>
		protected virtual bool SkipNullValues => true;

		public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return typeof(TDictionary).CreateInstance<TDictionary>(new Dictionary<TKey, TValue>());

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject but found {reader.TokenType} when deserializing {typeof(TDictionary).Name}.");

			var dictionary = new Dictionary<TKey, TValue>();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var keyString = reader.GetString();
				reader.Read();
				var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
				dictionary[ConvertKey(keyString)] = value;
			}

			return typeof(TDictionary).CreateInstance<TDictionary>(dictionary);
		}

		public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
		{
			var enumerable = (IEnumerable<KeyValuePair<TKey, TValue>>)value;
			if (enumerable == null)
			{
				writer.WriteNullValue();
				return;
			}

			// De-duplicate on the string key, preserving last-writer-wins, matching the base formatter.
			var seenEntries = new Dictionary<string, TValue>();
			foreach (var entry in enumerable)
			{
				if (SkipNullValues && entry.Value == null)
					continue;

				var key = KeyToString(entry.Key);
				if (key != null)
					seenEntries[key] = entry.Value;
			}

			writer.WriteStartObject();
			foreach (var entry in seenEntries)
			{
				writer.WritePropertyName(entry.Key);
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}

		private static string KeyToString(TKey key)
		{
			if (typeof(TKey) == typeof(string))
				return key?.ToString();

			return Convert.ToString(key, CultureInfo.InvariantCulture);
		}

		private static TKey ConvertKey(string key)
		{
			if (typeof(TKey) == typeof(string))
				return (TKey)(object)key;

			return (TKey)Convert.ChangeType(key, typeof(TKey), CultureInfo.InvariantCulture);
		}
	}

	/// <summary>
	/// A <see cref="VerbatimDictionaryKeysConverter{TDictionary,TInterface,TKey,TValue}"/> variant that also writes
	/// entries whose value is <c>null</c> (System.Text.Json replacement for the legacy
	/// <c>VerbatimDictionaryKeysPreservingNullFormatter</c>).
	/// </summary>
	internal class VerbatimDictionaryKeysPreservingNullConverter<TDictionary, TInterface, TKey, TValue>
		: VerbatimDictionaryKeysConverter<TDictionary, TInterface, TKey, TValue>
		where TDictionary : TInterface, IIsADictionary<TKey, TValue>
		where TInterface : IIsADictionary<TKey, TValue>
	{
		protected override bool SkipNullValues => false;
	}
}
