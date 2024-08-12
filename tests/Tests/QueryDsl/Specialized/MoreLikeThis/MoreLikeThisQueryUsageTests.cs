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

namespace Tests.QueryDsl.Specialized.MoreLikeThis;

public class MoreLikeThisQueryUsageTests : QueryDslUsageTestsBase
{
    public MoreLikeThisQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IMoreLikeThisQuery>(a => a.MoreLikeThis)
    {
        q =>
        {
            q.Like = null;
            q.Fields = null;
        },
        q =>
        {
            q.Like = Enumerable.Empty<Like>();
            q.Fields = null;
        },
        q =>
        {
            q.Fields = null;
            q.Like = new [] { new Like("") };
        },
    };

    protected override QueryContainer QueryInitializer => new MoreLikeThisQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Fields = Fields<Project>(p => p.Name),
        Like = new List<Like>
        {
            new LikeDocument<Project>(Project.Instance.Name) { Routing = Project.Instance.Name },
            "some long text"
        },
        Analyzer = "some_analyzer",
        BoostTerms = 1.1,
        Include = true,
        MaxDocumentFrequency = 12,
        MaxQueryTerms = 12,
        MaxWordLength = 300,
        MinDocumentFrequency = 1,
        MinTermFrequency = 1,
        MinWordLength = 10,
        MinimumShouldMatch = 1,
        StopWords = new[] { "and", "the" },
        Unlike = new List<Like>
        {
            "not like this text"
        }
    };

    protected override object QueryJson => new
    {
        more_like_this = new
        {
            fields = new[] { "name" },
            minimum_should_match = 1,
            stop_words = new[] { "and", "the" },
            min_term_freq = 1,
            max_query_terms = 12,
            min_doc_freq = 1,
            max_doc_freq = 12,
            min_word_length = 10,
            max_word_length = 300,
            boost_terms = 1.1,
            analyzer = "some_analyzer",
            include = true,
            like = new object[]
            {
                new
                {
                    _index = "project",
                    _id = Project.Instance.Name,
                    routing = Project.Instance.Name
                },
                "some long text"
            },
            unlike = new[] { "not like this text" },
            _name = "named_query",
            boost = 1.1
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .MoreLikeThis(sn => sn
            .Name("named_query")
            .Boost(1.1)
            .Like(l => l
                .Document(d => d.Id(Project.Instance.Name).Routing(Project.Instance.Name))
                .Text("some long text")
            )
            .Analyzer("some_analyzer")
            .BoostTerms(1.1)
            .Include()
            .MaxDocumentFrequency(12)
            .MaxQueryTerms(12)
            .MaxWordLength(300)
            .MinDocumentFrequency(1)
            .MinTermFrequency(1)
            .MinWordLength(10)
            .StopWords("and", "the")
            .MinimumShouldMatch(1)
            .Fields(f => f.Field(p => p.Name))
            .Unlike(l => l
                .Text("not like this text")
            )
        );
}
