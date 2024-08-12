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
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.Compound.Bool;

/**
	 * A query that matches documents matching boolean combinations of other queries.
	 * It is built using one or more boolean clauses, each clause with a typed occurrence.
	 *
	 * The occurrence types are:
	 *
	 * `must`::
	 * The clause (query) must appear in matching documents and will contribute to the score.
	 *
	 * `filter`::
	 * The clause (query) must appear in matching documents. However unlike `must`, the score of the query will be ignored.
	 *
	 * `should`::
	 * The clause (query) should appear in the matching document. In a boolean query with no `must` or `filter` clauses, one or more `should` clauses must match a document.
	 * The minimum number of should clauses to match can be set using the `minimum_should_match` parameter.
	 *
	 * `must_not`::
	 * The clause (query) must not appear in the matching documents.
	 *
	 * Check out the <<bool-queries,`bool` queries section>> for more details on `bool` queries with OSC.
	 *
	 * See the OpenSearch documentation on {ref_current}/query-dsl-bool-query.html[bool query] for more details.
	 */
public class BoolQueryUsageTests : QueryDslUsageTestsBase
{
    public BoolQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    // hide
    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IBoolQuery>(a => a.Bool)
    {
        q =>
        {
            q.MustNot = null;
            q.Should = null;
            q.Must = null;
            q.Filter = null;
        },
        q =>
        {
            q.MustNot = Enumerable.Empty<QueryContainer>();
            q.Should = Enumerable.Empty<QueryContainer>();
            q.Must = Enumerable.Empty<QueryContainer>();
            q.Filter = Enumerable.Empty<QueryContainer>();
        },
        q =>
        {
            q.MustNot = new[] { ConditionlessQuery };
            q.Should = new[] { ConditionlessQuery };
            q.Must = new[] { ConditionlessQuery };
            q.Filter = new[] { ConditionlessQuery };
        },
    };

    // hide
    protected override NotConditionlessWhen NotConditionlessWhen => new NotConditionlessWhen<IBoolQuery>(a => a.Bool)
    {
        q =>
        {
            q.MustNot = new[] { VerbatimQuery };
            q.Should = null;
            q.Must = null;
            q.Filter = null;
        },
        q =>
        {
            q.MustNot = null;
            q.Should = new[] { VerbatimQuery };
            q.Must = null;
            q.Filter = null;
        },
        q =>
        {
            q.MustNot = null;
            q.Should = null;
            q.Must = new[] { VerbatimQuery };
            q.Filter = null;
        },
        q =>
        {
            q.MustNot = null;
            q.Should = null;
            q.Must = null;
            q.Filter = new[] { VerbatimQuery };
        },
    };

    protected override QueryContainer QueryInitializer =>
        new BoolQuery
        {
            MustNot = new QueryContainer[] { new MatchAllQuery() },
            Should = new QueryContainer[] { new MatchAllQuery() },
            Must = new QueryContainer[] { new MatchAllQuery() },
            Filter = new QueryContainer[] { new MatchAllQuery() },
            MinimumShouldMatch = 1,
            Boost = 2
        };

    protected override object QueryJson => new
    {
        @bool = new
        {
            must = new[]
            {
                new { match_all = new { } }
            },
            must_not = new[]
            {
                new { match_all = new { } }
            },
            should = new[]
            {
                new { match_all = new { } }
            },
            filter = new[]
            {
                new { match_all = new { } }
            },
            minimum_should_match = 1,
            boost = 2.0,
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .Bool(b => b
            .MustNot(m => m.MatchAll())
            .Should(m => m.MatchAll())
            .Must(m => m.MatchAll())
            .Filter(f => f.MatchAll())
            .MinimumShouldMatch(1)
            .Boost(2));

    // hide
    [U]
    public void NullQueryDoesNotCauseANullReferenceException()
    {
        Action query = () => Client.Search<Project>(s => s
            .Query(q => q
                .Bool(b => b
                    .Filter(f => f
                        .Term(t => t.Name, null)
                    )
                )
            )
        );

        query.Should().NotThrow();
    }
}
