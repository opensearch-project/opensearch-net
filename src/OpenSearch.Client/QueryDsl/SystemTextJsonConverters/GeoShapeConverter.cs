/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Collections.Generic;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Client;

namespace OpenSearch.Client.QueryDsl.SystemTextJsonConverters
{
	/// <summary>
	/// Converter for <see cref="IGeoShape"/>.
	/// Handles polymorphic GeoJSON shapes discriminated by the "type" field.
	/// Supported types: point, multipoint, linestring, multilinestring, polygon,
	/// multipolygon, geometrycollection, envelope, circle.
	/// </summary>
	internal sealed class GeoShapeConverter : JsonConverter<IGeoShape>
	{
		public override IGeoShape Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.Null)
				return null;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException($"Expected StartObject for IGeoShape but got {reader.TokenType}");

			// Read into a JsonDocument to inspect the "type" discriminator
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (!root.TryGetProperty("type", out var typeProp))
				throw new JsonException("GeoShape JSON must contain a 'type' property");

			var typeName = typeProp.GetString()?.ToUpperInvariant();

			switch (typeName)
			{
				case "POINT":
					return ReadPointGeoShape(root);
				case "MULTIPOINT":
					return ReadMultiPointGeoShape(root);
				case "LINESTRING":
					return ReadLineStringGeoShape(root);
				case "MULTILINESTRING":
					return ReadMultiLineStringGeoShape(root);
				case "POLYGON":
					return ReadPolygonGeoShape(root);
				case "MULTIPOLYGON":
					return ReadMultiPolygonGeoShape(root);
				case "GEOMETRYCOLLECTION":
					return ReadGeometryCollection(root, options);
				case "ENVELOPE":
					return ReadEnvelopeGeoShape(root);
				case "CIRCLE":
					return ReadCircleGeoShape(root);
				default:
					throw new JsonException($"Unknown geo shape type: {typeName}");
			}
		}

		public override void Write(Utf8JsonWriter writer, IGeoShape value, JsonSerializerOptions options)
		{
			if (value == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartObject();
			writer.WriteString("type", value.Type);

			switch (value)
			{
				case IPointGeoShape point:
					writer.WritePropertyName("coordinates");
					WriteCoordinate(writer, point.Coordinates);
					break;
				case IMultiPointGeoShape multiPoint:
					writer.WritePropertyName("coordinates");
					WriteCoordinateArray(writer, multiPoint.Coordinates);
					break;
				case ILineStringGeoShape lineString:
					writer.WritePropertyName("coordinates");
					WriteCoordinateArray(writer, lineString.Coordinates);
					break;
				case IMultiLineStringGeoShape multiLineString:
					writer.WritePropertyName("coordinates");
					WriteCoordinateArrayArray(writer, multiLineString.Coordinates);
					break;
				case IPolygonGeoShape polygon:
					writer.WritePropertyName("coordinates");
					WriteCoordinateArrayArray(writer, polygon.Coordinates);
					break;
				case IMultiPolygonGeoShape multiPolygon:
					writer.WritePropertyName("coordinates");
					WriteCoordinateArrayArrayArray(writer, multiPolygon.Coordinates);
					break;
				case IEnvelopeGeoShape envelope:
					writer.WritePropertyName("coordinates");
					WriteCoordinateArray(writer, envelope.Coordinates);
					break;
				case ICircleGeoShape circle:
					writer.WritePropertyName("coordinates");
					WriteCoordinate(writer, circle.Coordinates);
					writer.WriteString("radius", circle.Radius);
					break;
				case IGeometryCollection collection:
					writer.WritePropertyName("geometries");
					writer.WriteStartArray();
					if (collection.Geometries != null)
					{
						foreach (var geometry in collection.Geometries)
							Write(writer, geometry, options);
					}
					writer.WriteEndArray();
					break;
			}

			writer.WriteEndObject();
		}

		private static PointGeoShape ReadPointGeoShape(JsonElement root)
		{
			var coords = root.GetProperty("coordinates");
			return new PointGeoShape(ReadCoordinate(coords));
		}

		private static MultiPointGeoShape ReadMultiPointGeoShape(JsonElement root)
		{
			var coords = root.GetProperty("coordinates");
			return new MultiPointGeoShape(ReadCoordinateArray(coords));
		}

		private static LineStringGeoShape ReadLineStringGeoShape(JsonElement root)
		{
			var coords = root.GetProperty("coordinates");
			return new LineStringGeoShape(ReadCoordinateArray(coords));
		}

		private static MultiLineStringGeoShape ReadMultiLineStringGeoShape(JsonElement root)
		{
			var coords = root.GetProperty("coordinates");
			return new MultiLineStringGeoShape(ReadCoordinateArrayArray(coords));
		}

		private static PolygonGeoShape ReadPolygonGeoShape(JsonElement root)
		{
			var coords = root.GetProperty("coordinates");
			return new PolygonGeoShape(ReadCoordinateArrayArray(coords));
		}

		private static MultiPolygonGeoShape ReadMultiPolygonGeoShape(JsonElement root)
		{
			var coords = root.GetProperty("coordinates");
			var result = new List<IEnumerable<IEnumerable<GeoCoordinate>>>();
			foreach (var polygon in coords.EnumerateArray())
				result.Add(ReadCoordinateArrayArray(polygon));
			return new MultiPolygonGeoShape(result);
		}

		private IGeoShape ReadGeometryCollection(JsonElement root, JsonSerializerOptions options)
		{
			var geometries = new List<IGeoShape>();
			if (root.TryGetProperty("geometries", out var geomArray))
			{
				foreach (var geom in geomArray.EnumerateArray())
				{
					var raw = geom.GetRawText();
					var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(raw));
					reader.Read();
					var shape = Read(ref reader, typeof(IGeoShape), options);
					if (shape != null)
						geometries.Add(shape);
				}
			}
			return new GeometryCollection(geometries);
		}

		private static EnvelopeGeoShape ReadEnvelopeGeoShape(JsonElement root)
		{
			var coords = root.GetProperty("coordinates");
			return new EnvelopeGeoShape(ReadCoordinateArray(coords));
		}

		private static CircleGeoShape ReadCircleGeoShape(JsonElement root)
		{
			var coords = root.GetProperty("coordinates");
			var radius = root.TryGetProperty("radius", out var r) ? r.GetString() : null;
			return new CircleGeoShape(ReadCoordinate(coords), radius);
		}

		private static GeoCoordinate ReadCoordinate(JsonElement element)
		{
			var values = new List<double>();
			foreach (var item in element.EnumerateArray())
				values.Add(item.GetDouble());

			if (values.Count >= 3)
				return new GeoCoordinate(values[1], values[0], values[2]);
			if (values.Count == 2)
				return new GeoCoordinate(values[1], values[0]);
			return null;
		}

		private static List<GeoCoordinate> ReadCoordinateArray(JsonElement element)
		{
			var result = new List<GeoCoordinate>();
			foreach (var item in element.EnumerateArray())
				result.Add(ReadCoordinate(item));
			return result;
		}

		private static List<IEnumerable<GeoCoordinate>> ReadCoordinateArrayArray(JsonElement element)
		{
			var result = new List<IEnumerable<GeoCoordinate>>();
			foreach (var item in element.EnumerateArray())
				result.Add(ReadCoordinateArray(item));
			return result;
		}

		private static void WriteCoordinate(Utf8JsonWriter writer, GeoCoordinate coord)
		{
			if (coord == null)
			{
				writer.WriteNullValue();
				return;
			}

			writer.WriteStartArray();
			writer.WriteNumberValue(coord.Longitude);
			writer.WriteNumberValue(coord.Latitude);
			if (coord.Z.HasValue)
				writer.WriteNumberValue(coord.Z.Value);
			writer.WriteEndArray();
		}

		private static void WriteCoordinateArray(Utf8JsonWriter writer, IEnumerable<GeoCoordinate> coords)
		{
			writer.WriteStartArray();
			if (coords != null)
			{
				foreach (var coord in coords)
					WriteCoordinate(writer, coord);
			}
			writer.WriteEndArray();
		}

		private static void WriteCoordinateArrayArray(Utf8JsonWriter writer, IEnumerable<IEnumerable<GeoCoordinate>> coords)
		{
			writer.WriteStartArray();
			if (coords != null)
			{
				foreach (var ring in coords)
					WriteCoordinateArray(writer, ring);
			}
			writer.WriteEndArray();
		}

		private static void WriteCoordinateArrayArrayArray(Utf8JsonWriter writer, IEnumerable<IEnumerable<IEnumerable<GeoCoordinate>>> coords)
		{
			writer.WriteStartArray();
			if (coords != null)
			{
				foreach (var polygon in coords)
					WriteCoordinateArrayArray(writer, polygon);
			}
			writer.WriteEndArray();
		}
	}
}
