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
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IGeoDistanceQuery"/>.
	/// JSON format:
	/// <code>
	/// {"distance": "10km", "distance_type": "arc", "field_name": {"lat": 40, "lon": -70}}
	/// </code>
	/// The field name is dynamic - any property that isn't a known keyword is the field name.
	/// </summary>
	internal sealed class GeoDistanceQueryConverter : JsonConverter<IGeoDistanceQuery>
	{
		private static readonly GeoLocationConverter LocationConverter = new GeoLocationConverter();

		public override IGeoDistanceQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IGeoDistanceQuery but got {reader.TokenType}");

			var query = new GeoDistanceQuery();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName token");

				var propertyName = reader.GetString();
				reader.Read();

				switch (propertyName)
				{
					case "_name":
						query.Name = reader.GetString();
						break;
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "distance":
						query.Distance = reader.GetString();
						break;
					case "distance_type":
						query.DistanceType = ParseDistanceType(reader.GetString());
						break;
					case "validation_method":
						query.ValidationMethod = ParseValidationMethod(reader.GetString());
						break;
					default:
						query.Field = new Field(propertyName);
						query.Location = LocationConverter.Read(ref reader, typeof(GeoLocation), options);
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

			if (value.Boost.HasValue)
			{
				writer.WritePropertyName("boost");
				writer.WriteNumberValue(value.Boost.Value);
			}

			if (value.ValidationMethod.HasValue)
			{
				writer.WritePropertyName("validation_method");
				writer.WriteStringValue(GetValidationMethodString(value.ValidationMethod.Value));
			}

			if (value.Distance != null)
			{
				writer.WritePropertyName("distance");
				writer.WriteStringValue(value.Distance.ToString());
			}

			if (value.DistanceType.HasValue)
			{
				writer.WritePropertyName("distance_type");
				writer.WriteStringValue(GetDistanceTypeString(value.DistanceType.Value));
			}

			if (value.Field != null)
			{
				writer.WritePropertyName(value.Field.ToString());
				LocationConverter.Write(writer, value.Location, options);
			}

			writer.WriteEndObject();
		}

		private static GeoDistanceType? ParseDistanceType(string value) => value?.ToLowerInvariant() switch
		{
			"arc" => GeoDistanceType.Arc,
			"plane" => GeoDistanceType.Plane,
			_ => null
		};

		private static string GetDistanceTypeString(GeoDistanceType value) => value switch
		{
			GeoDistanceType.Arc => "arc",
			GeoDistanceType.Plane => "plane",
			_ => value.ToString().ToLowerInvariant()
		};

		private static GeoValidationMethod? ParseValidationMethod(string value) => value?.ToLowerInvariant() switch
		{
			"coerce" => GeoValidationMethod.Coerce,
			"ignore_malformed" => GeoValidationMethod.IgnoreMalformed,
			"strict" => GeoValidationMethod.Strict,
			_ => null
		};

		private static string GetValidationMethodString(GeoValidationMethod value) => value switch
		{
			GeoValidationMethod.Coerce => "coerce",
			GeoValidationMethod.IgnoreMalformed => "ignore_malformed",
			GeoValidationMethod.Strict => "strict",
			_ => value.ToString().ToLowerInvariant()
		};
	}
}
