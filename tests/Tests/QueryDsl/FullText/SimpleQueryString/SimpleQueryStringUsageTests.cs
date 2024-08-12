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
using static OpenSearch.Client.Infer;


namespace Tests.QueryDsl.FullText.SimpleQueryString;

public class SimpleQueryStringUsageTests : QueryDslUsageTestsBase
{
    public SimpleQueryStringUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<ISimpleQueryStringQuery>(a => a.SimpleQueryString)
    {
        q => q.Query = null,
        q => q.Query = string.Empty,
    };

    protected override QueryContainer QueryInitializer => new SimpleQueryStringQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Fields = Field<Project>(p => p.Description).And("myOtherField"),
        Query = "hello world",
        Analyzer = "standard",
        DefaultOperator = Operator.Or,
        Flags = SimpleQueryStringFlags.And | SimpleQueryStringFlags.Near,
        Lenient = true,
        AnalyzeWildcard = true,
        MinimumShouldMatch = "30%",
        FuzzyPrefixLength = 0,
        FuzzyMaxExpansions = 50,
        FuzzyTranspositions = true,
        AutoGenerateSynonymsPhraseQuery = false
    };

    protected override object QueryJson => new
    {
        simple_query_string = new
        {
            _name = "named_query",
            boost = 1.1,
            fields = new[] { "description", "myOtherField" },
            query = "hello world",
            analyzer = "standard",
            default_operator = "or",
            flags = "AND|NEAR",
            lenient = true,
            analyze_wildcard = true,
            minimum_should_match = "30%",
            fuzzy_prefix_length = 0,
            fuzzy_max_expansions = 50,
            fuzzy_transpositions = true,
            auto_generate_synonyms_phrase_query = false
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .SimpleQueryString(c => c
            .Name("named_query")
            .Boost(1.1)
            .Fields(f => f.Field(p => p.Description).Field("myOtherField"))
            .Query("hello world")
            .Analyzer("standard")
            .DefaultOperator(Operator.Or)
            .Flags(SimpleQueryStringFlags.And | SimpleQueryStringFlags.Near)
            .Lenient()
            .AnalyzeWildcard()
            .MinimumShouldMatch("30%")
            .FuzzyPrefixLength(0)
            .FuzzyMaxExpansions(50)
            .FuzzyTranspositions()
            .AutoGenerateSynonymsPhraseQuery(false)
        );
}
