/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="DistanceFeatureQueryConverter"/>: a distance-feature query is a flat object with
	/// literal keys <c>_name</c>/<c>boost</c>/<c>field</c>/<c>origin</c>/<c>pivot</c>, where origin is a
	/// <c>Union&lt;GeoCoordinate, DateMath&gt;</c> and pivot a <c>Union&lt;Distance, Time&gt;</c>. Covers both the date
	/// (string origin / time pivot) and geo (coordinate origin / distance pivot) shapes, the common options, null and
	/// round-trip.
	/// </summary>
	public class DistanceFeatureQueryConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new DistanceFeatureQueryConverter());
			// Sub-converters that the field / origin / pivot values rely on.
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new GeoCoordinateConverter());
			options.Converters.Add(new DateMathConverter());
			options.Converters.Add(new DateMathExpressionConverter());
			options.Converters.Add(new DistanceConverter());
			options.Converters.Add(new TimeConverter());
			return options;
		}

		private static IDistanceFeatureQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<IDistanceFeatureQuery>(json, Options());

		private static string Serialize(IDistanceFeatureQuery value) =>
			JsonSerializer.Serialize(value, Options());

		// --- date shape ---

		[U] public void Deserialize_DateShape()
		{
			var query = Deserialize(@"{""boost"":1.1,""field"":""startedOn"",""origin"":""now"",""pivot"":""7d""}");
			query.Should().BeOfType<DistanceFeatureQuery>();
			query.Boost.Should().Be(1.1);
			query.Field.Name.Should().Be("startedOn");
			query.Origin.Should().NotBeNull();
			query.Pivot.Should().NotBeNull();
		}

		[U] public void Serialize_DateShape()
		{
			IDistanceFeatureQuery query = new DistanceFeatureQuery
			{
				Boost = 1.1, Field = "startedOn", Origin = DateMath.FromString("now"), Pivot = new Time("7d")
			};
			Serialize(query).Should().Be(@"{""boost"":1.1,""field"":""startedOn"",""origin"":""now"",""pivot"":""7d""}");
		}

		// --- geo shape (with _name) ---

		[U] public void Deserialize_GeoShape_WithName()
		{
			var query = Deserialize(@"{""_name"":""name"",""boost"":1.1,""field"":""location"",""origin"":[-70.0,70.0],""pivot"":""100mi""}");
			query.Name.Should().Be("name");
			query.Field.Name.Should().Be("location");
			query.Origin.Should().NotBeNull();
			query.Pivot.Should().NotBeNull();
		}

		[U] public void Serialize_GeoShape_WithName()
		{
			IDistanceFeatureQuery query = new DistanceFeatureQuery
			{
				Name = "name",
				Boost = 1.1,
				Field = "location",
				Origin = new GeoCoordinate(70, -70),
				Pivot = new Distance(100, DistanceUnit.Miles)
			};
			// GeoCoordinate longitude/latitude are doubles; System.Text.Json renders whole-number doubles without a
			// trailing ".0" (e.g. -70 not -70.0), which OpenSearch accepts identically.
			Serialize(query).Should().Be(@"{""_name"":""name"",""boost"":1.1,""field"":""location"",""origin"":[-70,70],""pivot"":""100mi""}");
		}

		// --- null ---

		[U] public void Deserialize_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Serialize_Null_WritesNull() =>
			JsonSerializer.Serialize<IDistanceFeatureQuery>(null, Options()).Should().Be("null");

		// --- round-trip ---

		[U] public void RoundTrip_DateShape()
		{
			IDistanceFeatureQuery original = new DistanceFeatureQuery
			{
				Boost = 2.0, Field = "startedOn", Origin = DateMath.FromString("now"), Pivot = new Time("7d")
			};
			var back = Deserialize(Serialize(original));
			back.Field.Name.Should().Be("startedOn");
			back.Boost.Should().Be(2.0);
			back.Origin.Should().NotBeNull();
			back.Pivot.Should().NotBeNull();
		}
	}
}
