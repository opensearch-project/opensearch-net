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
	/// System.Text.Json replacement for the legacy Utf8Json <c>SlicesFormatter</c>. <see cref="Slices"/> is a
	/// union of <see cref="long"/> and <see cref="string"/>. The read try-order is inherited from
	/// <see cref="UnionConverter{TFirst, TSecond}"/>: numeric first, string second.
	/// </summary>
	internal class SlicesConverter : JsonConverter<Slices>
	{
		private static readonly UnionConverter<long, string> Union = new();

		public override Slices Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var union = Union.Read(ref reader, typeToConvert, options);
			if (union == null) return null;
			return union.Tag switch
			{
				0 => new Slices(union.Item1),
				1 => new Slices(union.Item2),
				_ => null
			};
		}

		public override void Write(Utf8JsonWriter writer, Slices value, JsonSerializerOptions options) =>
			Union.Write(writer, value, options);
	}
}
