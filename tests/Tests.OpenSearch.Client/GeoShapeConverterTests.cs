/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="GeoShapeConverter"/>: dispatches an <see cref="IGeoShape"/> on the JSON
	/// <c>type</c> field (matched case-insensitively) to the concrete geo shape, reading <c>coordinates</c> (or
	/// <c>geometries</c>, plus <c>radius</c> for a circle) at the appropriate nesting depth. A JSON string is parsed as
	/// Well-Known Text, and a WKT-format shape is written back as a string. Mirrors the legacy Utf8Json
	/// <c>GeoShapeFormatter</c>.
	/// </summary>
	public class GeoShapeConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				PropertyNameCaseInsensitive = true,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new GeoCoordinateConverter());
			options.Converters.Add(new GeoShapeConverter());
			return options;
		}

		private static IGeoShape Deserialize(string json) =>
			JsonSerializer.Deserialize<IGeoShape>(json, Options());

		[U] public void Deserialize_Point()
		{
			var shape = Deserialize(@"{""type"":""point"",""coordinates"":[10.0,20.0]}");
			shape.Should().BeOfType<PointGeoShape>();
			var point = (PointGeoShape)shape;
			point.Coordinates.Longitude.Should().Be(10.0);
			point.Coordinates.Latitude.Should().Be(20.0);
		}

		[U] public void Deserialize_MultiPoint()
		{
			var shape = Deserialize(@"{""type"":""multipoint"",""coordinates"":[[10.0,20.0],[30.0,40.0]]}");
			shape.Should().BeOfType<MultiPointGeoShape>();
			((MultiPointGeoShape)shape).Coordinates.Should().HaveCount(2);
		}

		[U] public void Deserialize_LineString()
		{
			var shape = Deserialize(@"{""type"":""linestring"",""coordinates"":[[10.0,20.0],[30.0,40.0]]}");
			shape.Should().BeOfType<LineStringGeoShape>();
			((LineStringGeoShape)shape).Coordinates.Should().HaveCount(2);
		}

		[U] public void Deserialize_Polygon()
		{
			var shape = Deserialize(@"{""type"":""polygon"",""coordinates"":[[[10.0,20.0],[30.0,40.0],[50.0,60.0],[10.0,20.0]]]}");
			shape.Should().BeOfType<PolygonGeoShape>();
			((PolygonGeoShape)shape).Coordinates.First().Should().HaveCount(4);
		}

		[U] public void Deserialize_MultiPolygon()
		{
			var shape = Deserialize(@"{""type"":""multipolygon"",""coordinates"":[[[[10.0,20.0],[30.0,40.0],[50.0,60.0],[10.0,20.0]]]]}");
			shape.Should().BeOfType<MultiPolygonGeoShape>();
			((MultiPolygonGeoShape)shape).Coordinates.Should().HaveCount(1);
		}

		[U] public void Deserialize_Envelope()
		{
			var shape = Deserialize(@"{""type"":""envelope"",""coordinates"":[[10.0,20.0],[30.0,5.0]]}");
			shape.Should().BeOfType<EnvelopeGeoShape>();
			((EnvelopeGeoShape)shape).Coordinates.Should().HaveCount(2);
		}

		[U] public void Deserialize_Circle()
		{
			var shape = Deserialize(@"{""type"":""circle"",""coordinates"":[10.0,20.0],""radius"":""100m""}");
			shape.Should().BeOfType<CircleGeoShape>();
			var circle = (CircleGeoShape)shape;
			circle.Radius.Should().Be("100m");
			circle.Coordinates.Longitude.Should().Be(10.0);
		}

		[U] public void Deserialize_GeometryCollection()
		{
			var shape = Deserialize(
				@"{""type"":""geometrycollection"",""geometries"":[{""type"":""point"",""coordinates"":[10.0,20.0]}]}");
			shape.Should().BeOfType<GeometryCollection>();
			var collection = (GeometryCollection)shape;
			collection.Geometries.Should().HaveCount(1);
			collection.Geometries.First().Should().BeOfType<PointGeoShape>();
		}

		[U] public void Deserialize_TypeIsCaseInsensitive()
		{
			var shape = Deserialize(@"{""type"":""Point"",""coordinates"":[10.0,20.0]}");
			shape.Should().BeOfType<PointGeoShape>();
		}

		[U] public void Deserialize_UnknownType_ReturnsNull()
		{
			Deserialize(@"{""type"":""unknown"",""coordinates"":[10.0,20.0]}").Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Deserialize_WellKnownTextString()
		{
			var shape = Deserialize(@"""POINT (10 20)""");
			shape.Should().BeOfType<PointGeoShape>();
			var point = (PointGeoShape)shape;
			point.Coordinates.Longitude.Should().Be(10.0);
			point.Coordinates.Latitude.Should().Be(20.0);
		}

		[U] public void Serialize_Point()
		{
			IGeoShape shape = new PointGeoShape(new GeoCoordinate(20.0, 10.0));
			var json = JsonSerializer.Serialize(shape, Options());
			json.Should().Be(@"{""type"":""point"",""coordinates"":[10,20]}");
		}

		[U] public void Serialize_Circle()
		{
			IGeoShape shape = new CircleGeoShape(new GeoCoordinate(20.0, 10.0), "100m");
			var json = JsonSerializer.Serialize(shape, Options());
			json.Should().Be(@"{""type"":""circle"",""coordinates"":[10,20],""radius"":""100m""}");
		}

		[U] public void Serialize_Envelope()
		{
			IGeoShape shape = new EnvelopeGeoShape(new[] { new GeoCoordinate(20.0, 10.0), new GeoCoordinate(5.0, 30.0) });
			var json = JsonSerializer.Serialize(shape, Options());
			json.Should().Be(@"{""type"":""envelope"",""coordinates"":[[10,20],[30,5]]}");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<IGeoShape>(null, Options()).Should().Be("null");
		}

		[U] public void Serialize_WellKnownTextFormatShape_WritesString()
		{
			// GeoWKTReader marks the returned shape as GeoFormat.WellKnownText, so it must serialize back as a WKT
			// string rather than a GeoJSON object, matching the legacy formatter.
			var wktShape = GeoWKTReader.Read("POINT (10 20)");
			var json = JsonSerializer.Serialize(wktShape, Options());
			json.Should().StartWith("\"").And.EndWith("\"");
			json.Should().Contain("POINT");
		}

		[U] public void RoundTrip_Polygon_PreservesNesting()
		{
			IGeoShape shape = new PolygonGeoShape(new[]
			{
				new[]
				{
					new GeoCoordinate(20.0, 10.0), new GeoCoordinate(40.0, 30.0),
					new GeoCoordinate(60.0, 50.0), new GeoCoordinate(20.0, 10.0)
				}
			});
			var back = JsonSerializer.Deserialize<IGeoShape>(JsonSerializer.Serialize(shape, Options()), Options());
			back.Should().BeOfType<PolygonGeoShape>();
			((PolygonGeoShape)back).Coordinates.First().Should().HaveCount(4);
		}

		[U] public void RoundTrip_GeometryCollection_PreservesNestedShapes()
		{
			IGeoShape shape = new GeometryCollection(new IGeoShape[]
			{
				new PointGeoShape(new GeoCoordinate(20.0, 10.0)),
				new LineStringGeoShape(new[] { new GeoCoordinate(20.0, 10.0), new GeoCoordinate(40.0, 30.0) })
			});
			var back = JsonSerializer.Deserialize<IGeoShape>(JsonSerializer.Serialize(shape, Options()), Options());
			back.Should().BeOfType<GeometryCollection>();
			var geometries = ((GeometryCollection)back).Geometries.ToList();
			geometries.Should().HaveCount(2);
			geometries[0].Should().BeOfType<PointGeoShape>();
			geometries[1].Should().BeOfType<LineStringGeoShape>();
		}
	}
}
