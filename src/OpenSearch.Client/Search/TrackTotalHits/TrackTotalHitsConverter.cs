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
	/// A <see cref="System.Text.Json"/> converter for <see cref="TrackTotalHits"/> (a
	/// <see cref="Union{Boolean,Int64}"/>), replacing the vendored Utf8Json <c>TrackTotalHitsFormatter</c>
	/// as part of #388. Written/read as a boolean (track exactly or not) or a number (track up to N).
	/// </summary>
	internal sealed class TrackTotalHitsConverter : JsonConverter<TrackTotalHits>
	{
		public override TrackTotalHits Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.True:
				case JsonTokenType.False:
					return new TrackTotalHits(reader.GetBoolean());
				case JsonTokenType.Number:
					return new TrackTotalHits(reader.GetInt64());
				default:
					throw new JsonException($"Cannot deserialize {nameof(TrackTotalHits)} from token {reader.TokenType}.");
			}
		}

		public override void Write(Utf8JsonWriter writer, TrackTotalHits value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					writer.WriteBooleanValue(value.Item1);
					break;
				case 1:
					writer.WriteNumberValue(value.Item2);
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}
	}
}
