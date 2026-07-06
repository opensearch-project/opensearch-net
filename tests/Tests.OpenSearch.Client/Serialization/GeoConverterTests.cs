/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Unit tests for the System.Text.Json <see cref="GeoCoordinateConverter"/> and
	/// <see cref="GeoLocationConverter"/> that replaced the legacy Utf8Json geo formatters.
	/// </summary>
	public class GeoConverterTests
	{
		private static JsonSerializerOptions CoordinateOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new GeoCoordinateConverter());
			return options;
		}

		private static JsonSerializerOptions LocationOptions()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new GeoLocationConverter());
			return options;
		}

		// ----- GeoCoordinate: write -----

		[U] public void Coordinate_Write_TwoDimensional()
		{
			var json = JsonSerializer.Serialize(new GeoCoordinate(2.0, 1.0), CoordinateOptions());
			// Written as [lon, lat]
			json.Should().Be("[1,2]");
		}

		[U] public void Coordinate_Write_ThreeDimensional()
		{
			var json = JsonSerializer.Serialize(new GeoCoordinate(2.0, 1.0, 3.0), CoordinateOptions());
			// Written as [lon, lat, z]
			json.Should().Be("[1,2,3]");
		}

		[U] public void Coordinate_Write_Null()
		{
			var json = JsonSerializer.Serialize<GeoCoordinate>(null, CoordinateOptions());
			json.Should().Be("null");
		}

		// ----- GeoCoordinate: read -----

		[U] public void Coordinate_Read_TwoDimensional()
		{
			var coord = JsonSerializer.Deserialize<GeoCoordinate>("[1,2]", CoordinateOptions());
			coord.Should().NotBeNull();
			coord.Longitude.Should().Be(1.0);
			coord.Latitude.Should().Be(2.0);
			coord.Z.Should().BeNull();
		}

		[U] public void Coordinate_Read_ThreeDimensional()
		{
			var coord = JsonSerializer.Deserialize<GeoCoordinate>("[1,2,3]", CoordinateOptions());
			coord.Should().NotBeNull();
			coord.Longitude.Should().Be(1.0);
			coord.Latitude.Should().Be(2.0);
			coord.Z.Should().Be(3.0);
		}

		[U] public void Coordinate_Read_NonArrayReturnsNull()
		{
			var coord = JsonSerializer.Deserialize<GeoCoordinate>("\"nope\"", CoordinateOptions());
			coord.Should().BeNull();
		}

		[U] public void Coordinate_Read_WrongLengthReturnsNull()
		{
			var coord = JsonSerializer.Deserialize<GeoCoordinate>("[1]", CoordinateOptions());
			coord.Should().BeNull();
		}

		[U] public void Coordinate_RoundTrip_ThreeDimensional()
		{
			var options = CoordinateOptions();
			var original = new GeoCoordinate(2.0, 1.0, 3.0);
			var json = JsonSerializer.Serialize(original, options);
			var back = JsonSerializer.Deserialize<GeoCoordinate>(json, options);
			back.Latitude.Should().Be(original.Latitude);
			back.Longitude.Should().Be(original.Longitude);
			back.Z.Should().Be(original.Z);
		}

		// ----- GeoLocation: write -----

		[U] public void Location_Write_GeoJsonObject()
		{
			var location = new GeoLocation(1.0, 2.0) { Format = GeoFormat.GeoJson };
			var json = JsonSerializer.Serialize(location, LocationOptions());
			json.Should().Be("{\"lat\":1,\"lon\":2}");
		}

		[U] public void Location_Write_WellKnownText()
		{
			var location = new GeoLocation(1.0, 2.0) { Format = GeoFormat.WellKnownText };
			var json = JsonSerializer.Serialize(location, LocationOptions());
			// POINT (lon lat)
			json.Should().Be("\"POINT (2 1)\"");
		}

		[U] public void Location_Write_Null()
		{
			var json = JsonSerializer.Serialize<GeoLocation>(null, LocationOptions());
			json.Should().Be("null");
		}

		// ----- GeoLocation: read -----

		[U] public void Location_Read_GeoJsonObject()
		{
			var location = JsonSerializer.Deserialize<GeoLocation>("{\"lat\":1,\"lon\":2}", LocationOptions());
			location.Should().NotBeNull();
			location.Latitude.Should().Be(1.0);
			location.Longitude.Should().Be(2.0);
		}

		[U] public void Location_Read_ObjectIgnoresUnknownFields()
		{
			var location = JsonSerializer.Deserialize<GeoLocation>(
				"{\"lat\":1,\"extra\":{\"nested\":true},\"lon\":2}", LocationOptions());
			location.Latitude.Should().Be(1.0);
			location.Longitude.Should().Be(2.0);
		}

		[U] public void Location_Read_WellKnownText()
		{
			var location = JsonSerializer.Deserialize<GeoLocation>("\"POINT (2 1)\"", LocationOptions());
			location.Should().NotBeNull();
			location.Longitude.Should().Be(2.0);
			location.Latitude.Should().Be(1.0);
		}

		[U] public void Location_Read_Null()
		{
			var location = JsonSerializer.Deserialize<GeoLocation>("null", LocationOptions());
			location.Should().BeNull();
		}

		[U] public void Location_RoundTrip_GeoJson()
		{
			var options = LocationOptions();
			var original = new GeoLocation(1.5, 2.5) { Format = GeoFormat.GeoJson };
			var json = JsonSerializer.Serialize(original, options);
			var back = JsonSerializer.Deserialize<GeoLocation>(json, options);
			back.Latitude.Should().Be(original.Latitude);
			back.Longitude.Should().Be(original.Longitude);
		}

		[U] public void Location_RoundTrip_WellKnownText()
		{
			var options = LocationOptions();
			var original = new GeoLocation(1.5, 2.5) { Format = GeoFormat.WellKnownText };
			var json = JsonSerializer.Serialize(original, options);
			var back = JsonSerializer.Deserialize<GeoLocation>(json, options);
			back.Latitude.Should().Be(original.Latitude);
			back.Longitude.Should().Be(original.Longitude);
		}
	}
}
