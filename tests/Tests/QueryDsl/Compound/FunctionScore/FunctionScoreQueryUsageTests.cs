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

namespace Tests.QueryDsl.Compound.FunctionScore;

/*
	* The function_score allows you to modify the score of documents that are retrieved by a query.
	* This can be useful if, for example, a score function is computationally expensive and it is
	* sufficient to compute the score on a filtered set of documents.
	*
	* See the OpenSearch documentation on {ref_current}/query-dsl-function-score-query.html[function score query] for more details.
	*/
public class FunctionScoreQueryUsageTests : QueryDslUsageTestsBase
{
    public FunctionScoreQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IFunctionScoreQuery>(a => a.FunctionScore)
    {
        q => q.Functions = null,
        q => q.Functions = Enumerable.Empty<IScoreFunction>(),
    };

    protected override QueryContainer QueryInitializer => new FunctionScoreQuery()
    {
        Name = "named_query",
        Boost = 1.1,
        Query = new MatchAllQuery(),
        BoostMode = FunctionBoostMode.Multiply,
        ScoreMode = FunctionScoreMode.Sum,
        MaxBoost = 20.0,
        MinScore = 1.0,
        Functions = new List<IScoreFunction>
        {
            new ExponentialDecayFunction
            {
                Origin = 1.0,
                Decay = 0.5,
                Field = Field<Project>(p => p.NumberOfCommits),
                Scale = 0.1,
                Weight = 2.1,
                Filter = new NumericRangeQuery
                {
                    Field = Field<Project>(f => f.NumberOfContributors),
                    GreaterThan = 10
                }
            },
            new GaussDateDecayFunction
                { Origin = DateMath.Now, Field = Field<Project>(p => p.LastActivity), Decay = 0.5, Scale = TimeSpan.FromDays(1) },
            new LinearGeoDecayFunction
            {
                Origin = new GeoLocation(70, -70), Field = Field<Project>(p => p.LocationPoint), Scale = Distance.Miles(1),
                MultiValueMode = MultiValueMode.Average
            },
            new FieldValueFactorFunction
            {
                Field = Field<Project>(p => p.NumberOfContributors),
                Factor = 1.1,
                Missing = 0.1,
                Modifier = FieldValueFactorModifier.Square,
                Weight = 3,
                Filter = new TermQuery
                {
                    Field = Field<Project>(p => p.Branches),
                    Value = "dev"
                }
            },
            new RandomScoreFunction { Seed = 1337, Field = "_seq_no" },
            new RandomScoreFunction { Seed = "randomstring", Field = "_seq_no" },
            new WeightFunction { Weight = 1.0 },
            new ScriptScoreFunction { Script = new InlineScript("Math.log(2 + doc['numberOfCommits'].value)"), Weight = 2.0 }
        }
    };

    protected override object QueryJson => new
    {
        function_score = new
        {
            _name = "named_query",
            boost = 1.1,
            boost_mode = "multiply",
            functions = new object[]
            {
                new
                {
                    exp = new
                    {
                        numberOfCommits = new
                        {
                            origin = 1.0,
                            scale = 0.1,
                            decay = 0.5
                        }
                    },
                    weight = 2.1,
                    filter = new
                    {
                        range = new
                        {
                            numberOfContributors = new
                            {
                                gt = 10.0
                            }
                        }
                    }
                },
                new
                {
                    gauss = new
                    {
                        lastActivity = new
                        {
                            origin = "now",
                            scale = "1d",
                            decay = 0.5
                        }
                    }
                },
                new
                {
                    linear = new
                    {
                        locationPoint = new
                        {
                            origin = new
                            {
                                lat = 70.0,
                                lon = -70.0
                            },
                            scale = "1mi"
                        },
                        multi_value_mode = "avg"
                    }
                },
                new
                {
                    filter = new
                    {
                        term = new
                        {
                            branches = new
                            {
                                value = "dev"
                            }
                        }
                    },
                    field_value_factor = new
                    {
                        field = "numberOfContributors",
                        factor = 1.1,
                        missing = 0.1,
                        modifier = "square"
                    },
                    weight = 3.0
                },
                new { random_score = new { seed = 1337, field = "_seq_no" } },
                new { random_score = new { seed = "randomstring", field = "_seq_no" } },
                new { weight = 1.0 },
                new
                {
                    script_score = new
                    {
                        script = new
                        {
                            source = "Math.log(2 + doc['numberOfCommits'].value)"
                        }
                    },
                    weight = 2.0
                }
            },
            max_boost = 20.0,
            min_score = 1.0,
            query = new
            {
                match_all = new { }
            },
            score_mode = "sum"
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .FunctionScore(c => c
            .Name("named_query")
            .Boost(1.1)
            .Query(qq => qq.MatchAll())
            .BoostMode(FunctionBoostMode.Multiply)
            .ScoreMode(FunctionScoreMode.Sum)
            .MaxBoost(20.0)
            .MinScore(1.0)
            .Functions(f => f
                .Exponential(b => b
                    .Field(p => p.NumberOfCommits)
                    .Decay(0.5)
                    .Origin(1.0)
                    .Scale(0.1)
                    .Weight(2.1)
                    .Filter(fi => fi
                        .Range(r => r
                            .Field(p => p.NumberOfContributors)
                            .GreaterThan(10)
                        )
                    )
                )
                .GaussDate(b => b.Field(p => p.LastActivity).Origin(DateMath.Now).Decay(0.5).Scale("1d"))
                .LinearGeoLocation(b => b
                    .Field(p => p.LocationPoint)
                    .Origin(new GeoLocation(70, -70))
                    .Scale(Distance.Miles(1))
                    .MultiValueMode(MultiValueMode.Average)
                )
                .FieldValueFactor(b => b
                    .Field(p => p.NumberOfContributors)
                    .Factor(1.1)
                    .Missing(0.1)
                    .Modifier(FieldValueFactorModifier.Square)
                    .Weight(3)
                    .Filter(fi => fi
                        .Term(t => t
                            .Field(p => p.Branches)
                            .Value("dev")
                        )
                    )
                )
                .RandomScore(r => r.Seed(1337).Field("_seq_no"))
                .RandomScore(r => r.Seed("randomstring").Field("_seq_no"))
                .Weight(1.0)
                .ScriptScore(s => s
                    .Script(ss => ss
                        .Source("Math.log(2 + doc['numberOfCommits'].value)")
                    )
                    .Weight(2)
                )
            )
        );
}
