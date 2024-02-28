/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

internal class TrackTotalHitsFormatter : IJsonFormatter<TrackTotalHits>
{
	private static readonly UnionFormatter<bool, long> UnionFormatter = new();

	public void Serialize(ref JsonWriter writer, TrackTotalHits value, IJsonFormatterResolver formatterResolver) =>
		UnionFormatter.Serialize(ref writer, value, formatterResolver);

	public TrackTotalHits Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
	{
		var union = UnionFormatter.Deserialize(ref reader, formatterResolver);
		if (union == null) return null;
		return union.Tag switch
		{
			0 => new TrackTotalHits(union.Item1),
			1 => new TrackTotalHits(union.Item2),
			_ => null
		};
	}
}
