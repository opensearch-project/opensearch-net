/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>VerbatimDictionaryKeysFormatter</c>.
	/// Serializes an <see cref="IIsADictionary{TKey,TValue}"/> as a JSON object writing the keys "verbatim"
	/// and deserializes a JSON object back into the concrete <typeparamref name="TDictionary"/> implementation.
	///
	/// Keys of type <c>Field</c>, <c>PropertyName</c>, <c>IndexName</c> and <c>RelationName</c> are resolved through
	/// the connection-settings <c>Inferrer</c> (matching the legacy formatter); a <c>string</c> key is written as-is,
	/// and when no settings are available (or for any other key type) the converter falls back to
	/// <see cref="Convert.ToString(object,IFormatProvider)"/> with <see cref="CultureInfo.InvariantCulture"/> —
	/// identical to the base formatter's <c>settings == null</c> branch.
	/// </summary>
	internal class VerbatimDictionaryKeysConverter<TDictionary, TInterface, TKey, TValue> : JsonConverter<TInterface>
		where TDictionary : TInterface, IIsADictionary<TKey, TValue>
		where TInterface : IIsADictionary<TKey, TValue>
	{
		private readonly IConnectionSettingsValues _settings;

		public VerbatimDictionaryKeysConverter() { }

		public VerbatimDictionaryKeysConverter(IConnectionSettingsValues settings) => _settings = settings;

		/// <summary>When <c>false</c>, entries with a <c>null</c> value are still written (matches the PreservingNull formatter).</summary>
		protected virtual bool SkipNullValues => true;

		public override TInterface Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return typeof(TDictionary).CreateInstance<TDictionary>(new Dictionary<TKey, TValue>());

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject but found {reader.TokenType} when deserializing {typeof(TDictionary).Name}.");

			var dictionary = new Dictionary<TKey, TValue>();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var keyString = reader.GetString();
				reader.Read();
				var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
				dictionary[ConvertKey(keyString)] = value;
			}

			return typeof(TDictionary).CreateInstance<TDictionary>(dictionary);
		}

		public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
		{
			var enumerable = (IEnumerable<KeyValuePair<TKey, TValue>>)value;
			if (enumerable == null)
			{
				writer.WriteNullValue();
				return;
			}

			// De-duplicate on the string key, preserving last-writer-wins, matching the base formatter.
			var seenEntries = new Dictionary<string, TValue>();
			foreach (var entry in enumerable)
			{
				if (SkipNullValues && entry.Value == null)
					continue;

				var key = KeyToString(entry.Key);
				if (key != null)
					seenEntries[key] = entry.Value;
			}

			writer.WriteStartObject();
			foreach (var entry in seenEntries)
			{
				writer.WritePropertyName(entry.Key);
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}

		private string KeyToString(TKey key)
		{
			if (typeof(TKey) == typeof(string))
				return key?.ToString();

			// Resolve inferred key types through the runtime Inferrer, matching the legacy formatter. For PropertyName
			// keys mapped to an ignored property, the legacy formatter skipped the entry; that filtering is handled by
			// returning null here (the caller skips null keys).
			if (_settings != null)
			{
				switch (key)
				{
					case Field field:
						return _settings.Inferrer.Field(field);
					case PropertyName propertyName:
						if (propertyName.Property != null
							&& _settings.PropertyMappings.TryGetValue(propertyName.Property, out var mapping)
							&& mapping.Ignore)
							return null;
						return _settings.Inferrer.PropertyName(propertyName);
					case IndexName indexName:
						return _settings.Inferrer.IndexName(indexName);
					case RelationName relationName:
						return _settings.Inferrer.RelationName(relationName);
				}
			}

			return Convert.ToString(key, CultureInfo.InvariantCulture);
		}

		private static TKey ConvertKey(string key)
		{
			if (typeof(TKey) == typeof(string))
				return (TKey)(object)key;

			// The inferred key types wrap a string but are NOT IConvertible, so Convert.ChangeType throws. Use their
			// implicit string conversions (the legacy formatter relied on the same implicit conversions on read).
			if (typeof(TKey) == typeof(Field))
				return (TKey)(object)(Field)key;
			if (typeof(TKey) == typeof(PropertyName))
				return (TKey)(object)(PropertyName)key;
			if (typeof(TKey) == typeof(IndexName))
				return (TKey)(object)(IndexName)key;
			if (typeof(TKey) == typeof(RelationName))
				return (TKey)(object)(RelationName)key;

			return (TKey)Convert.ChangeType(key, typeof(TKey), CultureInfo.InvariantCulture);
		}
	}

	/// <summary>
	/// A <see cref="VerbatimDictionaryKeysConverter{TDictionary,TInterface,TKey,TValue}"/> variant that also writes
	/// entries whose value is <c>null</c> (System.Text.Json replacement for the legacy
	/// <c>VerbatimDictionaryKeysPreservingNullFormatter</c>).
	/// </summary>
	internal class VerbatimDictionaryKeysPreservingNullConverter<TDictionary, TInterface, TKey, TValue>
		: VerbatimDictionaryKeysConverter<TDictionary, TInterface, TKey, TValue>
		where TDictionary : TInterface, IIsADictionary<TKey, TValue>
		where TInterface : IIsADictionary<TKey, TValue>
	{
		public VerbatimDictionaryKeysPreservingNullConverter() { }

		public VerbatimDictionaryKeysPreservingNullConverter(IConnectionSettingsValues settings) : base(settings) { }

		protected override bool SkipNullValues => false;
	}
}
