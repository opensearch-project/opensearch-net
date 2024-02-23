/* SPDX-License-Identifier: Apache-2.0
 *
 * The OpenSearch Contributors require contributions made to
 * this file be licensed under the Apache-2.0 license or a
 * compatible open source license.
 */

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

internal class SlicesFormatter : IJsonFormatter<Slices>
{
	private static readonly UnionFormatter<long, string> UnionFormatter = new();

	public void Serialize(ref JsonWriter writer, Slices value, IJsonFormatterResolver formatterResolver) =>
		UnionFormatter.Serialize(ref writer, value, formatterResolver);

	public Slices Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
	{
		var union = UnionFormatter.Deserialize(ref reader, formatterResolver);
		if (union == null) return null;

		return union.Tag switch
		{
			0 => new Slices(union.Item1),
			1 => new Slices(union.Item2),
			_ => null
		};
	}
}
