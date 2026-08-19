/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="TokenFilterConverter"/>: dispatches an <see cref="ITokenFilter"/> to the
	/// concrete type named by the <c>type</c> discriminator field (including the <c>delimited_payload_filter</c>
	/// alias), and serializes by runtime type.
	/// </summary>
	public class TokenFilterConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new TokenFilterConverter());
			return options;
		}

		private static ITokenFilter Deserialize(string type) =>
			JsonSerializer.Deserialize<ITokenFilter>($@"{{""type"":""{type}""}}", Options());

		[U] public void Deserialize_AsciiFolding() => Deserialize("asciifolding").Should().BeOfType<AsciiFoldingTokenFilter>();
		[U] public void Deserialize_CommonGrams() => Deserialize("common_grams").Should().BeOfType<CommonGramsTokenFilter>();
		[U] public void Deserialize_DelimitedPayload() => Deserialize("delimited_payload").Should().BeOfType<DelimitedPayloadTokenFilter>();

		[U] public void Deserialize_DelimitedPayload_ByAlias()
		{
			// The legacy formatter mapped both "delimited_payload" and "delimited_payload_filter" to the same type.
			Deserialize("delimited_payload_filter").Should().BeOfType<DelimitedPayloadTokenFilter>();
		}

		[U] public void Deserialize_DictionaryDecompounder() => Deserialize("dictionary_decompounder").Should().BeOfType<DictionaryDecompounderTokenFilter>();
		[U] public void Deserialize_EdgeNGram() => Deserialize("edge_ngram").Should().BeOfType<EdgeNGramTokenFilter>();
		[U] public void Deserialize_Elision() => Deserialize("elision").Should().BeOfType<ElisionTokenFilter>();
		[U] public void Deserialize_Hunspell() => Deserialize("hunspell").Should().BeOfType<HunspellTokenFilter>();
		[U] public void Deserialize_HyphenationDecompounder() => Deserialize("hyphenation_decompounder").Should().BeOfType<HyphenationDecompounderTokenFilter>();
		[U] public void Deserialize_KeepTypes() => Deserialize("keep_types").Should().BeOfType<KeepTypesTokenFilter>();
		[U] public void Deserialize_Keep() => Deserialize("keep").Should().BeOfType<KeepWordsTokenFilter>();
		[U] public void Deserialize_KeywordMarker() => Deserialize("keyword_marker").Should().BeOfType<KeywordMarkerTokenFilter>();
		[U] public void Deserialize_KStem() => Deserialize("kstem").Should().BeOfType<KStemTokenFilter>();
		[U] public void Deserialize_Length() => Deserialize("length").Should().BeOfType<LengthTokenFilter>();
		[U] public void Deserialize_Limit() => Deserialize("limit").Should().BeOfType<LimitTokenCountTokenFilter>();
		[U] public void Deserialize_Lowercase() => Deserialize("lowercase").Should().BeOfType<LowercaseTokenFilter>();
		[U] public void Deserialize_NGram() => Deserialize("ngram").Should().BeOfType<NGramTokenFilter>();
		[U] public void Deserialize_PatternCapture() => Deserialize("pattern_capture").Should().BeOfType<PatternCaptureTokenFilter>();
		[U] public void Deserialize_PatternReplace() => Deserialize("pattern_replace").Should().BeOfType<PatternReplaceTokenFilter>();
		[U] public void Deserialize_PorterStem() => Deserialize("porter_stem").Should().BeOfType<PorterStemTokenFilter>();
		[U] public void Deserialize_Phonetic() => Deserialize("phonetic").Should().BeOfType<PhoneticTokenFilter>();
		[U] public void Deserialize_Reverse() => Deserialize("reverse").Should().BeOfType<ReverseTokenFilter>();
		[U] public void Deserialize_Shingle() => Deserialize("shingle").Should().BeOfType<ShingleTokenFilter>();
		[U] public void Deserialize_Snowball() => Deserialize("snowball").Should().BeOfType<SnowballTokenFilter>();
		[U] public void Deserialize_Stemmer() => Deserialize("stemmer").Should().BeOfType<StemmerTokenFilter>();
		[U] public void Deserialize_StemmerOverride() => Deserialize("stemmer_override").Should().BeOfType<StemmerOverrideTokenFilter>();
		[U] public void Deserialize_Stop() => Deserialize("stop").Should().BeOfType<StopTokenFilter>();
		[U] public void Deserialize_Synonym() => Deserialize("synonym").Should().BeOfType<SynonymTokenFilter>();
		[U] public void Deserialize_SynonymGraph() => Deserialize("synonym_graph").Should().BeOfType<SynonymGraphTokenFilter>();
		[U] public void Deserialize_Trim() => Deserialize("trim").Should().BeOfType<TrimTokenFilter>();
		[U] public void Deserialize_Truncate() => Deserialize("truncate").Should().BeOfType<TruncateTokenFilter>();
		[U] public void Deserialize_Unique() => Deserialize("unique").Should().BeOfType<UniqueTokenFilter>();
		[U] public void Deserialize_Uppercase() => Deserialize("uppercase").Should().BeOfType<UppercaseTokenFilter>();
		[U] public void Deserialize_WordDelimiter() => Deserialize("word_delimiter").Should().BeOfType<WordDelimiterTokenFilter>();
		[U] public void Deserialize_WordDelimiterGraph() => Deserialize("word_delimiter_graph").Should().BeOfType<WordDelimiterGraphTokenFilter>();
		[U] public void Deserialize_Fingerprint() => Deserialize("fingerprint").Should().BeOfType<FingerprintTokenFilter>();
		[U] public void Deserialize_NoriPartOfSpeech() => Deserialize("nori_part_of_speech").Should().BeOfType<NoriPartOfSpeechTokenFilter>();
		[U] public void Deserialize_KuromojiReadingForm() => Deserialize("kuromoji_readingform").Should().BeOfType<KuromojiReadingFormTokenFilter>();
		[U] public void Deserialize_KuromojiPartOfSpeech() => Deserialize("kuromoji_part_of_speech").Should().BeOfType<KuromojiPartOfSpeechTokenFilter>();
		[U] public void Deserialize_KuromojiStemmer() => Deserialize("kuromoji_stemmer").Should().BeOfType<KuromojiStemmerTokenFilter>();
		[U] public void Deserialize_IcuCollation() => Deserialize("icu_collation").Should().BeOfType<IcuCollationTokenFilter>();
		[U] public void Deserialize_IcuFolding() => Deserialize("icu_folding").Should().BeOfType<IcuFoldingTokenFilter>();
		[U] public void Deserialize_IcuNormalizer() => Deserialize("icu_normalizer").Should().BeOfType<IcuNormalizationTokenFilter>();
		[U] public void Deserialize_IcuTransform() => Deserialize("icu_transform").Should().BeOfType<IcuTransformTokenFilter>();
		[U] public void Deserialize_Condition() => Deserialize("condition").Should().BeOfType<ConditionTokenFilter>();
		[U] public void Deserialize_Multiplexer() => Deserialize("multiplexer").Should().BeOfType<MultiplexerTokenFilter>();
		[U] public void Deserialize_PredicateTokenFilter() => Deserialize("predicate_token_filter").Should().BeOfType<PredicateTokenFilter>();

		[U] public void Deserialize_UnknownType_ReturnsNull()
		{
			Deserialize("does_not_exist").Should().BeNull();
		}

		[U] public void Deserialize_MissingType_ReturnsNull()
		{
			var filter = JsonSerializer.Deserialize<ITokenFilter>(@"{""foo"":""bar""}", Options());
			filter.Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			var filter = JsonSerializer.Deserialize<ITokenFilter>("null", Options());
			filter.Should().BeNull();
		}

		[U] public void Serialize_ByRuntimeType()
		{
			ITokenFilter filter = new ReverseTokenFilter();

			var json = JsonSerializer.Serialize(filter, Options());

			json.Should().Contain(@"""type"":""reverse""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<ITokenFilter>(null, Options()).Should().Be("null");
		}
	}
}
