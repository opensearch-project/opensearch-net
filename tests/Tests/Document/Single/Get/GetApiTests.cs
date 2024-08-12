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
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Document.Single.Get;

public class GetApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, GetResponse<Project>, IGetRequest, GetDescriptor<Project>, GetRequest<Project>>
{
    public GetApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override Func<GetDescriptor<Project>, IGetRequest> Fluent => g => g.Routing(ProjectId);
    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override GetRequest<Project> Initializer => new GetRequest<Project>(ProjectId)
    {
        Routing = ProjectId
    };

    protected string ProjectId => Project.First.Name;

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/project/_doc/{U(ProjectId)}?routing={U(ProjectId)}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Get(ProjectId, f),
        (client, f) => client.GetAsync(ProjectId, f),
        (client, r) => client.Get<Project>(r),
        (client, r) => client.GetAsync<Project>(r)
    );

    protected override GetDescriptor<Project> NewDescriptor() => new GetDescriptor<Project>(ProjectId);

    protected override void ExpectResponse(GetResponse<Project> response)
    {
        response.Source.Should().NotBeNull();
        response.Source.Name.Should().Be(ProjectId);
        response.SequenceNumber.Should().HaveValue();
        response.PrimaryTerm.Should().HaveValue();
        response.Source.ShouldAdhereToSourceSerializerWhenSet();
        response.SequenceNumber.Should().BeGreaterOrEqualTo(0);
        response.PrimaryTerm.Should().BeGreaterOrEqualTo(1);
    }
}

public class GetNonExistentDocumentApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, GetResponse<Project>, IGetRequest, GetDescriptor<Project>, GetRequest<Project>>
{
    public GetNonExistentDocumentApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;
    protected override int ExpectStatusCode => 404;
    protected override Func<GetDescriptor<Project>, IGetRequest> Fluent => g => g.Routing(ProjectId);
    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override GetRequest<Project> Initializer => new GetRequest<Project>(ProjectId)
    {
        Routing = ProjectId
    };

    protected string ProjectId => CallIsolatedValue;

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/project/_doc/{U(ProjectId)}?routing={U(ProjectId)}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Get(ProjectId, f),
        (client, f) => client.GetAsync(ProjectId, f),
        (client, r) => client.Get<Project>(r),
        (client, r) => client.GetAsync<Project>(r)
    );

    protected override GetDescriptor<Project> NewDescriptor() => new GetDescriptor<Project>(ProjectId);

    protected override void ExpectResponse(GetResponse<Project> response)
    {
        response.Found.Should().BeFalse();
        response.Index.Should().Be("project");
        if (Cluster.ClusterConfiguration.Version < "2.0.0")
            response.Type.Should().Be("_doc");
        response.Id.Should().Be(CallIsolatedValue);
    }
}

public class GetNonExistentIndexDocumentApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, GetResponse<Project>, IGetRequest, GetDescriptor<Project>, GetRequest<Project>>
{
    public GetNonExistentIndexDocumentApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected string BadIndex => CallIsolatedValue + "-index";

    protected override bool ExpectIsValid => false;
    protected override int ExpectStatusCode => 404;

    protected override Func<GetDescriptor<Project>, IGetRequest> Fluent => (g) => g.Index(BadIndex);
    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override GetRequest<Project> Initializer => new GetRequest<Project>(BadIndex, ProjectId);
    protected string ProjectId => CallIsolatedValue;

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/{BadIndex}/_doc/{U(ProjectId)}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Get(ProjectId, f),
        (client, f) => client.GetAsync(ProjectId, f),
        (client, r) => client.Get<Project>(r),
        (client, r) => client.GetAsync<Project>(r)
    );

    protected override GetDescriptor<Project> NewDescriptor() =>
        new GetDescriptor<Project>(index: BadIndex, id: ProjectId);

    protected override void ExpectResponse(GetResponse<Project> response)
    {
        response.Found.Should().BeFalse();
        response.Index.Should().BeNullOrWhiteSpace();
        response.ServerError.Should().NotBeNull();
    }
}

public class GetApiParentTests
    : ApiIntegrationTestBase<ReadOnlyCluster, GetResponse<CommitActivity>, IGetRequest, GetDescriptor<CommitActivity>, GetRequest<CommitActivity>
    >
{
    public GetApiParentTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected CommitActivity CommitActivity => CommitActivity.CommitActivities.First();

    protected string CommitActivityId => CommitActivity.Id;

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;

    protected override Func<GetDescriptor<CommitActivity>, IGetRequest> Fluent => g => g
        .Routing(CommitActivity.ProjectName);

    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override GetRequest<CommitActivity> Initializer => new GetRequest<CommitActivity>(CommitActivityId)
    {
        Routing = CommitActivity.ProjectName
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/project/_doc/{U(CommitActivityId)}?routing={U(CommitActivity.ProjectName)}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Get(CommitActivityId, f),
        (client, f) => client.GetAsync(CommitActivityId, f),
        (client, r) => client.Get<CommitActivity>(r),
        (client, r) => client.GetAsync<CommitActivity>(r)
    );

    protected override GetDescriptor<CommitActivity> NewDescriptor() => new GetDescriptor<CommitActivity>(CommitActivity);

    protected override void ExpectResponse(GetResponse<CommitActivity> response)
    {
        response.Source.Should().NotBeNull();
        response.Source.Id.Should().Be(CommitActivityId);
        response.Routing.Should().NotBeNullOrEmpty();
    }
}

public class GetApiFieldsTests : GetApiTests
{
    public GetApiFieldsTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override Func<GetDescriptor<Project>, IGetRequest> Fluent => g => g
        .Routing(ProjectId)
        .StoredFields(
            p => p.Name,
            p => p.NumberOfCommits
        );

    protected override GetRequest<Project> Initializer => new GetRequest<Project>(ProjectId)
    {
        Routing = ProjectId,
        StoredFields = Infer.Fields<Project>(p => p.Name, p => p.NumberOfCommits)
    };

    protected override string UrlPath => $"/project/_doc/{U(ProjectId)}?stored_fields=name%2CnumberOfCommits&routing={U(ProjectId)}";

    protected override void ExpectResponse(GetResponse<Project> response)
    {
        response.Fields.Should().NotBeNull();
        response.Fields.ValueOf<Project, string>(p => p.Name).Should().Be(ProjectId);
        response.Fields.ValueOf<Project, int?>(p => p.NumberOfCommits).Should().BeGreaterThan(0);
    }

    protected override GetDescriptor<Project> NewDescriptor() => new GetDescriptor<Project>(ProjectId);
}
