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
			var converterType = typeof(ReadAsConverter<,>).MakeGenericType(typeToConvert, readAs);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}

		private class ReadAsConverter<TInterface, TConcrete> : JsonConverter<TInterface>
			where TConcrete : TInterface
		{
			public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
				JsonSerializer.Deserialize<TConcrete>(ref reader, options);

			public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
			{
				if (value == null)
				{
					writer.WriteNullValue();
					return;
				}

				// Serialize using the runtime type so the concrete shape is written.
				JsonSerializer.Serialize(writer, value, value.GetType(), options);
			}
		}
	}
}
