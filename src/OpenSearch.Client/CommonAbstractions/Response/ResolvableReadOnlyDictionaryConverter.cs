/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>ResolvableReadOnlyDictionaryFormatter{TKey,TValue}</c>
	/// used on members such as <c>TermVectorsResponse.TermVectors</c>, <c>ClusterHealthResponse.Indices</c> and
	/// <c>TypeFieldMappings.Mappings</c>. The value is a JSON object keyed by an inferred key type (<see cref="Field"/>,
	/// <see cref="IndexName"/>, …); each key is materialised through the type's implicit string conversion and the whole
	/// map is wrapped in a <see cref="ResolvableDictionaryProxy{TKey,TValue}"/> so lookups resolve through the runtime
	/// inferrer (matching the legacy formatter, which built the same proxy from the connection settings).
	/// </summary>
	internal class ResolvableReadOnlyDictionaryConverter<TKey, TValue>
		: SettingsAwareConverter<IReadOnlyDictionary<TKey, TValue>>
		where TKey : IUrlParameter
	{
		public ResolvableReadOnlyDictionaryConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IReadOnlyDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return new ResolvableDictionaryProxy<TKey, TValue>(Settings, null);

			if (reader.TokenType != JsonTokenType.StartObject)
			{
				reader.Skip();
				return new ResolvableDictionaryProxy<TKey, TValue>(Settings, null);
			}

			var dictionary = new Dictionary<TKey, TValue>();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var keyString = reader.GetString();
				reader.Read();
				var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
				dictionary[ConvertKey(keyString)] = value;
			}

			return new ResolvableDictionaryProxy<TKey, TValue>(Settings, dictionary);
		}

		public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<TKey, TValue> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			foreach (var kv in value)
			{
				writer.WritePropertyName(kv.Key?.GetString(Settings) ?? string.Empty);
				JsonSerializer.Serialize(writer, kv.Value, options);
			}
			writer.WriteEndObject();
		}

		// The inferred key types wrap a string but are NOT IConvertible; use their implicit string conversions
		// (the legacy formatter relied on the same conversions when deserializing the backing Dictionary<TKey,TValue>).
		private static TKey ConvertKey(string key)
		{
			if (typeof(TKey) == typeof(Field))
				return (TKey)(object)(Field)key;
			if (typeof(TKey) == typeof(IndexName))
				return (TKey)(object)(IndexName)key;
			if (typeof(TKey) == typeof(PropertyName))
				return (TKey)(object)(PropertyName)key;
			if (typeof(TKey) == typeof(RelationName))
				return (TKey)(object)(RelationName)key;

			return (TKey)(object)key;
		}
	}
}
