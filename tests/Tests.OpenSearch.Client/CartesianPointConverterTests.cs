/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="CartesianPointConverter"/>. A <see cref="CartesianPoint"/> serializes into
	/// one of four shapes depending on how it was constructed/parsed: object (<c>{"x":X,"y":Y}</c>), array
	/// (<c>[X,Y]</c>), Well-Known Text (<c>"POINT (X Y)"</c>), or a coordinate string (<c>"X,Y"</c>). Reading accepts
	/// objects/arrays (with an optional ignored third coordinate) and strings (WKT or comma-separated coordinates).
	/// </summary>
	public class CartesianPointConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new CartesianPointConverter());
			return options;
		}

		// ---- read: object ----

		[U] public void Read_Object()
		{
			var point = JsonSerializer.Deserialize<CartesianPoint>(@"{""x"":1.5,""y"":2.5}", Options());
			point.Should().NotBeNull();
			point.X.Should().Be(1.5f);
			point.Y.Should().Be(2.5f);
		}

		[U] public void Read_Object_WithZ_IgnoresZ()
		{
			var point = JsonSerializer.Deserialize<CartesianPoint>(@"{""x"":1.5,""y"":2.5,""z"":9.9}", Options());
			point.X.Should().Be(1.5f);
			point.Y.Should().Be(2.5f);
		}

		[U] public void Read_Object_UnknownProperty_Throws()
		{
			Action act = () => JsonSerializer.Deserialize<CartesianPoint>(@"{""x"":1.5,""bogus"":2.5}", Options());
			act.Should().Throw<JsonException>();
		}

		// ---- read: array ----

		[U] public void Read_Array()
		{
			var point = JsonSerializer.Deserialize<CartesianPoint>(@"[1.5,2.5]", Options());
			point.X.Should().Be(1.5f);
			point.Y.Should().Be(2.5f);
		}

		[U] public void Read_Array_WithZ_IgnoresZ()
		{
			var point = JsonSerializer.Deserialize<CartesianPoint>(@"[1.5,2.5,9.9]", Options());
			point.X.Should().Be(1.5f);
			point.Y.Should().Be(2.5f);
		}

		[U] public void Read_Array_TooManyCoordinates_Throws()
		{
			Action act = () => JsonSerializer.Deserialize<CartesianPoint>(@"[1,2,3,4]", Options());
			act.Should().Throw<JsonException>();
		}

		// ---- read: string ----

		[U] public void Read_CoordinateString()
		{
			var point = JsonSerializer.Deserialize<CartesianPoint>(@"""1.5,2.5""", Options());
			point.X.Should().Be(1.5f);
			point.Y.Should().Be(2.5f);
		}

		[U] public void Read_WellKnownText()
		{
			var point = JsonSerializer.Deserialize<CartesianPoint>(@"""POINT (1.5 2.5)""", Options());
			point.X.Should().Be(1.5f);
			point.Y.Should().Be(2.5f);
		}

		// ---- read: invalid ----

		[U] public void Read_UnexpectedToken_Throws()
		{
			Action act = () => JsonSerializer.Deserialize<CartesianPoint>(@"true", Options());
			act.Should().Throw<JsonException>();
		}

		// ---- write ----

		[U] public void Write_Object_Default()
		{
			// Default Format is Object.
			var json = JsonSerializer.Serialize(new CartesianPoint(1.5f, 2.5f), Options());
			json.Should().Be(@"{""x"":1.5,""y"":2.5}");
		}

		[U] public void Write_Array()
		{
			var point = JsonSerializer.Deserialize<CartesianPoint>(@"[1.5,2.5]", Options());
			var json = JsonSerializer.Serialize(point, Options());
			json.Should().Be(@"[1.5,2.5]");
		}

		[U] public void Write_WellKnownText()
		{
			var point = CartesianPoint.FromWellKnownText("POINT (1.5 2.5)");
			var json = JsonSerializer.Serialize(point, Options());
			json.Should().Be(@"""POINT (1.5 2.5)""");
		}

		[U] public void Write_CoordinateString()
		{
			var point = CartesianPoint.FromCoordinates("1.5,2.5");
			var json = JsonSerializer.Serialize(point, Options());
			json.Should().Be(@"""1.5,2.5""");
		}

		[U] public void Write_Null() =>
			JsonSerializer.Serialize<CartesianPoint>(null, Options()).Should().Be("null");

		// ---- round trip ----

		[U] public void RoundTrip_Object()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new CartesianPoint(3.25f, 4.75f), options);
			var back = JsonSerializer.Deserialize<CartesianPoint>(json, options);
			back.Should().Be(new CartesianPoint(3.25f, 4.75f));
		}

		[U] public void RoundTrip_CoordinateString()
		{
			var options = Options();
			var point = CartesianPoint.FromCoordinates("3.25,4.75");
			var json = JsonSerializer.Serialize(point, options);
			var back = JsonSerializer.Deserialize<CartesianPoint>(json, options);
			back.X.Should().Be(3.25f);
			back.Y.Should().Be(4.75f);
		}

		[U] public void RoundTrip_WellKnownText()
		{
			var options = Options();
			var point = CartesianPoint.FromWellKnownText("POINT (3.25 4.75)");
			var json = JsonSerializer.Serialize(point, options);
			var back = JsonSerializer.Deserialize<CartesianPoint>(json, options);
			back.X.Should().Be(3.25f);
			back.Y.Should().Be(4.75f);
		}
	}
}
