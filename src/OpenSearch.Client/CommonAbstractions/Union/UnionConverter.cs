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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Union{TFirst,TSecond}"/>, replacing
	/// the vendored Utf8Json <c>UnionFormatter</c> as part of #388. The encapsulated value is written
	/// directly (its own registered converter handles the shape); on read the first type is attempted,
	/// falling back to the second, mirroring the original best-effort behaviour.
	/// </summary>
	internal sealed class UnionConverter<TFirst, TSecond> : JsonConverter<Union<TFirst, TSecond>>
	{
		public override void Write(Utf8JsonWriter writer, Union<TFirst, TSecond> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					JsonSerializer.Serialize(writer, value.Item1, options);
					break;
				case 1:
					JsonSerializer.Serialize(writer, value.Item2, options);
					break;
				default:
					throw new JsonException($"Unrecognized {nameof(Union<TFirst, TSecond>)} tag value: {value.Tag}");
			}
		}

		public override Union<TFirst, TSecond> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var raw = document.RootElement.GetRawText();

			try
			{
				var first = JsonSerializer.Deserialize<TFirst>(raw, options);
				if (first != null) return new Union<TFirst, TSecond>(first);
			}
			catch (JsonException) { }

			try
			{
				var second = JsonSerializer.Deserialize<TSecond>(raw, options);
				if (second != null) return new Union<TFirst, TSecond>(second);
			}
			catch (JsonException) { }

			return null;
		}
	}
}
