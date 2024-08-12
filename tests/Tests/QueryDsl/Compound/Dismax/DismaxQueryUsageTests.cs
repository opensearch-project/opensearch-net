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

#pragma warning disable 618 //Testing an obsolete method

namespace Tests.QueryDsl.Compound.Dismax;

public class DismaxQueryUsageTests : QueryDslUsageTestsBase
{
    public DismaxQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IDisMaxQuery>(a => a.DisMax)
    {
        q => q.Queries = null,
        q => q.Queries = Enumerable.Empty<QueryContainer>(),
        q => q.Queries = new[] { ConditionlessQuery },
    };

    protected override NotConditionlessWhen NotConditionlessWhen => new NotConditionlessWhen<IDisMaxQuery>(a => a.DisMax)
    {
        q => q.Queries = new[] { VerbatimQuery },
        q => q.Queries = new[] { VerbatimQuery, ConditionlessQuery },
    };

    protected override QueryContainer QueryInitializer => new DisMaxQuery()
    {
        Name = "named_query",
        Boost = 1.1,
        TieBreaker = 0.11,
        Queries = new QueryContainer[]
        {
            new MatchAllQuery() { Name = "query1" },
            new MatchAllQuery() { Name = "query2" },
        }
    };

    protected override object QueryJson => new
    {
        dis_max = new
        {
            _name = "named_query",
            boost = 1.1,
            queries = new[]
            {
                new { match_all = new { _name = "query1" } },
                new { match_all = new { _name = "query2" } }
            },
            tie_breaker = 0.11
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .DisMax(c => c
            .Name("named_query")
            .Boost(1.1)
            .TieBreaker(0.11)
            .Queries(
                qq => qq.MatchAll(m => m.Name("query1")),
                qq => qq.MatchAll(m => m.Name("query2"))
            )
        );

    [U]
    public void NullQueryDoesNotCauseANullReferenceException()
    {
        Action query = () => Client.Search<Project>(s => s
            .Query(q => q
                .DisMax(dm => dm
                    .Queries(
                        dmq => dmq.Term(t => t.Name, null)
                    )
                )
            )
        );

        query.Should().NotThrow();
    }
}
