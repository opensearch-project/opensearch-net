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
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="ICharFilter"/>
	/// hierarchy, replacing the vendored Utf8Json <c>CharFilterFormatter</c> as part of #388.
	/// The dispatch table mirrors <c>CharFilterFormatter</c> exactly.
	/// </summary>
	internal sealed class CharFilterInterfaceConverter : PolymorphicInterfaceConverter<ICharFilter>
	{
		public CharFilterInterfaceConverter() : base(TypeByDiscriminator) { }

		private static readonly IReadOnlyDictionary<string, Type> TypeByDiscriminator = new Dictionary<string, Type>(StringComparer.Ordinal)
		{
			{ "html_strip", typeof(HtmlStripCharFilter) },
			{ "mapping", typeof(MappingCharFilter) },
			{ "pattern_replace", typeof(PatternReplaceCharFilter) },
			{ "kuromoji_iteration_mark", typeof(KuromojiIterationMarkCharFilter) },
			{ "icu_normalizer", typeof(IcuNormalizationCharFilter) },
		};
	}
}
