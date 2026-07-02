/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Globalization;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="GeoLocation"/>.
	/// Supports multiple read formats:
	/// <list type="bullet">
	///   <item>Object: <c>{"lat": 40.0, "lon": -70.0}</c></item>
	///   <item>Array: <c>[-70.0, 40.0]</c> (GeoJSON order: lon, lat)</item>
	///   <item>String: <c>"40.0,-70.0"</c> (lat,lon) or geohash</item>
	/// </list>
	/// Write format is always object: <c>{"lat": ..., "lon": ...}</c>
	/// </summary>
	internal sealed class GeoLocationConverter : JsonConverter<GeoLocation>
	{
		public override GeoLocation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;

				case JsonTokenType.String:
					return ReadFromString(ref reader);

				case JsonTokenType.StartArray:
					return ReadFromArray(ref reader);

				case JsonTokenType.StartObject:
					return ReadFromObject(ref reader);

				default:
					throw new JsonException($"Unexpected token {reader.TokenType} when reading GeoLocation");
			}
		}

		public override void Write(Utf8JsonWriter writer, GeoLocation value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WriteNumber("lat", value.Latitude);
			writer.WriteNumber("lon", value.Longitude);
			writer.WriteEndObject();
		}

		private static GeoLocation ReadFromString(ref Utf8JsonReader reader)
		{
			var str = reader.GetString();
			if (string.IsNullOrEmpty(str))
				return null;

			var parts = str.Split(',');
			if (parts.Length == 2
				&& double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var lat)
				&& double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var lon))
			{
				return new GeoLocation(lat, lon);
			}

			// Treat as geohash - return null as geohash decoding is not implemented here
			return null;
		}

		private static GeoLocation ReadFromArray(ref Utf8JsonReader reader)
		{
			// GeoJSON order: [lon, lat]
			reader.Read(); // Move to first element
			if (reader.TokenType == JsonTokenType.EndArray)
				return null;

			var lon = reader.GetDouble();
			reader.Read();
			var lat = reader.GetDouble();
			reader.Read(); // Move past EndArray

			return new GeoLocation(lat, lon);
		}

		private static GeoLocation ReadFromObject(ref Utf8JsonReader reader)
		{
			double lat = 0;
			double lon = 0;

			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				if (reader.TokenType != JsonTokenType.PropertyName)
					continue;

				var prop = reader.GetString();
				reader.Read();

				switch (prop)
				{
					case "lat":
						lat = reader.GetDouble();
						break;
					case "lon":
						lon = reader.GetDouble();
						break;
					default:
						reader.Skip();
						break;
				}
			}

			return new GeoLocation(lat, lon);
		}
	}
}
