/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// Shared write/read helpers for the verbatim dictionary-key converters (#388). "Verbatim" means
	/// the dictionary keys are written exactly as inferred (no camel-casing). Key inference is
	/// delegated to the registered key converter (<c>Field</c>/<c>IndexName</c>/<c>RelationName</c>/…)
	/// by serializing the key through the options and taking the resulting JSON string, so no
	/// connection-settings plumbing is needed here.
	/// </summary>
	internal static class VerbatimDictionaryKeys
	{
		public static void Write<TKey, TValue>(Utf8JsonWriter writer, IEnumerable<KeyValuePair<TKey, TValue>> value,
			JsonSerializerOptions options, bool skipNull)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			// Mirror the original formatter: dedupe on the inferred key (last value wins, first
			// position kept) via a Dictionary, then write in insertion order.
			var seen = new Dictionary<string, TValue>();
			foreach (var entry in value)
			{
				if (skipNull && entry.Value == null) continue;
				var key = InferKey(entry.Key, options);
				if (key != null) seen[key] = entry.Value;
			}

			writer.WriteStartObject();
			foreach (var entry in seen)
			{
				writer.WritePropertyName(entry.Key);
				JsonSerializer.Serialize(writer, entry.Value, options);
			}
			writer.WriteEndObject();
		}

		public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			if (document.RootElement.ValueKind != JsonValueKind.Object) return null;

			var dictionary = new Dictionary<TKey, TValue>();
			foreach (var member in document.RootElement.EnumerateObject())
			{
				var key = ConvertKey<TKey>(member.Name, options);
				dictionary[key] = DeserializeValue<TValue>(member.Value, options);
			}
			return dictionary;
		}

		private static TValue DeserializeValue<TValue>(JsonElement element, JsonSerializerOptions options)
		{
			// IQueryContainer (named-filters value) has no [ReadAs] and deliberately no interface converter
			// (an IQueryContainer converter would recurse with QueryContainerConverter's interface write),
			// so deserialize it as the concrete QueryContainer, which carries the converter.
			if (typeof(TValue) == typeof(IQueryContainer))
				return (TValue)(object)element.Deserialize<QueryContainer>(options);

			return element.Deserialize<TValue>(options);
		}

		private static string InferKey<TKey>(TKey key, JsonSerializerOptions options)
		{
			if (key == null) return null;
			if (key is string s) return s;

			var json = JsonSerializer.Serialize(key, options);
			using var document = JsonDocument.Parse(json);
			return document.RootElement.ValueKind == JsonValueKind.String ? document.RootElement.GetString() : json;
		}

		private static TKey ConvertKey<TKey>(string name, JsonSerializerOptions options)
		{
			if (typeof(TKey) == typeof(string)) return (TKey)(object)name;
			// Round-trip the property name through the registered key converter (Field/IndexName/…).
			return JsonSerializer.Deserialize<TKey>(JsonSerializer.Serialize(name), options);
		}
	}

	/// <summary>
	/// Verbatim-keys converter for an <see cref="IIsADictionary{TKey,TValue}"/> concrete type exposed
	/// through its interface (#388), e.g. <c>Analyzers</c>/<c>IAnalyzers</c>. Reads into a
	/// <see cref="Dictionary{TKey,TValue}"/> and reconstructs the concrete dictionary.
	/// </summary>
	internal sealed class VerbatimDictionaryKeysConverter<TDictionary, TInterface, TKey, TValue> : JsonConverter<TInterface>
		where TDictionary : TInterface, IIsADictionary<TKey, TValue>
		where TInterface : IIsADictionary<TKey, TValue>
	{
		public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.Write(writer, (IEnumerable<KeyValuePair<TKey, TValue>>)value, options, skipNull: true);

		public override TInterface Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
		{
			var dictionary = VerbatimDictionaryKeys.ReadDictionary<TKey, TValue>(ref reader, options);
			return dictionary == null ? default : typeof(TDictionary).CreateInstance<TDictionary>(dictionary);
		}
	}

	/// <summary> Verbatim-keys converter for an <see cref="IDictionary{TKey,TValue}"/> member (#388). </summary>
	internal sealed class VerbatimDictionaryInterfaceKeysConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>>
	{
		public override void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue> value, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.Write(writer, value, options, skipNull: true);

		public override IDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.ReadDictionary<TKey, TValue>(ref reader, options);
	}

	/// <summary> Verbatim-keys converter for an <see cref="IReadOnlyDictionary{TKey,TValue}"/> member (#388). </summary>
	internal sealed class VerbatimInterfaceReadOnlyDictionaryKeysConverter<TKey, TValue> : JsonConverter<IReadOnlyDictionary<TKey, TValue>>
	{
		public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<TKey, TValue> value, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.Write(writer, value, options, skipNull: true);

		public override IReadOnlyDictionary<TKey, TValue> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.ReadDictionary<TKey, TValue>(ref reader, options);
	}

	/// <summary> Verbatim-keys converter for a <see cref="Dictionary{TKey,TValue}"/> member (#388). </summary>
	internal sealed class VerbatimDictionaryKeysConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
	{
		public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.Write(writer, value, options, skipNull: true);

		public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.ReadDictionary<TKey, TValue>(ref reader, options);
	}

	/// <summary> As <see cref="VerbatimDictionaryKeysConverter{TKey,TValue}"/> but keeps null values (#388). </summary>
	internal sealed class VerbatimDictionaryKeysPreservingNullConverter<TKey, TValue> : JsonConverter<Dictionary<TKey, TValue>>
	{
		public override void Write(Utf8JsonWriter writer, Dictionary<TKey, TValue> value, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.Write(writer, value, options, skipNull: false);

		public override Dictionary<TKey, TValue> Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.ReadDictionary<TKey, TValue>(ref reader, options);
	}

	/// <summary>
	/// Verbatim-keys converter for the base formatter used directly on a concrete
	/// <see cref="Dictionary{TKey,TValue}"/>-derived member (#388).
	/// </summary>
	internal sealed class VerbatimDictionaryKeysBaseConverter<TDictionary, TKey, TValue> : JsonConverter<TDictionary>
		where TDictionary : IEnumerable<KeyValuePair<TKey, TValue>>
	{
		public override void Write(Utf8JsonWriter writer, TDictionary value, JsonSerializerOptions options) =>
			VerbatimDictionaryKeys.Write(writer, value, options, skipNull: true);

		public override TDictionary Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
		{
			var dictionary = VerbatimDictionaryKeys.ReadDictionary<TKey, TValue>(ref reader, options);
			if (dictionary == null) return default;
			return dictionary is TDictionary typed ? typed : typeof(TDictionary).CreateInstance<TDictionary>(dictionary);
		}
	}
}
