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
	/// A <see cref="System.Text.Json"/> converter for <see cref="StopWords"/>, the
	/// <c>Union&lt;string, IEnumerable&lt;string&gt;&gt;</c> used by analysis components (stop token
	/// filter, stop/standard analyzers, keep-words). Replaces the vendored Utf8Json
	/// <c>StopWordsFormatter</c> (#388): a single stop word is written as a JSON string, a set as a
	/// JSON string array, matching the wire format exactly.
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
					return new StopWords(JsonSerializer.Deserialize<List<string>>(ref reader, options));
				default:
					throw new JsonException($"Cannot deserialize {nameof(StopWords)} from token {reader.TokenType}.");
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
