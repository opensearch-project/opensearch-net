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
