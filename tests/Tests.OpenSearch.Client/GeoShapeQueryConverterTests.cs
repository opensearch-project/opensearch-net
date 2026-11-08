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
	/// Behavioural tests for the settings-aware <see cref="GeoShapeQueryConverter"/> (folds the legacy
	/// <c>CompositeFormatter&lt;IGeoShapeQuery, GeoShapeQueryFormatter, GeoShapeQueryFieldNameFormatter&gt;</c> read/write
	/// split). Write emits the field-name wrapper; Read consumes it. Both the <c>shape</c> and <c>indexed_shape</c>
	/// dispatch branches are covered.
	/// </summary>
	public class GeoShapeQueryConverterTests
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
			options.Converters.Add(new GeoShapeQueryConverter(settings));
			options.Converters.Add(new ReadAsConverterFactory());
			options.Converters.Add(new GeoCoordinateConverter());
			options.Converters.Add(new GeoShapeConverter());
			options.Converters.Add(new IdConverter(settings));
			options.Converters.Add(new IndexNameConverter(settings));
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new RoutingConverter(settings));
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		private static IGeoShapeQuery Deserialize(string json) =>
			JsonSerializer.Deserialize<IGeoShapeQuery>(json, Options());

		[U] public void Serialize_ShapeBranch_WrapsUnderFieldName()
		{
			IGeoShapeQuery query = new GeoShapeQuery
			{
				Field = "location",
				Name = "n",
				Boost = 1.1,
				IgnoreUnmapped = true,
				Relation = GeoShapeRelation.Intersects,
				Shape = new PointGeoShape(new GeoCoordinate(20, 10))
			};

			var json = JsonSerializer.Serialize(query, Options());

			json.Should().Contain(@"""_name"":""n""")
				.And.Contain(@"""boost"":1.1")
				.And.Contain(@"""ignore_unmapped"":true")
				.And.Contain(@"""location"":{""shape"":{""type"":""point"",""coordinates"":[10.0,20.0]}")
				.And.Contain(@"""relation"":""intersects""");
		}

		[U] public void Deserialize_ShapeBranch_ReadsFieldAndShape()
		{
			var query = Deserialize(
				@"{""_name"":""n"",""boost"":1.1,""ignore_unmapped"":true,""location"":{""shape"":{""type"":""point"",""coordinates"":[10,20]},""relation"":""intersects""}}");

			query.Should().NotBeNull();
			query.Field.Name.Should().Be("location");
			query.Name.Should().Be("n");
			query.Boost.Should().Be(1.1);
			query.IgnoreUnmapped.Should().BeTrue();
			query.Relation.Should().Be(GeoShapeRelation.Intersects);
			query.Shape.Should().BeOfType<PointGeoShape>();
		}

		[U] public void Deserialize_IndexedShapeBranch_ReadsFieldLookup()
		{
			var query = Deserialize(
				@"{""location"":{""indexed_shape"":{""id"":""1"",""index"":""shapes"",""path"":""geometry""},""relation"":""within""}}");

			query.Should().NotBeNull();
			query.Field.Name.Should().Be("location");
			query.IndexedShape.Should().NotBeNull();
			query.IndexedShape.Id.ToString().Should().Be("1");
			query.Relation.Should().Be(GeoShapeRelation.Within);
			query.Shape.Should().BeNull();
		}

		[U] public void Serialize_IndexedShapeBranch_WritesIndexedShape()
		{
			IGeoShapeQuery query = new GeoShapeQuery
			{
				Field = "location",
				IndexedShape = new FieldLookup { Id = "1", Index = "shapes", Path = "geometry" }
			};

			var json = JsonSerializer.Serialize(query, Options());

			json.Should().Contain(@"""location"":{""indexed_shape"":")
				.And.NotContain("\"shape\":");
		}

		[U] public void Deserialize_NoShapeOrIndexedShape_ReturnsNull()
		{
			// The legacy formatter returns null when neither shape nor indexed_shape is present.
			Deserialize(@"{""location"":{""relation"":""within""}}").Should().BeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull() => Deserialize("null").Should().BeNull();

		[U] public void Serialize_NullField_WritesNull()
		{
			IGeoShapeQuery query = new GeoShapeQuery { Shape = new PointGeoShape(new GeoCoordinate(20, 10)) };
			JsonSerializer.Serialize(query, Options()).Should().Be("null");
		}

		[U] public void Serialize_NullValue_WritesNull() =>
			JsonSerializer.Serialize<IGeoShapeQuery>(null, Options()).Should().Be("null");

		[U] public void RoundTrip_ShapeBranch()
		{
			IGeoShapeQuery original = new GeoShapeQuery
			{
				Field = "location",
				Name = "n",
				Boost = 2.0,
				IgnoreUnmapped = false,
				Relation = GeoShapeRelation.Contains,
				Shape = new PointGeoShape(new GeoCoordinate(20, 10))
			};

			var json = JsonSerializer.Serialize(original, Options());
			var back = JsonSerializer.Deserialize<IGeoShapeQuery>(json, Options());

			back.Field.Name.Should().Be("location");
			back.Name.Should().Be("n");
			back.Boost.Should().Be(2.0);
			back.IgnoreUnmapped.Should().BeFalse();
			back.Relation.Should().Be(GeoShapeRelation.Contains);
			back.Shape.Should().BeOfType<PointGeoShape>();
		}
	}
}
