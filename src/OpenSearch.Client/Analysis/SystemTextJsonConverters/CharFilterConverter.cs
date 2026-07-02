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
	/// Polymorphic converter for <see cref="ICharFilter"/>.
	/// Reads the "type" discriminator to dispatch to the correct concrete char filter type.
	/// </summary>
	internal sealed class CharFilterConverter : JsonConverter<ICharFilter>
	{
		public override ICharFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for ICharFilter but got {reader.TokenType}");

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
				case "html_strip":
					return JsonSerializer.Deserialize<HtmlStripCharFilter>(rawJson, options);
				case "mapping":
					return JsonSerializer.Deserialize<MappingCharFilter>(rawJson, options);
				case "pattern_replace":
					return JsonSerializer.Deserialize<PatternReplaceCharFilter>(rawJson, options);
				case "kuromoji_iteration_mark":
					return JsonSerializer.Deserialize<KuromojiIterationMarkCharFilter>(rawJson, options);
				case "icu_normalizer":
					return JsonSerializer.Deserialize<IcuNormalizationCharFilter>(rawJson, options);
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, ICharFilter value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value)
			{
				case HtmlStripCharFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case MappingCharFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case PatternReplaceCharFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case KuromojiIterationMarkCharFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				case IcuNormalizationCharFilter v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}
	}
}
