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

namespace OpenSearch.Client.Search.SystemTextJsonConverters;

/// <summary>
/// System.Text.Json converter for <see cref="TrackTotalHits"/>.
/// Can be: boolean true/false or integer (threshold).
/// </summary>
internal sealed class TrackTotalHitsConverter : JsonConverter<TrackTotalHits>
{
	public override TrackTotalHits? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		if (reader.TokenType == JsonTokenType.True)
			return new TrackTotalHits(true);

		if (reader.TokenType == JsonTokenType.False)
			return new TrackTotalHits(false);

		if (reader.TokenType == JsonTokenType.Number)
			return new TrackTotalHits(reader.GetInt64());

		// Handle string representations
		if (reader.TokenType == JsonTokenType.String)
		{
			var str = reader.GetString();
			if (bool.TryParse(str, out var boolVal))
				return new TrackTotalHits(boolVal);
			if (long.TryParse(str, out var longVal))
				return new TrackTotalHits(longVal);
		}

		throw new JsonException($"Cannot deserialize TrackTotalHits from token {reader.TokenType}");
	}

	public override void Write(Utf8JsonWriter writer, TrackTotalHits value, JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNullValue();
			return;
		}

		// Tag == 0 means first union type (bool), Tag == 1 means second (long)
		value.Match(
			boolValue => writer.WriteBooleanValue(boolValue),
			longValue => writer.WriteNumberValue(longValue)
		);
	}
}
