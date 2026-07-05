/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter factory for the dictionary shapes the built-in
	/// <c>System.Text.Json</c> dictionary support cannot round-trip (#388), mirroring the vendored
	/// Utf8Json <c>ReadOnlyDictionaryFormatter</c>, <c>InterfaceReadOnlyDictionaryFormatter</c> and
	/// <c>GenericDictionaryFormatter</c>.
	/// <para>
	/// STJ natively (de)serializes <see cref="Dictionary{TKey,TValue}"/> (and types deriving from it),
	/// the bare <see cref="IDictionary{TKey,TValue}"/>/<see cref="IReadOnlyDictionary{TKey,TValue}"/>
	/// interfaces, and the non-generic <see cref="IDictionary"/>, but only while the key is a
	/// <see cref="string"/> or enum, and only when it can construct the target itself. It cannot:
	/// </para>
	/// <list type="bullet">
	/// <item>
	/// deserialize a concrete <see cref="ReadOnlyDictionary{TKey,TValue}"/> (no parameterless
	/// constructor — it throws <see cref="NotSupportedException"/> even though writing works);
	/// </item>
	/// <item>
	/// handle any dictionary whose key is not a string/enum (e.g. <c>object</c> keys), throwing
	/// <see cref="NotSupportedException"/>;
	/// </item>
	/// <item>
	/// construct a custom concrete implementer of <see cref="IDictionary{TKey,TValue}"/>/
	/// <see cref="IReadOnlyDictionary{TKey,TValue}"/> that has no usable parameterless constructor.
	/// </item>
	/// </list>
	/// <para>
	/// <see cref="CanConvert"/> is deliberately conservative so the native fast path is preserved for
	/// everything STJ already handles: <c>Dictionary&lt;string,TV&gt;</c> and its subclasses, the bare
	/// generic interfaces, the non-generic <see cref="IDictionary"/>/<c>Hashtable</c>, and the client's
	/// own <see cref="IIsADictionary"/> types (which have dedicated converters).
	/// </para>
	/// </summary>
	internal sealed class ReadOnlyDictionaryConverterFactory : JsonConverterFactory
	{
		private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new();

		public override bool CanConvert(Type typeToConvert)
		{
			// (a) A closed ReadOnlyDictionary<,>: writing works but STJ cannot construct it on read.
			if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>))
				return true;

			// The client's IsADictionary family (IAnalyzers, AggregationDictionary, …) is handled by its
			// own converters/formatters — never take those over.
			if (typeof(IIsADictionary).IsAssignableFrom(typeToConvert))
				return false;

			// The bare generic dictionary interfaces are handled natively by STJ (they deserialize into a
			// Dictionary<,>); keep the fast path.
			if (typeToConvert.IsInterface && typeToConvert.IsGenericType)
			{
				var definition = typeToConvert.GetGenericTypeDefinition();
				if (definition == typeof(IDictionary<,>) || definition == typeof(IReadOnlyDictionary<,>))
					return false;
			}

			// Only consider types that implement a *generic* dictionary interface. Non-generic
			// IDictionary / Hashtable and custom non-generic implementers are handled natively by STJ.
			if (!TryGetGenericDictionaryArguments(typeToConvert, out var keyType, out _))
				return false;

			// Dictionary<,> itself and anything deriving from it (e.g. a `MyDictionary : Dictionary<string,string>`)
			// are handled natively by STJ for string/enum keys; keep the fast path.
			if (IsOrDerivesFromDictionary(typeToConvert))
				return false;

			// Non-string / non-enum keys (e.g. object keys): STJ throws NotSupportedException, so take over.
			if (keyType != typeof(string) && !keyType.IsEnum)
				return true;

			// A string/enum-keyed concrete custom implementer STJ cannot construct (no parameterless ctor).
			if (!typeToConvert.IsInterface && !typeToConvert.IsAbstract && typeToConvert.GetConstructor(Type.EmptyTypes) == null)
				return true;

			return false;
		}

		public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
			Cache.GetOrAdd(typeToConvert, t =>
			{
				Type keyType, valueType;
				if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>))
				{
					var args = t.GetGenericArguments();
					keyType = args[0];
					valueType = args[1];
				}
				else if (!TryGetGenericDictionaryArguments(t, out keyType, out valueType))
					throw new NotSupportedException($"'{t}' is not a supported dictionary type.");

				var converterType = typeof(Impl<,,>).MakeGenericType(t, keyType, valueType);
				return (JsonConverter)Activator.CreateInstance(converterType);
			});

		/// <summary>
		/// Finds the closed <see cref="IReadOnlyDictionary{TKey,TValue}"/> or
		/// <see cref="IDictionary{TKey,TValue}"/> the type implements and yields its key/value types.
		/// </summary>
		private static bool TryGetGenericDictionaryArguments(Type type, out Type keyType, out Type valueType)
		{
			foreach (var candidate in EnumerateSelfAndInterfaces(type))
			{
				if (!candidate.IsGenericType) continue;

				var definition = candidate.GetGenericTypeDefinition();
				if (definition == typeof(IReadOnlyDictionary<,>) || definition == typeof(IDictionary<,>))
				{
					var args = candidate.GetGenericArguments();
					keyType = args[0];
					valueType = args[1];
					return true;
				}
			}

			keyType = null;
			valueType = null;
			return false;
		}

		private static IEnumerable<Type> EnumerateSelfAndInterfaces(Type type)
		{
			if (type.IsInterface) yield return type;
			foreach (var i in type.GetInterfaces()) yield return i;
		}

		private static bool IsOrDerivesFromDictionary(Type type)
		{
			for (var current = type; current != null && current != typeof(object); current = current.BaseType)
			{
				if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Dictionary<,>))
					return true;
			}

			return false;
		}

		/// <summary>
		/// The generic worker: buffers the JSON object into a <see cref="Dictionary{TKey,TValue}"/> then
		/// materializes <typeparamref name="TDictionary"/> (a concrete <see cref="ReadOnlyDictionary{TKey,TValue}"/>,
		/// a constructor-injected custom type, an interface, or a parameterless-constructible type populated
		/// via <c>Add</c>). Keys are written/read verbatim as property names.
		/// </summary>
		private sealed class Impl<TDictionary, TKey, TValue> : JsonConverter<TDictionary>
		{
			public override TDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.Null)
					return default;

				if (reader.TokenType != JsonTokenType.StartObject)
					throw new JsonException($"Expected start of object to deserialize '{typeof(TDictionary)}'.");

				var buffer = new Dictionary<TKey, TValue>();
				while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read();
					var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
					buffer[ConvertKey(propertyName)] = value;
				}

				return (TDictionary)Materialize(buffer);
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
					writer.WritePropertyName(KeyToPropertyName(entry.Key));
					// Explicitly serialize the value (including nulls) so dictionary entries are not dropped
					// by the options' WhenWritingNull default, which applies to object members only.
					JsonSerializer.Serialize(writer, entry.Value, options);
				}
				writer.WriteEndObject();
			}

			private static readonly Func<string, TKey> KeyParser = BuildKeyParser();

			private static TKey ConvertKey(string propertyName) => KeyParser(propertyName);

			private static Func<string, TKey> BuildKeyParser()
			{
				var keyType = typeof(TKey);
				if (keyType == typeof(string) || keyType == typeof(object))
					return s => (TKey)(object)s;
				if (keyType.IsEnum)
					return s => (TKey)Enum.Parse(keyType, s, ignoreCase: true);
				if (typeof(IConvertible).IsAssignableFrom(keyType))
					return s => (TKey)Convert.ChangeType(s, keyType);

				// OSC identity types (IndexName, Id, Field, PropertyName, RelationName, TaskId, …) are not
				// IConvertible but construct from a string via an implicit operator or a string constructor.
				var implicitOp = keyType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null,
					new[] { typeof(string) }, null);
				if (implicitOp != null)
					return s => (TKey)implicitOp.Invoke(null, new object[] { s });

				var ctor = keyType.GetConstructor(new[] { typeof(string) });
				if (ctor != null)
					return s => (TKey)ctor.Invoke(new object[] { s });

				return s => (TKey)Convert.ChangeType(s, keyType);
			}

			private static string KeyToPropertyName(TKey key) => key as string ?? key?.ToString();

			private static object Materialize(Dictionary<TKey, TValue> buffer)
			{
				var targetType = typeof(TDictionary);

				// Concrete ReadOnlyDictionary<,>.
				if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>))
					return new ReadOnlyDictionary<TKey, TValue>(buffer);

				// An interface target — the buffered Dictionary<,> is itself a valid instance.
				if (targetType.IsInterface)
					return buffer;

				// A custom type with a public constructor taking IDictionary<,> or IEnumerable<KeyValuePair<,>>
				// (e.g. a read-only wrapper). Prefer it over a parameterless constructor.
				foreach (var constructor in targetType.GetConstructors())
				{
					var parameters = constructor.GetParameters();
					if (parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(typeof(Dictionary<TKey, TValue>)))
						return constructor.Invoke(new object[] { buffer });
				}

				// A parameterless-constructible type populated via its Add/indexer.
				if (targetType.GetConstructor(Type.EmptyTypes) != null)
				{
					var instance = Activator.CreateInstance(targetType);
					switch (instance)
					{
						case IDictionary<TKey, TValue> generic:
							foreach (var entry in buffer) generic[entry.Key] = entry.Value;
							return instance;
						case IDictionary nonGeneric:
							foreach (var entry in buffer) nonGeneric[entry.Key] = entry.Value;
							return instance;
					}
				}

				throw new JsonException(
					$"Cannot deserialize '{targetType}': it is not a ReadOnlyDictionary<,>, an interface, and has "
					+ "no constructor taking an IDictionary<,>/IEnumerable<KeyValuePair<,>> nor a parameterless constructor.");
			}
		}
	}
}
