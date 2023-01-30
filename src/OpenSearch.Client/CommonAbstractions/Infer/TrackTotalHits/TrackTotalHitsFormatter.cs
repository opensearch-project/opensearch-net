/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	internal class TrackTotalHitsFormatter : IJsonFormatter<TrackTotalHits>
	{
		public void Serialize(ref JsonWriter writer, TrackTotalHits value, IJsonFormatterResolver formatterResolver)
		{
			if (value?.LongValue is { } l)
				writer.WriteInt64(l);
			else if (value?.BoolValue is { } b)
				writer.WriteBoolean(b);
			else
				writer.WriteNull();
		}

		public TrackTotalHits Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver) =>
			reader.GetCurrentJsonToken() == JsonToken.Number
				? new TrackTotalHits(reader.ReadInt64())
				: new TrackTotalHits(reader.ReadBoolean());
	}
}
