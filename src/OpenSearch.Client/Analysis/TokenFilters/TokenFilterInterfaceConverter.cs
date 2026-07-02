/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="ITokenFilter"/>
	/// hierarchy, replacing the vendored Utf8Json <c>TokenFilterFormatter</c> as part of #388.
	/// The dispatch table mirrors <c>TokenFilterFormatter</c>, including the
	/// <c>delimited_payload</c>/<c>delimited_payload_filter</c> aliases.
	/// <para>
	/// Note: <c>condition</c> and <c>predicate_token_filter</c> embed an <c>IScript</c>, so full
	/// wire parity for those two additionally requires the <c>script</c> namespace converter (a
	/// separate migration slice). The dispatch itself is registered here.
	/// </para>
	/// </summary>
	internal sealed class TokenFilterInterfaceConverter : PolymorphicInterfaceConverter<ITokenFilter>
	{
		public TokenFilterInterfaceConverter() : base(TypeByDiscriminator) { }

		private static readonly IReadOnlyDictionary<string, Type> TypeByDiscriminator = new Dictionary<string, Type>(StringComparer.Ordinal)
		{
			{ "asciifolding", typeof(AsciiFoldingTokenFilter) },
			{ "common_grams", typeof(CommonGramsTokenFilter) },
			{ "delimited_payload", typeof(DelimitedPayloadTokenFilter) },
			{ "delimited_payload_filter", typeof(DelimitedPayloadTokenFilter) },
			{ "dictionary_decompounder", typeof(DictionaryDecompounderTokenFilter) },
			{ "edge_ngram", typeof(EdgeNGramTokenFilter) },
			{ "elision", typeof(ElisionTokenFilter) },
			{ "hunspell", typeof(HunspellTokenFilter) },
			{ "hyphenation_decompounder", typeof(HyphenationDecompounderTokenFilter) },
			{ "keep_types", typeof(KeepTypesTokenFilter) },
			{ "keep", typeof(KeepWordsTokenFilter) },
			{ "keyword_marker", typeof(KeywordMarkerTokenFilter) },
			{ "kstem", typeof(KStemTokenFilter) },
			{ "length", typeof(LengthTokenFilter) },
			{ "limit", typeof(LimitTokenCountTokenFilter) },
			{ "lowercase", typeof(LowercaseTokenFilter) },
			{ "ngram", typeof(NGramTokenFilter) },
			{ "pattern_capture", typeof(PatternCaptureTokenFilter) },
			{ "pattern_replace", typeof(PatternReplaceTokenFilter) },
			{ "porter_stem", typeof(PorterStemTokenFilter) },
			{ "phonetic", typeof(PhoneticTokenFilter) },
			{ "reverse", typeof(ReverseTokenFilter) },
			{ "shingle", typeof(ShingleTokenFilter) },
			{ "snowball", typeof(SnowballTokenFilter) },
			{ "stemmer", typeof(StemmerTokenFilter) },
			{ "stemmer_override", typeof(StemmerOverrideTokenFilter) },
			{ "stop", typeof(StopTokenFilter) },
			{ "synonym", typeof(SynonymTokenFilter) },
			{ "synonym_graph", typeof(SynonymGraphTokenFilter) },
			{ "trim", typeof(TrimTokenFilter) },
			{ "truncate", typeof(TruncateTokenFilter) },
			{ "unique", typeof(UniqueTokenFilter) },
			{ "uppercase", typeof(UppercaseTokenFilter) },
			{ "word_delimiter", typeof(WordDelimiterTokenFilter) },
			{ "word_delimiter_graph", typeof(WordDelimiterGraphTokenFilter) },
			{ "fingerprint", typeof(FingerprintTokenFilter) },
			{ "nori_part_of_speech", typeof(NoriPartOfSpeechTokenFilter) },
			{ "kuromoji_readingform", typeof(KuromojiReadingFormTokenFilter) },
			{ "kuromoji_part_of_speech", typeof(KuromojiPartOfSpeechTokenFilter) },
			{ "kuromoji_stemmer", typeof(KuromojiStemmerTokenFilter) },
			{ "icu_collation", typeof(IcuCollationTokenFilter) },
			{ "icu_folding", typeof(IcuFoldingTokenFilter) },
			{ "icu_normalizer", typeof(IcuNormalizationTokenFilter) },
			{ "icu_transform", typeof(IcuTransformTokenFilter) },
			{ "condition", typeof(ConditionTokenFilter) },
			{ "multiplexer", typeof(MultiplexerTokenFilter) },
			{ "predicate_token_filter", typeof(PredicateTokenFilter) },
		};
	}
}
