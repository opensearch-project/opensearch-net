/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.AnalysisConverters
{
	/// <summary>
	/// Polymorphic converter for <see cref="ITokenizer"/>.
	/// Reads the "type" discriminator to dispatch to the correct concrete tokenizer type.
	/// </summary>
	internal sealed class TokenizerConverter : JsonConverter<ITokenizer>
	{
		public override ITokenizer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for ITokenizer but got {reader.TokenType}");

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var typeString = root.TryGetProperty("type", out var typeProp)
				? typeProp.GetString()
				: null;

			if (typeString == null)
				return null;

			var rawJson = root.GetRawText();

			switch (typeString)
			{
				case "standard":
					return JsonSerializer.Deserialize<StandardTokenizer>(rawJson, options);
				case "letter":
					return JsonSerializer.Deserialize<LetterTokenizer>(rawJson, options);
				case "lowercase":
					return JsonSerializer.Deserialize<LowercaseTokenizer>(rawJson, options);
				case "whitespace":
					return JsonSerializer.Deserialize<WhitespaceTokenizer>(rawJson, options);
				case "keyword":
					return JsonSerializer.Deserialize<KeywordTokenizer>(rawJson, options);
				case "pattern":
					return JsonSerializer.Deserialize<PatternTokenizer>(rawJson, options);
				case "path_hierarchy":
					return JsonSerializer.Deserialize<PathHierarchyTokenizer>(rawJson, options);
				case "ngram":
					return JsonSerializer.Deserialize<NGramTokenizer>(rawJson, options);
				case "edge_ngram":
				case "edgengram":
					return JsonSerializer.Deserialize<EdgeNGramTokenizer>(rawJson, options);
				case "uax_url_email":
					return JsonSerializer.Deserialize<UaxEmailUrlTokenizer>(rawJson, options);
				case "char_group":
					return JsonSerializer.Deserialize<CharGroupTokenizer>(rawJson, options);
				case "kuromoji_tokenizer":
					return JsonSerializer.Deserialize<KuromojiTokenizer>(rawJson, options);
				case "icu_tokenizer":
					return JsonSerializer.Deserialize<IcuTokenizer>(rawJson, options);
				case "nori_tokenizer":
					return JsonSerializer.Deserialize<NoriTokenizer>(rawJson, options);
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

			switch (value)
			{
				case StandardTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case LetterTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case LowercaseTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case WhitespaceTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KeywordTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PatternTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PathHierarchyTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case NGramTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case EdgeNGramTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case UaxEmailUrlTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case CharGroupTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KuromojiTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IcuTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case NoriTokenizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}
	}
}
