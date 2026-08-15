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
	/// System.Text.Json replacement for the legacy Utf8Json <c>TokenizerFormatter</c>.
	///
	/// <see cref="ITokenizer"/> is polymorphic: the concrete type is selected by the value of the
	/// <c>type</c> field (with a couple of aliases, e.g. <c>edgengram</c> == <c>edge_ngram</c>). System.Text.Json's
	/// <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound, so — unlike the Utf8Json version which
	/// peeked at a byte segment and re-read it — we buffer the value into a <see cref="JsonDocument"/>, read the
	/// <c>type</c> discriminator from the DOM, then deserialize the whole element as the matching concrete type.
	/// Serialization writes by runtime type.
	/// </summary>
	internal class TokenizerConverter : JsonConverter<ITokenizer>
	{
		public override ITokenizer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var tokenizerType = root.TryGetProperty("type", out var t) ? t.GetString() : null;
			if (tokenizerType == null)
				return null;

			switch (tokenizerType)
			{
				case "char_group":
					return root.Deserialize<CharGroupTokenizer>(options);
				case "edgengram":
				case "edge_ngram":
					return root.Deserialize<EdgeNGramTokenizer>(options);
				case "ngram":
					return root.Deserialize<NGramTokenizer>(options);
				case "path_hierarchy":
					return root.Deserialize<PathHierarchyTokenizer>(options);
				case "pattern":
					return root.Deserialize<PatternTokenizer>(options);
				case "standard":
					return root.Deserialize<StandardTokenizer>(options);
				case "uax_url_email":
					return root.Deserialize<UaxEmailUrlTokenizer>(options);
				case "whitespace":
					return root.Deserialize<WhitespaceTokenizer>(options);
				case "kuromoji_tokenizer":
					return root.Deserialize<KuromojiTokenizer>(options);
				case "icu_tokenizer":
					return root.Deserialize<IcuTokenizer>(options);
				case "nori_tokenizer":
					return root.Deserialize<NoriTokenizer>(options);
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, ITokenizer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializer.Serialize(writer, value, value.GetType(), options);
		}
	}
}
