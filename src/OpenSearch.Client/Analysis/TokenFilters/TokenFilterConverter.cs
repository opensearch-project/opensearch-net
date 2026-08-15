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
	/// System.Text.Json replacement for the legacy Utf8Json <c>TokenFilterFormatter</c>.
	///
	/// <see cref="ITokenFilter"/> is polymorphic: the concrete type is selected by the value of the
	/// <c>type</c> field (with an alias, e.g. <c>delimited_payload_filter</c> == <c>delimited_payload</c>).
	/// System.Text.Json's <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound, so — unlike the
	/// Utf8Json version which peeked at a byte segment and re-read it — we buffer the value into a
	/// <see cref="JsonDocument"/>, read the <c>type</c> discriminator from the DOM, then deserialize the whole
	/// element as the matching concrete type. Serialization writes by runtime type.
	/// </summary>
	internal class TokenFilterConverter : JsonConverter<ITokenFilter>
	{
		public override ITokenFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var tokenFilterType = root.TryGetProperty("type", out var t) ? t.GetString() : null;
			if (tokenFilterType == null)
				return null;

			switch (tokenFilterType)
			{
				case "asciifolding":
					return root.Deserialize<AsciiFoldingTokenFilter>(options);
				case "common_grams":
					return root.Deserialize<CommonGramsTokenFilter>(options);
				case "delimited_payload":
				case "delimited_payload_filter":
					return root.Deserialize<DelimitedPayloadTokenFilter>(options);
				case "dictionary_decompounder":
					return root.Deserialize<DictionaryDecompounderTokenFilter>(options);
				case "edge_ngram":
					return root.Deserialize<EdgeNGramTokenFilter>(options);
				case "elision":
					return root.Deserialize<ElisionTokenFilter>(options);
				case "hunspell":
					return root.Deserialize<HunspellTokenFilter>(options);
				case "hyphenation_decompounder":
					return root.Deserialize<HyphenationDecompounderTokenFilter>(options);
				case "keep_types":
					return root.Deserialize<KeepTypesTokenFilter>(options);
				case "keep":
					return root.Deserialize<KeepWordsTokenFilter>(options);
				case "keyword_marker":
					return root.Deserialize<KeywordMarkerTokenFilter>(options);
				case "kstem":
					return root.Deserialize<KStemTokenFilter>(options);
				case "length":
					return root.Deserialize<LengthTokenFilter>(options);
				case "limit":
					return root.Deserialize<LimitTokenCountTokenFilter>(options);
				case "lowercase":
					return root.Deserialize<LowercaseTokenFilter>(options);
				case "ngram":
					return root.Deserialize<NGramTokenFilter>(options);
				case "pattern_capture":
					return root.Deserialize<PatternCaptureTokenFilter>(options);
				case "pattern_replace":
					return root.Deserialize<PatternReplaceTokenFilter>(options);
				case "porter_stem":
					return root.Deserialize<PorterStemTokenFilter>(options);
				case "phonetic":
					return root.Deserialize<PhoneticTokenFilter>(options);
				case "reverse":
					return root.Deserialize<ReverseTokenFilter>(options);
				case "shingle":
					return root.Deserialize<ShingleTokenFilter>(options);
				case "snowball":
					return root.Deserialize<SnowballTokenFilter>(options);
				case "stemmer":
					return root.Deserialize<StemmerTokenFilter>(options);
				case "stemmer_override":
					return root.Deserialize<StemmerOverrideTokenFilter>(options);
				case "stop":
					return root.Deserialize<StopTokenFilter>(options);
				case "synonym":
					return root.Deserialize<SynonymTokenFilter>(options);
				case "synonym_graph":
					return root.Deserialize<SynonymGraphTokenFilter>(options);
				case "trim":
					return root.Deserialize<TrimTokenFilter>(options);
				case "truncate":
					return root.Deserialize<TruncateTokenFilter>(options);
				case "unique":
					return root.Deserialize<UniqueTokenFilter>(options);
				case "uppercase":
					return root.Deserialize<UppercaseTokenFilter>(options);
				case "word_delimiter":
					return root.Deserialize<WordDelimiterTokenFilter>(options);
				case "word_delimiter_graph":
					return root.Deserialize<WordDelimiterGraphTokenFilter>(options);
				case "fingerprint":
					return root.Deserialize<FingerprintTokenFilter>(options);
				case "nori_part_of_speech":
					return root.Deserialize<NoriPartOfSpeechTokenFilter>(options);
				case "kuromoji_readingform":
					return root.Deserialize<KuromojiReadingFormTokenFilter>(options);
				case "kuromoji_part_of_speech":
					return root.Deserialize<KuromojiPartOfSpeechTokenFilter>(options);
				case "kuromoji_stemmer":
					return root.Deserialize<KuromojiStemmerTokenFilter>(options);
				case "icu_collation":
					return root.Deserialize<IcuCollationTokenFilter>(options);
				case "icu_folding":
					return root.Deserialize<IcuFoldingTokenFilter>(options);
				case "icu_normalizer":
					return root.Deserialize<IcuNormalizationTokenFilter>(options);
				case "icu_transform":
					return root.Deserialize<IcuTransformTokenFilter>(options);
				case "condition":
					return root.Deserialize<ConditionTokenFilter>(options);
				case "multiplexer":
					return root.Deserialize<MultiplexerTokenFilter>(options);
				case "predicate_token_filter":
					return root.Deserialize<PredicateTokenFilter>(options);
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, ITokenFilter value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}
	}
}
