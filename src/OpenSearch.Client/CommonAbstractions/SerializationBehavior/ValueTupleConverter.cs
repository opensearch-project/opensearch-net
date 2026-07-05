/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter factory for the two-arity <see cref="ValueTuple{T1,T2}"/>,
	/// replacing the vendored Utf8Json <c>TupleFormatter</c> as part of #388. STJ does not serialize a
	/// <c>ValueTuple</c>'s <c>Item1</c>/<c>Item2</c> fields (they are fields, not properties), so a tuple
	/// document member serialized as an empty object. This writes/reads
	/// <c>{ "Item1": …, "Item2": … }</c> with the field names verbatim (not inferred). STJ applies it to a
	/// <see cref="Nullable{T}"/> tuple automatically.
	/// </summary>
	internal sealed class ValueTupleConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) =>
			typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ValueTuple<,>);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var arguments = typeToConvert.GetGenericArguments();
			var converterType = typeof(ValueTupleConverter<,>).MakeGenericType(arguments);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}
	}

	/// <inheritdoc cref="ValueTupleConverterFactory" />
	internal sealed class ValueTupleConverter<T1, T2> : JsonConverter<(T1, T2)>
	{
		public override void Write(Utf8JsonWriter writer, (T1, T2) value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("Item1");
			JsonSerializer.Serialize(writer, value.Item1, options);
			writer.WritePropertyName("Item2");
			JsonSerializer.Serialize(writer, value.Item2, options);
			writer.WriteEndObject();
		}

		public override (T1, T2) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return default;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			var item1 = root.TryGetProperty("Item1", out var i1) ? i1.Deserialize<T1>(options) : default;
			var item2 = root.TryGetProperty("Item2", out var i2) ? i2.Deserialize<T2>(options) : default;
			return (item1, item2);
		}
	}
}
