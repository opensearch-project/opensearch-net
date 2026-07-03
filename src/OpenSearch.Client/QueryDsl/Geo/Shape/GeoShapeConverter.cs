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
	/// A <see cref="System.Text.Json"/> converter for <see cref="IGeoShape"/>, replacing the vendored
	/// Utf8Json <c>GeoShapeFormatter</c> as part of #388. Shapes are written either as Well-Known Text
	/// strings (when <see cref="GeoFormat.WellKnownText"/> is set) or as GeoJSON objects with a
	/// <c>type</c> discriminator plus <c>coordinates</c>/<c>radius</c>/<c>geometries</c>. Coordinates are
	/// delegated to the globally registered <see cref="GeoCoordinateConverter"/>, and geometry collections
	/// recurse back through this converter.
	/// </summary>
	internal sealed class GeoShapeConverter : JsonConverter<IGeoShape>
	{
		public override void Write(Utf8JsonWriter writer, IGeoShape value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			if (value is GeoShapeBase shapeBase && shapeBase.Format == GeoFormat.WellKnownText)
			{
				writer.WriteStringValue(GeoWKTWriter.Write(shapeBase));
				return;
			}

			writer.WriteStartObject();
			writer.WriteString("type", value.Type);

			switch (value)
			{
				case IPointGeoShape point:
					writer.WritePropertyName("coordinates");
					JsonSerializer.Serialize(writer, point.Coordinates, options);
					break;
				case IMultiPointGeoShape multiPoint:
					writer.WritePropertyName("coordinates");
					JsonSerializer.Serialize(writer, multiPoint.Coordinates, options);
					break;
				case ILineStringGeoShape lineString:
					writer.WritePropertyName("coordinates");
					JsonSerializer.Serialize(writer, lineString.Coordinates, options);
					break;
				case IMultiLineStringGeoShape multiLineString:
					writer.WritePropertyName("coordinates");
					JsonSerializer.Serialize(writer, multiLineString.Coordinates, options);
					break;
				case IPolygonGeoShape polygon:
					writer.WritePropertyName("coordinates");
					JsonSerializer.Serialize(writer, polygon.Coordinates, options);
					break;
				case IMultiPolygonGeoShape multiPolygon:
					writer.WritePropertyName("coordinates");
					JsonSerializer.Serialize(writer, multiPolygon.Coordinates, options);
					break;
				case IEnvelopeGeoShape envelope:
					writer.WritePropertyName("coordinates");
					JsonSerializer.Serialize(writer, envelope.Coordinates, options);
					break;
				case ICircleGeoShape circle:
					writer.WritePropertyName("coordinates");
					JsonSerializer.Serialize(writer, circle.Coordinates, options);
					writer.WriteString("radius", circle.Radius);
					break;
				case IGeometryCollection collection:
					writer.WritePropertyName("geometries");
					JsonSerializer.Serialize(writer, collection.Geometries, options);
					break;
			}

			writer.WriteEndObject();
		}

		public override IGeoShape Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			switch (reader.TokenType)
			{
				case JsonTokenType.Null:
					return null;
				case JsonTokenType.String:
					return GeoWKTReader.Read(reader.GetString());
				default:
					return ReadShape(ref reader, options);
			}
		}

		private static IGeoShape ReadShape(ref Utf8JsonReader reader, JsonSerializerOptions options)
		{
			using var document = JsonDocument.ParseValue(ref reader);
			var root = document.RootElement;
			if (root.ValueKind != JsonValueKind.Object) return null;

			string typeName = null;
			if (root.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
				typeName = typeElement.GetString()?.ToUpperInvariant();

			switch (typeName)
			{
				case GeoShapeType.Circle:
					return ParseCircleGeoShape(root, options);
				case GeoShapeType.Envelope:
					return new EnvelopeGeoShape { Coordinates = GetCoordinates<List<GeoCoordinate>>(root, options) };
				case GeoShapeType.LineString:
					return new LineStringGeoShape { Coordinates = GetCoordinates<List<GeoCoordinate>>(root, options) };
				case GeoShapeType.MultiLineString:
					return new MultiLineStringGeoShape { Coordinates = GetCoordinates<List<List<GeoCoordinate>>>(root, options) };
				case GeoShapeType.Point:
					return new PointGeoShape { Coordinates = GetCoordinates<GeoCoordinate>(root, options) };
				case GeoShapeType.MultiPoint:
					return new MultiPointGeoShape { Coordinates = GetCoordinates<List<GeoCoordinate>>(root, options) };
				case GeoShapeType.Polygon:
					return new PolygonGeoShape { Coordinates = GetCoordinates<List<List<GeoCoordinate>>>(root, options) };
				case GeoShapeType.MultiPolygon:
					return new MultiPolygonGeoShape { Coordinates = GetCoordinates<List<List<List<GeoCoordinate>>>>(root, options) };
				case GeoShapeType.GeometryCollection:
					return ParseGeometryCollection(root, options);
				default:
					return null;
			}
		}

		private static CircleGeoShape ParseCircleGeoShape(JsonElement root, JsonSerializerOptions options)
		{
			GeoCoordinate coordinate = null;
			if (root.TryGetProperty("coordinates", out var coordinatesElement))
				coordinate = coordinatesElement.Deserialize<GeoCoordinate>(options);

			string radius = null;
			if (root.TryGetProperty("radius", out var radiusElement) && radiusElement.ValueKind == JsonValueKind.String)
				radius = radiusElement.GetString();

			return new CircleGeoShape { Coordinates = coordinate, Radius = radius };
		}

		private static GeometryCollection ParseGeometryCollection(JsonElement root, JsonSerializerOptions options)
		{
			IEnumerable<IGeoShape> geometries = null;
			if (root.TryGetProperty("geometries", out var geometriesElement))
				geometries = geometriesElement.Deserialize<List<IGeoShape>>(options);

			return new GeometryCollection { Geometries = geometries };
		}

		private static T GetCoordinates<T>(JsonElement root, JsonSerializerOptions options)
		{
			if (root.TryGetProperty("coordinates", out var coordinatesElement))
				return coordinatesElement.Deserialize<T>(options);

			return default;
		}
	}
}
