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
	/// System.Text.Json replacement for the legacy Utf8Json <c>GeoPolygonQueryFormatter</c>.
	///
	/// <see cref="IGeoPolygonQuery"/> is a field-name query serialized as
	/// <c>{ "_name"?, "boost"?, "validation_method"?, "&lt;field&gt;": { "points": [ &lt;locations&gt; ] } }</c>: the common
	/// query options sit at the top level alongside a single member whose key is the query's
	/// <see cref="IFieldNameQuery.Field"/> resolved through the runtime <c>Inferrer</c> (hence a
	/// <see cref="SettingsAwareConverter{T}"/>). Unlike the other geo field-name queries, the field body is itself an
	/// object wrapping the point list under a <c>points</c> key. On read the property key that is not one of the
	/// recognized options is taken verbatim as the field name (no inference, matching the legacy formatter); the enum
	/// and point-list members are delegated to <see cref="JsonSerializer"/> so the registered converters apply.
	/// </summary>
	internal class GeoPolygonQueryConverter : SettingsAwareConverter<IGeoPolygonQuery>
	{
		public GeoPolygonQueryConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IGeoPolygonQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				return null;

			var query = new GeoPolygonQuery();

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
					break;

				var propertyName = reader.GetString();
				reader.Read(); // advance to the value

				switch (propertyName)
				{
					case "_name":
						query.Name = reader.GetString();
						break;
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "validation_method":
						query.ValidationMethod = JsonSerializer.Deserialize<GeoValidationMethod>(ref reader, options);
						break;
					default:
						query.Field = propertyName;
						// Field body is a nested object: { "points": [ ... ] }.
						if (reader.TokenType == JsonTokenType.StartObject)
						{
							while (reader.Read())
							{
								if (reader.TokenType == JsonTokenType.EndObject)
									break;

								var innerName = reader.GetString();
								reader.Read(); // advance to the value
								if (innerName == "points")
									query.Points = JsonSerializer.Deserialize<IEnumerable<GeoLocation>>(ref reader, options);
								else
									reader.Skip();
							}
						}
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, IGeoPolygonQuery value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();

			if (!string.IsNullOrEmpty(value.Name))
			{
				writer.WritePropertyName("_name");
				writer.WriteStringValue(value.Name);
			}

			if (value.Boost != null)
			{
				writer.WritePropertyName("boost");
				writer.WriteNumberValue(value.Boost.Value);
			}

			if (value.ValidationMethod != null)
			{
				writer.WritePropertyName("validation_method");
				JsonSerializer.Serialize(writer, value.ValidationMethod.Value, options);
			}

			writer.WritePropertyName(Settings.Inferrer.Field(value.Field));
			writer.WriteStartObject();
			writer.WritePropertyName("points");
			JsonSerializer.Serialize(writer, value.Points, options);
			writer.WriteEndObject();

			writer.WriteEndObject();
		}
	}
}
