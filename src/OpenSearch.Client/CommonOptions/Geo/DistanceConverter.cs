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
	/// System.Text.Json replacement for the legacy Utf8Json <c>DistanceFormatter</c>. A
	/// <see cref="Distance"/> is serialized as its string representation, and read from a JSON
	/// string; any non-string token is skipped and yields <c>null</c>.
	/// </summary>
	internal class DistanceConverter : JsonConverter<Distance>
	{
		public override Distance Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
			{
				reader.Skip();
				return null;
			}

			var value = reader.GetString();
			return value == null
				? null
				: new Distance(value);
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
