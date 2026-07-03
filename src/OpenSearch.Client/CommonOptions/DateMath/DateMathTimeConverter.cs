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
	/// A <see cref="System.Text.Json"/> converter for <see cref="DateMathTime"/>, replacing the
	/// vendored Utf8Json <c>DateMathTimeFormatter</c> as part of #388. Serialized as its string
	/// expression (e.g. <c>"2d"</c>).
	/// </summary>
	internal sealed class DateMathTimeConverter : JsonConverter<DateMathTime>
	{
		public override void Write(Utf8JsonWriter writer, DateMathTime value, JsonSerializerOptions options)
		{
			if (value is null)
				writer.WriteNullValue();
			else
				writer.WriteStringValue(value.ToString());
		}

		public override DateMathTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;
			return reader.GetString();
		}
	}
}
