/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.Aggregations
{
	/// <summary>
	/// Fast, deterministic unit coverage for the System.Text.Json aggregate response reader
	/// (<c>AggregateConverter</c>, #388). The *AggregationUsageTests assert responses only under
	/// integration ([I]); these shapes regressed there and are pinned here as [U] tests so a
	/// break is caught without a live cluster. Each case feeds a recorded `aggregations` object
	/// and asserts the reader produced the right aggregate type.
	/// </summary>
	public class AggregateResponseReaderTests
	{
		[U]
		public void ScriptedMetric_ObjectValue_IsReadAsScriptedMetricAggregate()
		{
			const string json = @"{ ""scripted_metric#by_state"": { ""value"": { ""Stable"": 3, ""BellyUp"": 2 } } }";
			Expect(json).NoRoundTrip().DeserializesTo<AggregateDictionary>((_, aggs) =>
			{
				var sm = aggs.ScriptedMetric("by_state");
				sm.Should().NotBeNull();
				var dict = sm.Value<IDictionary<string, int>>();
				dict.Should().ContainKey("Stable").And.ContainKey("BellyUp");
			});
		}

		[U]
		public void MaxBucket_ValueWithKeys_IsReadAsKeyedValueAggregate()
		{
			const string json = @"{ ""bucket_metric_value#max"": { ""value"": null, ""keys"": [] } }";
			Expect(json).NoRoundTrip().DeserializesTo<AggregateDictionary>((_, aggs) =>
			{
				var max = aggs.MaxBucket("max");
				max.Should().NotBeNull();
				max.Keys.Should().NotBeNull();
				max.Value.Should().BeNull();
			});
		}

		[U]
		public void GeoBounds_IsReadWithTopLeftAndBottomRight()
		{
			const string json = @"{ ""geo_bounds#viewport"": { ""bounds"": {
				""top_left"": { ""lat"": 1.0, ""lon"": 2.0 }, ""bottom_right"": { ""lat"": 3.0, ""lon"": 4.0 } } } }";
			Expect(json).NoRoundTrip().DeserializesTo<AggregateDictionary>((_, aggs) =>
			{
				var bounds = aggs.GeoBounds("viewport");
				bounds.Should().NotBeNull();
				bounds.Bounds.TopLeft.Should().NotBeNull();
				bounds.Bounds.BottomRight.Should().NotBeNull();
			});
		}

		[U]
		public void GeoCentroid_WithResults_IsReadWithLocationAndCount()
		{
			const string json = @"{ ""geo_centroid#centroid"": { ""location"": { ""lat"": 1.5, ""lon"": 2.5 }, ""count"": 10 } }";
			Expect(json).NoRoundTrip().DeserializesTo<AggregateDictionary>((_, aggs) =>
			{
				var centroid = aggs.GeoCentroid("centroid");
				centroid.Should().NotBeNull();
				centroid.Location.Should().NotBeNull();
				centroid.Count.Should().Be(10);
			});
		}

		[U]
		public void GeoCentroid_NoResults_IsReadFromBareCount()
		{
			// A geo_centroid with no matching documents returns only { "count": 0 } (no location).
			const string json = @"{ ""geo_centroid#centroid"": { ""count"": 0 } }";
			Expect(json).NoRoundTrip().DeserializesTo<AggregateDictionary>((_, aggs) =>
			{
				var centroid = aggs.GeoCentroid("centroid");
				centroid.Should().NotBeNull();
				centroid.Count.Should().Be(0);
				centroid.Location.Should().BeNull();
			});
		}

		[U]
		public void MatrixStats_IsReadWithFields()
		{
			const string json = @"{ ""matrix_stats#ms"": { ""doc_count"": 5, ""fields"": [
				{ ""name"": ""f"", ""count"": 5, ""mean"": 1.0, ""variance"": 2.0, ""skewness"": 0.0, ""kurtosis"": 1.0,
				  ""covariance"": { ""f"": 2.0 }, ""correlation"": { ""f"": 1.0 } } ] } }";
			Expect(json).NoRoundTrip().DeserializesTo<AggregateDictionary>((_, aggs) =>
			{
				var ms = aggs.MatrixStats("ms");
				ms.Should().NotBeNull();
				ms.Fields.Should().HaveCount(1);
				ms.DocCount.Should().Be(5);
			});
		}

		[U]
		public void Composite_IsReadWithAfterKeyAndObjectKeyBuckets()
		{
			const string json = @"{ ""composite#comp"": { ""after_key"": { ""p"": ""x"" },
				""buckets"": [ { ""key"": { ""p"": ""x"" }, ""doc_count"": 3 } ] } }";
			Expect(json).NoRoundTrip().DeserializesTo<AggregateDictionary>((_, aggs) =>
			{
				var composite = aggs.Composite("comp");
				composite.Should().NotBeNull();
				composite.AfterKey.Should().NotBeNull();
				composite.Buckets.Should().HaveCount(1);
				composite.Buckets[0].Key.Should().ContainKey("p");
			});
		}

		[U]
		public void SignificantTerms_IsReadWithScoreAndBgCount()
		{
			const string json = @"{ ""sigterms#sig"": { ""doc_count"": 10, ""bg_count"": 100,
				""buckets"": [ { ""key"": ""foo"", ""doc_count"": 5, ""score"": 1.2, ""bg_count"": 8 } ] } }";
			Expect(json).NoRoundTrip().DeserializesTo<AggregateDictionary>((_, aggs) =>
			{
				var sig = aggs.SignificantTerms("sig");
				sig.Should().NotBeNull();
				sig.DocCount.Should().Be(10);
				sig.BgCount.Should().Be(100);
				var bucket = sig.Buckets.First();
				bucket.Key.Should().Be("foo");
				bucket.BgCount.Should().Be(8);
				bucket.Score.Should().BeApproximately(1.2, 0.0001);
			});
		}

		[U]
		public void AnonymousFilters_IsReadWithFiltersBuckets()
		{
			const string json = @"{ ""filters#f"": { ""buckets"": [
				{ ""doc_count"": 1 }, { ""doc_count"": 2 }, { ""doc_count"": 3 }, { ""doc_count"": 4 } ] } }";
			Expect(json).NoRoundTrip().DeserializesTo<AggregateDictionary>((_, aggs) =>
			{
				var filters = aggs.Filters("f");
				filters.Should().NotBeNull();
				var buckets = filters.AnonymousBuckets();
				buckets.Should().HaveCount(4);
				buckets[0].DocCount.Should().Be(1);
			});
		}
	}
}
