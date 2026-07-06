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
	/// System.Text.Json replacement for the legacy Utf8Json <c>UnionListFormatter&lt;TCollection, TFirst, TSecond&gt;</c>.
	/// Serializes/deserializes a JSON array whose elements are each a union of
	/// <typeparamref name="TFirst"/> and <typeparamref name="TSecond"/>. Per-element try-read is delegated to
	/// <see cref="UnionConverter{TFirst, TSecond}"/>, which buffers each element into a <see cref="JsonDocument"/> and
	/// attempts each type in turn (the <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound).
	/// </summary>
	internal class UnionListConverter<TCollection, TFirst, TSecond> : JsonConverter<TCollection>
		where TCollection : List<Union<TFirst, TSecond>>, new()
	{
		private static readonly UnionConverter<TFirst, TSecond> ItemConverter = new();

		public override TCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartArray)
				throw new JsonException($"Expected {JsonTokenType.StartArray} but got {reader.TokenType}.");

			var list = new TCollection();
			reader.Read(); // advance past StartArray to first element (or EndArray)
			while (reader.TokenType != JsonTokenType.EndArray)
			{
				list.Add(ItemConverter.Read(ref reader, typeof(Union<TFirst, TSecond>), options));
				reader.Read();
			}

			return list;
		}

		public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var item in value)
				ItemConverter.Write(writer, item, options);
			writer.WriteEndArray();
		}
	}
}
