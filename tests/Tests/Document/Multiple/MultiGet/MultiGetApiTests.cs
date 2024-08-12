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
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Document.Multiple.MultiGet;

public class MultiGetSimplifiedApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, MultiGetResponse, IMultiGetRequest, MultiGetDescriptor, MultiGetRequest>
{
    private readonly IEnumerable<long> _ids = Developer.Developers.Select(d => d.Id).Take(10);

    public MultiGetSimplifiedApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        ids = _ids
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<MultiGetDescriptor, IMultiGetRequest> Fluent => d => d
        .Index<Developer>()
        .GetMany<Developer>(_ids);

    protected override HttpMethod HttpMethod => HttpMethod.POST;


    protected override MultiGetRequest Initializer => new MultiGetRequest(Index<Developer>())
    {
        Documents = _ids
            .Select(n => new MultiGetOperation<Developer>(n))
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/devs/_mget";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.MultiGet(f),
        (client, f) => client.MultiGetAsync(f),
        (client, r) => client.MultiGet(r),
        (client, r) => client.MultiGetAsync(r)
    );

    protected override void ExpectResponse(MultiGetResponse response)
    {
        response.Hits.Should().NotBeEmpty().And.HaveCount(10);
        foreach (var document in response.Hits)
        {
            document.Index.Should().NotBeNullOrWhiteSpace();
            if (Cluster.ClusterConfiguration.Version < "2.0.0")
                document.Type.Should().NotBeNullOrWhiteSpace();
            document.Id.Should().NotBeNullOrWhiteSpace();
            document.Found.Should().BeTrue();
        }
    }
}

public class MultiGetApiTests : ApiIntegrationTestBase<ReadOnlyCluster, MultiGetResponse, IMultiGetRequest, MultiGetDescriptor, MultiGetRequest>
{
    private readonly IEnumerable<long> _ids = Developer.Developers.Select(d => d.Id).Take(10);

    public MultiGetApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson { get; } = new
    {
        docs = Developer.Developers.Select(p => new { _id = p.Id, routing = p.Id.ToString(), _source = false }).Take(10)
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<MultiGetDescriptor, IMultiGetRequest> Fluent => d => d
        .Index<Developer>()
        .GetMany<Developer>(_ids, (g, i) => g.Routing(i.ToString()).Source(false));

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override MultiGetRequest Initializer => new MultiGetRequest(Index<Developer>())
    {
        Documents = _ids
            .Select(n => new MultiGetOperation<Developer>(n) { Routing = n.ToString(), Source = false })
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/devs/_mget";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.MultiGet(f),
        (client, f) => client.MultiGetAsync(f),
        (client, r) => client.MultiGet(r),
        (client, r) => client.MultiGetAsync(r)
    );

    protected override void ExpectResponse(MultiGetResponse response)
    {
        response.Hits.Should().NotBeEmpty().And.HaveCount(10);
        foreach (var hit in response.Hits)
        {
            hit.Index.Should().NotBeNullOrWhiteSpace();
            if (Cluster.ClusterConfiguration.Version < "2.0.0")
                hit.Type.Should().NotBeNullOrWhiteSpace();
            hit.Id.Should().NotBeNullOrWhiteSpace();
            hit.Found.Should().BeTrue();
        }
        foreach (var document in response.SourceMany<Project>(_ids)) document.ShouldAdhereToSourceSerializerWhenSet();
    }
}


public class MultiGetMetadataApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, MultiGetResponse, IMultiGetRequest, MultiGetDescriptor, MultiGetRequest>
{
    private readonly IEnumerable<string> _ids = Project.Projects.Select(d => d.Name).Take(10);

    public MultiGetMetadataApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        docs = _ids.Select(i => new
        {
            _id = i,
            routing = i
        })
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<MultiGetDescriptor, IMultiGetRequest> Fluent => d => d
        .Index<Project>()
        .GetMany<Project>(_ids, (op, id) => op.Routing(id));

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override MultiGetRequest Initializer => new MultiGetRequest(Index<Project>())
    {
        Documents = _ids.Select(n => new MultiGetOperation<Project>(n) { Routing = n })
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/project/_mget";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.MultiGet(f),
        (client, f) => client.MultiGetAsync(f),
        (client, r) => client.MultiGet(r),
        (client, r) => client.MultiGetAsync(r)
    );

    protected override void ExpectResponse(MultiGetResponse response)
    {
        response.Hits.Should().NotBeEmpty().And.HaveCount(10);

        foreach (var hit in response.GetMany<Project>(_ids))
        {
            hit.Index.Should().NotBeNullOrWhiteSpace();
            if (Cluster.ClusterConfiguration.Version < "2.0.0")
                hit.Type.Should().NotBeNullOrWhiteSpace();
            hit.Id.Should().NotBeNullOrWhiteSpace();
            hit.Found.Should().BeTrue();
            hit.Version.Should().Be(1);
            hit.PrimaryTerm.Should().BeGreaterOrEqualTo(1);
            hit.SequenceNumber.Should().BeGreaterOrEqualTo(0);
            hit.Source.ShouldAdhereToSourceSerializerWhenSet();
        }
    }
}

public class MultiGetParentApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, MultiGetResponse, IMultiGetRequest, MultiGetDescriptor, MultiGetRequest>
{
    private readonly IEnumerable<CommitActivity> _activities = CommitActivity.CommitActivities.Take(10);

    public MultiGetParentApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        docs = _activities.Select(p => new { _id = p.Id, routing = p.ProjectName })
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<MultiGetDescriptor, IMultiGetRequest> Fluent => d => d
        .Index<Project>()
        .GetMany<CommitActivity>(_activities.Select(c => c.Id), (m, id) => m.Routing(_activities.Single(a => a.Id == id).ProjectName));

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override MultiGetRequest Initializer => new MultiGetRequest(Index<Project>())
    {
        Documents = _activities.Select(n => new MultiGetOperation<CommitActivity>(n.Id) { Routing = n.ProjectName })
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/project/_mget";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.MultiGet(f),
        (client, f) => client.MultiGetAsync(f),
        (client, r) => client.MultiGet(r),
        (client, r) => client.MultiGetAsync(r)
    );

    protected override void ExpectResponse(MultiGetResponse response)
    {
        response.Hits.Should().NotBeEmpty().And.HaveCount(10);

        foreach (var hit in response.GetMany<CommitActivity>(_activities.Select(c => c.Id)))
        {
            hit.Index.Should().NotBeNullOrWhiteSpace();
            if (Cluster.ClusterConfiguration.Version < "2.0.0")
                hit.Type.Should().NotBeNullOrWhiteSpace();
            hit.Id.Should().NotBeNullOrWhiteSpace();
            hit.Found.Should().BeTrue();
            hit.Version.Should().Be(1);
            hit.Routing.Should().NotBeNullOrEmpty();
        }
    }
}
