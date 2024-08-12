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

namespace Tests.QueryDsl.FullText.QueryString;

public class QueryStringUsageTests : QueryDslUsageTestsBase
{
    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IQueryStringQuery>(a => a.QueryString)
    {
        q => q.Query = null,
        q => q.Query = string.Empty,
    };

    public QueryStringUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object QueryJson => new
    {
        query_string = new
        {
            _name = "named_query",
            boost = 1.1,
            query = "hello world",
            default_operator = "or",
            analyzer = "standard",
            quote_analyzer = "keyword",
            allow_leading_wildcard = true,
            fuzzy_max_expansions = 3,
            fuzziness = "AUTO",
            fuzzy_prefix_length = 2,
            analyze_wildcard = true,
            max_determinized_states = 2,
            minimum_should_match = 2,
            lenient = true,
            fields = new[] { "description", "myOtherField" },
            tie_breaker = 1.2,
            rewrite = "constant_score",
            fuzzy_rewrite = "constant_score",
            quote_field_suffix = "'",
            escape = true,
            auto_generate_synonyms_phrase_query = false
        }
    };

    protected override QueryContainer QueryInitializer => new QueryStringQuery
    {
        Fields = Field<Project>(p => p.Description).And("myOtherField"),
        Boost = 1.1,
        Name = "named_query",
        Query = "hello world",
        DefaultOperator = Operator.Or,
        Analyzer = "standard",
        QuoteAnalyzer = "keyword",
        AllowLeadingWildcard = true,
        MaximumDeterminizedStates = 2,
        Escape = true,
        FuzzyPrefixLength = 2,
        FuzzyMaxExpansions = 3,
        FuzzyRewrite = MultiTermQueryRewrite.ConstantScore,
        Rewrite = MultiTermQueryRewrite.ConstantScore,
        Fuzziness = Fuzziness.Auto,
        TieBreaker = 1.2,
        AnalyzeWildcard = true,
        MinimumShouldMatch = 2,
        QuoteFieldSuffix = "'",
        Lenient = true,
        AutoGenerateSynonymsPhraseQuery = false
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .QueryString(c => c
            .Name("named_query")
            .Boost(1.1)
            .Fields(f => f.Field(p => p.Description).Field("myOtherField"))
            .Query("hello world")
            .DefaultOperator(Operator.Or)
            .Analyzer("standard")
            .QuoteAnalyzer("keyword")
            .AllowLeadingWildcard()
            .MaximumDeterminizedStates(2)
            .Escape()
            .FuzzyPrefixLength(2)
            .FuzzyMaxExpansions(3)
            .FuzzyRewrite(MultiTermQueryRewrite.ConstantScore)
            .Rewrite(MultiTermQueryRewrite.ConstantScore)
            .Fuzziness(Fuzziness.Auto)
            .TieBreaker(1.2)
            .AnalyzeWildcard()
            .MinimumShouldMatch(2)
            .QuoteFieldSuffix("'")
            .Lenient()
            .AutoGenerateSynonymsPhraseQuery(false)
        );
}
