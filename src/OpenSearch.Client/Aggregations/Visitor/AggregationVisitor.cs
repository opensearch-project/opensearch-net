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

namespace OpenSearch.Client;

public interface IAggregationVisitor
{
    /// <summary>
    /// The current depth of the node being visited
    /// </summary>
    int Depth { get; set; }

    /// <summary>
    /// Hints the relation with the parent, i.e aggregations contained inside a bucket aggregation will have AggregationVisitorScope.Bucket set.
    /// </summary>
    AggregationVisitorScope Scope { get; set; }

    /// <summary>
    /// Visit the aggregation container just before we dispatch into the aggregation it holds
    /// </summary>
    /// <param name="aggregationContainer"></param>
    void Visit(IAggregationContainer aggregationContainer);

    /// <summary>
    /// Visit every aggregation item just before they are visited by their specialized Visit() implementation
    /// </summary>
    /// <param name="aggregation">The IAggregation object that will be visited</param>
    void Visit(IAggregation aggregation);

    void Visit(IAverageAggregation aggregation);

    void Visit(IValueCountAggregation aggregation);

    void Visit(IMaxAggregation aggregation);

    void Visit(IMinAggregation aggregation);

    void Visit(IStatsAggregation aggregation);

    void Visit(ISumAggregation aggregation);

    void Visit(IExtendedStatsAggregation aggregation);

    void Visit(IDateHistogramAggregation aggregation);

    void Visit(IPercentilesAggregation aggregation);

    void Visit(IDateRangeAggregation aggregation);

    void Visit(IFilterAggregation aggregation);

    void Visit(IFiltersAggregation aggregation);

    void Visit(IGeoDistanceAggregation aggregation);

    void Visit(IGeoHashGridAggregation aggregation);

    void Visit(IGeoLineAggregation aggregation);

    void Visit(IGeoTileGridAggregation aggregation);

    void Visit(IGeoBoundsAggregation aggregation);

    void Visit(IHistogramAggregation aggregation);

    void Visit(IGlobalAggregation aggregation);

    void Visit(IIpRangeAggregation aggregation);

    void Visit(ICardinalityAggregation aggregation);

    void Visit(IMissingAggregation aggregation);

    void Visit(INestedAggregation aggregation);

    void Visit(IParentAggregation aggregation);

    void Visit(IReverseNestedAggregation aggregation);

    void Visit(IRangeAggregation aggregation);

    void Visit(IRareTermsAggregation aggregation);

    void Visit(ITermsAggregation aggregation);

    void Visit(ISignificantTermsAggregation aggregation);

    void Visit(ISignificantTextAggregation aggregation);

    void Visit(IPercentileRanksAggregation aggregation);

    void Visit(ITopHitsAggregation aggregation);

    void Visit(IChildrenAggregation aggregation);

    void Visit(IScriptedMetricAggregation aggregation);

    void Visit(IAverageBucketAggregation aggregation);

    void Visit(IDerivativeAggregation aggregation);

    void Visit(IMaxBucketAggregation aggregation);

    void Visit(IMinBucketAggregation aggregation);

    void Visit(ISumBucketAggregation aggregation);

    void Visit(IStatsBucketAggregation aggregation);

    void Visit(IExtendedStatsBucketAggregation aggregation);

    void Visit(IPercentilesBucketAggregation aggregation);

    void Visit(IMovingAverageAggregation aggregation);

    void Visit(ICumulativeSumAggregation aggregation);

    void Visit(ISerialDifferencingAggregation aggregation);

    void Visit(IBucketScriptAggregation aggregation);

    void Visit(IBucketSelectorAggregation aggregation);

    void Visit(IBucketSortAggregation aggregation);

    void Visit(ISamplerAggregation aggregation);

    void Visit(IDiversifiedSamplerAggregation aggregation);

    void Visit(IGeoCentroidAggregation aggregation);

    void Visit(ICompositeAggregation aggregation);

    void Visit(IMedianAbsoluteDeviationAggregation aggregation);

    void Visit(IAdjacencyMatrixAggregation aggregation);

    void Visit(IAutoDateHistogramAggregation aggregation);

    void Visit(IMatrixStatsAggregation aggregation);

    void Visit(IWeightedAverageAggregation aggregation);

    void Visit(IMovingFunctionAggregation aggregation);

    void Visit(IMultiTermsAggregation aggregation);

    void Visit(IVariableWidthHistogramAggregation aggregation);
}

public class AggregationVisitor : IAggregationVisitor
{
    public int Depth { get; set; }

    public AggregationVisitorScope Scope { get; set; }

    public virtual void Visit(IValueCountAggregation aggregation) { }

    public virtual void Visit(IMinAggregation aggregation) { }

    public virtual void Visit(ISumAggregation aggregation) { }

    public virtual void Visit(IDateHistogramAggregation aggregation) { }

    public virtual void Visit(IDateRangeAggregation aggregation) { }

    public virtual void Visit(IFiltersAggregation aggregation) { }

    public virtual void Visit(IGeoHashGridAggregation aggregation) { }

    public virtual void Visit(IGeoLineAggregation aggregation) { }

    public virtual void Visit(IGeoTileGridAggregation aggregation) { }

    public virtual void Visit(IHistogramAggregation aggregation) { }

    public virtual void Visit(IIpRangeAggregation aggregation) { }

    public virtual void Visit(IMissingAggregation aggregation) { }

    public virtual void Visit(IReverseNestedAggregation aggregation) { }

    public virtual void Visit(ITermsAggregation aggregation) { }

    public virtual void Visit(ISignificantTextAggregation aggregation) { }

    public virtual void Visit(IPercentileRanksAggregation aggregation) { }

    public virtual void Visit(IChildrenAggregation aggregation) { }

    public virtual void Visit(IAverageBucketAggregation aggregation) { }

    public virtual void Visit(IMaxBucketAggregation aggregation) { }

    public virtual void Visit(ISumBucketAggregation aggregation) { }

    public virtual void Visit(IStatsBucketAggregation aggregation) { }

    public virtual void Visit(IExtendedStatsBucketAggregation aggregation) { }

    public virtual void Visit(IPercentilesBucketAggregation aggregation) { }

    public virtual void Visit(ICumulativeSumAggregation aggregation) { }

    public virtual void Visit(IBucketScriptAggregation aggregation) { }

    public virtual void Visit(ISamplerAggregation aggregation) { }

    public virtual void Visit(IDiversifiedSamplerAggregation aggregation) { }

    public virtual void Visit(IBucketSelectorAggregation aggregation) { }

    public virtual void Visit(IBucketSortAggregation aggregation) { }

    public virtual void Visit(ISerialDifferencingAggregation aggregation) { }

    public virtual void Visit(IMovingAverageAggregation aggregation) { }

    public virtual void Visit(IMinBucketAggregation aggregation) { }

    public virtual void Visit(IDerivativeAggregation aggregation) { }

    public virtual void Visit(IScriptedMetricAggregation aggregation) { }

    public virtual void Visit(ITopHitsAggregation aggregation) { }

    public virtual void Visit(ISignificantTermsAggregation aggregation) { }

    public virtual void Visit(IRangeAggregation aggregation) { }

    public virtual void Visit(IRareTermsAggregation aggregation) { }

    public virtual void Visit(INestedAggregation aggregation) { }

    public virtual void Visit(IParentAggregation aggregation) { }

    public virtual void Visit(ICardinalityAggregation aggregation) { }

    public virtual void Visit(IGlobalAggregation aggregation) { }

    public virtual void Visit(IGeoBoundsAggregation aggregation) { }

    public virtual void Visit(IGeoDistanceAggregation aggregation) { }

    public virtual void Visit(IFilterAggregation aggregation) { }

    public virtual void Visit(IPercentilesAggregation aggregation) { }

    public virtual void Visit(IExtendedStatsAggregation aggregation) { }

    public virtual void Visit(IStatsAggregation aggregation) { }

    public virtual void Visit(IMaxAggregation aggregation) { }

    public virtual void Visit(IAverageAggregation aggregation) { }

    public virtual void Visit(IGeoCentroidAggregation aggregation) { }

    public virtual void Visit(ICompositeAggregation aggregation) { }

    public virtual void Visit(IMedianAbsoluteDeviationAggregation aggregation) { }

    public virtual void Visit(IAdjacencyMatrixAggregation aggregation) { }

    public virtual void Visit(IAutoDateHistogramAggregation aggregation) { }

    public virtual void Visit(IMatrixStatsAggregation aggregation) { }

    public virtual void Visit(IWeightedAverageAggregation aggregation) { }

    public virtual void Visit(IMovingFunctionAggregation aggregation) { }

    public virtual void Visit(IMultiTermsAggregation aggregationContainer) { }

    public virtual void Visit(IAggregation aggregation) { }

    public virtual void Visit(IAggregationContainer aggregationContainer) { }

    public virtual void Visit(IVariableWidthHistogramAggregation aggregationContainer) { }

}
