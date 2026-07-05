/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter factory for <see cref="IIsADictionary{TKey,TValue}"/>
	/// concrete types that do not carry a dedicated <c>[JsonFormatter]</c> (replacing the vendored Utf8Json
	/// <c>IsADictionaryFormatterResolver</c> as part of #388). Unlike a verbatim-keys dictionary, these
	/// camel-case their keys through the client's <see cref="IConnectionSettingsValues.DefaultFieldNameInferrer"/>
	/// and include null values, matching OSC conventions. Types with a <c>[JsonFormatter]</c> (verbatim
	/// dictionaries such as <c>Analyzers</c>, <c>Properties</c>, <c>AggregationDictionary</c>) are excluded
	/// so their dedicated converters keep handling them.
	/// </summary>
	internal sealed class IsADictionaryConverterFactory : JsonConverterFactory
	{
		private readonly IConnectionSettingsValues _settings;

		public IsADictionaryConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

		public override bool CanConvert(Type typeToConvert)
		{
			if (!typeof(IIsADictionary).IsAssignableFrom(typeToConvert)) return false;
			if (typeToConvert.IsInterface || typeToConvert.IsAbstract) return false;
			if (HasJsonFormatter(typeToConvert)) return false;
			return GetGenericArguments(typeToConvert) != null;
		}

		private static bool HasJsonFormatter(Type type)
		{
			if (type.GetCustomAttribute<OpenSearch.Net.Utf8Json.JsonFormatterAttribute>(true) != null) return true;
			foreach (var interfaceType in type.GetInterfaces())
				if (interfaceType.GetCustomAttribute<OpenSearch.Net.Utf8Json.JsonFormatterAttribute>(false) != null) return true;
			return false;
		}

		private static Type[] GetGenericArguments(Type type)
		{
			foreach (var interfaceType in type.GetInterfaces())
			{
				if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IIsADictionary<,>))
					return interfaceType.GetGenericArguments();
			}
			return null;
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var arguments = GetGenericArguments(typeToConvert);
			var converterType = typeof(IsADictionaryConverter<,,>).MakeGenericType(typeToConvert, arguments[0], arguments[1]);
			return (JsonConverter)Activator.CreateInstance(converterType, _settings);
		}
	}

	/// <inheritdoc cref="IsADictionaryConverterFactory" />
	internal sealed class IsADictionaryConverter<TDictionary, TKey, TValue> : JsonConverter<TDictionary>
		where TDictionary : class, IIsADictionary<TKey, TValue>
	{
		private readonly IConnectionSettingsValues _settings;

		public IsADictionaryConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override void Write(Utf8JsonWriter writer, TDictionary value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var mutator = _settings.DefaultFieldNameInferrer;
			writer.WriteStartObject();
			foreach (var entry in value)
			{
				writer.WritePropertyName(mutator(KeyToString(entry.Key)));
				// Dictionary values are written even when null (WhenWritingNull does not apply here).
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}

		public override TDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			if (document.RootElement.ValueKind != JsonValueKind.Object) return null;

			var intermediate = new Dictionary<TKey, TValue>();
			foreach (var member in document.RootElement.EnumerateObject())
				intermediate[ConvertKey(member.Name)] = member.Value.Deserialize<TValue>(options);

			return typeof(TDictionary).CreateInstance<TDictionary>(intermediate);
		}

		private static string KeyToString(TKey key) => key is string s ? s : key?.ToString();

		private static TKey ConvertKey(string name)
		{
			if (typeof(TKey) == typeof(string) || typeof(TKey) == typeof(object))
				return (TKey)(object)name;
			return (TKey)Convert.ChangeType(name, typeof(TKey));
		}
	}
}
