/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the settings-aware <see cref="GeoPolygonQueryConverter"/>: a field-name query
	/// serialized as <c>{ options..., "&lt;field&gt;": { "points": [ &lt;locations&gt; ] } }</c> where the field key is resolved
	/// through the runtime <c>Inferrer</c>. Mirrors the legacy Utf8Json <c>GeoPolygonQueryFormatter</c>.
	/// </summary>
	public class GeoPolygonQueryConverterTests
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
			options.Converters.Add(new GeoPolygonQueryConverter(settings));
			options.Converters.Add(new GeoLocationConverter());
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		private static IGeoPolygonQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<IGeoPolygonQuery>(json, Options());

		[U] public void Serialize_WrapsPointsUnderFieldNameAndPointsKey()
		{
			IGeoPolygonQuery query = new GeoPolygonQuery
			{
				Field = "location",
				Points = new[] { new GeoLocation(45, -45), new GeoLocation(-34, 34) }
			};

			var json = JsonSerializer.Serialize(query, Options());

			json.Should().Be(
				@"{""location"":{""points"":[{""lat"":45.0,""lon"":-45.0},{""lat"":-34.0,""lon"":34.0}]}}");
		}

		[U] public void Serialize_EmitsCommonOptionsAndEnums()
		{
			IGeoPolygonQuery query = new GeoPolygonQuery
			{
				Field = "location",
				Name = "named_query",
				Boost = 1.1,
				ValidationMethod = GeoValidationMethod.Strict,
				Points = new[] { new GeoLocation(45, -45) }
			};

			var json = JsonSerializer.Serialize(query, Options());

			json.Should().Contain(@"""_name"":""named_query""")
				.And.Contain(@"""boost"":1.1")
				.And.Contain(@"""validation_method"":""strict""")
				.And.Contain(@"""location"":{""points"":[{""lat"":45.0,""lon"":-45.0}]}");
		}

		[U] public void Deserialize_ReadsFieldAndPoints()
		{
			var query = Deserialize(
				@"{""location"":{""points"":[{""lat"":45,""lon"":-45},{""lat"":-34,""lon"":34}]}}");

			query.Should().NotBeNull();
			query.Field.Name.Should().Be("location");
			query.Points.Should().HaveCount(2);
			query.Points.First().Latitude.Should().Be(45);
		}

		[U] public void Deserialize_ReadsCommonOptionsAndEnums()
		{
			var query = Deserialize(
				@"{""_name"":""n"",""boost"":2.5,""validation_method"":""coerce"",""location"":{""points"":[{""lat"":1,""lon"":2}]}}");

			query.Name.Should().Be("n");
			query.Boost.Should().Be(2.5);
			query.ValidationMethod.Should().Be(GeoValidationMethod.Coerce);
			query.Field.Name.Should().Be("location");
			query.Points.Should().HaveCount(1);
		}

		[U] public void Deserialize_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Serialize_Null_WritesNull() =>
			JsonSerializer.Serialize<IGeoPolygonQuery>(null, Options()).Should().Be("null");

		[U] public void RoundTrip_PreservesFieldWrapperAndPoints()
		{
			IGeoPolygonQuery original = new GeoPolygonQuery
			{
				Field = "location",
				Name = "n",
				Boost = 1.1,
				ValidationMethod = GeoValidationMethod.Strict,
				Points = new[] { new GeoLocation(45, -45), new GeoLocation(-34, 34), new GeoLocation(70, -70) }
			};

			var json = JsonSerializer.Serialize(original, Options());
			var back = JsonSerializer.Deserialize<IGeoPolygonQuery>(json, Options());

			back.Field.Name.Should().Be("location");
			back.Name.Should().Be("n");
			back.Boost.Should().Be(1.1);
			back.ValidationMethod.Should().Be(GeoValidationMethod.Strict);
			back.Points.Should().HaveCount(3);
			back.Points.Last().Latitude.Should().Be(70);
		}
	}
}
