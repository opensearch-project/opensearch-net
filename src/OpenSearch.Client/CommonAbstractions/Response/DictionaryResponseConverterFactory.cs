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
using JsonFormatterAttribute = OpenSearch.Net.Utf8Json.JsonFormatterAttribute;

namespace OpenSearch.Client
{
	/// <summary>
	/// Constructs the System.Text.Json converter for the three generic response-dictionary formatter families at
	/// runtime. This is a <see cref="JsonConverterFactory"/> because the target converters
	/// (<see cref="DictionaryResponseConverter{TResponse,TKey,TValue}"/>,
	/// <see cref="ResolvableDictionaryResponseConverter{TResponse,TKey,TValue}"/> and
	/// <see cref="DynamicResponseConverter{TResponse}"/>) are open generics that cannot be added to
	/// <c>JsonSerializerOptions.Converters</c> directly, and because the resolvable variant needs the runtime
	/// <see cref="IConnectionSettingsValues"/> which a compile-time <c>[JsonConverter]</c> attribute cannot supply.
	///
	/// The generic arguments are discovered from the legacy
	/// <c>[JsonFormatter(typeof(DictionaryResponseFormatter&lt;...&gt;))]</c> (and the resolvable/dynamic equivalents)
	/// attribute that already annotates every concrete response type (e.g. <c>RemoteInfoResponse</c>,
	/// <c>GetMappingResponse</c>, <c>ClusterStateResponse</c>). Reusing that attribute means no per-type registration is
	/// required and the STJ mapping stays in lock-step with the legacy one. The type-arg order is preserved verbatim
	/// (<c>&lt;TResponse, TKey, TValue&gt;</c>, or just <c>&lt;TResponse&gt;</c> for the dynamic family).
	/// </summary>
	internal class DictionaryResponseConverterFactory : JsonConverterFactory
	{
		private readonly IConnectionSettingsValues _settings;

		public DictionaryResponseConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

		public override bool CanConvert(Type typeToConvert) =>
			TryGetResponseFormatter(typeToConvert, out _, out _);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (!TryGetResponseFormatter(typeToConvert, out var formatterDefinition, out var args))
				return null;

			if (formatterDefinition == typeof(DictionaryResponseFormatter<,,>))
			{
				// args = [TResponse, TKey, TValue]
				var converterType = typeof(DictionaryResponseConverter<,,>).MakeGenericType(args[0], args[1], args[2]);
				return (JsonConverter)Activator.CreateInstance(converterType);
			}

			if (formatterDefinition == typeof(ResolvableDictionaryResponseFormatter<,,>))
			{
				// args = [TResponse, TKey, TValue]; the resolvable converter needs settings for key resolution.
				var converterType = typeof(ResolvableDictionaryResponseConverter<,,>).MakeGenericType(args[0], args[1], args[2]);
				return (JsonConverter)Activator.CreateInstance(converterType, _settings);
			}

			if (formatterDefinition == typeof(DynamicResponseFormatter<>))
			{
				// args = [TResponse]
				var converterType = typeof(DynamicResponseConverter<>).MakeGenericType(args[0]);
				return (JsonConverter)Activator.CreateInstance(converterType);
			}

			return null;
		}

		// Recognizes a type whose legacy [JsonFormatter] points at one of the three generic response-dictionary
		// formatter definitions, and returns that open generic definition plus the closed generic arguments.
		private static bool TryGetResponseFormatter(Type typeToConvert, out Type formatterDefinition, out Type[] args)
		{
			formatterDefinition = null;
			args = null;

			var attr = typeToConvert.GetCustomAttribute<JsonFormatterAttribute>();
			var formatterType = attr?.FormatterType;
			if (formatterType == null || !formatterType.IsGenericType)
				return false;

			var definition = formatterType.GetGenericTypeDefinition();
			if (definition != typeof(DictionaryResponseFormatter<,,>) &&
				definition != typeof(ResolvableDictionaryResponseFormatter<,,>) &&
				definition != typeof(DynamicResponseFormatter<>))
				return false;

			// Bind only when converting the response type itself (the first generic arg), not TKey/TValue.
			var genericArgs = formatterType.GetGenericArguments();
			if (genericArgs[0] != typeToConvert)
				return false;

			formatterDefinition = definition;
			args = genericArgs;
			return true;
		}
	}
}
