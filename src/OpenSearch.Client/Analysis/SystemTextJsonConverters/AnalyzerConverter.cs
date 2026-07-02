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
	/// Polymorphic converter for <see cref="IAnalyzer"/>.
	/// Reads the "type" discriminator to dispatch to the correct concrete analyzer type.
	/// </summary>
	internal sealed class AnalyzerConverter : JsonConverter<IAnalyzer>
	{
		public override IAnalyzer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IAnalyzer but got {reader.TokenType}");

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var typeString = root.TryGetProperty("type", out var typeProp)
				? typeProp.GetString()
				: null;

			var hasTokenizer = root.TryGetProperty("tokenizer", out _);
			var rawJson = root.GetRawText();

			switch (typeString)
			{
				case "custom":
					return JsonSerializer.Deserialize<CustomAnalyzer>(rawJson, options);
				case "standard":
					return JsonSerializer.Deserialize<StandardAnalyzer>(rawJson, options);
				case "simple":
					return JsonSerializer.Deserialize<SimpleAnalyzer>(rawJson, options);
				case "whitespace":
					return JsonSerializer.Deserialize<WhitespaceAnalyzer>(rawJson, options);
				case "stop":
					return JsonSerializer.Deserialize<StopAnalyzer>(rawJson, options);
				case "keyword":
					return JsonSerializer.Deserialize<KeywordAnalyzer>(rawJson, options);
				case "pattern":
					return JsonSerializer.Deserialize<PatternAnalyzer>(rawJson, options);
				case "language":
					return JsonSerializer.Deserialize<LanguageAnalyzer>(rawJson, options);
				case "snowball":
					return JsonSerializer.Deserialize<SnowballAnalyzer>(rawJson, options);
				case "fingerprint":
					return JsonSerializer.Deserialize<FingerprintAnalyzer>(rawJson, options);
				case "icu_analyzer":
					return JsonSerializer.Deserialize<IcuAnalyzer>(rawJson, options);
				case "kuromoji":
					return JsonSerializer.Deserialize<KuromojiAnalyzer>(rawJson, options);
				case "nori":
					return JsonSerializer.Deserialize<NoriAnalyzer>(rawJson, options);
				default:
					if (hasTokenizer)
						return JsonSerializer.Deserialize<CustomAnalyzer>(rawJson, options);
					return JsonSerializer.Deserialize<LanguageAnalyzer>(rawJson, options);
			}
		}

		public override void Write(Utf8JsonWriter writer, IAnalyzer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value)
			{
				case CustomAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case StandardAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case SimpleAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case WhitespaceAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case StopAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KeywordAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PatternAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case LanguageAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case SnowballAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case FingerprintAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IcuAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KuromojiAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case NoriAnalyzer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}
	}
}
