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
	/// System.Text.Json replacement for the legacy Utf8Json <c>GeoDistanceQueryFormatter</c>.
	///
	/// <see cref="IGeoDistanceQuery"/> is a field-name query serialized as
	/// <c>{ "_name"?, "boost"?, "validation_method"?, "distance"?, "distance_type"?, "&lt;field&gt;": &lt;location&gt; }</c>:
	/// the common query options sit at the top level alongside a single member whose key is the query's
	/// <see cref="IFieldNameQuery.Field"/> resolved through the runtime <c>Inferrer</c> (hence a
	/// <see cref="SettingsAwareConverter{T}"/>) and whose value is the <see cref="GeoLocation"/> body. On read the
	/// property key that is not one of the recognized options is taken verbatim as the field name (no inference,
	/// matching the legacy formatter). Enum, <see cref="Distance"/>, and location members are delegated to
	/// <see cref="JsonSerializer"/> so the registered converters apply.
	/// </summary>
	internal class GeoDistanceQueryConverter : SettingsAwareConverter<IGeoDistanceQuery>
	{
		public GeoDistanceQueryConverter(IConnectionSettingsValues settings) : base(settings) { }

		public override IGeoDistanceQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				return null;

			var query = new GeoDistanceQuery();

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
					case "distance":
						query.Distance = JsonSerializer.Deserialize<Distance>(ref reader, options);
						break;
					case "distance_type":
						query.DistanceType = JsonSerializer.Deserialize<GeoDistanceType>(ref reader, options);
						break;
					default:
						query.Field = propertyName;
						query.Location = JsonSerializer.Deserialize<GeoLocation>(ref reader, options);
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, IGeoDistanceQuery value, JsonSerializerOptions options)
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
				OpenSearch.Net.Serialization.Converters.RealNumberFormat.WriteDouble(writer, value.Boost.Value);
			}

			if (value.ValidationMethod != null)
			{
				writer.WritePropertyName("validation_method");
				JsonSerializer.Serialize(writer, value.ValidationMethod.Value, options);
			}

			if (value.Distance != null)
			{
				writer.WritePropertyName("distance");
				JsonSerializer.Serialize(writer, value.Distance, options);
			}

			if (value.DistanceType != null)
			{
				writer.WritePropertyName("distance_type");
				JsonSerializer.Serialize(writer, value.DistanceType.Value, options);
			}

			writer.WritePropertyName(Settings.Inferrer.Field(value.Field));
			JsonSerializer.Serialize(writer, value.Location, options);

			writer.WriteEndObject();
		}
	}
}
