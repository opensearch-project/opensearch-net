/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Text.Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>FieldFormatter</c>. A <see cref="Field"/> resolves its
	/// name through the runtime <c>Inferrer</c> (hence a <see cref="SettingsAwareConverter{T}"/>) and is serialized
	/// either as a plain JSON string (when it carries no <see cref="Field.Format"/>) or as an object
	/// <c>{ "field": ..., "format": ... }</c> when a format is present. On read a JSON string yields a
	/// <see cref="Field"/> from the string, an object reads the <c>field</c>/<c>boost</c>/<c>format</c> members, a
	/// <c>null</c> token yields <c>null</c>, and any other token throws — matching the legacy formatter exactly.
	/// <see cref="Field"/> is also used as a dictionary key, so the legacy <c>IObjectPropertyNameFormatter&lt;Field&gt;</c>
	/// behavior (which always serialized the resolved field name as a string) is preserved via
	/// <see cref="ReadAsPropertyName"/>/<see cref="WriteAsPropertyName"/>.
	/// </summary>
	internal class FieldConverter : SettingsAwareConverter<Field>
	{
		public FieldConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override Field Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return new Field(reader.GetString());
				case JsonTokenType.StartObject:
					string field = null;
					double? boost = null;
					string format = null;

					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject)
							break;

						if (reader.TokenType != JsonTokenType.PropertyName)
							continue;

						var property = reader.GetString();
						reader.Read();
						switch (property)
						{
							case "field":
								field = reader.GetString();
								break;
							case "boost":
								boost = reader.GetDouble();
								break;
							case "format":
								format = reader.GetString();
								break;
							default:
								reader.Skip();
								break;
						}
					}

					return new Field(field, boost, format);
				default:
					throw new JsonException($"Cannot deserialize {typeof(Field).FullName} from {reader.TokenType}");
			}
		}

		public override void Write(Utf8JsonWriter writer, Field value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			var fieldName = Settings.Inferrer.Field(value);
			if (string.IsNullOrEmpty(value.Format))
				writer.WriteStringValue(fieldName);
			else
			{
				writer.WriteStartObject();
				writer.WriteString("field", fieldName);
				writer.WriteString("format", value.Format);
				writer.WriteEndObject();
			}
		}

		public override Field ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.PropertyName)
				return new Field(reader.GetString());

			reader.Skip();
			return null;
		}

		public override void WriteAsPropertyName(Utf8JsonWriter writer, Field value, JsonSerializerOptions options)
		{
			var fieldName = Settings.Inferrer.Field(value);
			writer.WritePropertyName(fieldName);
		}
	}
}
