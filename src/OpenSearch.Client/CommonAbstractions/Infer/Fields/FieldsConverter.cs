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
	/// A <see cref="System.Text.Json"/> converter for <see cref="Fields"/> (a list of
	/// <see cref="Field"/>), replacing the vendored Utf8Json formatter as part of #388. Written as a
	/// JSON array of inferred field-name strings; read back from an array of strings. Constructed with
	/// the connection settings for inference (decision D1).
	/// </summary>
	internal sealed class FieldsConverter : JsonConverter<Fields>
	{
		private readonly IConnectionSettingsValues _settings;

		public FieldsConverter(IConnectionSettingsValues settings) =>
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

		public override Fields Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new Field(reader.GetString());
				case JsonTokenType.StartArray:
				{
					var fields = new List<Field>();
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndArray) break;
						if (reader.TokenType == JsonTokenType.String)
							fields.Add(new Field(reader.GetString()));
						else
							reader.Skip();
					}
					return fields.Count == 0 ? null : new Fields(fields);
				}
				default:
					throw new JsonException($"Cannot deserialize {nameof(Fields)} from token {reader.TokenType}.");
			}
		}

		public override void Write(Utf8JsonWriter writer, Fields value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			foreach (var field in value)
				writer.WriteStringValue(_settings.Inferrer.Field(field));
			writer.WriteEndArray();
		}
	}
}
