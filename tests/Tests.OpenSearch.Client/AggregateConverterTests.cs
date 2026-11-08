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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.OpenSearch.Client.Serialization
{
	/// <summary>
	/// Behavioural tests for <see cref="AggregateConverter"/>, the System.Text.Json replacement for the legacy
	/// Utf8Json <c>AggregateFormatter</c>. The converter picks the concrete <see cref="IAggregate"/> subtype from the
	/// FIRST property of the response object (after an optional leading <c>meta</c>), preserving the legacy key
	/// dispatch precedence. Each test asserts the type chosen for a representative response shape.
	/// </summary>
	public class AggregateConverterTests
	{
		private static JsonSerializerOptions Options()
		{
			var settings = new ConnectionSettings();
			var options = new JsonSerializerOptions();
			options.Converters.Add(new AggregateConverter(settings));
			return options;
		}

		private static IAggregate Deserialize(string json) =>
			JsonSerializer.Deserialize<IAggregate>(json, Options());

		[U] public void SingleValueMetric()
		{
			var agg = Deserialize(@"{""value"":42.0}");
			agg.Should().BeOfType<ValueAggregate>();
			((ValueAggregate)agg).Value.Should().Be(42.0);
		}

		[U] public void SingleValueMetric_WithValueAsString()
		{
			var agg = Deserialize(@"{""value"":42.0,""value_as_string"":""42""}");
			agg.Should().BeOfType<ValueAggregate>();
			var value = (ValueAggregate)agg;
			value.Value.Should().Be(42.0);
			value.ValueAsString.Should().Be("42");
		}

		[U] public void SingleValueMetric_NullValue()
		{
			var agg = Deserialize(@"{""value"":null}");
			agg.Should().BeOfType<ValueAggregate>();
			((ValueAggregate)agg).Value.Should().BeNull();
		}

		[U] public void KeyedValueMetric_WithKeys()
		{
			var agg = Deserialize(@"{""value"":10.0,""keys"":[""a"",""b""]}");
			agg.Should().BeOfType<KeyedValueAggregate>();
			var keyed = (KeyedValueAggregate)agg;
			keyed.Value.Should().Be(10.0);
			keyed.Keys.Should().BeEquivalentTo("a", "b");
		}

		[U] public void ScriptedMetric_WhenValueIsObject()
		{
			// A non-number/non-null "value" is captured as a scripted-metric lazy document.
			var agg = Deserialize(@"{""value"":{""transactions"":123}}");
			agg.Should().BeOfType<ScriptedMetricAggregate>();
		}

		[U] public void StatsAggregate()
		{
			var agg = Deserialize(@"{""count"":10,""min"":1.0,""max"":9.0,""avg"":5.0,""sum"":50.0}");
			agg.Should().BeOfType<StatsAggregate>();
			var stats = (StatsAggregate)agg;
			stats.Count.Should().Be(10);
			stats.Min.Should().Be(1.0);
			stats.Max.Should().Be(9.0);
			stats.Average.Should().Be(5.0);
			stats.Sum.Should().Be(50.0);
		}

		[U] public void ExtendedStatsAggregate()
		{
			var json =
				@"{""count"":10,""min"":1.0,""max"":9.0,""avg"":5.0,""sum"":50.0,""sum_of_squares"":300.0," +
				@"""variance"":6.5,""std_deviation"":2.5}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<ExtendedStatsAggregate>();
			var stats = (ExtendedStatsAggregate)agg;
			stats.SumOfSquares.Should().Be(300.0);
			stats.Variance.Should().Be(6.5);
			stats.StdDeviation.Should().Be(2.5);
			// core stats carried over
			stats.Count.Should().Be(10);
			stats.Sum.Should().Be(50.0);
		}

		[U] public void StatsAggregate_SkipsAsStringSiblingsBeforeExtended()
		{
			// The *_as_string siblings between the core stats and sum_of_squares must be skipped, not treated as the
			// start of the extended-stats section.
			var json =
				@"{""count"":10,""min"":1.0,""max"":9.0,""avg"":5.0,""sum"":50.0,""min_as_string"":""1.0""," +
				@"""max_as_string"":""9.0"",""sum_of_squares"":300.0,""variance"":6.5}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<ExtendedStatsAggregate>();
			((ExtendedStatsAggregate)agg).SumOfSquares.Should().Be(300.0);
		}

		[U] public void GeoCentroid_NoResults_CountOnly()
		{
			// A bare "count" with no following fields is the empty geo_centroid response, NOT a stats aggregate.
			var agg = Deserialize(@"{""count"":0}");
			agg.Should().BeOfType<GeoCentroidAggregate>();
			((GeoCentroidAggregate)agg).Count.Should().Be(0);
		}

		[U] public void GeoCentroid_WithLocation()
		{
			var agg = Deserialize(@"{""location"":{""lat"":52.0,""lon"":4.0},""count"":5}");
			agg.Should().BeOfType<GeoCentroidAggregate>();
			var centroid = (GeoCentroidAggregate)agg;
			centroid.Count.Should().Be(5);
			centroid.Location.Should().NotBeNull();
			centroid.Location.Latitude.Should().Be(52.0);
			centroid.Location.Longitude.Should().Be(4.0);
		}

		[U] public void TermsBuckets()
		{
			var json =
				@"{""doc_count_error_upper_bound"":0,""sum_other_doc_count"":3," +
				@"""buckets"":[{""key"":""ca"",""doc_count"":7},{""key"":""ma"",""doc_count"":5}]}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<BucketAggregate>();
			var bucket = (BucketAggregate)agg;
			bucket.DocCountErrorUpperBound.Should().Be(0);
			bucket.SumOtherDocCount.Should().Be(3);
			bucket.Items.Should().HaveCount(2);

			// Materialise into the strongly-typed terms shape (exercises KeyedBucket<object> construction).
			var terms = new AggregateDictionary(new Dictionary<string, IAggregate> { ["states"] = bucket })
				.Terms("states");
			terms.Buckets.Should().HaveCount(2);
			terms.Buckets.First().Key.Should().Be("ca");
			terms.Buckets.First().DocCount.Should().Be(7);
		}

		[U] public void TermsBuckets_PlainBucketsOnly()
		{
			var agg = Deserialize(@"{""buckets"":[{""key"":""x"",""doc_count"":2}]}");
			agg.Should().BeOfType<BucketAggregate>();
			((BucketAggregate)agg).Items.Should().HaveCount(1);
		}

		[U] public void NestedSubAggregations()
		{
			// A terms bucket containing a nested single-value metric sub-aggregate.
			var json =
				@"{""buckets"":[{""key"":""ca"",""doc_count"":7,""avg_commits"":{""value"":3.5}}]}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<BucketAggregate>();
			var bucket = (BucketAggregate)agg;
			bucket.Items.Should().HaveCount(1);
			var keyed = (KeyedBucket<object>)bucket.Items.First();
			keyed.Key.Should().Be("ca");
			var sub = keyed["avg_commits"];
			sub.Should().BeOfType<ValueAggregate>();
			((ValueAggregate)sub).Value.Should().Be(3.5);
		}

		[U] public void TopHitsAggregate()
		{
			var json =
				@"{""hits"":{""total"":{""value"":10,""relation"":""eq""},""max_score"":1.5," +
				@"""hits"":[{""_index"":""project"",""_id"":""1"",""_source"":{""name"":""x""}}]}}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<TopHitsAggregate>();
			var topHits = (TopHitsAggregate)agg;
			topHits.MaxScore.Should().Be(1.5);
			topHits.Total.Should().NotBeNull();
			topHits.Total.Value.Should().Be(10);
		}

		[U] public void SingleBucketAggregate_WithSubAggregation()
		{
			var json = @"{""doc_count"":100,""inner"":{""value"":7.0}}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<SingleBucketAggregate>();
			var single = (SingleBucketAggregate)agg;
			single.DocCount.Should().Be(100);
			single["inner"].Should().BeOfType<ValueAggregate>();
		}

		[U] public void SingleBucketAggregate_DocCountOnly()
		{
			var agg = Deserialize(@"{""doc_count"":42}");
			agg.Should().BeOfType<SingleBucketAggregate>();
			((SingleBucketAggregate)agg).DocCount.Should().Be(42);
		}

		[U] public void FiltersAggregate_NamedBuckets()
		{
			// "buckets" as an object (not array) is a named-filters aggregate.
			var json = @"{""buckets"":{""errors"":{""doc_count"":3},""warnings"":{""doc_count"":5}}}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<FiltersAggregate>();
			var filters = (FiltersAggregate)agg;
			filters.NamedBucket("errors").DocCount.Should().Be(3);
			filters.NamedBucket("warnings").DocCount.Should().Be(5);
		}

		[U] public void PercentilesAggregate_KeyedObject()
		{
			var json = @"{""values"":{""50.0"":1.5,""95.0"":9.0}}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<PercentilesAggregate>();
			var percentiles = (PercentilesAggregate)agg;
			percentiles.Items.Should().HaveCount(2);
			percentiles.Items[0].Percentile.Should().Be(50.0);
			percentiles.Items[0].Value.Should().Be(1.5);
		}

		[U] public void PercentilesAggregate_Array()
		{
			var json = @"{""values"":[{""key"":50.0,""value"":1.5},{""key"":95.0,""value"":9.0}]}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<PercentilesAggregate>();
			((PercentilesAggregate)agg).Items.Should().HaveCount(2);
		}

		[U] public void DateHistogramBuckets()
		{
			var json =
				@"{""buckets"":[{""key_as_string"":""2020-01-01"",""key"":1577836800000,""doc_count"":4}]}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<BucketAggregate>();
			var bucket = (BucketAggregate)agg;
			var item = (DateHistogramBucket)bucket.Items.First();
			item.KeyAsString.Should().Be("2020-01-01");
			item.Key.Should().Be(1577836800000d);
			item.DocCount.Should().Be(4);
		}

		[U] public void RangeBuckets()
		{
			var json =
				@"{""buckets"":[{""key"":""*-100.0"",""to"":100.0,""to_as_string"":""100.0"",""doc_count"":2}]}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<BucketAggregate>();
			var range = (RangeBucket)((BucketAggregate)agg).Items.First();
			range.To.Should().Be(100.0);
			range.DocCount.Should().Be(2);
		}

		[U] public void LeadingMeta_IsCapturedAndDispatchStillWorks()
		{
			var json = @"{""meta"":{""foo"":""bar""},""value"":7.0}";
			var agg = Deserialize(json);
			agg.Should().BeOfType<ValueAggregate>();
			((ValueAggregate)agg).Value.Should().Be(7.0);
			agg.Meta.Should().ContainKey("foo");
		}

		[U] public void UnknownFirstProperty_ReturnsNull()
		{
			// "top" and "type" are in the legacy key table but have no handler, so they yield null.
			Deserialize(@"{""top"":{}}").Should().BeNull();
			Deserialize(@"{""unrecognised"":123}").Should().BeNull();
		}

		[U] public void EmptyObject_ReturnsNull()
		{
			Deserialize(@"{}").Should().BeNull();
		}

		[U] public void MetaOnly_ReturnsNull()
		{
			Deserialize(@"{""meta"":{""foo"":""bar""}}").Should().BeNull();
		}

		[U] public void Null_ReturnsNull()
		{
			Deserialize("null").Should().BeNull();
		}

		[U] public void Write_Throws()
		{
			var act = () => JsonSerializer.Serialize<IAggregate>(new ValueAggregate { Value = 1 }, Options());
			act.Should().Throw<System.NotSupportedException>();
		}
	}
}
