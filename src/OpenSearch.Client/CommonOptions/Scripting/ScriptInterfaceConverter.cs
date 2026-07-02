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
	/// A <see cref="System.Text.Json"/> converter for the polymorphic <see cref="IScript"/>
	/// hierarchy, replacing the vendored Utf8Json <c>ScriptFormatter</c> as part of #388.
	/// <para>
	/// Scripts have no <c>type</c> discriminator; the concrete type is inferred from which field is
	/// present — <c>source</c> (or the legacy <c>inline</c>) means an <see cref="InlineScript"/>,
	/// <c>id</c> means an <see cref="IndexedScript"/>. This reuses the shared converter's
	/// <c>ResolveType</c> seam, so write (serialize the concrete runtime type) and the shared
	/// buffering logic are inherited.
	/// </para>
	/// <para>
	/// This is a widely referenced type: migrating it unblocks the <c>condition</c> and
	/// <c>predicate_token_filter</c> token filters and the script-bearing queries/aggregations.
	/// </para>
	/// </summary>
	internal sealed class ScriptInterfaceConverter : PolymorphicInterfaceConverter<IScript>
	{
		public ScriptInterfaceConverter() : base(Empty) { }

		private static readonly IReadOnlyDictionary<string, Type> Empty = new Dictionary<string, Type>();

		protected override Type ResolveType(string discriminator, JsonElement document)
		{
			if (document.TryGetProperty("id", out _)) return typeof(IndexedScript);
			if (document.TryGetProperty("source", out _) || document.TryGetProperty("inline", out _))
				return typeof(InlineScript);
			return null;
		}
	}
}
