/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json
	/// <c>VerbatimDictionaryKeysFormatter&lt;PerFieldAnalyzer, IPerFieldAnalyzer, Field, string&gt;</c>. Reads/writes a
	/// JSON object mapping a field to its analyzer name.
	///
	/// This does NOT reuse the shared <see cref="VerbatimDictionaryKeysConverter{TDictionary,TInterface,TKey,TValue}"/>
	/// because that converter is deliberately settings-independent: it resolves non-<c>string</c> keys with
	/// <c>Convert.ToString</c>/<c>Convert.ChangeType</c>, which for a <see cref="Field"/> key would serialize the field's
	/// debug display on write and throw on read. <c>PerFieldAnalyzer</c>'s keys are <see cref="Field"/>s that MUST be
	/// resolved through the runtime <c>Inferrer</c> (exactly as the legacy base formatter did in its <c>_keyIsField</c>
	/// branch), so this is a <see cref="SettingsAwareConverter{T}"/> with a small concrete body rather than duplicated
	/// generic logic in the shared file.
	/// </summary>
	internal class PerFieldAnalyzerConverter : SettingsAwareConverter<IPerFieldAnalyzer>
	{
		public PerFieldAnalyzerConverter(IConnectionSettingsValues settings) : base(settings) { }

		// A JSON null yields an empty instance (matching the legacy formatter). STJ skips the converter for a null
		// reference type unless HandleNull is true.
		public override bool HandleNull => true;

		public override IPerFieldAnalyzer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return new PerFieldAnalyzer();

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject but found {reader.TokenType} when deserializing {nameof(PerFieldAnalyzer)}.");

			var analyzer = new PerFieldAnalyzer();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected property name.");

				var key = reader.GetString();
				reader.Read();
				var value = JsonSerializer.Deserialize<string>(ref reader, options);
				analyzer[key] = value;
			}

			return analyzer;
		}

		public override void Write(Utf8JsonWriter writer, IPerFieldAnalyzer value, JsonSerializerOptions options)
		{
			var enumerable = value as IEnumerable<KeyValuePair<Field, string>>;
			if (enumerable == null)
			{
				writer.WriteNullValue();
				return;
			}

			// Resolve Field keys through the Inferrer and de-duplicate on the resolved string key
			// (last-writer-wins), matching the legacy VerbatimDictionaryKeysBaseFormatter _keyIsField branch.
			var seenEntries = new Dictionary<string, string>();
			foreach (var entry in enumerable)
			{
				if (entry.Value == null) // legacy default SkipValue skips null values
					continue;

				var key = Settings.Inferrer.Field(entry.Key);
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
	}
}
