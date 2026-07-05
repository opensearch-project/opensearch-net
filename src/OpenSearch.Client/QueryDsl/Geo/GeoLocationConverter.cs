/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// A <see cref="System.Text.Json"/> converter for <see cref="GeoLocation"/>, replacing the
	/// vendored Utf8Json <c>GeoLocationFormatter</c> as part of #388. Serialized as
	/// <c>{ "lat": …, "lon": … }</c> by default, or as a Well-Known Text <c>POINT (lon lat)</c> string
	/// when the value's <see cref="GeoLocation.Format"/> is <see cref="GeoFormat.WellKnownText"/> (which
	/// is set when it was read from a WKT string). Latitude/longitude are get-only and set through the
	/// constructor, so a converter is required on read.
	/// </summary>
	internal sealed class GeoLocationConverter : JsonConverter<GeoLocation>
	{
		public override GeoLocation Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null) return null;

			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;

			switch (root.ValueKind)
			{
				case JsonValueKind.Object:
				{
					double lat = 0, lon = 0;
					foreach (var member in root.EnumerateObject())
					{
						if (member.Name == "lat") lat = member.Value.GetDouble();
						else if (member.Name == "lon") lon = member.Value.GetDouble();
					}
					return new GeoLocation(lat, lon);
				}
				case JsonValueKind.Array when root.GetArrayLength() == 2:
					// GeoJSON order is [lon, lat].
					return new GeoLocation(root[1].GetDouble(), root[0].GetDouble());
				case JsonValueKind.String:
					// Well-Known Text point: POINT (lon lat).
					return ParseWellKnownTextPoint(root.GetString());
				default:
					return null;
			}
		}

		private static GeoLocation ParseWellKnownTextPoint(string wkt)
		{
			if (string.IsNullOrEmpty(wkt)) return null;

			var open = wkt.IndexOf('(');
			var close = wkt.IndexOf(')', open + 1);
			if (open < 0 || close < 0) return null;

			var coordinates = wkt.Substring(open + 1, close - open - 1)
				.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (coordinates.Length < 2) return null;

			if (!double.TryParse(coordinates[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon)
				|| !double.TryParse(coordinates[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat))
				return null;

			return new GeoLocation(lat, lon) { Format = GeoFormat.WellKnownText };
		}

		public override void Write(Utf8JsonWriter writer, GeoLocation value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (value.Format == GeoFormat.WellKnownText)
			{
				var lon = value.Longitude.ToString(CultureInfo.InvariantCulture);
				var lat = value.Latitude.ToString(CultureInfo.InvariantCulture);
				writer.WriteStringValue($"{GeoShapeType.Point} ({lon} {lat})");
				return;
			}

			writer.WriteStartObject();
			writer.WritePropertyName("lat");
			JsonSerializer.Serialize(writer, value.Latitude, options);
			writer.WritePropertyName("lon");
			JsonSerializer.Serialize(writer, value.Longitude, options);
			writer.WriteEndObject();
		}
	}
}
