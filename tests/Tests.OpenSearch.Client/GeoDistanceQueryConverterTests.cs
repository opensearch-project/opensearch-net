/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for the settings-aware <see cref="GeoDistanceQueryConverter"/>: a field-name query
	/// serialized as <c>{ options..., "&lt;field&gt;": &lt;location&gt; }</c> where the field key is resolved through the
	/// runtime <c>Inferrer</c>. Mirrors the legacy Utf8Json <c>GeoDistanceQueryFormatter</c>.
	/// </summary>
	public class GeoDistanceQueryConverterTests
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
			options.Converters.Add(new GeoDistanceQueryConverter(settings));
			options.Converters.Add(new GeoLocationConverter());
			options.Converters.Add(new DistanceConverter());
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		private static IGeoDistanceQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<IGeoDistanceQuery>(json, Options());

		[U] public void Serialize_WrapsLocationUnderFieldName()
		{
			IGeoDistanceQuery query = new GeoDistanceQuery
			{
				Field = "location",
				Location = new GeoLocation(34, -34),
				Distance = "200m"
			};

			var json = JsonSerializer.Serialize(query, Options());

			json.Should().Be(@"{""distance"":""200m"",""location"":{""lat"":34,""lon"":-34}}");
		}

		[U] public void Serialize_EmitsCommonOptionsAndEnums()
		{
			IGeoDistanceQuery query = new GeoDistanceQuery
			{
				Field = "location",
				Name = "named_query",
				Boost = 1.1,
				DistanceType = GeoDistanceType.Arc,
				ValidationMethod = GeoValidationMethod.IgnoreMalformed,
				Distance = "200m",
				Location = new GeoLocation(34, -34)
			};

			var json = JsonSerializer.Serialize(query, Options());

			json.Should().Contain(@"""_name"":""named_query""")
				.And.Contain(@"""boost"":1.1")
				.And.Contain(@"""validation_method"":""ignore_malformed""")
				.And.Contain(@"""distance"":""200m""")
				.And.Contain(@"""distance_type"":""arc""")
				.And.Contain(@"""location"":{""lat"":34,""lon"":-34}");
		}

		[U] public void Deserialize_ReadsFieldAndLocation()
		{
			var query = Deserialize(@"{""distance"":""200m"",""location"":{""lat"":34,""lon"":-34}}");

			query.Should().NotBeNull();
			query.Field.Name.Should().Be("location");
			query.Distance.ToString().Should().Be("200m");
			query.Location.Latitude.Should().Be(34);
			query.Location.Longitude.Should().Be(-34);
		}

		[U] public void Deserialize_ReadsCommonOptionsAndEnums()
		{
			var query = Deserialize(
				@"{""_name"":""n"",""boost"":2.5,""validation_method"":""strict"",""distance"":""1km"",""distance_type"":""plane"",""location"":{""lat"":1,""lon"":2}}");

			query.Name.Should().Be("n");
			query.Boost.Should().Be(2.5);
			query.ValidationMethod.Should().Be(GeoValidationMethod.Strict);
			query.DistanceType.Should().Be(GeoDistanceType.Plane);
			query.Field.Name.Should().Be("location");
		}

		[U] public void Deserialize_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Serialize_Null_WritesNull() =>
			JsonSerializer.Serialize<IGeoDistanceQuery>(null, Options()).Should().Be("null");

		[U] public void RoundTrip_PreservesFieldWrapperAndBody()
		{
			IGeoDistanceQuery original = new GeoDistanceQuery
			{
				Field = "location",
				Name = "n",
				Boost = 1.1,
				DistanceType = GeoDistanceType.Arc,
				ValidationMethod = GeoValidationMethod.Strict,
				Distance = "200m",
				Location = new GeoLocation(34, -34)
			};

			var json = JsonSerializer.Serialize(original, Options());
			var back = JsonSerializer.Deserialize<IGeoDistanceQuery>(json, Options());

			back.Field.Name.Should().Be("location");
			back.Name.Should().Be("n");
			back.Boost.Should().Be(1.1);
			back.DistanceType.Should().Be(GeoDistanceType.Arc);
			back.ValidationMethod.Should().Be(GeoValidationMethod.Strict);
			back.Distance.ToString().Should().Be("200m");
			back.Location.Latitude.Should().Be(34);
		}
	}
}
