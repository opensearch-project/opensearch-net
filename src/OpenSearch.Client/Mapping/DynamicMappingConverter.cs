/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>DynamicMappingFormatter</c>.
	///
	/// A <see cref="Union{Boolean, DynamicMapping}"/> mapping value is serialized as either a JSON boolean
	/// (<c>true</c>/<c>false</c>) or a JSON string naming the <see cref="DynamicMapping"/> enum member
	/// (currently only <c>"strict"</c>). On read:
	/// <list type="bullet">
	/// <item><description>a JSON boolean yields the boolean branch;</description></item>
	/// <item><description>the string <c>"true"</c>/<c>"false"</c> yields the boolean branch (matching the legacy
	/// automata which accepted these spellings), and <c>"strict"</c> yields <see cref="DynamicMapping.Strict"/>;</description></item>
	/// <item><description>an unrecognised string yields <c>null</c> (exactly as the legacy formatter did);</description></item>
	/// <item><description>a <c>null</c> token yields <c>null</c>, and any other token throws.</description></item>
	/// </list>
	/// Writes preserve the original branch exactly.
	/// </summary>
	internal class DynamicMappingConverter : JsonConverter<Union<bool, DynamicMapping>>
	{
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
					throw new JsonException($"Cannot parse Union<bool, DynamicMapping> from token '{reader.TokenType}'");
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
					writer.WriteStringValue(value.Item2.GetStringValue());
					break;
			}
		}
	}
}
