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

using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.TermLevel.Regexp;

public class RegexpQueryUsageTests : QueryDslUsageTestsBase
{
    public RegexpQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IRegexpQuery>(a => a.Regexp)
    {
        q => q.Field = null,
        q => q.Value = null,
        q => q.Value = string.Empty
    };

    protected override QueryContainer QueryInitializer => new RegexpQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Field = "description",
        Value = "s.*y",
        Flags = "INTERSECTION|COMPLEMENT|EMPTY",
        MaximumDeterminizedStates = 20000,
        Rewrite = MultiTermQueryRewrite.TopTerms(10)
    };

    protected override object QueryJson => new
    {
        regexp = new
        {
            description = new
            {
                _name = "named_query",
                boost = 1.1,
                flags = "INTERSECTION|COMPLEMENT|EMPTY",
                max_determinized_states = 20000,
                value = "s.*y",
                rewrite = "top_terms_10"
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .Regexp(c => c
            .Name("named_query")
            .Boost(1.1)
            .Field(p => p.Description)
            .Value("s.*y")
            .Flags("INTERSECTION|COMPLEMENT|EMPTY")
            .MaximumDeterminizedStates(20000)
            .Rewrite(MultiTermQueryRewrite.TopTerms(10))
        );
}
