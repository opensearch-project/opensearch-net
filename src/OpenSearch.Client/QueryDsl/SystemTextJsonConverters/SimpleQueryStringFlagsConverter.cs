/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Collections.Generic;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="SimpleQueryStringFlags"/>? (nullable).
	/// Serializes as a pipe-delimited string: "OR|AND|PREFIX"
	/// Reads: splits on | and parses each flag name.
	/// </summary>
	internal sealed class SimpleQueryStringFlagsConverter : JsonConverter<SimpleQueryStringFlags?>
	{
		public override SimpleQueryStringFlags? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException($"Expected String for SimpleQueryStringFlags but got {reader.TokenType}");

			var flagsStr = reader.GetString();
			if (string.IsNullOrEmpty(flagsStr))
				return null;

			var parts = flagsStr.Split('|');
			var result = default(SimpleQueryStringFlags);
			var hasValue = false;

			foreach (var part in parts)
			{
				var parsed = ParseFlag(part.Trim());
				if (parsed.HasValue)
				{
					result |= parsed.Value;
					hasValue = true;
				}
			}

			return hasValue ? result : null;
		}

		public override void Write(Utf8JsonWriter writer, SimpleQueryStringFlags? value, JsonSerializerOptions options)
		{
			if (!value.HasValue)
			{
				writer.WriteNullValue();
				return;
			}

			var flags = value.Value;
			var list = new List<string>(13);

			if (flags.HasFlag(SimpleQueryStringFlags.All)) list.Add("ALL");
			if (flags.HasFlag(SimpleQueryStringFlags.None)) list.Add("NONE");
			if (flags.HasFlag(SimpleQueryStringFlags.And)) list.Add("AND");
			if (flags.HasFlag(SimpleQueryStringFlags.Or)) list.Add("OR");
			if (flags.HasFlag(SimpleQueryStringFlags.Not)) list.Add("NOT");
			if (flags.HasFlag(SimpleQueryStringFlags.Prefix)) list.Add("PREFIX");
			if (flags.HasFlag(SimpleQueryStringFlags.Phrase)) list.Add("PHRASE");
			if (flags.HasFlag(SimpleQueryStringFlags.Precedence)) list.Add("PRECEDENCE");
			if (flags.HasFlag(SimpleQueryStringFlags.Escape)) list.Add("ESCAPE");
			if (flags.HasFlag(SimpleQueryStringFlags.Whitespace)) list.Add("WHITESPACE");
			if (flags.HasFlag(SimpleQueryStringFlags.Fuzzy)) list.Add("FUZZY");
			if (flags.HasFlag(SimpleQueryStringFlags.Near)) list.Add("NEAR");
			if (flags.HasFlag(SimpleQueryStringFlags.Slop)) list.Add("SLOP");

			writer.WriteStringValue(string.Join("|", list));
		}

		private static SimpleQueryStringFlags? ParseFlag(string flag) => flag.ToUpperInvariant() switch
		{
			"ALL" => SimpleQueryStringFlags.All,
			"NONE" => SimpleQueryStringFlags.None,
			"AND" => SimpleQueryStringFlags.And,
			"OR" => SimpleQueryStringFlags.Or,
			"NOT" => SimpleQueryStringFlags.Not,
			"PREFIX" => SimpleQueryStringFlags.Prefix,
			"PHRASE" => SimpleQueryStringFlags.Phrase,
			"PRECEDENCE" => SimpleQueryStringFlags.Precedence,
			"ESCAPE" => SimpleQueryStringFlags.Escape,
			"WHITESPACE" => SimpleQueryStringFlags.Whitespace,
			"FUZZY" => SimpleQueryStringFlags.Fuzzy,
			"NEAR" => SimpleQueryStringFlags.Near,
			"SLOP" => SimpleQueryStringFlags.Slop,
			_ => null
		};
	}
}
