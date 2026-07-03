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
	/// Per-property <see cref="System.Text.Json"/> converter replacing the vendored Utf8Json
	/// <c>SingleOrEnumerableFormatter&lt;T&gt;</c> (#388). Writes the sequence as a normal JSON array,
	/// and on read leniently accepts either an array or a single value (which OpenSearch may send when
	/// a field holds one element), normalizing to a sequence. Applied per-member via
	/// <see cref="OpenSearch.Net.DataContractResolver.PropertyConverterOverridesOpenGeneric"/>.
	/// </summary>
	internal sealed class SingleOrEnumerableConverter<T> : JsonConverter<IEnumerable<T>>
	{
		public override IEnumerable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			if (reader.TokenType == JsonTokenType.StartArray)
				return JsonSerializer.Deserialize<List<T>>(ref reader, options);

			return new[] { JsonSerializer.Deserialize<T>(ref reader, options) };
		}

		public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
		{
			if (value == null) { writer.WriteNullValue(); return; }
			// Serializing the sequence type (not this property) uses the default enumerable handling and
			// does not re-enter this per-property converter.
			JsonSerializer.Serialize(writer, value, options);
		}
	}

	/// <summary>
	/// Per-property <see cref="System.Text.Json"/> converter replacing the vendored Utf8Json
	/// <c>SerializeAsSingleFormatter&lt;T&gt;</c> (#388). On read it accepts an array or a single value
	/// (as <see cref="SingleOrEnumerableConverter{T}"/>), but on write it emits only the first element
	/// as a scalar (or null when empty).
	/// </summary>
	internal sealed class SerializeAsSingleConverter<T> : JsonConverter<IEnumerable<T>>
	{
		public override IEnumerable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			if (reader.TokenType == JsonTokenType.StartArray)
				return JsonSerializer.Deserialize<List<T>>(ref reader, options);

			return new[] { JsonSerializer.Deserialize<T>(ref reader, options) };
		}

		public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
		{
			if (value == null) { writer.WriteNullValue(); return; }

			using var enumerator = value.GetEnumerator();
			if (!enumerator.MoveNext()) { writer.WriteNullValue(); return; }

			JsonSerializer.Serialize(writer, enumerator.Current, options);
		}
	}
}
