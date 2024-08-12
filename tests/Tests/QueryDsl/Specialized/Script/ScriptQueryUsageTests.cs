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

namespace Tests.QueryDsl.Specialized.Script;

/**
	* A query allowing to define {ref_current}/modules-scripting.html[scripts] as queries.
	*
	* See the OpenSearch documentation on {ref_current}/query-dsl-script-query.html[script query] for more details.
	*/
public class ScriptQueryUsageTests : QueryDslUsageTestsBase
{
    private static readonly string _templateString = "doc['numberOfCommits'].value > params.param1";

    public ScriptQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IScriptQuery>(a => a.Script)
    {
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

    protected override QueryContainer QueryInitializer => new ScriptQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Script = new InlineScript(_templateString)
        {
            Params = new Dictionary<string, object>
            {
                { "param1", 50 }
            }
        },
    };

    protected override object QueryJson => new
    {
        script = new
        {
            _name = "named_query",
            boost = 1.1,
            script = new
            {
                source = "doc['numberOfCommits'].value > params.param1",
                @params = new { param1 = 50 }
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .Script(sn => sn
            .Name("named_query")
            .Boost(1.1)
            .Script(s => s
                .Source(_templateString)
                .Params(p => p.Add("param1", 50))
            )
        );
}
