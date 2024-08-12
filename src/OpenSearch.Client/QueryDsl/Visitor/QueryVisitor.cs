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

public interface IQueryVisitor
{
    /// <summary>
    /// The current depth of the node being visited
    /// </summary>
    int Depth { get; set; }

    /// <summary>
    /// Hints the relation with the parent, i,e queries inside a Must clause will have VisitorScope.Must set.
    /// </summary>
    VisitorScope Scope { get; set; }

    /// <summary>
    /// Visit the query container just before we dispatch into the query it holds
    /// </summary>
    /// <param name="queryDescriptor"></param>
    void Visit(IQueryContainer queryDescriptor);

    /// <summary>
    /// Visit every query item just before they are visited by their specialized Visit() implementation
    /// </summary>
    /// <param name="query">The IQuery object that will be visited</param>
    void Visit(IQuery query);

    void Visit(IBoolQuery query);

    void Visit(IBoostingQuery query);

    void Visit(IConstantScoreQuery query);

    void Visit(IDisMaxQuery query);

    void Visit(IDistanceFeatureQuery query);

    void Visit(IFunctionScoreQuery query);

    void Visit(IFuzzyQuery query);

    void Visit(IFuzzyNumericQuery query);

    void Visit(IFuzzyDateQuery query);

    void Visit(IFuzzyStringQuery query);

    void Visit(IHasChildQuery query);

    void Visit(IHasParentQuery query);

    void Visit(IIdsQuery query);

    void Visit(IIntervalsQuery query);

    void Visit(IKnnQuery query);

    void Visit(IMatchQuery query);

    void Visit(IMatchPhraseQuery query);

    void Visit(IMatchPhrasePrefixQuery query);

    void Visit(IMatchAllQuery query);

    void Visit(IMatchBoolPrefixQuery query);

    void Visit(IMatchNoneQuery query);

    void Visit(IMoreLikeThisQuery query);

    void Visit(IMultiMatchQuery query);

    void Visit(INestedQuery query);

    void Visit(IPrefixQuery query);

    void Visit(IQueryStringQuery query);

    void Visit(IRankFeatureQuery query);

    void Visit(IRangeQuery query);

    void Visit(IRegexpQuery query);

    void Visit(ISimpleQueryStringQuery query);

    void Visit(ITermQuery query);

    void Visit(IWildcardQuery query);

    void Visit(ITermsQuery query);

    void Visit(IScriptQuery query);

    void Visit(IScriptScoreQuery query);

    void Visit(IGeoPolygonQuery query);

    void Visit(IGeoDistanceQuery query);

    void Visit(IGeoBoundingBoxQuery query);

    void Visit(IExistsQuery query);

    void Visit(IDateRangeQuery query);

    void Visit(INumericRangeQuery query);

    void Visit(ILongRangeQuery query);

    void Visit(ITermRangeQuery query);

    void Visit(ISpanFirstQuery query);

    void Visit(ISpanNearQuery query);

    void Visit(ISpanNotQuery query);

    void Visit(ISpanOrQuery query);

    void Visit(ISpanTermQuery query);

    void Visit(ISpanQuery query);

    void Visit(ISpanSubQuery query);

    void Visit(ISpanContainingQuery query);

    void Visit(ISpanWithinQuery query);

    void Visit(ISpanMultiTermQuery query);

    void Visit(ISpanFieldMaskingQuery query);

    void Visit(IGeoShapeQuery query);

    void Visit(IShapeQuery query);

    void Visit(IRawQuery query);

    void Visit(IPercolateQuery query);

    void Visit(IParentIdQuery query);

    void Visit(ITermsSetQuery query);
}

public class QueryVisitor : IQueryVisitor
{
    public int Depth { get; set; }

    public VisitorScope Scope { get; set; }

    public virtual void Visit(IQueryContainer query) { }

    public virtual void Visit(IQuery query) { }

    public virtual void Visit(IBoolQuery query) { }

    public virtual void Visit(IBoostingQuery query) { }

    public virtual void Visit(IConstantScoreQuery query) { }

    public virtual void Visit(IDisMaxQuery query) { }

    public virtual void Visit(IDistanceFeatureQuery query) { }

    public virtual void Visit(ISpanContainingQuery query) { }

    public virtual void Visit(ISpanWithinQuery query) { }

    public virtual void Visit(IDateRangeQuery query) { }

    public virtual void Visit(INumericRangeQuery query) { }

    public virtual void Visit(ILongRangeQuery query) { }

    public virtual void Visit(ITermRangeQuery query) { }

    public virtual void Visit(IFunctionScoreQuery query) { }

    public virtual void Visit(IFuzzyQuery query) { }

    public virtual void Visit(IFuzzyStringQuery query) { }

    public virtual void Visit(IFuzzyNumericQuery query) { }

    public virtual void Visit(IFuzzyDateQuery query) { }

    public virtual void Visit(IGeoShapeQuery query) { }

    public virtual void Visit(IShapeQuery query) { }

    public virtual void Visit(IHasChildQuery query) { }

    public virtual void Visit(IHasParentQuery query) { }

    public virtual void Visit(IIdsQuery query) { }

    public virtual void Visit(IIntervalsQuery query) { }

    public virtual void Visit(IKnnQuery query) { }

    public virtual void Visit(IMatchQuery query) { }

    public virtual void Visit(IMatchPhraseQuery query) { }

    public virtual void Visit(IMatchPhrasePrefixQuery query) { }

    public virtual void Visit(IMatchAllQuery query) { }

    public virtual void Visit(IMatchBoolPrefixQuery query) { }

    public virtual void Visit(IMatchNoneQuery query) { }

    public virtual void Visit(IMoreLikeThisQuery query) { }

    public virtual void Visit(IMultiMatchQuery query) { }

    public virtual void Visit(INestedQuery query) { }

    public virtual void Visit(IPrefixQuery query) { }

    public virtual void Visit(IQueryStringQuery query) { }

    public virtual void Visit(IRankFeatureQuery query) { }

    public virtual void Visit(IRangeQuery query) { }

    public virtual void Visit(IRegexpQuery query) { }

    public virtual void Visit(ISimpleQueryStringQuery query) { }

    public virtual void Visit(ISpanFirstQuery query) { }

    public virtual void Visit(ISpanNearQuery query) { }

    public virtual void Visit(ISpanNotQuery query) { }

    public virtual void Visit(ISpanOrQuery query) { }

    public virtual void Visit(ISpanTermQuery query) { }

    public virtual void Visit(ISpanSubQuery query) { }

    public virtual void Visit(ISpanMultiTermQuery query) { }

    public virtual void Visit(ISpanFieldMaskingQuery query) { }

    public virtual void Visit(ITermQuery query) { }

    public virtual void Visit(IWildcardQuery query) { }

    public virtual void Visit(ITermsQuery query) { }

    public virtual void Visit(IScriptQuery query) { }

    public virtual void Visit(IScriptScoreQuery query) { }

    public virtual void Visit(IGeoPolygonQuery query) { }

    public virtual void Visit(IGeoDistanceQuery query) { }

    public virtual void Visit(ISpanQuery query) { }

    public virtual void Visit(IGeoBoundingBoxQuery query) { }

    public virtual void Visit(IExistsQuery query) { }

    public virtual void Visit(IRawQuery query) { }

    public virtual void Visit(IPercolateQuery query) { }

    public virtual void Visit(IParentIdQuery query) { }

    public virtual void Visit(ITermsSetQuery query) { }

    public virtual void Visit(IQueryVisitor visitor) { }
}
