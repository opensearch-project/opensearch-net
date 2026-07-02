/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.AnalysisConverters
{
	/// <summary>
	/// Polymorphic converter for <see cref="ITokenFilter"/>.
	/// Reads the "type" discriminator to dispatch to the correct concrete token filter type.
	/// </summary>
	internal sealed class TokenFilterConverter : JsonConverter<ITokenFilter>
	{
		public override ITokenFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for ITokenFilter but got {reader.TokenType}");

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var typeString = root.TryGetProperty("type", out var typeProp)
				? typeProp.GetString()
				: null;

			if (typeString == null)
				return null;

			var rawJson = root.GetRawText();

			switch (typeString)
			{
				case "asciifolding":
					return JsonSerializer.Deserialize<AsciiFoldingTokenFilter>(rawJson, options);
				case "common_grams":
					return JsonSerializer.Deserialize<CommonGramsTokenFilter>(rawJson, options);
				case "delimited_payload":
				case "delimited_payload_filter":
					return JsonSerializer.Deserialize<DelimitedPayloadTokenFilter>(rawJson, options);
				case "dictionary_decompounder":
					return JsonSerializer.Deserialize<DictionaryDecompounderTokenFilter>(rawJson, options);
				case "edge_ngram":
					return JsonSerializer.Deserialize<EdgeNGramTokenFilter>(rawJson, options);
				case "elision":
					return JsonSerializer.Deserialize<ElisionTokenFilter>(rawJson, options);
				case "hunspell":
					return JsonSerializer.Deserialize<HunspellTokenFilter>(rawJson, options);
				case "hyphenation_decompounder":
					return JsonSerializer.Deserialize<HyphenationDecompounderTokenFilter>(rawJson, options);
				case "keep_types":
					return JsonSerializer.Deserialize<KeepTypesTokenFilter>(rawJson, options);
				case "keep":
					return JsonSerializer.Deserialize<KeepWordsTokenFilter>(rawJson, options);
				case "keyword_marker":
					return JsonSerializer.Deserialize<KeywordMarkerTokenFilter>(rawJson, options);
				case "kstem":
					return JsonSerializer.Deserialize<KStemTokenFilter>(rawJson, options);
				case "length":
					return JsonSerializer.Deserialize<LengthTokenFilter>(rawJson, options);
				case "limit":
					return JsonSerializer.Deserialize<LimitTokenCountTokenFilter>(rawJson, options);
				case "lowercase":
					return JsonSerializer.Deserialize<LowercaseTokenFilter>(rawJson, options);
				case "ngram":
					return JsonSerializer.Deserialize<NgramTokenFilter>(rawJson, options);
				case "pattern_capture":
					return JsonSerializer.Deserialize<PatternCaptureTokenFilter>(rawJson, options);
				case "pattern_replace":
					return JsonSerializer.Deserialize<PatternReplaceTokenFilter>(rawJson, options);
				case "porter_stem":
					return JsonSerializer.Deserialize<PorterStemTokenFilter>(rawJson, options);
				case "reverse":
					return JsonSerializer.Deserialize<ReverseTokenFilter>(rawJson, options);
				case "shingle":
					return JsonSerializer.Deserialize<ShingleTokenFilter>(rawJson, options);
				case "snowball":
					return JsonSerializer.Deserialize<SnowballTokenFilter>(rawJson, options);
				case "stemmer":
					return JsonSerializer.Deserialize<StemmerTokenFilter>(rawJson, options);
				case "stemmer_override":
					return JsonSerializer.Deserialize<StemmerOverrideTokenFilter>(rawJson, options);
				case "stop":
					return JsonSerializer.Deserialize<StopTokenFilter>(rawJson, options);
				case "synonym":
					return JsonSerializer.Deserialize<SynonymTokenFilter>(rawJson, options);
				case "synonym_graph":
					return JsonSerializer.Deserialize<SynonymGraphTokenFilter>(rawJson, options);
				case "trim":
					return JsonSerializer.Deserialize<TrimTokenFilter>(rawJson, options);
				case "truncate":
					return JsonSerializer.Deserialize<TruncateTokenFilter>(rawJson, options);
				case "unique":
					return JsonSerializer.Deserialize<UniqueTokenFilter>(rawJson, options);
				case "uppercase":
					return JsonSerializer.Deserialize<UppercaseTokenFilter>(rawJson, options);
				case "word_delimiter":
					return JsonSerializer.Deserialize<WordDelimiterTokenFilter>(rawJson, options);
				case "word_delimiter_graph":
					return JsonSerializer.Deserialize<WordDelimiterGraphTokenFilter>(rawJson, options);
				case "fingerprint":
					return JsonSerializer.Deserialize<FingerprintTokenFilter>(rawJson, options);
				case "nori_part_of_speech":
					return JsonSerializer.Deserialize<NoriPartOfSpeechTokenFilter>(rawJson, options);
				case "kuromoji_readingform":
					return JsonSerializer.Deserialize<KuromojiReadingFormTokenFilter>(rawJson, options);
				case "kuromoji_part_of_speech":
					return JsonSerializer.Deserialize<KuromojiPartOfSpeechTokenFilter>(rawJson, options);
				case "kuromoji_stemmer":
					return JsonSerializer.Deserialize<KuromojiStemmerTokenFilter>(rawJson, options);
				case "icu_collation":
					return JsonSerializer.Deserialize<IcuCollationTokenFilter>(rawJson, options);
				case "icu_folding":
					return JsonSerializer.Deserialize<IcuFoldingTokenFilter>(rawJson, options);
				case "icu_normalizer":
					return JsonSerializer.Deserialize<IcuNormalizationTokenFilter>(rawJson, options);
				case "icu_transform":
					return JsonSerializer.Deserialize<IcuTransformTokenFilter>(rawJson, options);
				case "condition":
					return JsonSerializer.Deserialize<ConditionTokenFilter>(rawJson, options);
				case "multiplexer":
					return JsonSerializer.Deserialize<MultiplexerTokenFilter>(rawJson, options);
				case "predicate_token_filter":
					return JsonSerializer.Deserialize<PredicateTokenFilter>(rawJson, options);
				case "remove_duplicates":
					return JsonSerializer.Deserialize<RemoveDuplicatesTokenFilter>(rawJson, options);
				case "phonetic":
					return JsonSerializer.Deserialize<PhoneticTokenFilter>(rawJson, options);
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

			switch (value)
			{
				case AsciiFoldingTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case CommonGramsTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case DelimitedPayloadTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case DictionaryDecompounderTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case EdgeNGramTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case ElisionTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case HunspellTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case HyphenationDecompounderTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KeepTypesTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KeepWordsTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KeywordMarkerTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KStemTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case LengthTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case LimitTokenCountTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case LowercaseTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case NgramTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PatternCaptureTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PatternReplaceTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PorterStemTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case ReverseTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case ShingleTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case SnowballTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case StemmerTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case StemmerOverrideTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case StopTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case SynonymTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case SynonymGraphTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case TrimTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case TruncateTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case UniqueTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case UppercaseTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case WordDelimiterTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case WordDelimiterGraphTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case FingerprintTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case NoriPartOfSpeechTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KuromojiReadingFormTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KuromojiPartOfSpeechTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KuromojiStemmerTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IcuCollationTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IcuFoldingTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IcuNormalizationTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IcuTransformTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case ConditionTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case MultiplexerTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PredicateTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case RemoveDuplicatesTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PhoneticTokenFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}
	}
}
