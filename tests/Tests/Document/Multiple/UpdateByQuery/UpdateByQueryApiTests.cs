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
using static OpenSearch.Client.Infer;

namespace Tests.Document.Multiple.UpdateByQuery;

public class UpdateByQueryApiTests
    : ApiIntegrationTestBase<IntrusiveOperationCluster, UpdateByQueryResponse, IUpdateByQueryRequest,
        UpdateByQueryDescriptor<UpdateByQueryApiTests.Test>, UpdateByQueryRequest>
{
    public UpdateByQueryApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson { get; } = new { query = new { match_all = new { } } };
    protected override int ExpectStatusCode => 200;

    protected override Func<UpdateByQueryDescriptor<Test>, IUpdateByQueryRequest> Fluent => d => d
        .Index(CallIsolatedValue)
        .Query(q => q.MatchAll())
        .Refresh()
        .Conflicts(Conflicts.Proceed);

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override UpdateByQueryRequest Initializer => new UpdateByQueryRequest(CallIsolatedValue)
    {
        Query = new MatchAllQuery(),
        Refresh = true,
        Conflicts = Conflicts.Proceed
    };

    protected override bool SupportsDeserialization => false;

    protected override string UrlPath => $"/{CallIsolatedValue}/_update_by_query?refresh=true&conflicts=proceed";

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var index in values.Values)
        {
            Client.Indices.Create(index, c => c
                .Map<Test>(map => map
                    .Dynamic(false)
                    .Properties(props => props
                        .Text(s => s.Name(p => p.Text))
                    )
                )
            );
            Client.Index(new Test { Text = "words words", Flag = "bar" }, i => i.Index(index).Refresh(Refresh.True));
            Client.Index(new Test { Text = "words words", Flag = "foo" }, i => i.Index(index).Refresh(Refresh.True));
            Client.Map<Test>(m => m
                .Index(index)
                .Properties(props => props
                    .Text(s => s.Name(p => p.Text))
                    .Keyword(s => s.Name(p => p.Flag))
                )
            );

            var searchResults = SearchFlags(index);
            searchResults.Total.Should().Be(0);
        }
    }

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.UpdateByQuery(f),
        (client, f) => client.UpdateByQueryAsync(f),
        (client, r) => client.UpdateByQuery(r),
        (client, r) => client.UpdateByQueryAsync(r)
    );

    protected override void OnAfterCall(IOpenSearchClient client) => client.Indices.Refresh(CallIsolatedValue);

    protected override UpdateByQueryDescriptor<Test> NewDescriptor() => new UpdateByQueryDescriptor<Test>(CallIsolatedValue);

    private ISearchResponse<Test> SearchFlags(string index) =>
        Client.Search<Test>(s => s
            .Index(index)
            .Query(q => q.Match(m => m.Field(p => p.Flag).Query("foo")))
        );

    protected override void ExpectResponse(UpdateByQueryResponse response)
    {
        response.Task.Should().BeNull();
        response.Took.Should().BeGreaterThan(0);
        response.Total.Should().Be(2);
        response.Updated.Should().Be(2);
        response.Batches.Should().Be(1);

        var searchResponse = SearchFlags(CallIsolatedValue);
        searchResponse.Total.Should().Be(1);
    }

    public class Test
    {
        public string Flag { get; set; }
        public string Text { get; set; }
    }
}

public class UpdateByQueryWaitForCompletionApiTests : UpdateByQueryApiTests
{
    public UpdateByQueryWaitForCompletionApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override Func<UpdateByQueryDescriptor<Test>, IUpdateByQueryRequest> Fluent => d => d
        .Query(q => q.MatchAll())
        .Index(CallIsolatedValue)
        .WaitForCompletion(false)
        .Conflicts(Conflicts.Proceed);

    protected override UpdateByQueryRequest Initializer => new UpdateByQueryRequest(CallIsolatedValue)
    {
        Query = new MatchAllQuery(),
        WaitForCompletion = false,
        Conflicts = Conflicts.Proceed
    };

    protected override string UrlPath => $"/{CallIsolatedValue}/_update_by_query?wait_for_completion=false&conflicts=proceed";

    protected override void ExpectResponse(UpdateByQueryResponse response)
    {
        response.Task.Should().NotBeNull();
        response.Task.TaskNumber.Should().BeGreaterThan(0);
        response.Task.NodeId.Should().NotBeNullOrWhiteSpace();
        response.Task.FullyQualifiedId.Should().NotBeNullOrWhiteSpace();
    }
}

public class UpdateByQueryWithFailuresApiTests : UpdateByQueryApiTests
{
    private static readonly string _script = "ctx._source.text = 'x'";

    public UpdateByQueryWithFailuresApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;

    protected override object ExpectJson { get; } =
        new
        {
            query = new { match = new { flag = new { query = "bar" } } },
            script = new { source = "ctx._source.text = 'x'" }
        };

    protected override int ExpectStatusCode => 409;

    protected override Func<UpdateByQueryDescriptor<Test>, IUpdateByQueryRequest> Fluent => d => d
        .Index(CallIsolatedValue)
        .Query(q => q.Match(m => m.Field(p => p.Flag).Query("bar")))
        .Script(ss => ss.Source(_script));

    protected override UpdateByQueryRequest Initializer => new UpdateByQueryRequest(CallIsolatedValue)
    {
        Query = new MatchQuery
        {
            Field = Field<Test>(p => p.Flag),
            Query = "bar"
        },
        Script = new InlineScript(_script),
    };

    protected override string UrlPath => $"/{CallIsolatedValue}/_update_by_query";

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var index in values.Values)
        {
            Client.Indices.Create(index, c => c
                .Settings(s => s
                    .RefreshInterval(-1)
                )
            );
            Client.Index(new Test { Text = "test1", Flag = "bar" }, i => i.Index(index).Id(1).Refresh(Refresh.True));
            Client.Index(new Test { Text = "test2", Flag = "bar" }, i => i.Index(index).Id(1));
        }
    }

    protected override void ExpectResponse(UpdateByQueryResponse response)
    {
        response.VersionConflicts.Should().Be(1);
        response.Failures.Should().NotBeEmpty();
        var failure = response.Failures.First();

        failure.Index.Should().NotBeNullOrWhiteSpace();
        failure.Status.Should().Be(409);
        failure.Id.Should().NotBeNullOrWhiteSpace();

        failure.Cause.Should().NotBeNull();
        failure.Cause.IndexUUID.Should().NotBeNullOrWhiteSpace();
        failure.Cause.Reason.Should().NotBeNullOrWhiteSpace();
        failure.Cause.Index.Should().NotBeNullOrWhiteSpace();
        failure.Cause.Shard.Should().NotBeNull();
        failure.Cause.Type.Should().NotBeNullOrWhiteSpace();
    }
}
