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
	/// A <see cref="System.Text.Json"/> converter for the <see cref="INormalizer"/> hierarchy,
	/// replacing the vendored Utf8Json <c>NormalizerFormatter</c> as part of #388.
	/// <para>
	/// OpenSearch ships no built-in normalizers, so <see cref="CustomNormalizer"/> is the only
	/// concrete type; like <c>NormalizerFormatter</c>, deserialization always resolves to it.
	/// </para>
	/// </summary>
	internal sealed class NormalizerInterfaceConverter : PolymorphicInterfaceConverter<INormalizer>
	{
		public NormalizerInterfaceConverter() : base(TypeByDiscriminator) { }

		private static readonly IReadOnlyDictionary<string, Type> TypeByDiscriminator = new Dictionary<string, Type>(StringComparer.Ordinal)
		{
			{ "custom", typeof(CustomNormalizer) },
		};

		protected override Type ResolveType(string discriminator, JsonElement document) => typeof(CustomNormalizer);
	}
}
