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
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Search.Search.Rescoring;

/**
	 * Rescoring can help to improve precision by reordering just the top (eg 100 - 500) documents
	 * returned by the query and post_filter phases, using a secondary (usually more costly) algorithm,
	 * instead of applying the costly algorithm to all documents in the index.
	 *
	 * See the OpenSearch documentation on {ref_current}/search-request-rescore.html[Rescoring] for more detail.
	 */
public class RescoreUsageTests : SearchUsageTestBase
{
    public RescoreUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        from = 10,
        size = 20,
        query = new
        {
            match_all = new { }
        },
        rescore = new object[]
        {
            new
            {
                window_size = 20,
                query = new
                {
                    score_mode = "multiply",
                    rescore_query = new
                    {
                        constant_score = new
                        {
                            filter = new
                            {
                                terms = new
                                {
                                    tags = new[] { "eos", "sit", "sed" }
                                }
                            }
                        }
                    }
                }
            },
            new
            {
                query = new
                {
                    score_mode = "total",
                    rescore_query = new
                    {
                        function_score = new
                        {
                            functions = new object[]
                            {
                                new
                                {
                                    random_score = new
                                    {
                                        seed = 1337,
                                        field = "_seq_no"
                                    }
                                }
                            }
                        }
                    }
                }
            },
        }
    };

    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .From(10)
        .Size(20)
        .Query(q => q
            .MatchAll()
        )
        .Rescore(r => r
            .Rescore(rr => rr
                .WindowSize(20)
                .RescoreQuery(rq => rq
                    .ScoreMode(ScoreMode.Multiply)
                    .Query(q => q
                        .ConstantScore(cs => cs
                            .Filter(f => f
                                .Terms(t => t
                                    .Field(p => p.Tags.First())
                                    .Terms("eos", "sit", "sed")
                                )
                            )
                        )
                    )
                )
            )
            .Rescore(rr => rr
                .RescoreQuery(rq => rq
                    .ScoreMode(ScoreMode.Total)
                    .Query(q => q
                        .FunctionScore(fs => fs
                            .Functions(f => f
                                .RandomScore(rs => rs.Seed(1337).Field("_seq_no"))
                            )
                        )
                    )
                )
            )
        );

    protected override SearchRequest<Project> Initializer => new SearchRequest<Project>
    {
        From = 10,
        Size = 20,
        Query = new QueryContainer(new MatchAllQuery()),
        Rescore = new List<IRescore>
        {
            new Rescore
            {
                WindowSize = 20,
                Query = new RescoreQuery
                {
                    ScoreMode = ScoreMode.Multiply,
                    Query = new ConstantScoreQuery
                    {
                        Filter = new TermsQuery
                        {
                            Field = Field<Project>(p => p.Tags.First()),
                            Terms = new[] { "eos", "sit", "sed" }
                        }
                    }
                }
            },
            new Rescore
            {
                Query = new RescoreQuery
                {
                    ScoreMode = ScoreMode.Total,
                    Query = new FunctionScoreQuery
                    {
                        Functions = new List<IScoreFunction>
                        {
                            new RandomScoreFunction { Seed = 1337, Field = "_seq_no" },
                        }
                    }
                }
            }
        }
    };
}
