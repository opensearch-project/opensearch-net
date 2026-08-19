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
	/// System.Text.Json replacement for the legacy Utf8Json <c>NormalizerFormatter</c>. Deserializes an
	/// <see cref="INormalizer"/> as the concrete <see cref="CustomNormalizer"/> (the only normalizer shape), and
	/// serializes via the <see cref="ICustomNormalizer"/> contract.
	/// </summary>
	internal class NormalizerConverter : JsonConverter<INormalizer>
	{
		public override INormalizer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			return JsonSerializer.Deserialize<CustomNormalizer>(ref reader, options);
		}

		public override void Write(Utf8JsonWriter writer, INormalizer value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			JsonSerializer.Serialize(writer, value as ICustomNormalizer, options);
		}
	}
}
