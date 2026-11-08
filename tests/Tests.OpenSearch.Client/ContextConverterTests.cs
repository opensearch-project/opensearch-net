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
	/// Behavioural tests for <see cref="ContextConverter"/>. A <see cref="Context"/> is a union of a category
	/// <c>string</c> or a <see cref="GeoLocation"/> object; the converter probes string first then geo (as the legacy
	/// union formatter did). Null yields/writes null.
	/// </summary>
	public class ContextConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new ContextConverter());
			options.Converters.Add(new GeoLocationConverter());
			return options;
		}

		private static Context Deserialize(string json) =>
			JsonSerializer.Deserialize<Context>(json, Options());

		// ---- read ----

		[U] public void Read_String_BecomesCategory()
		{
			var context = Deserialize(@"""electronics""");
			context.Should().NotBeNull();
			context.Tag.Should().Be(0);
			context.Category.Should().Be("electronics");
			context.Geo.Should().BeNull();
		}

		[U] public void Read_Object_BecomesGeo()
		{
			var context = Deserialize(@"{""lat"":40.5,""lon"":-70.2}");
			context.Should().NotBeNull();
			context.Tag.Should().Be(1);
			context.Geo.Should().NotBeNull();
			context.Geo.Latitude.Should().Be(40.5);
			context.Geo.Longitude.Should().Be(-70.2);
			context.Category.Should().BeNull();
		}

		[U] public void Read_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		// ---- write ----

		[U] public void Write_Category_WritesString()
		{
			var json = JsonSerializer.Serialize(new Context("electronics"), Options());
			json.Should().Be(@"""electronics""");
		}

		[U] public void Write_Geo_WritesObject()
		{
			var geo = new GeoLocation(40.5, -70.2);
			var json = JsonSerializer.Serialize(new Context(geo), Options());
			json.Should().Be(@"{""lat"":40.5,""lon"":-70.2}");
		}

		[U] public void Write_Null_WritesNull() =>
			JsonSerializer.Serialize<Context>(null, Options()).Should().Be("null");

		// ---- round trip ----

		[U] public void RoundTrip_Category()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new Context("books"), options);
			var back = JsonSerializer.Deserialize<Context>(json, options);
			back.Category.Should().Be("books");
		}

		[U] public void RoundTrip_Geo()
		{
			var options = Options();
			var json = JsonSerializer.Serialize(new Context(new GeoLocation(1.5, 2.5)), options);
			var back = JsonSerializer.Deserialize<Context>(json, options);
			back.Geo.Latitude.Should().Be(1.5);
			back.Geo.Longitude.Should().Be(2.5);
		}
	}
}
