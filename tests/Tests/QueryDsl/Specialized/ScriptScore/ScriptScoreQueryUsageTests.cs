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
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.Specialized.ScriptScore;

/**
	* A query allowing you to modify the score of documents that are retrieved by a query.
	* This can be useful if, for example, a score function is computationally expensive and
	* it is sufficient to compute the score on a filtered set of documents.
	*
	* See the OpenSearch documentation on {ref_current}/query-dsl-script-score-query.html[script_score query] for more details.
	*/
public class ScriptScoreQueryUsageTests : QueryDslUsageTestsBase
{
    private static readonly string _scriptScoreSource = "decayNumericLinear(params.origin, params.scale, params.offset, params.decay, doc['numberOfCommits'].value)";

    public ScriptScoreQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IScriptScoreQuery>(a => a.ScriptScore)
    {
        q =>
        {
            q.Query = null;
        },
        q =>
        {
            q.Script = null;
        },
        q =>
        {
            q.Script = new InlineScript(null);
        },
        q =>
        {
            q.Script = new InlineScript("");
        },
        q =>
        {
            q.Script = new IndexedScript(null);
        },
        q =>
        {
            q.Script = new IndexedScript("");
        }
    };

    protected override QueryContainer QueryInitializer => new ScriptScoreQuery
    {
        Name = "named_query",
        Boost = 1.1,
        MinScore = 1.2,
        Query = new NumericRangeQuery
        {
            Field = Infer.Field<Project>(f => f.NumberOfCommits),
            GreaterThan = 50
        },
        Script = new InlineScript(_scriptScoreSource)
        {
            Params = new Dictionary<string, object>
            {
                { "origin", 100 },
                { "scale", 10 },
                { "decay", 0.5 },
                { "offset", 0 }
            }
        },
    };

    protected override object QueryJson => new
    {
        script_score = new
        {
            _name = "named_query",
            boost = 1.1,
            min_score = 1.2,
            query = new
            {
                range = new
                {
                    numberOfCommits = new
                    {
                        gt = 50.0
                    }
                }
            },
            script = new
            {
                source = _scriptScoreSource,
                @params = new
                {
                    origin = 100,
                    scale = 10,
                    decay = 0.5,
                    offset = 0
                }
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .ScriptScore(sn => sn
            .Name("named_query")
            .Boost(1.1)
            .MinScore(1.2)
            .Query(qq => qq
                .Range(r => r
                    .Field(f => f.NumberOfCommits)
                    .GreaterThan(50)
                )
            )
            .Script(s => s
                .Source(_scriptScoreSource)
                .Params(p => p
                    .Add("origin", 100)
                    .Add("scale", 10)
                    .Add("decay", 0.5)
                    .Add("offset", 0)
                )
            )
        );
}
