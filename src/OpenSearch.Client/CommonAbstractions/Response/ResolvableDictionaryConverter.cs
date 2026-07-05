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
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter factory for response dictionaries that derive from
	/// <see cref="ResolvableDictionaryProxy{TKey,TValue}"/> (e.g. <c>FieldCapabilitiesFields</c>,
	/// <c>IndicesStatsDictionary</c>), replacing the vendored <c>ResolvableDictionaryFormatterBase</c>
	/// as part of #388. These types have no public/parameterless constructor and cannot be built by the
	/// generic dictionary factory; they expose a constructor taking the connection settings plus the
	/// backing dictionary, which this converter invokes after reading the JSON object.
	/// </summary>
	internal sealed class ResolvableDictionaryConverterFactory : JsonConverterFactory
	{
		private readonly IConnectionSettingsValues _settings;

		public ResolvableDictionaryConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

		public override bool CanConvert(Type typeToConvert) => FindProxyBase(typeToConvert) != null;

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var proxyBase = FindProxyBase(typeToConvert);
			var args = proxyBase.GetGenericArguments();
			var converterType = typeof(ResolvableDictionaryConverter<,,>).MakeGenericType(typeToConvert, args[0], args[1]);
			return (JsonConverter)Activator.CreateInstance(converterType, _settings);
		}

		private static Type FindProxyBase(Type type)
		{
			for (var current = type; current != null && current != typeof(object); current = current.BaseType)
			{
				if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ResolvableDictionaryProxy<,>))
					return current;
			}
			return null;
		}
	}

	/// <summary>
	/// A <see cref="System.Text.Json"/> converter factory for a bare
	/// <see cref="IReadOnlyDictionary{TKey,TValue}"/> whose key is an OSC inference type
	/// (<see cref="IUrlParameter"/>, e.g. <c>Field</c>/<c>IndexName</c>), replacing the vendored
	/// <c>ResolvableReadOnlyDictionaryFormatter</c> (#388). It materializes a key-resolving
	/// <see cref="ResolvableDictionaryProxy{TKey,TValue}"/> so lookups by an inferred key (e.g.
	/// <c>termVectors[Field&lt;T&gt;(p =&gt; p.Name)]</c>) succeed, which a plain dictionary would not support.
	/// </summary>
	internal sealed class ResolvableReadOnlyDictionaryConverterFactory : JsonConverterFactory
	{
		private readonly IConnectionSettingsValues _settings;

		public ResolvableReadOnlyDictionaryConverterFactory(IConnectionSettingsValues settings) => _settings = settings;

		public override bool CanConvert(Type typeToConvert)
		{
			if (!typeToConvert.IsInterface || !typeToConvert.IsGenericType) return false;
			if (typeToConvert.GetGenericTypeDefinition() != typeof(IReadOnlyDictionary<,>)) return false;
			return typeof(IUrlParameter).IsAssignableFrom(typeToConvert.GetGenericArguments()[0]);
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			var args = typeToConvert.GetGenericArguments();
			var converterType = typeof(ResolvableReadOnlyDictionaryConverter<,>).MakeGenericType(args[0], args[1]);
			return (JsonConverter)Activator.CreateInstance(converterType, _settings);
		}
	}

	/// <inheritdoc cref="ResolvableReadOnlyDictionaryConverterFactory" />
	internal sealed class ResolvableReadOnlyDictionaryConverter<TKey, TValue> : JsonConverter<IReadOnlyDictionary<TKey, TValue>>
		where TKey : IUrlParameter
	{
		private readonly IConnectionSettingsValues _settings;

		public ResolvableReadOnlyDictionaryConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override IReadOnlyDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			var backing = new Dictionary<TKey, TValue>();
			if (reader.TokenType == JsonTokenType.StartObject)
			{
				using var document = JsonDocument.ParseValue(ref reader);
				foreach (var member in document.RootElement.EnumerateObject())
					backing[ResolvableKeyParser<TKey>.Parse(member.Name)] = member.Value.Deserialize<TValue>(options);
			}

			return new ResolvableDictionaryProxy<TKey, TValue>(_settings, backing);
		}

		public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<TKey, TValue> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			foreach (var entry in value)
			{
				writer.WritePropertyName(entry.Key?.GetString(_settings));
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}
	}

	/// <summary>Builds a string → <typeparamref name="TKey"/> parser for an OSC inference key type.</summary>
	internal static class ResolvableKeyParser<TKey>
	{
		public static readonly Func<string, TKey> Parse = Build();

		private static Func<string, TKey> Build()
		{
			var keyType = typeof(TKey);
			var implicitOp = keyType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(string) }, null);
			if (implicitOp != null)
				return s => (TKey)implicitOp.Invoke(null, new object[] { s });

			var stringCtor = keyType.GetConstructor(new[] { typeof(string) });
			if (stringCtor != null)
				return s => (TKey)stringCtor.Invoke(new object[] { s });

			return s => (TKey)(object)s;
		}
	}

	/// <inheritdoc cref="ResolvableDictionaryConverterFactory" />
	internal sealed class ResolvableDictionaryConverter<TDictionary, TKey, TValue> : JsonConverter<TDictionary>
		where TKey : IUrlParameter
	{
		private readonly IConnectionSettingsValues _settings;

		public ResolvableDictionaryConverter(IConnectionSettingsValues settings) => _settings = settings;

		public override TDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return default;

			var backing = new Dictionary<TKey, TValue>();
			if (reader.TokenType == JsonTokenType.StartObject)
			{
				using var document = JsonDocument.ParseValue(ref reader);
				foreach (var member in document.RootElement.EnumerateObject())
					backing[KeyParser(member.Name)] = member.Value.Deserialize<TValue>(options);
			}

			return Construct(backing);
		}

		public override void Write(Utf8JsonWriter writer, TDictionary value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			foreach (var entry in (IEnumerable<KeyValuePair<TKey, TValue>>)value)
			{
				writer.WritePropertyName(entry.Key?.GetString(_settings));
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}

		private TDictionary Construct(Dictionary<TKey, TValue> backing)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			foreach (var ctor in typeof(TDictionary).GetConstructors(flags))
			{
				var parameters = ctor.GetParameters();
				if (parameters.Length != 2) continue;
				if (!parameters[0].ParameterType.IsInstanceOfType(_settings)) continue;
				if (!parameters[1].ParameterType.IsAssignableFrom(typeof(Dictionary<TKey, TValue>))) continue;
				return (TDictionary)ctor.Invoke(new object[] { _settings, backing });
			}

			throw new JsonException($"Cannot construct '{typeof(TDictionary)}': no (settings, dictionary) constructor found.");
		}

		private static readonly Func<string, TKey> KeyParser = BuildKeyParser();

		private static Func<string, TKey> BuildKeyParser()
		{
			var keyType = typeof(TKey);
			var implicitOp = keyType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null,
				new[] { typeof(string) }, null);
			if (implicitOp != null)
				return s => (TKey)implicitOp.Invoke(null, new object[] { s });

			var stringCtor = keyType.GetConstructor(new[] { typeof(string) });
			if (stringCtor != null)
				return s => (TKey)stringCtor.Invoke(new object[] { s });

			return s => (TKey)(object)s;
		}
	}
}
