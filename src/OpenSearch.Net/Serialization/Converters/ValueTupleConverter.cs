/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ValueTupleFormatter</c>. A <c>System.ValueTuple</c>
	/// exposes its elements as public FIELDS (Item1, Item2, …, and Rest for arity &gt; 7), which STJ does not serialize
	/// by default (IncludeFields is off), so a tuple member would otherwise render as an empty object. This factory
	/// (de)serializes the tuple as an object with <c>Item1..ItemN</c> properties, matching the legacy formatter,
	/// without turning on field serialization globally.
	/// </summary>
	public class ValueTupleConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert)
		{
			var t = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
			return t.IsValueType && t.IsGenericType && t.FullName != null && t.FullName.StartsWith("System.ValueTuple`", StringComparison.Ordinal);
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			// STJ requires a converter whose handled type exactly matches the requested type, so build a
			// Nullable-typed converter for Nullable<ValueTuple<...>> and a plain one for the bare tuple.
			var underlying = Nullable.GetUnderlyingType(typeToConvert);
			var converterType = underlying != null
				? typeof(NullableValueTupleConverter<>).MakeGenericType(underlying)
				: typeof(ValueTupleConverter<>).MakeGenericType(typeToConvert);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}
	}

	internal class NullableValueTupleConverter<TTuple> : JsonConverter<TTuple?> where TTuple : struct
	{
		private readonly ValueTupleConverter<TTuple> _inner = new ValueTupleConverter<TTuple>();

		public override TTuple? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			reader.TokenType == JsonTokenType.Null ? (TTuple?)null : _inner.Read(ref reader, typeof(TTuple), options);

		public override void Write(Utf8JsonWriter writer, TTuple? value, JsonSerializerOptions options)
		{
			if (value.HasValue)
				_inner.Write(writer, value.Value, options);
			else
				writer.WriteNullValue();
		}
	}

	internal class ValueTupleConverter<TTuple> : JsonConverter<TTuple>
	{
		// The tuple's Item1..ItemN (and Rest) fields, in declaration order.
		private static readonly FieldInfo[] Fields = typeof(TTuple)
			.GetFields(BindingFlags.Public | BindingFlags.Instance)
			.OrderBy(f => f.Name == "Rest" ? int.MaxValue : int.Parse(f.Name.Substring("Item".Length)))
			.ToArray();

		public override TTuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return default;

			var boxed = (object)Activator.CreateInstance(typeof(TTuple));
			var byName = Fields.ToDictionary(f => f.Name, f => f);

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject when reading {typeof(TTuple).Name}.");

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var name = reader.GetString();
				reader.Read();
				if (name != null && byName.TryGetValue(name, out var field))
					field.SetValue(boxed, JsonSerializer.Deserialize(ref reader, field.FieldType, options));
				else
					reader.Skip();
			}

			return (TTuple)boxed;
		}

		public override void Write(Utf8JsonWriter writer, TTuple value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			foreach (var field in Fields)
			{
				writer.WritePropertyName(field.Name);
				JsonSerializer.Serialize(writer, field.GetValue(value), field.FieldType, options);
			}
			writer.WriteEndObject();
		}
	}
}
