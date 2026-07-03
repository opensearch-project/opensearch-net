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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IBucketsPath"/> (pipeline
	/// aggregations' <c>buckets_path</c>), replacing the vendored Utf8Json <c>BucketsPathFormatter</c>
	/// as part of #388. A single path is written as a string; a multi-path as a <c>{ name: path }</c>
	/// object.
	/// </summary>
	internal sealed class BucketsPathConverter : JsonConverter<IBucketsPath>
	{
		public override void Write(Utf8JsonWriter writer, IBucketsPath value, JsonSerializerOptions options)
		{
			switch (value)
			{
				case null:
					writer.WriteNullValue();
					break;
				case SingleBucketsPath single:
					writer.WriteStringValue(single.BucketsPath);
					break;
				case MultiBucketsPath multi:
					writer.WriteStartObject();
					foreach (var entry in (IEnumerable<KeyValuePair<string, string>>)multi)
						writer.WriteString(entry.Key, entry.Value);
					writer.WriteEndObject();
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}

		public override IBucketsPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new SingleBucketsPath(reader.GetString());
				case JsonTokenType.StartObject:
				{
					using var document = JsonDocument.ParseValue(ref reader);
					var map = new Dictionary<string, string>();
					foreach (var member in document.RootElement.EnumerateObject())
						map[member.Name] = member.Value.GetString();
					return new MultiBucketsPath(map);
				}
				default:
					throw new JsonException($"Cannot deserialize {nameof(IBucketsPath)} from token {reader.TokenType}.");
			}
		}
	}
}
