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
using OpenSearch.Net; // SortOrder enum
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="SortConverter"/>: an <see cref="ISort"/> is polymorphic on the wire — a bare
	/// field string, a single-key <c>{ field: { order, … } }</c> object (or <c>{ field: "asc" }</c> short-form), a
	/// <c>_script</c> sort or a <c>_geo_distance</c> sort whose field is the array-valued property.
	/// </summary>
	public class SortConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				// Mirror the real serializer's relaxed encoder so script source (doc['x'].value) is not \u-escaped.
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new SortConverter(settings));
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new ScriptConverter());
			options.Converters.Add(new GeoLocationConverter());
			options.Converters.Add(new GeoCoordinateConverter());
			options.Converters.Add(new StringEnumConverterFactory());
			return options;
		}

		private static ISort Deserialize(string json) => JsonSerializer.Deserialize<ISort>(json, Options());

		private static string Serialize(ISort sort) => JsonSerializer.Serialize(sort, Options());

		[U] public void Deserialize_BareFieldString()
		{
			var sort = Deserialize(@"""startedOn""");
			sort.Should().BeOfType<FieldSort>();
			((FieldSort)sort).Field.Name.Should().Be("startedOn");
		}

		[U] public void Deserialize_FieldWithOrderShortForm()
		{
			var sort = Deserialize(@"{""name"":""desc""}");
			sort.Should().BeOfType<FieldSort>();
			var fieldSort = (FieldSort)sort;
			fieldSort.Field.Name.Should().Be("name");
			fieldSort.Order.Should().Be(SortOrder.Descending);
		}

		[U] public void Deserialize_FieldWithOrderObject()
		{
			var sort = Deserialize(@"{""startedOn"":{""order"":""asc"",""mode"":""avg""}}");
			sort.Should().BeOfType<FieldSort>();
			var fieldSort = (FieldSort)sort;
			fieldSort.Field.Name.Should().Be("startedOn");
			fieldSort.Order.Should().Be(SortOrder.Ascending);
			fieldSort.Mode.Should().Be(SortMode.Average);
		}

		[U] public void Deserialize_ScoreField()
		{
			var sort = Deserialize(@"{""_score"":{""order"":""desc""}}");
			sort.Should().BeOfType<FieldSort>();
			var fieldSort = (FieldSort)sort;
			fieldSort.Field.Name.Should().Be("_score");
			fieldSort.Order.Should().Be(SortOrder.Descending);
		}

		[U] public void Deserialize_GeoDistance()
		{
			var sort = Deserialize(
				@"{""_geo_distance"":{""locationPoint"":[{""lat"":70.0,""lon"":-70.0},{""lat"":-12.0,""lon"":12.0}],""order"":""asc"",""mode"":""min"",""distance_type"":""arc"",""unit"":""cm""}}");
			sort.Should().BeOfType<GeoDistanceSort>();
			var geo = (GeoDistanceSort)sort;
			geo.Field.Name.Should().Be("locationPoint");
			geo.Points.Should().HaveCount(2);
			geo.Order.Should().Be(SortOrder.Ascending);
			geo.Mode.Should().Be(SortMode.Min);
			geo.DistanceType.Should().Be(GeoDistanceType.Arc);
			geo.Unit.Should().Be(DistanceUnit.Centimeters);
		}

		[U] public void Deserialize_GeoDistance_PointsOnly()
		{
			var sort = Deserialize(
				@"{""_geo_distance"":{""locationPoint"":[{""lat"":70.0,""lon"":-70.0}]}}");
			sort.Should().BeOfType<GeoDistanceSort>();
			var geo = (GeoDistanceSort)sort;
			geo.Field.Name.Should().Be("locationPoint");
			geo.Points.Should().HaveCount(1);
		}

		[U] public void Deserialize_Script()
		{
			var sort = Deserialize(
				@"{""_script"":{""order"":""asc"",""type"":""number"",""script"":{""source"":""doc['x'].value""}}}");
			sort.Should().BeOfType<ScriptSort>();
			var script = (ScriptSort)sort;
			script.Type.Should().Be("number");
			script.Order.Should().Be(SortOrder.Ascending);
			script.Script.Should().NotBeNull();
		}

		[U] public void Deserialize_Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Serialize_FieldSort()
		{
			var json = Serialize(new FieldSort { Field = "name", Order = SortOrder.Descending });
			json.Should().Be(@"{""name"":{""order"":""desc""}}");
		}

		[U] public void Serialize_ScoreField()
		{
			var json = Serialize(new FieldSort { Field = "_score", Order = SortOrder.Descending });
			json.Should().Be(@"{""_score"":{""order"":""desc""}}");
		}

		[U] public void Serialize_GeoDistance()
		{
			var json = Serialize(new GeoDistanceSort
			{
				Field = "locationPoint",
				Order = SortOrder.Ascending,
				DistanceType = GeoDistanceType.Arc,
				Unit = DistanceUnit.Centimeters,
				Mode = SortMode.Min,
				Points = new[] { new GeoLocation(70, -70), new GeoLocation(-12, 12) }
			});

			json.Should().StartWith(@"{""_geo_distance"":{");
			json.Should().Contain(@"""order"":""asc""");
			json.Should().Contain(@"""distance_type"":""arc""");
			json.Should().Contain(@"""unit"":""cm""");
			json.Should().Contain(@"""mode"":""min""");
			json.Should().Contain(@"""locationPoint"":[");
			// The geo body must NOT emit the special _geo_distance sort-key or a Field property inside the body.
			json.Should().NotContain(@"""field""");
		}

		[U] public void Serialize_Script()
		{
			var json = Serialize(new ScriptSort
			{
				Type = "number",
				Order = SortOrder.Ascending,
				Script = new InlineScript("doc['x'].value")
			});
			json.Should().StartWith(@"{""_script"":{");
			json.Should().Contain(@"""type"":""number""");
			json.Should().Contain(@"""order"":""asc""");
			json.Should().Contain(@"""source"":""doc['x'].value""");
		}

		[U] public void Serialize_Null_WritesNull()
		{
			JsonSerializer.Serialize<ISort>(null, Options()).Should().Be("null");
		}

		[U] public void RoundTrip_FieldSort()
		{
			var options = Options();
			var json = JsonSerializer.Serialize<ISort>(new FieldSort { Field = "startedOn", Order = SortOrder.Ascending }, options);
			var back = JsonSerializer.Deserialize<ISort>(json, options);
			back.Should().BeOfType<FieldSort>();
			((FieldSort)back).Field.Name.Should().Be("startedOn");
			back.Order.Should().Be(SortOrder.Ascending);
		}

		[U] public void RoundTrip_GeoDistance()
		{
			var options = Options();
			var json = JsonSerializer.Serialize<ISort>(new GeoDistanceSort
			{
				Field = "locationPoint",
				Order = SortOrder.Ascending,
				Points = new[] { new GeoLocation(70, -70), new GeoLocation(-12, 12) }
			}, options);
			var back = JsonSerializer.Deserialize<ISort>(json, options);
			back.Should().BeOfType<GeoDistanceSort>();
			var geo = (GeoDistanceSort)back;
			geo.Field.Name.Should().Be("locationPoint");
			geo.Points.Count().Should().Be(2);
			geo.Order.Should().Be(SortOrder.Ascending);
		}
	}
}
