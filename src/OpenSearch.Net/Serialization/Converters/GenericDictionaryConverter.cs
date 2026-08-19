/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Net.Serialization.Converters
{
	/// <summary>
	/// Handles dictionary types that System.Text.Json's built-in dictionary support cannot: those keyed by
	/// <see cref="object"/> (STJ rejects a non-string/non-primitive dictionary key) and read-only dictionary types
	/// that lack a public parameterless constructor (e.g. <c>ReadOnlyDictionary&lt;,&gt;</c>, a custom
	/// <c>IReadOnlyDictionary&lt;,&gt;</c>, or an <c>IIsADictionary</c> implementation). Keys are written verbatim via
	/// <c>ToString</c>; values delegate to the surrounding options. On read the target concrete type is reconstructed
	/// through a parameterless constructor + indexer, or a constructor accepting an <c>IDictionary&lt;TKey,TValue&gt;</c>.
	///
	/// The common string-keyed cases (<c>Dictionary&lt;string,T&gt;</c>, <c>IDictionary&lt;string,T&gt;</c>,
	/// <c>Hashtable</c>) are left to STJ's built-in support — <see cref="CanConvert"/> only claims the types STJ fails.
	/// </summary>
	public class GenericDictionaryConverterFactory : JsonConverterFactory
	{
		// Optional field-name inferrer (from the high-level connection settings' DefaultFieldNameInferrer) applied to
		// keys of IIsADictionary types, matching the legacy dynamic-object serialization of those types.
		private readonly Func<string, string> _fieldNameInferrer;

		public GenericDictionaryConverterFactory() { }

		public GenericDictionaryConverterFactory(Func<string, string> fieldNameInferrer) => _fieldNameInferrer = fieldNameInferrer;

		public override bool CanConvert(Type typeToConvert)
		{
			if (typeToConvert == typeof(string) || typeToConvert.IsPrimitive)
				return false;

			if (!TryGetKeyValueTypes(typeToConvert, out var key, out _))
				return false;

			// object keys are STJ's own gap (it rejects them). Otherwise only claim types STJ cannot instantiate:
			// a CONCRETE read-only / ctor-injected dictionary (e.g. ReadOnlyDictionary<,>) or a read-only INTERFACE
			// keyed by string/object (e.g. IReadOnlyDictionary<string,object>). Do NOT claim interfaces keyed by an
			// inferred type (Field/IndexName/…) — those are handled by their own settings-aware converters, and this
			// converter would emit the key's raw ToString() instead of the inferred name.
			if (key == typeof(object))
				return true;
			if (!IsReadOnlyDictionaryType(typeToConvert))
				return false;
			if (typeToConvert.IsInterface || typeToConvert.IsAbstract)
				return key == typeof(string) || key == typeof(object);
			return true;
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			TryGetKeyValueTypes(typeToConvert, out var key, out var value);
			var converterType = typeof(GenericDictionaryConverter<,,>).MakeGenericType(typeToConvert, key, value);
			// IIsADictionary types camel-case (inferrer) their keys; plain dictionaries write keys verbatim.
			var inferrer = IsIsADictionary(typeToConvert) ? _fieldNameInferrer : null;
			return (JsonConverter)Activator.CreateInstance(converterType, inferrer);
		}

		private static bool IsIsADictionary(Type type) =>
			type.GetInterfaces().Any(i => i.Name == "IIsADictionary");

		// True when the type implements IDictionary<,> or IReadOnlyDictionary<,>; returns the key/value types.
		internal static bool TryGetKeyValueTypes(Type type, out Type key, out Type value)
		{
			key = value = null;
			// Include the type itself: GetInterfaces() does not list `type` when it is itself IDictionary<,>/
			// IReadOnlyDictionary<,> (e.g. a member declared as the interface).
			var candidates = type.IsInterface ? new[] { type }.Concat(type.GetInterfaces()) : type.GetInterfaces();
			foreach (var i in candidates)
			{
				if (!i.IsGenericType)
					continue;
				var def = i.GetGenericTypeDefinition();
				if (def == typeof(IDictionary<,>) || def == typeof(IReadOnlyDictionary<,>))
				{
					var args = i.GetGenericArguments();
					key = args[0];
					value = args[1];
					return true;
				}
			}
			return false;
		}

		// A type STJ cannot populate: no public parameterless ctor + public Add, i.e. read-only / ctor-injected.
		private static bool IsReadOnlyDictionaryType(Type type)
		{
			if (type.IsInterface || type.IsAbstract)
				return true;
			var hasParameterless = type.GetConstructor(Type.EmptyTypes) != null;
			return !hasParameterless;
		}
	}

	internal class GenericDictionaryConverter<TDictionary, TKey, TValue> : JsonConverter<TDictionary>
	{
		private readonly Func<string, string> _fieldNameInferrer;

		public GenericDictionaryConverter() { }

		public GenericDictionaryConverter(Func<string, string> fieldNameInferrer) => _fieldNameInferrer = fieldNameInferrer;

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
				var key = entry.Key is string s ? s : Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
				if (_fieldNameInferrer != null && key != null)
					key = _fieldNameInferrer(key);
				writer.WritePropertyName(key ?? string.Empty);
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}

		public override TDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return default;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject when reading {typeof(TDictionary).Name}.");

			var intermediate = new Dictionary<TKey, TValue>();
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var keyString = reader.GetString();
				reader.Read();
				// For object-typed values, produce native CLR primitives (string/long/double/bool/nested) rather than
				// JsonElement, matching the legacy PrimitiveObjectFormatter so round-tripped values compare equal.
				var value = typeof(TValue) == typeof(object)
					? (TValue)ReadPrimitiveObject(ref reader)
					: JsonSerializer.Deserialize<TValue>(ref reader, options);
				intermediate[ConvertKey(keyString)] = value;
			}

			return Construct(intermediate);
		}

		// Reads a JSON value into native CLR primitives (mirrors the legacy Utf8Json PrimitiveObjectFormatter):
		// integral number -> long, fractional -> double, string, bool, object -> Dictionary<string,object>,
		// array -> List<object>, null -> null. Avoids JsonElement leaking for object-typed dictionary values.
		private static object ReadPrimitiveObject(ref Utf8JsonReader reader)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return reader.GetString();
				case JsonTokenType.Number:
					return reader.TryGetInt64(out var l) ? l : (object)reader.GetDouble();
				case JsonTokenType.True:
					return true;
				case JsonTokenType.False:
					return false;
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.StartObject:
					var dict = new Dictionary<string, object>();
					while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
					{
						var name = reader.GetString();
						reader.Read();
						dict[name] = ReadPrimitiveObject(ref reader);
					}
					return dict;
				case JsonTokenType.StartArray:
					var list = new List<object>();
					while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
						list.Add(ReadPrimitiveObject(ref reader));
					return list;
				default:
					reader.Skip();
					return null;
			}
		}

		private static TKey ConvertKey(string key)
		{
			if (typeof(TKey) == typeof(string) || typeof(TKey) == typeof(object))
				return (TKey)(object)key;
			return (TKey)Convert.ChangeType(key, typeof(TKey), CultureInfo.InvariantCulture);
		}

		private static TDictionary Construct(Dictionary<TKey, TValue> entries)
		{
			var t = typeof(TDictionary);

			// Interface target (IDictionary<,>, IReadOnlyDictionary<,>): a plain Dictionary satisfies it.
			if (t.IsInterface)
				return (TDictionary)(object)entries;

			// Constructor taking IDictionary<TKey,TValue> (ReadOnlyDictionary<,>, IsADictionaryBase, custom wrappers).
			var dictCtor = t.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.FirstOrDefault(c =>
				{
					var ps = c.GetParameters();
					return ps.Length == 1 && ps[0].ParameterType.IsAssignableFrom(typeof(IDictionary<TKey, TValue>));
				});
			if (dictCtor != null)
				return (TDictionary)dictCtor.Invoke(new object[] { entries });

			// Parameterless ctor + IDictionary<,> Add.
			var parameterless = t.GetConstructor(Type.EmptyTypes);
			if (parameterless != null)
			{
				var instance = (TDictionary)parameterless.Invoke(null);
				if (instance is IDictionary<TKey, TValue> typed)
				{
					foreach (var kv in entries)
						typed[kv.Key] = kv.Value;
					return instance;
				}
				if (instance is IDictionary nonGeneric)
				{
					foreach (var kv in entries)
						nonGeneric[kv.Key] = kv.Value;
					return instance;
				}
			}

			throw new JsonException($"Cannot construct {t.Name}: no usable constructor.");
		}
	}
}
