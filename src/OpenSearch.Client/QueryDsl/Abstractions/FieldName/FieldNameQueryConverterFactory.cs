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
	/// Constructs the settings-aware <see cref="FieldNameQueryConverter{T,TInterface}"/> for each field-name query
	/// interface at runtime. This is a <see cref="JsonConverterFactory"/> because
	/// <see cref="FieldNameQueryConverter{T,TInterface}"/> is an open generic that cannot be added to
	/// <c>JsonSerializerOptions.Converters</c> directly, and because it needs the runtime
	/// <see cref="IConnectionSettingsValues"/> which a compile-time <c>[JsonConverter]</c> attribute cannot supply.
	///
	/// The concrete/interface type pair is discovered from the legacy
	/// <c>[JsonFormatter(typeof(FieldNameQueryFormatter&lt;TConcrete, TInterface&gt;))]</c> attribute that already
	/// annotates every field-name query interface (e.g. <c>IMatchQuery</c>, <c>ITermQuery</c>). Reusing that
	/// attribute means no per-type registration is required and the STJ path stays in lock-step with the legacy
	/// mapping.
	/// </summary>
	internal class FieldNameQueryConverterFactory : JsonConverterFactory
	{
		private readonly IConnectionSettingsValues _settings;

		public FieldNameQueryConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

		public override bool CanConvert(Type typeToConvert) => TryGetFieldNameFormatterArgs(typeToConvert, out _);

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			if (!TryGetFieldNameFormatterArgs(typeToConvert, out var args))
				return null;

			// args = [TConcrete, TInterface]; the STJ converter takes the same <T, TInterface> order.
			var converterType = typeof(FieldNameQueryConverter<,>).MakeGenericType(args[0], args[1]);
			return (JsonConverter)Activator.CreateInstance(converterType, _settings);
		}

		// Only handle a type whose legacy [JsonFormatter] points at FieldNameQueryFormatter<TConcrete, TInterface>
		// and whose TInterface is exactly the type being converted (so we bind on the interface, not the concrete).
		private static bool TryGetFieldNameFormatterArgs(Type typeToConvert, out Type[] args)
		{
			args = null;
			var attr = typeToConvert.GetCustomAttribute<JsonFormatterAttribute>();
			var formatterType = attr?.FormatterType;
			if (formatterType == null || !formatterType.IsGenericType)
				return false;

			if (formatterType.GetGenericTypeDefinition() != typeof(FieldNameQueryFormatter<,>))
				return false;

			var genericArgs = formatterType.GetGenericArguments();
			// genericArgs = [TConcrete, TInterface]; bind only when converting the interface itself.
			if (genericArgs[1] != typeToConvert)
				return false;

			args = genericArgs;
			return true;
		}
	}
}
