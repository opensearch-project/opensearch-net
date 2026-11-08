/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>SingleOrEnumerableFormatter&lt;T&gt;</c>. On read a bare
	/// scalar is coerced into a one-element sequence (some OpenSearch fields accept <c>"x"</c> or <c>["x"]</c>); an
	/// array reads as itself. On write the value is always emitted as a JSON array.
	/// </summary>
	internal class SingleOrEnumerableConverter<T> : JsonConverter<IEnumerable<T>>
	{
		public override IEnumerable<T> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType == JsonTokenType.StartArray)
				return JsonSerializer.Deserialize<List<T>>(ref reader, options);

			return new List<T> { JsonSerializer.Deserialize<T>(ref reader, options) };
		}

		public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
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

	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>SerializeAsSingleFormatter&lt;T&gt;</c>: reads
	/// single-or-array (like <see cref="SingleOrEnumerableConverter{T}"/>) but writes only the first element as a
	/// bare scalar (a null / empty sequence writes null).
	/// </summary>
	internal class SerializeAsSingleConverter<T> : JsonConverter<IEnumerable<T>>
	{
		public override IEnumerable<T> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType == JsonTokenType.StartArray)
				return JsonSerializer.Deserialize<List<T>>(ref reader, options);

			return new List<T> { JsonSerializer.Deserialize<T>(ref reader, options) };
		}

		public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			using var e = value.GetEnumerator();
			if (!e.MoveNext())
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializer.Serialize(writer, e.Current, options);
		}
	}
}
