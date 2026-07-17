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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>AggregationDictionaryFormatter</c> (the request-side
	/// container of named aggregations, i.e. the <c>aggs</c>/<c>aggregations</c> object).
	///
	/// <para>The legacy formatter delegated to <c>VerbatimDictionaryInterfaceKeysFormatter&lt;string,
	/// IAggregationContainer&gt;</c>: on read it built a <c>Dictionary&lt;string, IAggregationContainer&gt;</c> and
	/// wrapped it in an <see cref="AggregationDictionary"/>; on write it emitted each entry with its key "verbatim"
	/// (string keys are already the final wire name), skipping entries whose value is <c>null</c> and de-duplicating on
	/// the key (last-writer-wins).</para>
	///
	/// <para>Keys are plain <c>string</c>s, so — like the base verbatim converter's <c>settings == null</c> branch —
	/// no <c>Inferrer</c> is required and this is a plain <see cref="JsonConverter{T}"/>. Each
	/// <see cref="IAggregationContainer"/> value is delegated to <see cref="JsonSerializer"/>, relying on the
	/// <c>[ReadAs(typeof(AggregationContainer))]</c> contract configured on the supplied options.</para>
	///
	/// <para>Constructing the <see cref="AggregationDictionary"/> from the read entries runs its key validation
	/// (reserved aggregation names throw), exactly as the legacy formatter did.</para>
	/// </summary>
	internal class AggregationContainerConverter : JsonConverter<AggregationDictionary>
	{
		public override AggregationDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject but found {reader.TokenType} when deserializing {nameof(AggregationDictionary)}.");

			var dictionary = new Dictionary<string, IAggregationContainer>();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var key = reader.GetString();
				reader.Read();
				var value = JsonSerializer.Deserialize<IAggregationContainer>(ref reader, options);
				if (key != null)
					dictionary[key] = value;
			}

			return new AggregationDictionary(dictionary);
		}

		public override void Write(Utf8JsonWriter writer, AggregationDictionary value, JsonSerializerOptions options)
		{
			var enumerable = (IEnumerable<KeyValuePair<string, IAggregationContainer>>)value;
			if (enumerable == null)
			{
				writer.WriteNullValue();
				return;
			}

			// De-duplicate on the string key, preserving last-writer-wins and skipping null values (matches the base
			// verbatim formatter's default SkipValue behaviour).
			var seenEntries = new Dictionary<string, IAggregationContainer>();
			foreach (var entry in enumerable)
			{
				if (entry.Value == null)
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
