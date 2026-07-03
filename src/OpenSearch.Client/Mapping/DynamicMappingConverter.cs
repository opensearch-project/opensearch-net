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
	/// A <see cref="System.Text.Json"/> converter for the mapping <c>dynamic</c> setting
	/// (<see cref="Union{Boolean,DynamicMapping}"/>), replacing the vendored Utf8Json
	/// <c>DynamicMappingFormatter</c> as part of #388. Written as a boolean or the string
	/// <c>"strict"</c>; on read <c>true</c>/<c>false</c> (as bool or string) map to the boolean arm and
	/// <c>"strict"</c> maps to <see cref="DynamicMapping.Strict"/>.
	/// </summary>
	internal sealed class DynamicMappingConverter : JsonConverter<Union<bool, DynamicMapping>>
	{
		public override void Write(Utf8JsonWriter writer, Union<bool, DynamicMapping> value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					writer.WriteBooleanValue(value.Item1);
					break;
				case 1:
					// DynamicMapping is a [StringEnum]; the registered enum converter writes "strict".
					JsonSerializer.Serialize(writer, value.Item2, options);
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}

		public override Union<bool, DynamicMapping> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.True:
				case JsonTokenType.False:
					return new Union<bool, DynamicMapping>(reader.GetBoolean());
				case JsonTokenType.String:
					switch (reader.GetString())
					{
						case "true": return new Union<bool, DynamicMapping>(true);
						case "false": return new Union<bool, DynamicMapping>(false);
						case "strict": return new Union<bool, DynamicMapping>(DynamicMapping.Strict);
						default: return null;
					}
				default:
					throw new JsonException($"Cannot parse Union<bool, DynamicMapping> from token '{reader.TokenType}'");
			}
		}
	}
}
