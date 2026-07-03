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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Slices"/>, replacing the vendored
	/// Utf8Json <c>SlicesFormatter</c> (a <c>Union&lt;long, string&gt;</c>) as part of #388. A numeric
	/// value is written/read as its <see cref="long"/> arm, a string value as its <see cref="string"/>
	/// arm (e.g. <c>"auto"</c>).
	/// </summary>
	internal sealed class SlicesConverter : JsonConverter<Slices>
	{
		public override void Write(Utf8JsonWriter writer, Slices value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Tag)
			{
				case 0:
					writer.WriteNumberValue(value.Item1);
					break;
				case 1:
					writer.WriteStringValue(value.Item2);
					break;
				default:
					writer.WriteNullValue();
					break;
			}
		}

		public override Slices Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.Number:
					return new Slices(reader.GetInt64());
				case JsonTokenType.String:
					return new Slices(reader.GetString());
				default:
					throw new JsonException($"Cannot deserialize {nameof(Slices)} from token {reader.TokenType}.");
			}
		}
	}
}
