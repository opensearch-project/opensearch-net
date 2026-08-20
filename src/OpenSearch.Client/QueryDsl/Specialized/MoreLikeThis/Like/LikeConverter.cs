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
	/// System.Text.Json replacement for the legacy Utf8Json <c>LikeFormatter</c>. A <see cref="Like"/> is a union of
	/// either free text (a JSON string, tag 0) or an <see cref="ILikeDocument"/> (a JSON object, tag 1). Reading and
	/// writing are delegated to the migrated <see cref="UnionConverter{TFirst, TSecond}"/> exactly as the legacy
	/// formatter delegated to <c>UnionFormatter&lt;string, ILikeDocument&gt;</c>; on read the resulting union is
	/// re-wrapped into a <see cref="Like"/> preserving the original branch.
	/// </summary>
	internal class LikeConverter : JsonConverter<Like>
	{
		// attemptTSecondIfTFirstIsNull: an object payload deserializes to a null string under STJ (without throwing),
		// so fall through to the ILikeDocument branch when the string attempt yields null — otherwise an object Like
		// (a document) would read as null.
		private static readonly UnionConverter<string, ILikeDocument> UnionConverter =
			new UnionConverter<string, ILikeDocument>(attemptTSecondIfTFirstIsNull: true);

		public override Like Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var union = UnionConverter.Read(ref reader, typeToConvert, options);

			if (union == null)
				return null;

			switch (union.Tag)
			{
				case 0:
					return new Like(union.Item1);
				case 1:
					return new Like(union.Item2);
				default:
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, Like value, JsonSerializerOptions options) =>
			UnionConverter.Write(writer, value, options);
	}
}
