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
	/// Polymorphic converter for <see cref="INormalizer"/>.
	/// Currently only custom normalizers are supported.
	/// {"type": "custom", "filter": ["lowercase"], "char_filter": ["mapping"]}
	/// </summary>
	internal sealed class NormalizerConverter : JsonConverter<INormalizer>
	{
		public override INormalizer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for INormalizer but got {reader.TokenType}");

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;
			var rawJson = root.GetRawText();

			// Currently OpenSearch only supports custom normalizers
			return JsonSerializer.Deserialize<CustomNormalizer>(rawJson, options);
		}

		public override void Write(Utf8JsonWriter writer, INormalizer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value)
			{
				case CustomNormalizer v:
					JsonSerializer.Serialize(writer, v, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}
	}
}
