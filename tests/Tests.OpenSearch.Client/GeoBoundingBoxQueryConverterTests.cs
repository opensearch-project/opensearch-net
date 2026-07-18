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
	/// Behavioural tests for the settings-aware <see cref="GeoBoundingBoxQueryConverter"/>: a field-name query
	/// serialized as <c>{ options..., "&lt;field&gt;": { ...bounding box... } }</c> where the field key is resolved through
	/// the runtime <c>Inferrer</c>. Mirrors the legacy Utf8Json <c>GeoBoundingBoxQueryFormatter</c>.
	/// </summary>
	public class GeoBoundingBoxQueryConverterTests
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
			options.Converters.Add(new GeoBoundingBoxQueryConverter(settings));
			options.Converters.Add(new ReadAsConverterFactory());
			options.Converters.Add(new GeoLocationConverter());
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		private static IGeoBoundingBoxQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<IGeoBoundingBoxQuery>(json, Options());

		[U] public void Serialize_WrapsBodyUnderFieldName()
		{
			IGeoBoundingBoxQuery query = new GeoBoundingBoxQuery
			{
				Field = "location",
				BoundingBox = new BoundingBox
				{
					TopLeft = new GeoLocation(34, -34),
					BottomRight = new GeoLocation(-34, 34)
				}
			};

			var json = JsonSerializer.Serialize(query, Options());

			json.Should().Be(
				@"{""location"":{""bottom_right"":{""lat"":-34,""lon"":34},""top_left"":{""lat"":34,""lon"":-34}}}");
		}

		[U] public void Serialize_EmitsCommonOptionsAndEnums()
		{
			IGeoBoundingBoxQuery query = new GeoBoundingBoxQuery
			{
				Field = "location",
				Name = "named_query",
				Boost = 1.1,
				Type = GeoExecution.Indexed,
				ValidationMethod = GeoValidationMethod.Strict,
				BoundingBox = new BoundingBox { WellKnownText = "BBOX (-34, 34, 34, -34)" }
			};

			var json = JsonSerializer.Serialize(query, Options());

			json.Should().Contain(@"""_name"":""named_query""")
				.And.Contain(@"""boost"":1.1")
				.And.Contain(@"""validation_method"":""strict""")
				.And.Contain(@"""type"":""indexed""")
				.And.Contain(@"""location"":{""wkt"":""BBOX (-34, 34, 34, -34)""}");
		}

		[U] public void Deserialize_ReadsFieldAndBody()
		{
			var query = Deserialize(
				@"{""location"":{""top_left"":{""lat"":34,""lon"":-34},""bottom_right"":{""lat"":-34,""lon"":34}}}");

			query.Should().NotBeNull();
			query.Field.Name.Should().Be("location");
			query.BoundingBox.TopLeft.Latitude.Should().Be(34);
			query.BoundingBox.BottomRight.Longitude.Should().Be(34);
		}

		[U] public void Deserialize_ReadsCommonOptionsAndEnums()
		{
			var query = Deserialize(
				@"{""_name"":""n"",""boost"":2.5,""validation_method"":""strict"",""type"":""indexed"",""location"":{""wkt"":""BBOX (0,0,0,0)""}}");

			query.Name.Should().Be("n");
			query.Boost.Should().Be(2.5);
			query.ValidationMethod.Should().Be(GeoValidationMethod.Strict);
			query.Type.Should().Be(GeoExecution.Indexed);
			query.Field.Name.Should().Be("location");
			query.BoundingBox.WellKnownText.Should().Be("BBOX (0,0,0,0)");
		}

		[U] public void Deserialize_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Serialize_Null_WritesNull() =>
			JsonSerializer.Serialize<IGeoBoundingBoxQuery>(null, Options()).Should().Be("null");

		[U] public void RoundTrip_PreservesFieldWrapperAndBody()
		{
			IGeoBoundingBoxQuery original = new GeoBoundingBoxQuery
			{
				Field = "location",
				Name = "n",
				Boost = 1.1,
				ValidationMethod = GeoValidationMethod.IgnoreMalformed,
				Type = GeoExecution.Memory,
				BoundingBox = new BoundingBox
				{
					TopLeft = new GeoLocation(34, -34),
					BottomRight = new GeoLocation(-34, 34)
				}
			};

			var json = JsonSerializer.Serialize(original, Options());
			var back = JsonSerializer.Deserialize<IGeoBoundingBoxQuery>(json, Options());

			back.Field.Name.Should().Be("location");
			back.Name.Should().Be("n");
			back.Boost.Should().Be(1.1);
			back.ValidationMethod.Should().Be(GeoValidationMethod.IgnoreMalformed);
			back.Type.Should().Be(GeoExecution.Memory);
			back.BoundingBox.TopLeft.Latitude.Should().Be(34);
			back.BoundingBox.BottomRight.Longitude.Should().Be(34);
		}
	}
}
