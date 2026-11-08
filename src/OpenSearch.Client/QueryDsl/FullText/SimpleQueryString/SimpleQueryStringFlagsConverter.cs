/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>SimpleQueryStringFlagsFormatter</c>. The
	/// <see cref="SimpleQueryStringFlags"/> <c>[Flags]</c> enum is serialized as a single "|"-delimited string using
	/// the exact upper-case token names (<c>ALL|NONE|AND|OR|...</c>). The write order and token spelling are preserved
	/// exactly from the legacy formatter. On read the string is split on "|", each token mapped back to its enum member
	/// (case-insensitive, ignoring unknown tokens), and the results OR'd together.
	/// </summary>
	internal class SimpleQueryStringFlagsConverter : JsonConverter<SimpleQueryStringFlags?>
	{
		// The converter must run even when the incoming token is null so that a JSON null round-trips to a null value.
		public override bool HandleNull => true;

		public override SimpleQueryStringFlags? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var flags = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
			return flags?.Split('|')
				.Select(flag => flag.ToEnum<SimpleQueryStringFlags>())
				.Where(s => s.HasValue)
				.Aggregate(default(SimpleQueryStringFlags), (current, s) => current | s.Value);
		}

		public override void Write(Utf8JsonWriter writer, SimpleQueryStringFlags? value, JsonSerializerOptions options)
		{
			if (!value.HasValue)
			{
				writer.WriteNullValue();
				return;
			}

			var e = value.Value;
			var list = new List<string>(13);
			if (e.HasFlag(SimpleQueryStringFlags.All)) list.Add("ALL");
			if (e.HasFlag(SimpleQueryStringFlags.None)) list.Add("NONE");
			if (e.HasFlag(SimpleQueryStringFlags.And)) list.Add("AND");
			if (e.HasFlag(SimpleQueryStringFlags.Or)) list.Add("OR");
			if (e.HasFlag(SimpleQueryStringFlags.Not)) list.Add("NOT");
			if (e.HasFlag(SimpleQueryStringFlags.Prefix)) list.Add("PREFIX");
			if (e.HasFlag(SimpleQueryStringFlags.Phrase)) list.Add("PHRASE");
			if (e.HasFlag(SimpleQueryStringFlags.Precedence)) list.Add("PRECEDENCE");
			if (e.HasFlag(SimpleQueryStringFlags.Escape)) list.Add("ESCAPE");
			if (e.HasFlag(SimpleQueryStringFlags.Whitespace)) list.Add("WHITESPACE");
			if (e.HasFlag(SimpleQueryStringFlags.Fuzzy)) list.Add("FUZZY");
			if (e.HasFlag(SimpleQueryStringFlags.Near)) list.Add("NEAR");
			if (e.HasFlag(SimpleQueryStringFlags.Slop)) list.Add("SLOP");
			var flags = string.Join("|", list);
			writer.WriteStringValue(flags);
		}
	}
}
