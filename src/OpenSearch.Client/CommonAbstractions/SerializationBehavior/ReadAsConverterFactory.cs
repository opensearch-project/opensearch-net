/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ReadAsFormatterResolver</c>. When a type is marked
	/// with <see cref="ReadAsAttribute"/> (typically an interface or abstract type that cannot be instantiated
	/// directly), deserialization is delegated to the concrete type named by the attribute. Serialization is left
	/// to the runtime type's normal handling.
	/// </summary>
	internal class ReadAsConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) =>
			typeToConvert.GetCustomAttribute<ReadAsAttribute>() != null &&
			typeToConvert.GetCustomAttribute<ReadAsAttribute>().Type != typeToConvert;

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var readAs = typeToConvert.GetCustomAttribute<ReadAsAttribute>().Type;

			// [ReadAs] may name an OPEN generic definition (e.g. ISuggest<T> -> [ReadAs(typeof(Suggest<>))]). Close it
			// with the interface's own generic arguments so the concrete type matches (Suggest<T> for ISuggest<T>).
			if (readAs.IsGenericTypeDefinition && typeToConvert.IsGenericType)
				readAs = readAs.MakeGenericType(typeToConvert.GetGenericArguments());

			var converterType = typeof(ReadAsConverter<,>).MakeGenericType(typeToConvert, readAs);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}

		private class ReadAsConverter<TInterface, TConcrete> : JsonConverter<TInterface>
			where TConcrete : TInterface
		{
			// Options identical to the caller's except this ReadAs factory is removed, so serializing TInterface through
			// them uses the interface's data contract without re-entering this converter. Built once per caller-options
			// instance (there is effectively one), lazily on first write.
			private JsonSerializerOptions _contractOptions;

			public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
				JsonSerializer.Deserialize<TConcrete>(ref reader, options);

			public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
			{
				if (value == null)
				{
					writer.WriteNullValue();
					return;
				}

				// Serialize through the declared interface's contract (mirrors the legacy ReadAsFormatter, which used
				// DynamicObjectResolver.ExcludeNullCamelCase.GetFormatter<T>() rather than the runtime type). This
				// honours the interface's [DataMember] allow-list, dropping non-[DataMember] convenience members such as
				// the OSC mapping attribute classes' public bool DocValues / Store getters (which return non-null
				// defaults). Serializing the runtime attribute type instead would leak them — e.g. a [Wildcard] mapping
				// surfaced by AutoMap emitting "doc_values":true / "store":false.
				JsonSerializer.Serialize(writer, value, GetContractOptions(options));
			}

			private JsonSerializerOptions GetContractOptions(JsonSerializerOptions options)
			{
				if (_contractOptions != null)
					return _contractOptions;

				var clone = new JsonSerializerOptions(options);
				for (var i = clone.Converters.Count - 1; i >= 0; i--)
				{
					if (clone.Converters[i] is ReadAsConverterFactory)
						clone.Converters.RemoveAt(i);
				}

				return _contractOptions = clone;
			}
		}
	}
}
