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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Distance"/>, replacing the vendored
	/// Utf8Json <c>DistanceFormatter</c> as part of #388. A distance is written as its string form
	/// (e.g. <c>"12km"</c>) and parsed back from that string.
	/// </summary>
	internal sealed class DistanceConverter : JsonConverter<Distance>
	{
		public override Distance Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new Distance(reader.GetString());
				case JsonTokenType.Number:
					return new Distance(reader.GetDouble());
				default:
					throw new JsonException($"Cannot deserialize {nameof(Distance)} from token {reader.TokenType}.");
			}
		}

		public override void Write(Utf8JsonWriter writer, Distance value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(value.ToString());
		}
	}
}
