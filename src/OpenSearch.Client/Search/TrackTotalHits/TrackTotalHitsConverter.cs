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
	/// System.Text.Json replacement for the legacy Utf8Json <c>TrackTotalHitsFormatter</c>. A
	/// <see cref="TrackTotalHits"/> is a union of <see cref="bool"/> and <see cref="long"/>.
	/// </summary>
	internal class TrackTotalHitsConverter : JsonConverter<TrackTotalHits>
	{
		private static readonly UnionConverter<bool, long> Union = new();

		public override TrackTotalHits Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var union = Union.Read(ref reader, typeToConvert, options);
			if (union == null) return null;
			return union.Tag switch
			{
				0 => new TrackTotalHits(union.Item1),
				1 => new TrackTotalHits(union.Item2),
				_ => null
			};
		}

		public override void Write(Utf8JsonWriter writer, TrackTotalHits value, JsonSerializerOptions options) =>
			Union.Write(writer, value, options);
	}
}
