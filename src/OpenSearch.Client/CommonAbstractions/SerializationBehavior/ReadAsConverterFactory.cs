/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter factory that honors the client's <c>[ReadAs]</c>
	/// attribute for any interface used as a (nested) property (#388). Utf8Json's <c>ReadAsFormatter</c>
	/// deserializes an interface into the concrete type named by <c>[ReadAs]</c>; STJ cannot
	/// instantiate an interface on its own, so this factory supplies that mapping generically (for
	/// example <c>ISpanQuery</c> → <c>SpanQuery</c>). Writing serializes the value's runtime type.
	/// <para>
	/// Interfaces handled by a dedicated converter (field-name queries, scripts, containers, …) do not
	/// carry <c>[ReadAs]</c>, so they are unaffected.
	/// </para>
	/// </summary>
	internal sealed class ReadAsConverterFactory : JsonConverterFactory
	{
		private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new();

		public override bool CanConvert(Type typeToConvert) =>
			typeToConvert.IsInterface && typeToConvert.GetCustomAttribute<ReadAsAttribute>() != null;

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
			Cache.GetOrAdd(typeToConvert, t =>
			{
				var concrete = t.GetCustomAttribute<ReadAsAttribute>().Type;
				var converterType = typeof(ReadAsConverter<,>).MakeGenericType(t, concrete);
				return (JsonConverter)Activator.CreateInstance(converterType);
			});

		private sealed class ReadAsConverter<TInterface, TConcrete> : JsonConverter<TInterface>
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

				// Serialize the concrete runtime type; it is not TInterface, so this does not recurse.
				JsonSerializer.Serialize(writer, value, value.GetType(), options);
			}
		}
	}
}
