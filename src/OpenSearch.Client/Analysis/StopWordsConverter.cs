/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>StopWordsFormatter</c>. A <see cref="StopWords"/> is a
	/// union of either a single (CSV) string or an array of strings. On read the shape is dispatched on the JSON token:
	/// a <c>StartArray</c> becomes the string-collection branch (tag 1), anything else is read as the single-string
	/// branch (tag 0). On write the legacy shape is preserved exactly — the string branch writes a bare string and the
	/// collection branch writes a JSON array.
	/// </summary>
	internal class StopWordsConverter : JsonConverter<StopWords>
	{
		public override StopWords Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var stopwords = JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
				return new StopWords(stopwords);
			}

			var stopword = reader.GetString();
			return new StopWords(stopword);
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
