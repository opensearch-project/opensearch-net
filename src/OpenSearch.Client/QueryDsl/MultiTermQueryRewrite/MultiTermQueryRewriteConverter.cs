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
	/// System.Text.Json replacement for the legacy Utf8Json <c>MultiTermQueryRewriteFormatter</c>. A
	/// <see cref="MultiTermQueryRewrite"/> is serialized as a single JSON string (its <see cref="object.ToString"/>
	/// representation) and reconstructed on read via <see cref="MultiTermQueryRewrite.Create"/>.
	/// </summary>
	internal class MultiTermQueryRewriteConverter : JsonConverter<MultiTermQueryRewrite>
	{
		public override MultiTermQueryRewrite Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException($"Invalid token type {reader.TokenType} to deserialize {nameof(MultiTermQueryRewrite)} from");

			return MultiTermQueryRewrite.Create(reader.GetString());
		}

		public override void Write(Utf8JsonWriter writer, MultiTermQueryRewrite value, JsonSerializerOptions options)
		{
			if (value == null)
				writer.WriteNullValue();
			else
				writer.WriteStringValue(value.ToString());
		}
	}
}
