/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>GeoLocationFormatter</c>. A
	/// <see cref="GeoLocation"/> is read from either a Well-Known-Text <c>POINT (lon lat)</c> string or a
	/// GeoJson-style <c>{ "lat": .., "lon": .. }</c> object, and written back in whichever format the
	/// instance carries.
	/// </summary>
	internal class GeoLocationConverter : JsonConverter<GeoLocation>
	{
		public override GeoLocation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					var wkt = reader.GetString();
					using (var tokenizer = new WellKnownTextTokenizer(new StringReader(wkt)))
					{
						var token = tokenizer.NextToken();
						if (token != TokenType.Word)
							throw new GeoWKTException(
								$"Expected word but found {tokenizer.TokenString()}", tokenizer.LineNumber, tokenizer.Position);

						var type = tokenizer.TokenValue.ToUpperInvariant();
						if (type != GeoShapeType.Point)
							throw new GeoWKTException(
								$"Expected {GeoShapeType.Point} but found {type}", tokenizer.LineNumber, tokenizer.Position);

						if (GeoWKTReader.NextEmptyOrOpen(tokenizer) == TokenType.Word)
							return null;

						var lon = GeoWKTReader.NextNumber(tokenizer);
						var lat = GeoWKTReader.NextNumber(tokenizer);
						return new GeoLocation(lat, lon) { Format = GeoFormat.WellKnownText };
					}
				case JsonTokenType.StartObject:
				{
					double lat = 0;
					double lon = 0;
					while (reader.Read())
					{
						if (reader.TokenType == JsonTokenType.EndObject)
							break;

						var propertyName = reader.GetString();
						reader.Read();
						switch (propertyName)
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

					return new GeoLocation(lat, lon) { Format = GeoFormat.GeoJson };
				}
				default:
					reader.Skip();
					return null;
			}
		}

		public override void Write(Utf8JsonWriter writer, GeoLocation value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			switch (value.Format)
			{
				case GeoFormat.GeoJson:
					writer.WriteStartObject();
					writer.WritePropertyName("lat");
					writer.WriteNumberValue(value.Latitude);
					writer.WritePropertyName("lon");
					writer.WriteNumberValue(value.Longitude);
					writer.WriteEndObject();
					break;
				case GeoFormat.WellKnownText:
					var lon = value.Longitude.ToString(CultureInfo.InvariantCulture);
					var lat = value.Latitude.ToString(CultureInfo.InvariantCulture);
					var length = GeoShapeType.Point.Length + lon.Length + lat.Length + 4;
					var builder = new StringBuilder(length)
						.Append(GeoShapeType.Point)
						.Append(" (")
						.Append(lon)
						.Append(" ")
						.Append(lat)
						.Append(")");
					writer.WriteStringValue(builder.ToString());
					break;
			}
		}
	}
}
