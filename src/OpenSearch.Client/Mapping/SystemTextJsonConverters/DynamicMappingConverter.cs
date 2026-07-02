/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.Mapping.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="Union{TFirst, TSecond}"/> where TFirst is bool and TSecond is DynamicMapping.
	/// Handles: boolean true/false, or string "strict"/"true"/"false".
	/// </summary>
	internal sealed class DynamicMappingConverter : JsonConverter<Union<bool, DynamicMapping>>
	{
		public override Union<bool, DynamicMapping> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			switch (reader.TokenType)
			{
				case JsonTokenType.True:
					return new Union<bool, DynamicMapping>(true);
				case JsonTokenType.False:
					return new Union<bool, DynamicMapping>(false);
				case JsonTokenType.String:
					var str = reader.GetString();
					switch (str?.ToLowerInvariant())
					{
						case "true":
							return new Union<bool, DynamicMapping>(true);
						case "false":
							return new Union<bool, DynamicMapping>(false);
						case "strict":
							return new Union<bool, DynamicMapping>(DynamicMapping.Strict);
						default:
							return null;
					}
				default:
					throw new JsonException($"Cannot deserialize Union<bool, DynamicMapping> from token '{reader.TokenType}'");
			}
		}

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
					// DynamicMapping.Strict -> "strict"
					writer.WriteStringValue("strict");
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}
	}
}
