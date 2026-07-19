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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// Constructs the <see cref="VerbatimDictionaryKeysConverter{TDictionary,TInterface,TKey,TValue}"/> for each
	/// <c>IIsADictionary</c> interface at runtime. This is a <see cref="JsonConverterFactory"/> because the converter
	/// is an open generic (four type parameters) that cannot be added to <c>JsonSerializerOptions.Converters</c>
	/// directly.
	///
	/// The four type arguments are discovered from the legacy
	/// <c>[JsonFormatter(typeof(VerbatimDictionaryKeysFormatter&lt;TDictionary, TInterface, TKey, TValue&gt;))]</c>
	/// attribute that already annotates every such interface (e.g. <c>IAliases</c>, <c>IRelations</c>,
	/// <c>INormalizers</c>). Reusing that attribute means no per-type registration is required and the STJ path stays
	/// in lock-step with the legacy mapping.
	///
	/// Without this factory, an interface-typed dictionary property (e.g. <c>IAliases Aliases</c>) has no matching
	/// converter and System.Text.Json falls back to its default dictionary handling, which cannot instantiate the
	/// abstract interface and throws <see cref="NotSupportedException"/> ("The collection type '...' is abstract, an
	/// interface, or is read only ...").
	/// </summary>
	internal class VerbatimDictionaryKeysConverterFactory : JsonConverterFactory
	{
		public override bool CanConvert(Type typeToConvert) => TryGetFormatterArgs(typeToConvert, out _);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (!TryGetFormatterArgs(typeToConvert, out var args))
				return null;

			// args = [TDictionary, TInterface, TKey, TValue]; the STJ converter takes the same order.
			var converterType = typeof(VerbatimDictionaryKeysConverter<,,,>).MakeGenericType(args);
			return (JsonConverter)Activator.CreateInstance(converterType);
		}

		// Only handle a type whose legacy [JsonFormatter] points at the four-argument, interface-bound
		// VerbatimDictionaryKeysFormatter<TDictionary, TInterface, TKey, TValue> and whose TInterface is exactly the
		// type being converted (so we bind on the interface, not the concrete dictionary).
		private static bool TryGetFormatterArgs(Type typeToConvert, out Type[] args)
		{
			args = null;
			var formatterType = typeToConvert.GetCustomAttribute<JsonFormatterAttribute>()?.FormatterType;
			if (formatterType == null || !formatterType.IsGenericType)
				return false;

			if (formatterType.GetGenericTypeDefinition() != typeof(VerbatimDictionaryKeysFormatter<,,,>))
				return false;

			var genericArgs = formatterType.GetGenericArguments();
			// genericArgs = [TDictionary, TInterface, TKey, TValue]; bind only when converting the interface itself.
			if (genericArgs[1] != typeToConvert)
				return false;

			args = genericArgs;
			return true;
		}
	}
}
