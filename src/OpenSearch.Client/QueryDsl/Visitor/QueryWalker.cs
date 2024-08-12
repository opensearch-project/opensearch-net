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

namespace OpenSearch.Client;

public class QueryWalker
{
    public void Walk(IQueryContainer qd, IQueryVisitor visitor)
    {
        visitor.Visit(qd);
        VisitQuery(qd.MatchAll, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.MatchNone, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.MoreLikeThis, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.MultiMatch, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Fuzzy, visitor, (v, d) =>
        {
            v.Visit(d);
            VisitQuery(d as IFuzzyStringQuery, visitor, (vv, dd) => v.Visit(dd));
            VisitQuery(d as IFuzzyNumericQuery, visitor, (vv, dd) => v.Visit(dd));
            VisitQuery(d as IFuzzyDateQuery, visitor, (vv, dd) => v.Visit(dd));
        });
        VisitQuery(qd.Range, visitor, (v, d) =>
        {
            v.Visit(d);
            VisitQuery(d as IDateRangeQuery, visitor, (vv, dd) => v.Visit(dd));
            VisitQuery(d as INumericRangeQuery, visitor, (vv, dd) => v.Visit(dd));
            VisitQuery(d as ILongRangeQuery, visitor, (vv, dd) => v.Visit(dd));
            VisitQuery(d as ITermRangeQuery, visitor, (vv, dd) => v.Visit(dd));
        });
        VisitQuery(qd.GeoShape, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Shape, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Ids, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Intervals, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Prefix, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.QueryString, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Range, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.RankFeature, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Regexp, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.SimpleQueryString, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Term, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Terms, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Wildcard, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Match, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.MatchPhrase, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.MatchBoolPrefix, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.MatchPhrasePrefix, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Script, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.ScriptScore, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Exists, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.GeoPolygon, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.GeoDistance, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.GeoBoundingBox, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.RawQuery, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.Percolate, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.ParentId, visitor, (v, d) => v.Visit(d));
        VisitQuery(qd.TermsSet, visitor, (v, d) => v.Visit(d));

        VisitQuery(qd.Bool, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(v, d.Filter, VisitorScope.Filter);
            Accept(v, d.Must, VisitorScope.Must);
            Accept(v, d.MustNot, VisitorScope.MustNot);
            Accept(v, d.Should, VisitorScope.Should);
        });

        VisitSpan(qd, visitor);

        VisitQuery(qd.Boosting, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(v, d.PositiveQuery, VisitorScope.PositiveQuery);
            Accept(v, d.NegativeQuery, VisitorScope.NegativeQuery);
        });
        VisitQuery(qd.ConstantScore, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(v, d.Filter);
        });
        VisitQuery(qd.DisMax, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(v, d.Queries);
        });
        VisitQuery(qd.DistanceFeature, visitor, (v, d) =>
        {
            v.Visit(d);
        });
        VisitQuery(qd.FunctionScore, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(v, d.Query);
        });
        VisitQuery(qd.HasChild, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(v, d.Query);
        });
        VisitQuery(qd.HasParent, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(v, d.Query);
        });
        VisitQuery(qd.Knn, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(v, d.Filter);
        });
        VisitQuery(qd.Nested, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(v, d.Query);
        });
    }

    public void Walk(ISpanQuery qd, IQueryVisitor visitor)
    {
        VisitSpanSubQuery(qd.SpanFirst, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Match);
        });
        VisitSpanSubQuery(qd.SpanNear, visitor, (v, d) =>
        {
            v.Visit(d);
            foreach (var q in d.Clauses ?? Enumerable.Empty<ISpanQuery>())
                Accept(visitor, q);
        });
        VisitSpanSubQuery(qd.SpanNot, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Include);
            Accept(visitor, d.Exclude);
        });

        VisitSpanSubQuery(qd.SpanOr, visitor, (v, d) =>
        {
            v.Visit(d);
            foreach (var q in d.Clauses ?? Enumerable.Empty<ISpanQuery>())
                Accept(visitor, q);
        });

        VisitSpanSubQuery(qd.SpanTerm, visitor, (v, d) => v.Visit(d));
        VisitSpanSubQuery(qd.SpanMultiTerm, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Match);
        });
        VisitSpanSubQuery(qd.SpanContaining, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Big);
            Accept(visitor, d.Little);
        });
        VisitSpanSubQuery(qd.SpanWithin, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Big);
            Accept(visitor, d.Little);
        });
    }

    private static void Accept(IQueryVisitor visitor, IEnumerable<IQueryContainer> queries, VisitorScope scope = VisitorScope.Query)
    {
        if (queries == null) return;

        foreach (var f in queries) Accept(visitor, f, scope);
    }

    private static void Accept(IQueryVisitor visitor, IQueryContainer query, VisitorScope scope = VisitorScope.Query)
    {
        if (query == null) return;

        visitor.Scope = scope;
        query.Accept(visitor);
    }

    private static void Accept(IQueryVisitor visitor, ISpanQuery query, VisitorScope scope = VisitorScope.Span)
    {
        if (query == null) return;

        visitor.Scope = scope;
        query.Accept(visitor);
    }

    private static void VisitSpan<T>(T qd, IQueryVisitor visitor) where T : class, IQueryContainer
    {
        VisitSpanSubQuery(qd.SpanFirst, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Match);
        });
        VisitSpanSubQuery(qd.SpanNear, visitor, (v, d) =>
        {
            v.Visit(d);
            foreach (var q in d.Clauses ?? Enumerable.Empty<ISpanQuery>())
                Accept(visitor, q);
        });
        VisitSpanSubQuery(qd.SpanNot, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Include);
            Accept(visitor, d.Exclude);
        });

        VisitSpanSubQuery(qd.SpanOr, visitor, (v, d) =>
        {
            v.Visit(d);
            foreach (var q in d.Clauses ?? Enumerable.Empty<ISpanQuery>())
                Accept(visitor, q);
        });

        VisitSpanSubQuery(qd.SpanTerm, visitor, (v, d) => v.Visit(d));
        VisitSpanSubQuery(qd.SpanMultiTerm, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Match);
        });
        VisitSpanSubQuery(qd.SpanContaining, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Big);
            Accept(visitor, d.Little);
        });
        VisitSpanSubQuery(qd.SpanWithin, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Big);
            Accept(visitor, d.Little);
        });
        VisitSpanSubQuery(qd.SpanFieldMasking, visitor, (v, d) =>
        {
            v.Visit(d);
            Accept(visitor, d.Query);
        });
    }

    private static void VisitQuery<T>(T qd, IQueryVisitor visitor, Action<IQueryVisitor, T> scoped)
        where T : class, IQuery
    {
        if (qd == null) return;

        visitor.Depth = visitor.Depth + 1;
        visitor.Visit(qd);
        scoped(visitor, qd);
        visitor.Depth = visitor.Depth - 1;
    }

    private static void VisitSpanSubQuery<T>(T qd, IQueryVisitor visitor, Action<IQueryVisitor, T> scoped)
        where T : class, ISpanSubQuery
    {
        if (qd == null) return;

        VisitQuery(qd, visitor, (v, d) =>
        {
            visitor.Visit(qd);
            scoped(v, d);
        });
    }
}
