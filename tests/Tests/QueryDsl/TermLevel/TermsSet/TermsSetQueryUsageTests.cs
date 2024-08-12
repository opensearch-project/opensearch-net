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

using System.Linq;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.TermLevel.TermsSet;

/**
	* Returns any documents that match with at least one or more of the provided terms. The terms are not
	* analyzed and thus must match exactly. The number of terms that must match varies per document and
	* is either controlled by a minimum should match field or computed per document in a minimum should match script.
	*
	* Be sure to read the OpenSearch documentation on {ref_current}/query-dsl-terms-set-query.html[Terms Set query] for more information.
	*
	* [float]
	*=== Minimum should match with field
	*
	* The field that controls the number of required terms that must match must be a number field
	*/
public class TermsSetQueryUsageTests : QueryDslUsageTestsBase
{
    public TermsSetQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<ITermsSetQuery>(a => a.TermsSet)
    {
        q => q.Field = null,
        q => q.Terms = null,
        q => q.Terms = Enumerable.Empty<object>(),
        q => q.Terms = new[] { "" },
        q =>
        {
            q.MinimumShouldMatchField = null;
            q.MinimumShouldMatchScript = null;
        }
    };

    protected override QueryContainer QueryInitializer => new TermsSetQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Field = Infer.Field<Project>(p => p.Branches),
        Terms = new[] { "main", "dev" },
        MinimumShouldMatchField = Infer.Field<Project>(p => p.RequiredBranches)
    };

    protected override object QueryJson => new
    {
        terms_set = new
        {
            branches = new
            {
                _name = "named_query",
                boost = 1.1,
                terms = new[] { "main", "dev" },
                minimum_should_match_field = "requiredBranches"
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .TermsSet(c => c
            .Name("named_query")
            .Boost(1.1)
            .Field(p => p.Branches)
            .Terms("main", "dev")
            .MinimumShouldMatchField(p => p.RequiredBranches)
        );
}

/**[float]
	*=== Minimum should match with script
	*
	* Scripts can also be used to control how many terms are required to match in a more dynamic way.
	*
	* The `params.num_terms` parameter is available in the script to indicate the number of
	* terms that have been specified in the query.
	*/
public class TermsSetScriptQueryUsageTests : QueryDslUsageTestsBase
{
    public TermsSetScriptQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override QueryContainer QueryInitializer => new TermsSetQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Field = Infer.Field<Project>(p => p.Branches),
        Terms = new[] { "main", "dev" },
        MinimumShouldMatchScript = new InlineScript("doc['requiredBranches'].size() == 0 ? params.num_terms : Math.min(params.num_terms, doc['requiredBranches'].value)")
    };

    protected override object QueryJson => new
    {
        terms_set = new
        {
            branches = new
            {
                _name = "named_query",
                boost = 1.1,
                terms = new[] { "main", "dev" },
                minimum_should_match_script = new
                {
                    source = "doc['requiredBranches'].size() == 0 ? params.num_terms : Math.min(params.num_terms, doc['requiredBranches'].value)"
                }
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .TermsSet(c => c
            .Name("named_query")
            .Boost(1.1)
            .Field(p => p.Branches)
            .Terms("main", "dev")
            .MinimumShouldMatchScript(s => s
                .Source("doc['requiredBranches'].size() == 0 ? params.num_terms : Math.min(params.num_terms, doc['requiredBranches'].value)")
            )
        );
}
