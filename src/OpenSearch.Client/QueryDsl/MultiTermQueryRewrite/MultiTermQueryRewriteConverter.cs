/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="MultiTermQueryRewrite"/>, replacing
	/// the vendored Utf8Json <c>MultiTermQueryRewriteFormatter</c> as part of #388. Serialized as its
	/// string form (e.g. <c>"constant_score"</c> or <c>"top_terms_10"</c>); read back via
	/// <see cref="MultiTermQueryRewrite"/>'s string factory. Used by the <c>rewrite</c> option of the
	/// fuzzy/wildcard/prefix/regexp queries.
	/// </summary>
	internal sealed class MultiTermQueryRewriteConverter : JsonConverter<MultiTermQueryRewrite>
	{
		public override void Write(Utf8JsonWriter writer, MultiTermQueryRewrite value, JsonSerializerOptions options)
		{
			if (value == null) writer.WriteNullValue();
			else writer.WriteStringValue(value.ToString());
		}

		public override MultiTermQueryRewrite Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException($"Invalid token type {reader.TokenType} to deserialize {nameof(MultiTermQueryRewrite)} from");

			return MultiTermQueryRewrite.Create(reader.GetString());
		}
	}
}
