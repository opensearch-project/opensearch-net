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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Like"/>, replacing the vendored
	/// Utf8Json <c>LikeFormatter</c> as part of #388. <see cref="Like"/> is a
	/// <see cref="Union{TFirst,TSecond}"/> of <see cref="string"/> (like text) or
	/// <see cref="ILikeDocument"/> (an indexed or artificial document), and is serialized as either a
	/// JSON string or a like-document object accordingly.
	/// </summary>
	internal sealed class LikeConverter : JsonConverter<Like>
	{
		public override void Write(Utf8JsonWriter writer, Like value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					JsonSerializer.Serialize(writer, value.Item1, options);
					break;
				case 1:
					JsonSerializer.Serialize(writer, value.Item2, options);
					break;
				default:
					throw new JsonException($"Unrecognized {nameof(Like)} tag value: {value.Tag}");
			}
		}

		public override Like Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new Like(reader.GetString());
				case JsonTokenType.StartObject:
					var doc = JsonSerializer.Deserialize<LikeDocument<object>>(ref reader, options);
					return doc == null ? null : new Like(doc);
				default:
					throw new JsonException($"Unexpected token '{reader.TokenType}' when parsing {nameof(Like)}.");
			}
		}
	}
}
