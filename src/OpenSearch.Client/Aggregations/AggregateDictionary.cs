/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// Contains aggregates that are returned by OpenSearch. In OSC, Aggregation always refers to an aggregation
/// request to OpenSearch and an Aggregate describes an aggregation response.
/// </summary>
[JsonFormatter(typeof(AggregateDictionaryFormatter))]
public class AggregateDictionary : IsAReadOnlyDictionaryBase<string, IAggregate>
{
    internal static readonly char[] TypedKeysSeparator = { '#' };

    [SerializationConstructor]
    public AggregateDictionary(IReadOnlyDictionary<string, IAggregate> backingDictionary) : base(backingDictionary) { }

    public static AggregateDictionary Default { get; } = new AggregateDictionary(EmptyReadOnly<string, IAggregate>.Dictionary);

    protected override string Sanitize(string key)
    {
        //typed_keys = true on results in aggregation keys being returned as "<type>#<name>"
        var tokens = TypedKeyTokens(key);
        return tokens.Length > 1 ? tokens[1] : tokens[0];
    }

    internal static string[] TypedKeyTokens(string key)
    {
        var tokens = key.Split(TypedKeysSeparator, 2, StringSplitOptions.RemoveEmptyEntries);
        return tokens;
    }

    public ValueAggregate Min(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate Max(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate Sum(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate Cardinality(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate Average(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate ValueCount(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate AverageBucket(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate Derivative(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate SumBucket(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate MovingAverage(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate CumulativeSum(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate BucketScript(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate SerialDifferencing(string key) => TryGet<ValueAggregate>(key);

    public ValueAggregate WeightedAverage(string key) => TryGet<ValueAggregate>(key);

    public KeyedValueAggregate MaxBucket(string key) => TryGet<KeyedValueAggregate>(key);

    public KeyedValueAggregate MinBucket(string key) => TryGet<KeyedValueAggregate>(key);

    public ScriptedMetricAggregate ScriptedMetric(string key)
    {
        var valueMetric = TryGet<ValueAggregate>(key);

        return valueMetric != null
            ? new ScriptedMetricAggregate(valueMetric.Value) { Meta = valueMetric.Meta }
            : TryGet<ScriptedMetricAggregate>(key);
    }

    public StatsAggregate Stats(string key) => TryGet<StatsAggregate>(key);

    public StatsAggregate StatsBucket(string key) => TryGet<StatsAggregate>(key);

    public ExtendedStatsAggregate ExtendedStats(string key) => TryGet<ExtendedStatsAggregate>(key);

    public ExtendedStatsAggregate ExtendedStatsBucket(string key) => TryGet<ExtendedStatsAggregate>(key);

    public GeoBoundsAggregate GeoBounds(string key) => TryGet<GeoBoundsAggregate>(key);

    public GeoLineAggregate GeoLine(string key) => TryGet<GeoLineAggregate>(key);

    public PercentilesAggregate Percentiles(string key) => TryGet<PercentilesAggregate>(key);

    public PercentilesAggregate PercentilesBucket(string key) => TryGet<PercentilesAggregate>(key);

    public PercentilesAggregate PercentileRanks(string key) => TryGet<PercentilesAggregate>(key);

    public TopHitsAggregate TopHits(string key) => TryGet<TopHitsAggregate>(key);

    public FiltersAggregate Filters(string key)
    {
        var named = TryGet<FiltersAggregate>(key);
        if (named != null)
            return named;

        var anonymous = TryGet<BucketAggregate>(key);
        return anonymous != null
            ? new FiltersAggregate { Buckets = anonymous.Items.OfType<FiltersBucketItem>().ToList(), Meta = anonymous.Meta }
            : null;
    }

    public SingleBucketAggregate Global(string key) => TryGet<SingleBucketAggregate>(key);

    public SingleBucketAggregate Filter(string key) => TryGet<SingleBucketAggregate>(key);

    public SingleBucketAggregate Missing(string key) => TryGet<SingleBucketAggregate>(key);

    public SingleBucketAggregate Nested(string key) => TryGet<SingleBucketAggregate>(key);

    public ValueAggregate Normalize(string key) => TryGet<ValueAggregate>(key);

    public SingleBucketAggregate ReverseNested(string key) => TryGet<SingleBucketAggregate>(key);

    public SingleBucketAggregate Children(string key) => TryGet<SingleBucketAggregate>(key);

    public SingleBucketAggregate Parent(string key) => TryGet<SingleBucketAggregate>(key);

    public SingleBucketAggregate Sampler(string key) => TryGet<SingleBucketAggregate>(key);

    public SingleBucketAggregate DiversifiedSampler(string key) => TryGet<SingleBucketAggregate>(key);

    public GeoCentroidAggregate GeoCentroid(string key) => TryGet<GeoCentroidAggregate>(key);

    public SignificantTermsAggregate<TKey> SignificantTerms<TKey>(string key)
    {
        var bucket = TryGet<BucketAggregate>(key);
        return bucket == null
            ? null
            : new SignificantTermsAggregate<TKey>
            {
                BgCount = bucket.BgCount,
                DocCount = bucket.DocCount,
                Buckets = GetSignificantTermsBuckets<TKey>(bucket.Items).ToList(),
                Meta = bucket.Meta
            };
    }

    public SignificantTermsAggregate<string> SignificantTerms(string key) => SignificantTerms<string>(key);

    public SignificantTermsAggregate<TKey> SignificantText<TKey>(string key)
    {
        var bucket = TryGet<BucketAggregate>(key);
        return bucket == null
            ? null
            : new SignificantTermsAggregate<TKey>
            {
                BgCount = bucket.BgCount,
                DocCount = bucket.DocCount,
                Buckets = GetSignificantTermsBuckets<TKey>(bucket.Items).ToList(),
                Meta = bucket.Meta
            };
    }

    public SignificantTermsAggregate<string> SignificantText(string key) => SignificantText<string>(key);

    public TermsAggregate<TKey> Terms<TKey>(string key)
    {
        var bucket = TryGet<BucketAggregate>(key);
        return bucket == null
            ? null
            : new TermsAggregate<TKey>
            {
                DocCountErrorUpperBound = bucket.DocCountErrorUpperBound,
                SumOtherDocCount = bucket.SumOtherDocCount,
                Buckets = GetKeyedBuckets<TKey>(bucket.Items).ToList(),
                Meta = bucket.Meta
            };
    }

    public TermsAggregate<string> Terms(string key) => Terms<string>(key);

    public MultiBucketAggregate<KeyedBucket<double>> Histogram(string key) => GetMultiKeyedBucketAggregate<double>(key);

    public MultiBucketAggregate<KeyedBucket<string>> GeoHash(string key) => GetMultiKeyedBucketAggregate<string>(key);

    public MultiBucketAggregate<KeyedBucket<string>> GeoTile(string key) => GetMultiKeyedBucketAggregate<string>(key);

    public MultiBucketAggregate<KeyedBucket<string>> AdjacencyMatrix(string key) => GetMultiKeyedBucketAggregate<string>(key);

    public MultiBucketAggregate<RareTermsBucket<TKey>> RareTerms<TKey>(string key)
    {
        var bucket = TryGet<BucketAggregate>(key);
        return bucket == null
            ? null
            : new MultiBucketAggregate<RareTermsBucket<TKey>>
            {
                Buckets = GetRareTermsBuckets<TKey>(bucket.Items).ToList(),
                Meta = bucket.Meta
            };
    }

    public MultiBucketAggregate<RareTermsBucket<string>> RareTerms(string key) => RareTerms<string>(key);

    public MultiBucketAggregate<RangeBucket> Range(string key) => GetMultiBucketAggregate<RangeBucket>(key);

    public MultiBucketAggregate<RangeBucket> DateRange(string key) => GetMultiBucketAggregate<RangeBucket>(key);

    public MultiBucketAggregate<IpRangeBucket> IpRange(string key) => GetMultiBucketAggregate<IpRangeBucket>(key);

    public MultiBucketAggregate<RangeBucket> GeoDistance(string key) => GetMultiBucketAggregate<RangeBucket>(key);

    public MultiBucketAggregate<DateHistogramBucket> DateHistogram(string key) => GetMultiBucketAggregate<DateHistogramBucket>(key);

    public MultiBucketAggregate<VariableWidthHistogramBucket> VariableWidthHistogram(string key) => GetMultiBucketAggregate<VariableWidthHistogramBucket>(key);

    public MultiTermsAggregate<string> MultiTerms(string key) => MultiTerms<string>(key);

    public MultiTermsAggregate<TKey> MultiTerms<TKey>(string key)
    {
        var bucket = TryGet<BucketAggregate>(key);
        return bucket == null
            ? null
            : new MultiTermsAggregate<TKey>
            {
                DocCountErrorUpperBound = bucket.DocCountErrorUpperBound,
                SumOtherDocCount = bucket.SumOtherDocCount,
                Buckets = GetMultiTermsBuckets<TKey>(bucket.Items).ToList(),
                Meta = bucket.Meta
            };
    }

    public AutoDateHistogramAggregate AutoDateHistogram(string key)
    {
        var bucket = TryGet<BucketAggregate>(key);
        if (bucket == null) return null;

        return new AutoDateHistogramAggregate
        {
            Buckets = bucket.Items.OfType<DateHistogramBucket>().ToList(),
            Meta = bucket.Meta,
            AutoInterval = bucket.AutoInterval
        };
    }

    public CompositeBucketAggregate Composite(string key)
    {
        var bucket = TryGet<BucketAggregate>(key);
        if (bucket == null) return null;

        return new CompositeBucketAggregate
        {
            Buckets = bucket.Items.OfType<CompositeBucket>().ToList(),
            Meta = bucket.Meta,
            AfterKey = bucket.AfterKey
        };
    }

    public MatrixStatsAggregate MatrixStats(string key) => TryGet<MatrixStatsAggregate>(key);

    public ValueAggregate MedianAbsoluteDeviation(string key) => TryGet<ValueAggregate>(key);

    private TAggregate TryGet<TAggregate>(string key) where TAggregate : class, IAggregate =>
        BackingDictionary.TryGetValue(key, out var agg) ? agg as TAggregate : null;

    private MultiBucketAggregate<TBucket> GetMultiBucketAggregate<TBucket>(string key)
        where TBucket : IBucket
    {
        var bucket = TryGet<BucketAggregate>(key);
        if (bucket == null) return null;

        return new MultiBucketAggregate<TBucket>
        {
            Buckets = bucket.Items.OfType<TBucket>().ToList(),
            Meta = bucket.Meta
        };
    }

    private MultiBucketAggregate<KeyedBucket<TKey>> GetMultiKeyedBucketAggregate<TKey>(string key)
    {
        var bucket = TryGet<BucketAggregate>(key);
        if (bucket == null) return null;

        return new MultiBucketAggregate<KeyedBucket<TKey>>
        {
            Buckets = GetKeyedBuckets<TKey>(bucket.Items).ToList(),
            Meta = bucket.Meta
        };
    }

    private IEnumerable<KeyedBucket<TKey>> GetKeyedBuckets<TKey>(IEnumerable<IBucket> items)
    {
        var buckets = items.Cast<KeyedBucket<object>>();

        foreach (var bucket in buckets)
            yield return new KeyedBucket<TKey>(bucket.BackingDictionary)
            {
                Key = GetKeyFromBucketKey<TKey>(bucket.Key),
                KeyAsString = bucket.KeyAsString,
                DocCount = bucket.DocCount,
                DocCountErrorUpperBound = bucket.DocCountErrorUpperBound
            };
    }

    private IEnumerable<SignificantTermsBucket<TKey>> GetSignificantTermsBuckets<TKey>(IEnumerable<IBucket> items)
    {
        var buckets = items.Cast<SignificantTermsBucket<object>>();

        foreach (var bucket in buckets)
            yield return new SignificantTermsBucket<TKey>(bucket.BackingDictionary)
            {
                Key = GetKeyFromBucketKey<TKey>(bucket.Key),
                BgCount = bucket.BgCount,
                DocCount = bucket.DocCount,
                Score = bucket.Score
            };
    }

    private IEnumerable<RareTermsBucket<TKey>> GetRareTermsBuckets<TKey>(IEnumerable<IBucket> items)
    {
        var buckets = items.Cast<KeyedBucket<object>>();

        foreach (var bucket in buckets)
            yield return new RareTermsBucket<TKey>(bucket.BackingDictionary)
            {
                Key = GetKeyFromBucketKey<TKey>(bucket.Key),
                DocCount = bucket.DocCount.GetValueOrDefault(0)
            };
    }

    private static IEnumerable<MultiTermsBucket<TKey>> GetMultiTermsBuckets<TKey>(IEnumerable<IBucket> items)
    {
        var buckets = items.Cast<KeyedBucket<object>>();

        foreach (var bucket in buckets)
        {
            var aggregates = new MultiTermsBucket<TKey>(bucket.BackingDictionary)
            {
                DocCount = bucket.DocCount,
                DocCountErrorUpperBound = bucket.DocCountErrorUpperBound,
                KeyAsString = bucket.KeyAsString
            };

            if (bucket.Key is List<object> allKeys)
                aggregates.Key = allKeys.Select(GetKeyFromBucketKey<TKey>).ToList();
            else
                aggregates.Key = new[] { GetKeyFromBucketKey<TKey>(bucket.Key) };

            yield return aggregates;
        }
    }

    private static TKey GetKeyFromBucketKey<TKey>(object key) =>
        typeof(TKey).IsEnum
            ? (TKey)Enum.Parse(typeof(TKey), key.ToString(), true)
            : (TKey)Convert.ChangeType(key, typeof(TKey));
}
