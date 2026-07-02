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
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="IAnalyzer"/>
	/// hierarchy, replacing the vendored Utf8Json <c>AnalyzerFormatter</c> as part of #388.
	/// <para>
	/// Beyond the named-type table it reproduces <c>AnalyzerFormatter</c>'s fallback: when the
	/// <c>type</c> is absent or unrecognized, a <c>tokenizer</c> field means a
	/// <see cref="CustomAnalyzer"/>, otherwise a <see cref="LanguageAnalyzer"/> (whose
	/// <c>type</c> is the language name, e.g. <c>english</c>).
	/// </para>
	/// </summary>
	internal sealed class AnalyzerInterfaceConverter : PolymorphicInterfaceConverter<IAnalyzer>
	{
		public AnalyzerInterfaceConverter() : base(TypeByDiscriminator) { }

		private static readonly IReadOnlyDictionary<string, Type> TypeByDiscriminator = new Dictionary<string, Type>(StringComparer.Ordinal)
		{
			{ "stop", typeof(StopAnalyzer) },
			{ "standard", typeof(StandardAnalyzer) },
			{ "snowball", typeof(SnowballAnalyzer) },
			{ "pattern", typeof(PatternAnalyzer) },
			{ "keyword", typeof(KeywordAnalyzer) },
			{ "whitespace", typeof(WhitespaceAnalyzer) },
			{ "simple", typeof(SimpleAnalyzer) },
			{ "fingerprint", typeof(FingerprintAnalyzer) },
			{ "kuromoji", typeof(KuromojiAnalyzer) },
			{ "nori", typeof(NoriAnalyzer) },
			{ "icu_analyzer", typeof(IcuAnalyzer) },
		};

		protected override Type ResolveType(string discriminator, JsonElement document)
		{
			var mapped = base.ResolveType(discriminator, document);
			if (mapped != null) return mapped;

			return document.TryGetProperty("tokenizer", out _) ? typeof(CustomAnalyzer) : typeof(LanguageAnalyzer);
		}
	}
}
