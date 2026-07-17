/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>GeoShapeFormatter</c> (and
	/// <c>GeoShapeFormatter&lt;TShape&gt;</c>).
	///
	/// An <see cref="IGeoShape"/> is polymorphic and can arrive in one of three shapes:
	/// <list type="bullet">
	/// <item>a JSON <c>null</c> → <c>null</c>;</item>
	/// <item>a JSON string → parsed as Well-Known Text via <see cref="GeoWKTReader"/>;</item>
	/// <item>a JSON object whose <c>type</c> field selects the concrete geo shape (<c>point</c>, <c>multipoint</c>,
	/// <c>linestring</c>, <c>multilinestring</c>, <c>polygon</c>, <c>multipolygon</c>, <c>envelope</c>, <c>circle</c>,
	/// <c>geometrycollection</c>). The <c>type</c> is matched case-insensitively (upper-invariant), matching the legacy
	/// formatter, then the <c>coordinates</c> (or <c>geometries</c>, plus <c>radius</c> for a circle) are read into the
	/// nesting depth appropriate for the shape.</item>
	/// </list>
	///
	/// Because <see cref="Utf8JsonReader"/> is forward-only and cannot be rewound — unlike the Utf8Json version which
	/// pre-scanned a byte segment for the <c>type</c> field then re-read it — we buffer the value into a
	/// <see cref="JsonDocument"/>, read the discriminator from the DOM and then read the coordinate data from the same
	/// element. Coordinate arrays and nested geometries are deserialized through <see cref="JsonSerializerOptions"/> so
	/// the registered <c>GeoCoordinate</c> converter (and this converter, recursively) are used.
	///
	/// On write we reproduce the legacy output exactly: a shape whose <c>Format</c> is
	/// <see cref="GeoFormat.WellKnownText"/> is written as a WKT string via <see cref="GeoWKTWriter"/>; otherwise an
	/// object with a <c>type</c> field followed by <c>coordinates</c> (or <c>geometries</c>, plus <c>radius</c> for a
	/// circle), dispatched on the runtime shape type.
	/// </summary>
	internal class GeoShapeConverter : JsonConverter<IGeoShape>
	{
		public override bool CanConvert(Type typeToConvert) => typeof(IGeoShape).IsAssignableFrom(typeToConvert);

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
			using var doc = JsonDocument.ParseValue(ref reader);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Object)
				return null;

			var typeName = root.TryGetProperty("type", out var typeElement) && typeElement.ValueKind == JsonValueKind.String
				? typeElement.GetString()?.ToUpperInvariant()
				: null;

			switch (typeName)
			{
				case GeoShapeType.Circle:
					return ParseCircleGeoShape(root, options);
				case GeoShapeType.Envelope:
					return new EnvelopeGeoShape { Coordinates = GetCoordinates<IEnumerable<GeoCoordinate>>(root, options) };
				case GeoShapeType.LineString:
					return new LineStringGeoShape { Coordinates = GetCoordinates<IEnumerable<GeoCoordinate>>(root, options) };
				case GeoShapeType.MultiLineString:
					return new MultiLineStringGeoShape { Coordinates = GetCoordinates<IEnumerable<IEnumerable<GeoCoordinate>>>(root, options) };
				case GeoShapeType.Point:
					return new PointGeoShape { Coordinates = GetCoordinates<GeoCoordinate>(root, options) };
				case GeoShapeType.MultiPoint:
					return new MultiPointGeoShape { Coordinates = GetCoordinates<IEnumerable<GeoCoordinate>>(root, options) };
				case GeoShapeType.Polygon:
					return new PolygonGeoShape { Coordinates = GetCoordinates<IEnumerable<IEnumerable<GeoCoordinate>>>(root, options) };
				case GeoShapeType.MultiPolygon:
					return new MultiPolygonGeoShape { Coordinates = GetCoordinates<IEnumerable<IEnumerable<IEnumerable<GeoCoordinate>>>>(root, options) };
				case GeoShapeType.GeometryCollection:
					return ParseGeometryCollection(root, options);
				default:
					return null;
			}
		}

		private static GeometryCollection ParseGeometryCollection(JsonElement root, JsonSerializerOptions options)
		{
			var geometries = root.TryGetProperty("geometries", out var g)
				? g.Deserialize<IEnumerable<IGeoShape>>(options)
				: System.Linq.Enumerable.Empty<IGeoShape>();
			return new GeometryCollection { Geometries = geometries };
		}

		private static CircleGeoShape ParseCircleGeoShape(JsonElement root, JsonSerializerOptions options)
		{
			GeoCoordinate coordinate = null;
			string radius = null;

			if (root.TryGetProperty("coordinates", out var c))
				coordinate = c.Deserialize<GeoCoordinate>(options);
			if (root.TryGetProperty("radius", out var r) && r.ValueKind == JsonValueKind.String)
				radius = r.GetString();

			return new CircleGeoShape { Coordinates = coordinate, Radius = radius };
		}

		private static T GetCoordinates<T>(JsonElement root, JsonSerializerOptions options) =>
			root.TryGetProperty("coordinates", out var c)
				? c.Deserialize<T>(options)
				: default;

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
	}
}
