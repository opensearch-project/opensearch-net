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

using System.Collections.Generic;
using System.Linq;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.QueryDsl.Joining.Nested;

/**
	* Nested query allows to query nested objects / docs (see {ref_current}/nested.html[nested mapping]).
	* The query is executed against the nested objects / docs as if they were indexed as separate
	* docs (they are, internally) and resulting in the root parent doc (or parent nested mapping).
	*
	* See the OpenSearch documentation on {ref_current}/query-dsl-nested-query.html[nested query] for more details.
	*/
public class NestedUsageTests : QueryDslUsageTestsBase
{
    public NestedUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<INestedQuery>(a => a.Nested)
    {
        q => q.Query = null,
        q => q.Query = ConditionlessQuery,
        q => q.Path = null,
    };

    protected override QueryContainer QueryInitializer => new NestedQuery
    {
        Name = "named_query",
        Boost = 1.1,
        InnerHits = new InnerHits { Explain = true },
        Path = Field<Project>(p => p.Tags),
        Query = new TermsQuery
        {
            Field = Field<Project>(p => p.Tags.First().Name),
            Terms = new[] { "lorem", "ipsum" }
        },
        IgnoreUnmapped = true
    };

    protected override object QueryJson => new
    {
        nested = new
        {
            _name = "named_query",
            boost = 1.1,
            query = new
            {
                terms = new Dictionary<string, object>
                {
                    { "tags.name", new[] { "lorem", "ipsum" } }
                }
            },
            ignore_unmapped = true,
            path = "tags",
            inner_hits = new
            {
                explain = true
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .Nested(c => c
            .Name("named_query")
            .Boost(1.1)
            .InnerHits(i => i.Explain())
            .Path(p => p.Tags)
            .Query(nq => nq
                .Terms(t => t
                    .Field(f => f.Tags.First().Name)
                    .Terms("lorem", "ipsum")
                )
            )
            .IgnoreUnmapped()
        );
}
