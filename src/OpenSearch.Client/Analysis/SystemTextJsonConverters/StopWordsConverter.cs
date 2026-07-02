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

namespace OpenSearch.Client.AnalysisConverters
{
	/// <summary>
	/// Converter for <see cref="StopWords"/>.
	/// StopWords can be either a string (predefined list name like "_english_")
	/// or an array of strings (custom stop words).
	/// </summary>
	internal sealed class StopWordsConverter : JsonConverter<StopWords>
	{
		public override StopWords Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;

				case JsonTokenType.String:
					return new StopWords(reader.GetString());

				case JsonTokenType.StartArray:
					var words = JsonSerializer.Deserialize<List<string>>(ref reader, options);
					return new StopWords(words);

				default:
					throw new JsonException($"Unexpected token type {reader.TokenType} for StopWords");
			}
		}

		public override void Write(Utf8JsonWriter writer, StopWords value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					writer.WriteStringValue(value.Item1);
					break;
				case 1:
					JsonSerializer.Serialize(writer, value.Item2, options);
					break;
			}
		}
	}
}
