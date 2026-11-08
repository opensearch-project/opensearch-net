/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>AnalyzerFormatter</c>.
	///
	/// An <see cref="IAnalyzer"/> is polymorphic: the concrete type is chosen by the JSON <c>type</c> field (with a
	/// fallback that inspects whether a <c>tokenizer</c> field is present to distinguish a custom analyzer from a
	/// language analyzer). Because <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound — unlike the
	/// Utf8Json version which re-read a byte segment — we buffer the value into a <see cref="JsonDocument"/>, read the
	/// discriminator from the DOM, and then deserialize the whole element as the resolved concrete type.
	/// </summary>
	internal class AnalyzerConverter : JsonConverter<IAnalyzer>
	{
		public override IAnalyzer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var analyzerType = root.TryGetProperty("type", out var t) ? t.GetString() : null;
			var tokenizerPresent = root.TryGetProperty("tokenizer", out _);

			switch (analyzerType)
			{
				case "stop": return root.Deserialize<StopAnalyzer>(options);
				case "standard": return root.Deserialize<StandardAnalyzer>(options);
				case "snowball": return root.Deserialize<SnowballAnalyzer>(options);
				case "pattern": return root.Deserialize<PatternAnalyzer>(options);
				case "keyword": return root.Deserialize<KeywordAnalyzer>(options);
				case "whitespace": return root.Deserialize<WhitespaceAnalyzer>(options);
				case "simple": return root.Deserialize<SimpleAnalyzer>(options);
				case "fingerprint": return root.Deserialize<FingerprintAnalyzer>(options);
				case "kuromoji": return root.Deserialize<KuromojiAnalyzer>(options);
				case "nori": return root.Deserialize<NoriAnalyzer>(options);
				case "icu_analyzer": return root.Deserialize<IcuAnalyzer>(options);
				default:
					return tokenizerPresent
						? (IAnalyzer)root.Deserialize<CustomAnalyzer>(options)
						: root.Deserialize<LanguageAnalyzer>(options);
			}
		}

		public override void Write(Utf8JsonWriter writer, IAnalyzer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Type)
			{
				case "stop": Serialize<IStopAnalyzer>(writer, value, options); break;
				case "standard": Serialize<IStandardAnalyzer>(writer, value, options); break;
				case "snowball": Serialize<ISnowballAnalyzer>(writer, value, options); break;
				case "pattern": Serialize<IPatternAnalyzer>(writer, value, options); break;
				case "keyword": Serialize<IKeywordAnalyzer>(writer, value, options); break;
				case "whitespace": Serialize<IWhitespaceAnalyzer>(writer, value, options); break;
				case "simple": Serialize<ISimpleAnalyzer>(writer, value, options); break;
				case "fingerprint": Serialize<IFingerprintAnalyzer>(writer, value, options); break;
				case "kuromoji": Serialize<IKuromojiAnalyzer>(writer, value, options); break;
				case "nori": Serialize<INoriAnalyzer>(writer, value, options); break;
				case "icu_analyzer": Serialize<IIcuAnalyzer>(writer, value, options); break;
				case "custom": Serialize<ICustomAnalyzer>(writer, value, options); break;
				default: Serialize<ILanguageAnalyzer>(writer, value, options); break;
			}
		}

		private static void Serialize<TAnalyzer>(Utf8JsonWriter writer, IAnalyzer value, JsonSerializerOptions options)
			where TAnalyzer : class, IAnalyzer =>
			JsonSerializer.Serialize(writer, value as TAnalyzer, options);
	}
}
