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
	/// System.Text.Json replacement for the legacy Utf8Json <c>NullableStringBooleanFormatter</c>. Handles a
	/// <see cref="Nullable{Boolean}"/> whose value may arrive from OpenSearch as a JSON boolean, a string
	/// containing a boolean (e.g. <c>"true"</c>), or null.
	/// </summary>
	internal class NullableStringBooleanConverter : JsonConverter<bool?>
	{
		public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.True:
				case JsonTokenType.False:
					return reader.GetBoolean();
				case JsonTokenType.String:
					var s = reader.GetString();
					if (!bool.TryParse(s, out var b))
						throw new JsonException($"Cannot parse {typeof(bool).FullName} from: {s}");

					return b;
				default:
					throw new JsonException($"Cannot parse {typeof(bool).FullName} from: {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteBooleanValue(value.Value);
		}
	}
}
