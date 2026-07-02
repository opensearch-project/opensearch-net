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
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="ITokenizer"/>
	/// hierarchy, replacing the vendored Utf8Json <c>TokenizerFormatter</c> as part of #388.
	/// <para>
	/// The dispatch table mirrors <c>TokenizerFormatter</c> and additionally registers
	/// <c>keyword</c>, <c>letter</c> and <c>lowercase</c> — concrete types that exist in the client
	/// but were absent from the hand-written read dispatch. This closes that drift, matching the
	/// table produced by the converter generator (see <c>ConverterSpikeGenerator</c>).
	/// </para>
	/// </summary>
	internal sealed class TokenizerInterfaceConverter : PolymorphicInterfaceConverter<ITokenizer>
	{
		public TokenizerInterfaceConverter() : base(TypeByDiscriminator) { }

		private static readonly IReadOnlyDictionary<string, Type> TypeByDiscriminator = new Dictionary<string, Type>(StringComparer.Ordinal)
		{
			{ "char_group", typeof(CharGroupTokenizer) },
			{ "edgengram", typeof(EdgeNGramTokenizer) },
			{ "edge_ngram", typeof(EdgeNGramTokenizer) },
			{ "ngram", typeof(NGramTokenizer) },
			{ "path_hierarchy", typeof(PathHierarchyTokenizer) },
			{ "pattern", typeof(PatternTokenizer) },
			{ "standard", typeof(StandardTokenizer) },
			{ "uax_url_email", typeof(UaxEmailUrlTokenizer) },
			{ "whitespace", typeof(WhitespaceTokenizer) },
			{ "keyword", typeof(KeywordTokenizer) },
			{ "letter", typeof(LetterTokenizer) },
			{ "lowercase", typeof(LowercaseTokenizer) },
			{ "kuromoji_tokenizer", typeof(KuromojiTokenizer) },
			{ "icu_tokenizer", typeof(IcuTokenizer) },
			{ "nori_tokenizer", typeof(NoriTokenizer) },
		};
	}
}
