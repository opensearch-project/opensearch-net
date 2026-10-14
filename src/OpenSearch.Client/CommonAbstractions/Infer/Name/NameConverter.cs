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
	/// System.Text.Json counterpart of <see cref="NameFormatter"/>. Serializes a <see cref="Name"/> as a JSON string.
	/// </summary>
	internal class NameConverter : JsonConverter<Name>
	{
		public override Name Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
				return new Name(reader.GetString());

			reader.Skip();
			return null;
		}

		public override void Write(Utf8JsonWriter writer, Name value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStringValue(value.ToString());
		}

		public override Name ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			new Name(reader.GetString());

		public override void WriteAsPropertyName(Utf8JsonWriter writer, Name value, JsonSerializerOptions options) =>
			writer.WritePropertyName(value?.ToString() ?? string.Empty);
	}
}
