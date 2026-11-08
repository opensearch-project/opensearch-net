/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Text.Json;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Serialization.Converters;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="CompositeAggregationSourceConverter"/>, the System.Text.Json replacement for
	/// the legacy Utf8Json <c>CompositeAggregationSourceFormatter</c>. A source serializes as the doubly-nested
	/// <c>{ "&lt;name&gt;": { "&lt;source_type&gt;": { body } } }</c>; the source type dispatches to the concrete type
	/// (terms / date_histogram / histogram / geotile_grid). Covers each dispatch branch, the shared field / order /
	/// missing_bucket members, and round-tripping.
	/// </summary>
	public class CompositeAggregationSourceConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				TypeInfoResolver = new HighLevelContractResolver(settings)
			};
			options.Converters.Add(new CompositeAggregationSourceConverter());
			options.Converters.Add(new FieldConverter(settings));
			options.Converters.Add(new ScriptConverter());
			options.Converters.Add(new StringEnumConverterFactory()); // SortOrder
			return options;
		}

		private static ICompositeAggregationSource Deserialize(string json) =>
			JsonSerializer.Deserialize<ICompositeAggregationSource>(json, Options());

		private static string Serialize(ICompositeAggregationSource value) =>
			JsonSerializer.Serialize(value, Options());

		[U] public void Read_TermsSource()
		{
			var source = Deserialize(@"{""branches"":{""terms"":{""field"":""branches.keyword""}}}");
			source.Should().BeOfType<TermsCompositeAggregationSource>();
			source.Name.Should().Be("branches");
			source.Field.Name.Should().Be("branches.keyword");
			source.SourceType.Should().Be("terms");
		}

		[U] public void Read_HistogramSource()
		{
			var source = Deserialize(@"{""bc"":{""histogram"":{""field"":""requiredBranches"",""interval"":1.0}}}");
			source.Should().BeOfType<HistogramCompositeAggregationSource>();
			((IHistogramCompositeAggregationSource)source).Interval.Should().Be(1.0);
		}

		[U] public void Read_DateHistogramSource()
		{
			var source = Deserialize(@"{""started"":{""date_histogram"":{""field"":""startedOn"",""format"":""yyyy-MM-dd""}}}");
			source.Should().BeOfType<DateHistogramCompositeAggregationSource>();
			((IDateHistogramCompositeAggregationSource)source).Format.Should().Be("yyyy-MM-dd");
		}

		[U] public void Read_GeoTileGridSource()
		{
			var source = Deserialize(@"{""geo"":{""geotile_grid"":{""field"":""locationPoint"",""precision"":12}}}");
			source.Should().BeOfType<GeoTileGridCompositeAggregationSource>();
			((IGeoTileGridCompositeAggregationSource)source).Precision.Should().Be(GeoTilePrecision.Precision12);
		}

		[U] public void Read_CommonMembers_MissingBucket_And_Order()
		{
			var source = Deserialize(@"{""branches"":{""terms"":{""field"":""b.keyword"",""missing_bucket"":true,""order"":""asc""}}}");
			source.MissingBucket.Should().Be(true);
			source.Order.Should().Be(SortOrder.Ascending);
		}

		[U] public void Read_NonObject_ReturnsNull() =>
			Deserialize("null").Should().BeNull();

		[U] public void Write_TermsSource_NestedShape()
		{
			ICompositeAggregationSource source = new TermsCompositeAggregationSource("branches")
			{
				Field = "branches.keyword"
			};
			var json = Serialize(source);
			json.Should().Be(@"{""branches"":{""terms"":{""field"":""branches.keyword""}}}");
		}

		[U] public void Write_HistogramSource_WithInterval()
		{
			ICompositeAggregationSource source = new HistogramCompositeAggregationSource("bc")
			{
				Field = "requiredBranches",
				Interval = 1
			};
			var json = Serialize(source);
			json.Should().StartWith(@"{""bc"":{""histogram"":{");
			json.Should().Contain(@"""field"":""requiredBranches""");
			json.Should().Contain(@"""interval"":1");
		}

		[U] public void Write_GeoTileGridSource_WithPrecision()
		{
			ICompositeAggregationSource source = new GeoTileGridCompositeAggregationSource("geo")
			{
				Field = "locationPoint",
				Precision = GeoTilePrecision.Precision12
			};
			var json = Serialize(source);
			json.Should().StartWith(@"{""geo"":{""geotile_grid"":{");
			json.Should().Contain(@"""precision"":12");
		}

		[U] public void RoundTrip_TermsSource_WithOrderAndMissingBucket()
		{
			var options = Options();
			ICompositeAggregationSource source = new TermsCompositeAggregationSource("branches")
			{
				Field = "branches.keyword",
				MissingBucket = true,
				Order = SortOrder.Ascending
			};
			var json = JsonSerializer.Serialize(source, options);
			var back = JsonSerializer.Deserialize<ICompositeAggregationSource>(json, options);
			back.Should().BeOfType<TermsCompositeAggregationSource>();
			back.Name.Should().Be("branches");
			back.Field.Name.Should().Be("branches.keyword");
			back.MissingBucket.Should().Be(true);
			back.Order.Should().Be(SortOrder.Ascending);
		}
	}
}
