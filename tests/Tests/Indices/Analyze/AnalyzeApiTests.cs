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
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.Analyze;

public class AnalyzeApiTests : ApiIntegrationTestBase<ReadOnlyCluster, AnalyzeResponse, IAnalyzeRequest, AnalyzeDescriptor, AnalyzeRequest>
{
    public AnalyzeApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        text = new[] { "hello world", "domination" },
        char_filter = new[] { "html_strip" },
        tokenizer = "keyword",
        filter = new[] { "lowercase", "stop" }
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<AnalyzeDescriptor, IAnalyzeRequest> Fluent => d => d
        .Text("hello world", "domination")
        .CharFilter("html_strip")
        .Tokenizer("keyword")
        .Filter("lowercase", "stop");

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override AnalyzeRequest Initializer => new AnalyzeRequest
    {
        Text = new[] { "hello world", "domination" },
        CharFilter = new[] { "html_strip" },
        Tokenizer = "keyword",
        Filter = new[] { "lowercase", "stop" }
    };

    protected override string UrlPath => $"/_analyze";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.Analyze(f),
        (client, f) => client.Indices.AnalyzeAsync(f),
        (client, r) => client.Indices.Analyze(r),
        (client, r) => client.Indices.AnalyzeAsync(r)
    );
}

public class AnalyzeInlineAnalyzerApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, AnalyzeResponse, IAnalyzeRequest, AnalyzeDescriptor, AnalyzeRequest>
{
    protected const string TextToAnalyze = "F# is <b>THE SUPERIOR</b> language :) :gandalf: ";

    public AnalyzeInlineAnalyzerApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        text = new[] { TextToAnalyze },
        tokenizer = new { max_token_length = 7, type = "standard" },
        char_filter = new object[]
        {
            "html_strip",
            new { type = "mapping", mappings = new[] { "F# => fsharp" } }
        },
        filter = new object[]
        {
            "lowercase",
            new { type = "stop", stopwords = new[] { "_english_", "the" } }
        }
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<AnalyzeDescriptor, IAnalyzeRequest> Fluent => d => d
        .Text(TextToAnalyze)
        .CharFilter(c => c
            .Name("html_strip")
            .Mapping(m => m.Mappings("F# => fsharp"))
        )
        .Filter(t => t
            .Name("lowercase")
            .Stop(s => s.StopWords("_english_", "the"))
        )
        .Tokenizer(t => t.Standard(s => s.MaxTokenLength(7)));

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override AnalyzeRequest Initializer => new AnalyzeRequest
    {
        Text = new[] { TextToAnalyze },
        Tokenizer = new StandardTokenizer { MaxTokenLength = 7 },
        CharFilter = new AnalyzeCharFilters
        {
            "html_strip",
            new MappingCharFilter { Mappings = new[] { "F# => fsharp" } }
        },
        Filter = new AnalyzeTokenFilters
        {
            "lowercase",
            new StopTokenFilter { StopWords = new[] { "_english_", "the" } }
        }
    };

    protected override string UrlPath => $"/_analyze";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.Analyze(f),
        (client, f) => client.Indices.AnalyzeAsync(f),
        (client, r) => client.Indices.Analyze(r),
        (client, r) => client.Indices.AnalyzeAsync(r)
    );

    protected override void ExpectResponse(AnalyzeResponse response)
    {
        //TIL standard chops up words greater than `MaxTokenLength` classic tokenizer drops them
        response.Tokens.Should().HaveCount(6);
        var tokens = response.Tokens.Select(t => t.Token).ToList();
        tokens.Should().Contain("fsharp", "gandalf");
    }
}

public class AnalyzeInlineNormalizerApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, AnalyzeResponse, IAnalyzeRequest, AnalyzeDescriptor, AnalyzeRequest>
{
    private const string TextToAnalyze = "F# is <b>THE SUPERIOR</b> language :) :gandalf: ";

    public AnalyzeInlineNormalizerApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        text = new[] { TextToAnalyze },
        char_filter = new object[]
        {
            new { type = "mapping", mappings = new[] { "F# => fsharp" } }
        },
        filter = new object[]
        {
            "lowercase"
        }
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<AnalyzeDescriptor, IAnalyzeRequest> Fluent => d => d
        .Text(TextToAnalyze)
        .CharFilter(c => c
            .Mapping(m => m.Mappings("F# => fsharp"))
        )
        .Filter(t => t
            .Name("lowercase")
        );

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override AnalyzeRequest Initializer => new AnalyzeRequest
    {
        Text = new[] { TextToAnalyze },
        CharFilter = new AnalyzeCharFilters
        {
            new MappingCharFilter { Mappings = new[] { "F# => fsharp" } }
        },
        Filter = new AnalyzeTokenFilters
        {
            "lowercase"
        }
    };

    protected override string UrlPath => $"/_analyze";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.Analyze(f),
        (client, f) => client.Indices.AnalyzeAsync(f),
        (client, r) => client.Indices.Analyze(r),
        (client, r) => client.Indices.AnalyzeAsync(r)
    );

    protected override void ExpectResponse(AnalyzeResponse response)
    {
        response.Tokens.Should().HaveCount(1);
        var token = response.Tokens.Single().Token;
        token.Should().Be("fsharp is <b>the superior</b> language :) :gandalf: ");
    }
}

public class AnalyzeExplainApiTests : AnalyzeInlineAnalyzerApiTests
{
    public AnalyzeExplainApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        text = new[] { TextToAnalyze },
        tokenizer = new { max_token_length = 7, type = "standard" },
        char_filter = new object[]
        {
            "html_strip",
            new { type = "mapping", mappings = new[] { "F# => fsharp" } }
        },
        filter = new object[]
        {
            "lowercase",
            new { type = "stop", stopwords = new[] { "_english_", "the" } }
        },
        explain = true
    };

    protected override Func<AnalyzeDescriptor, IAnalyzeRequest> Fluent => d => base.Fluent(d.Explain());

    protected override AnalyzeRequest Initializer
    {
        get
        {
            var r = base.Initializer;
            r.Explain = true;
            return r;
        }
    }

    protected override void ExpectResponse(AnalyzeResponse response)
    {
        response.Tokens.Should().HaveCount(0);
        response.Detail.Should().NotBeNull("details should not be null because explain was specified");
        response.Detail.CustomAnalyzer.Should().BeTrue();
        response.Detail.CharFilters.Should().NotBeEmpty();
        foreach (var c in response.Detail.CharFilters)
        {
            c.Name.Should().NotBeNullOrWhiteSpace();
            c.FilteredText.Should().NotBeEmpty();
        }
        response.Detail.Filters.Should().NotBeEmpty();
        foreach (var c in response.Detail.Filters)
            AssertTokenDetail(c);

        response.Detail.Tokenizer.Should().NotBeNull();
        AssertTokenDetail(response.Detail.Tokenizer);
    }

    private static void AssertTokenDetail(TokenDetail c)
    {
        c.Name.Should().NotBeNullOrWhiteSpace();
        foreach (var t in c.Tokens) t.Token.Should().NotBeNullOrWhiteSpace();
    }
}
