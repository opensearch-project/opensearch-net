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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Search.Request;

/**
	 * The suggest feature suggests similar looking terms based on a provided text by using a suggester.
	 *
	 * See the OpenSearch documentation on {ref_current}/search-suggesters.html[Suggesters] for more detail.
	 */
public class SuggestUsageTests : SearchUsageTestBase
{
    public SuggestUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson =>
        new
        {
            query = ProjectFilterExpectedJson,
            docvalue_fields = new[] { "state" },
            suggest = new Dictionary<string, object>
            {
                {
                    "my-completion-suggest", new
                    {
                        completion = new
                        {
                            analyzer = "simple",
                            contexts = new
                            {
                                color = new object[]
                                {
                                    new { context = Project.First.Suggest.Contexts.Values.SelectMany(v => v).First() }
                                }
                            },
                            field = "suggest",
                            fuzzy = new
                            {
                                fuzziness = "AUTO",
                                min_length = 1,
                                prefix_length = 2,
                                transpositions = true,
                                unicode_aware = false
                            },
                            size = 8,
                            skip_duplicates = true
                        },
                        prefix = Project.Instance.Name
                    }
                },
                {
                    "my-phrase-suggest", new
                    {
                        phrase = new
                        {
                            collate = new
                            {
                                query = new
                                {
                                    source = "{ \"match\": { \"{{field_name}}\": \"{{suggestion}}\" }}",
                                },
                                @params = new
                                {
                                    field_name = "title"
                                },
                                prune = true,
                            },
                            confidence = 10.1,
                            direct_generator = new[]
                            {
                                new { field = "description" }
                            },
                            field = "name",
                            gram_size = 1,
                            real_word_error_likelihood = 0.5,
                            token_limit = 5,
                            force_unigrams = false
                        },
                        text = "hello world"
                    }
                },
                {
                    "my-term-suggest", new
                    {
                        term = new
                        {
                            analyzer = "standard",
                            field = "name",
                            max_edits = 1,
                            max_inspections = 2,
                            max_term_freq = 3.0,
                            min_doc_freq = 4.0,
                            min_word_length = 5,
                            prefix_length = 6,
                            shard_size = 7,
                            size = 8,
                            suggest_mode = "always"
                        },
                        text = "hello world"
                    }
                }
            }
        };

    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .Query(q => ProjectFilter)
        .DocValueFields(d => d
            .Field(f => f.State)
        )
        .Suggest(ss => ss
            .Term("my-term-suggest", t => t
                .MaxEdits(1)
                .MaxInspections(2)
                .MaxTermFrequency(3)
                .MinDocFrequency(4)
                .MinWordLength(5)
                .PrefixLength(6)
                .SuggestMode(SuggestMode.Always)
                .Analyzer("standard")
                .Field(p => p.Name)
                .ShardSize(7)
                .Size(8)
                .Text("hello world")
            )
            .Completion("my-completion-suggest", c => c
                .Contexts(ctxs => ctxs
                    .Context("color",
                        ctx => ctx.Context(Project.First.Suggest.Contexts.Values.SelectMany(v => v).First())
                    )
                )
                .Fuzzy(f => f
                    .Fuzziness(Fuzziness.Auto)
                    .MinLength(1)
                    .PrefixLength(2)
                    .Transpositions()
                    .UnicodeAware(false)
                )
                .Analyzer("simple")
                .Field(p => p.Suggest)
                .Size(8)
                .Prefix(Project.Instance.Name)
                .SkipDuplicates()
            )
            .Phrase("my-phrase-suggest", ph => ph
                .Collate(c => c
                    .Query(q => q
                        .Source("{ \"match\": { \"{{field_name}}\": \"{{suggestion}}\" }}")
                    )
                    .Params(p => p.Add("field_name", "title"))
                    .Prune()
                )
                .Confidence(10.1)
                .DirectGenerator(d => d
                    .Field(p => p.Description)
                )
                .GramSize(1)
                .Field(p => p.Name)
                .Text("hello world")
                .RealWordErrorLikelihood(0.5)
                .TokenLimit(5)
                .ForceUnigrams(false)
            )
        );

    protected override SearchRequest<Project> Initializer =>
        new SearchRequest<Project>
        {
            Query = ProjectFilter,
            DocValueFields = Fields<Project>(f => f.State),
            Suggest = new SuggestContainer
            {
                {
                    "my-term-suggest", new SuggestBucket
                    {
                        Text = "hello world",
                        Term = new TermSuggester
                        {
                            MaxEdits = 1,
                            MaxInspections = 2,
                            MaxTermFrequency = 3,
                            MinDocFrequency = 4,
                            MinWordLength = 5,
                            PrefixLength = 6,
                            SuggestMode = SuggestMode.Always,
                            Analyzer = "standard",
                            Field = Field<Project>(p => p.Name),
                            ShardSize = 7,
                            Size = 8
                        }
                    }
                },
                {
                    "my-completion-suggest", new SuggestBucket
                    {
                        Prefix = Project.Instance.Name,
                        Completion = new CompletionSuggester
                        {
                            Contexts = new Dictionary<string, IList<ISuggestContextQuery>>
                            {
                                {
                                    "color",
                                    new List<ISuggestContextQuery>
                                        { new SuggestContextQuery { Context = Project.First.Suggest.Contexts.Values.SelectMany(v => v).First() } }
                                }
                            },
                            Fuzzy = new SuggestFuzziness
                            {
                                Fuzziness = Fuzziness.Auto,
                                MinLength = 1,
                                PrefixLength = 2,
                                Transpositions = true,
                                UnicodeAware = false
                            },
                            Analyzer = "simple",
                            Field = Field<Project>(p => p.Suggest),
                            Size = 8,
                            SkipDuplicates = true
                        }
                    }
                },
                {
                    "my-phrase-suggest", new SuggestBucket
                    {
                        Text = "hello world",
                        Phrase = new PhraseSuggester
                        {
                            Collate = new PhraseSuggestCollate
                            {
                                Query = new PhraseSuggestCollateQuery
                                {
                                    Source = "{ \"match\": { \"{{field_name}}\": \"{{suggestion}}\" }}",
                                },
                                Params = new Dictionary<string, object>
                                {
                                    { "field_name", "title" }
                                },
                                Prune = true
                            },
                            Confidence = 10.1,
                            DirectGenerator = new List<DirectGenerator>
                            {
                                new DirectGenerator { Field = "description" }
                            },
                            GramSize = 1,
                            Field = "name",
                            RealWordErrorLikelihood = 0.5,
                            TokenLimit = 5,
                            ForceUnigrams = false
                        }
                    }
                },
            }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        var myCompletionSuggest = response.Suggest["my-completion-suggest"];

        myCompletionSuggest.Should().NotBeNull();
        var suggest = myCompletionSuggest.First();
        suggest.Text.Should().Be(Project.Instance.Name);
        suggest.Length.Should().BeGreaterThan(0);
        var option = suggest.Options.First();
        option.Text.Should().NotBeNullOrEmpty();
        option.Index.Should().Be("project");
        option.Id.Should().NotBeNull();
        option.Source.Should().NotBeNull();
        option.Source.Name.Should().NotBeNullOrWhiteSpace();
        option.Source.ShouldAdhereToSourceSerializerWhenSet();
        option.Score.Should().BeGreaterThan(0);
        option.Fields.Should().NotBeNull().And.NotBeEmpty();
        option.Fields.Should().ContainKey("state");
        option.Contexts.Should().NotBeNull().And.NotBeEmpty();
        option.Contexts.Should().ContainKey("color");
        var colorContexts = option.Contexts["color"];
        colorContexts.Should().NotBeNull().And.HaveCount(1);
        colorContexts.First().Category.Should().Be(Project.First.Suggest.Contexts.Values.SelectMany(v => v).First());
    }
}
