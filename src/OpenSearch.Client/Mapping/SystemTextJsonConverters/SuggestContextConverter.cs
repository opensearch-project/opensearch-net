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

namespace OpenSearch.Client.Mapping.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="ISuggestContext"/>.
	/// Polymorphic: dispatches to CategorySuggestContext or GeoSuggestContext
	/// based on the "type" discriminator field ("category" or "geo").
	/// </summary>
	internal sealed class SuggestContextConverter : JsonConverter<ISuggestContext>
	{
		public override ISuggestContext Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for ISuggestContext but got {reader.TokenType}");

			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			var typeString = root.TryGetProperty("type", out var typeProp)
				? typeProp.GetString()
				: null;

			var rawJson = root.GetRawText();

			switch (typeString)
			{
				case "geo":
					return JsonSerializer.Deserialize<GeoSuggestContext>(rawJson, options);
				case "category":
				default:
					return JsonSerializer.Deserialize<CategorySuggestContext>(rawJson, options);
			}
		}

		public override void Write(Utf8JsonWriter writer, ISuggestContext value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value)
			{
				case GeoSuggestContext geo:
					JsonSerializer.Serialize(writer, geo, options);
					break;
				case CategorySuggestContext category:
					JsonSerializer.Serialize(writer, category, options);
					break;
				default:
					JsonSerializer.Serialize(writer, value, value.GetType(), options);
					break;
			}
		}
	}
}
