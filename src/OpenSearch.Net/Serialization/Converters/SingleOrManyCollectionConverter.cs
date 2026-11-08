/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// A <see cref="JsonConverter{T}"/> for <see cref="IReadOnlyCollection{T}"/> that accepts either a
	/// JSON array or a single value. OpenSearch frequently omits the surrounding array when a field
	/// holds only one element (e.g. <c>"foo"</c> instead of <c>["foo"]</c>); in that case the single
	/// value is wrapped into a one-element collection. This mirrors the legacy
	/// <c>InterfaceReadOnlyCollectionSingleOrEnumerableFormatter&lt;T&gt;</c>.
	/// </summary>
	public class SingleOrManyCollectionConverter<T> : JsonConverter<IReadOnlyCollection<T>>
	{
		/// <inheritdoc />
		public override IReadOnlyCollection<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var list = new List<T>();
				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndArray)
						return new ReadOnlyCollection<T>(list);

					list.Add(JsonSerializer.Deserialize<T>(ref reader, options));
				}

				throw new JsonException("Unexpected end of JSON when deserializing a single-or-many collection.");
			}

			// A single, non-array value: read one element and wrap it in a one-element collection.
			var single = JsonSerializer.Deserialize<T>(ref reader, options);
			return new ReadOnlyCollection<T>(new List<T>(1) { single });
		}

		/// <inheritdoc />
		public override void Write(Utf8JsonWriter writer, IReadOnlyCollection<T> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var item in value)
				JsonSerializer.Serialize(writer, item, options);
			writer.WriteEndArray();
		}
	}
}
