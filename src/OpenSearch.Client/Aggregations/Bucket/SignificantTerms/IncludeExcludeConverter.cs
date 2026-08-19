/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>IncludeExcludeFormatter</c>.
	/// An <see cref="IncludeExclude"/> serializes as an array of strings (<see cref="IncludeExclude.Values"/>) when
	/// values are set, otherwise as a string (<see cref="IncludeExclude.Pattern"/>). On read, an array yields the
	/// values form, a string yields the pattern form, and null yields <c>null</c>; any other token throws.
	/// </summary>
	internal class IncludeExcludeConverter : JsonConverter<IncludeExclude>
	{
		public override IncludeExclude Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.StartArray:
					var values = JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
					return new IncludeExclude(values);
				case JsonTokenType.String:
					return new IncludeExclude(reader.GetString());
				default:
					throw new JsonException($"Unexpected token {reader.TokenType} when deserializing {nameof(IncludeExclude)}");
			}
		}

		public override void Write(Utf8JsonWriter writer, IncludeExclude value, JsonSerializerOptions options)
		{
			if (value == null)
				writer.WriteNullValue();
			else if (value.Values != null)
				JsonSerializer.Serialize(writer, value.Values, options);
			else
				writer.WriteStringValue(value.Pattern);
		}
	}
}
