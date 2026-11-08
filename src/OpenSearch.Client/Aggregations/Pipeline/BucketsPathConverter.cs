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
	/// System.Text.Json replacement for the legacy Utf8Json <c>BucketsPathFormatter</c>.
	///
	/// <see cref="IBucketsPath"/> is a union of JSON shapes:
	/// <list type="bullet">
	/// <item><description>a <c>string</c> becomes a <see cref="SingleBucketsPath"/>;</description></item>
	/// <item><description>an object becomes a <see cref="MultiBucketsPath"/> (a string→string dictionary);</description></item>
	/// <item><description>any other token (including an array or <c>null</c>) yields <c>null</c>.</description></item>
	/// </list>
	/// On write a <see cref="SingleBucketsPath"/> is emitted as its string, a <see cref="MultiBucketsPath"/> as a JSON
	/// object of its key/value pairs, and anything else as <c>null</c> — matching the legacy formatter exactly.
	/// </summary>
	internal class BucketsPathConverter : JsonConverter<IBucketsPath>
	{
		public override IBucketsPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.String:
					return new SingleBucketsPath(reader.GetString());
				case JsonTokenType.StartObject:
					var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options);
					return new MultiBucketsPath(dict);
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, IBucketsPath value, JsonSerializerOptions options)
		{
			switch (value)
			{
				case SingleBucketsPath single:
					writer.WriteStringValue(single.BucketsPath);
					break;
				case MultiBucketsPath multi:
					writer.WriteStartObject();
					foreach (var kv in multi)
					{
						writer.WritePropertyName(kv.Key);
						writer.WriteStringValue(kv.Value);
					}
					writer.WriteEndObject();
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}
	}
}
