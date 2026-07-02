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
	/// Converter for <see cref="IDistanceFeatureQuery"/>.
	/// JSON format (field is an explicit property, not a wrapper key):
	/// <code>
	/// {"field": "...", "origin": "...", "pivot": "...", "boost": 1.0}
	/// </code>
	/// Origin can be a GeoCoordinate or DateMath; Pivot can be Distance or Time.
	/// </summary>
	internal sealed class DistanceFeatureQueryConverter : JsonConverter<IDistanceFeatureQuery>
	{
		private static readonly GeoLocationConverter LocationConverter = new GeoLocationConverter();

		public override IDistanceFeatureQuery Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IDistanceFeatureQuery but got {reader.TokenType}");

			var query = new DistanceFeatureQuery();

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					throw new JsonException("Expected PropertyName");

				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "field":
						query.Field = new Field(reader.GetString());
						break;
					case "origin":
						query.Origin = ReadOrigin(ref reader, options);
						break;
					case "pivot":
						query.Pivot = ReadPivot(ref reader);
						break;
					case "boost":
						query.Boost = reader.GetDouble();
						break;
					case "_name":
						query.Name = reader.GetString();
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return query;
		}

		public override void Write(Utf8JsonWriter writer, IDistanceFeatureQuery value, JsonSerializerOptions options)
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

			if (value.Field != null)
			{
				writer.WritePropertyName("field");
				writer.WriteStringValue(value.Field.ToString());
			}

			if (value.Origin != null)
			{
				writer.WritePropertyName("origin");
				WriteOrigin(writer, value.Origin, options);
			}

			if (value.Pivot != null)
			{
				writer.WritePropertyName("pivot");
				WritePivot(writer, value.Pivot);
			}

			writer.WriteEndObject();
		}

		private static Union<GeoCoordinate, DateMath> ReadOrigin(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				// Could be DateMath or a lat,lon string - treat as DateMath
				var str = reader.GetString();
				return new Union<GeoCoordinate, DateMath>(DateMath.FromString(str));
			}

			if (reader.TokenType == JsonTokenType.StartObject)
			{
				var loc = LocationConverter.Read(ref reader, typeof(GeoLocation), options);
				if (loc != null)
					return new Union<GeoCoordinate, DateMath>(new GeoCoordinate(loc.Latitude, loc.Longitude));
			}

			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var loc = LocationConverter.Read(ref reader, typeof(GeoLocation), options);
				if (loc != null)
					return new Union<GeoCoordinate, DateMath>(new GeoCoordinate(loc.Latitude, loc.Longitude));
			}

			return null;
		}

		private static Union<Distance, Time> ReadPivot(ref Utf8JsonReader reader)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				var str = reader.GetString();
				// Try to determine if it's a distance or time
				// Distances typically end with m, km, mi, etc.
				// Times typically end with d, h, m, s, ms, micros, nanos
				// Simple heuristic: if it parses as a Time, use Time
				return new Union<Distance, Time>(str);
			}

			reader.Skip();
			return null;
		}

		private static void WriteOrigin(Utf8JsonWriter writer, Union<GeoCoordinate, DateMath> origin, JsonSerializerOptions options)
		{
			if (origin.Tag == 0 && origin.Item1 != null)
				LocationConverter.Write(writer, origin.Item1, options);
			else if (origin.Tag == 1 && origin.Item2 != null)
				writer.WriteStringValue(origin.Item2.ToString());
			else
				writer.WriteNullValue();
		}

		private static void WritePivot(Utf8JsonWriter writer, Union<Distance, Time> pivot)
		{
			if (pivot.Tag == 0 && pivot.Item1 != null)
				writer.WriteStringValue(pivot.Item1.ToString());
			else if (pivot.Tag == 1 && pivot.Item2 != null)
				writer.WriteStringValue(pivot.Item2.ToString());
			else
				writer.WriteNullValue();
		}
	}
}
