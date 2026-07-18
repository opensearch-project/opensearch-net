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
	/// System.Text.Json replacement for the legacy Utf8Json <c>GeoBoundingBoxQueryFormatter</c>.
	///
	/// <see cref="IGeoBoundingBoxQuery"/> is a field-name query serialized as
	/// <c>{ "_name"?, "boost"?, "validation_method"?, "type"?, "&lt;field&gt;": { ...bounding box... } }</c>: the
	/// common query options sit at the top level alongside a single member whose key is the query's
	/// <see cref="IFieldNameQuery.Field"/> resolved through the runtime <c>Inferrer</c> (hence a
	/// <see cref="SettingsAwareConverter{T}"/>) and whose value is the <see cref="IBoundingBox"/> body. On read the
	/// property key that is not one of the recognized options is taken verbatim as the field name (no inference,
	/// matching the legacy formatter). Enum and body members are delegated to <see cref="JsonSerializer"/> so the
	/// registered converters apply.
	/// </summary>
	internal class GeoBoundingBoxQueryConverter : SettingsAwareConverter<IGeoBoundingBoxQuery>
	{
		public GeoBoundingBoxQueryConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IGeoBoundingBoxQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				return null;

			var query = new GeoBoundingBoxQuery();

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
					case "type":
						query.Type = JsonSerializer.Deserialize<GeoExecution>(ref reader, options);
						break;
					default:
						query.Field = propertyName;
						query.BoundingBox = JsonSerializer.Deserialize<IBoundingBox>(ref reader, options);
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, IGeoBoundingBoxQuery value, JsonSerializerOptions options)
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

			if (value.Type != null)
			{
				writer.WritePropertyName("type");
				JsonSerializer.Serialize(writer, value.Type.Value, options);
			}

			writer.WritePropertyName(Settings.Inferrer.Field(value.Field));
			JsonSerializer.Serialize(writer, value.BoundingBox, options);

			writer.WriteEndObject();
		}
	}
}
