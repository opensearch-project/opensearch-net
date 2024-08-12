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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Document.Single.Delete;

public class DeleteApiTests
    : ApiIntegrationTestBase<WritableCluster, DeleteResponse, IDeleteRequest, DeleteDescriptor<Project>, DeleteRequest<Project>>
{
    public DeleteApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.DELETE;

    protected override Func<DeleteDescriptor<Project>, IDeleteRequest> Fluent => d => d.Routing(CallIsolatedValue);
    protected override DeleteRequest<Project> Initializer => new DeleteRequest<Project>(CallIsolatedValue) { Routing = CallIsolatedValue };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/project/_doc/{CallIsolatedValue}?routing={U(CallIsolatedValue)}";

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var id in values.Values)
            Client.Index(Project.Instance, i => i.Id(id).Routing(id));
    }

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Delete(CallIsolatedValue, f),
        (client, f) => client.DeleteAsync(CallIsolatedValue, f),
        (client, r) => client.Delete(r),
        (client, r) => client.DeleteAsync(r)
    );

    protected override DeleteDescriptor<Project> NewDescriptor() => new DeleteDescriptor<Project>(CallIsolatedValue);

    protected override void ExpectResponse(DeleteResponse response)
    {
        response.ShouldBeValid();
        response.Result.Should().Be(Result.Deleted);
        response.Shards.Should().NotBeNull();
        response.Shards.Total.Should().BeGreaterOrEqualTo(1);
        response.Shards.Successful.Should().BeGreaterOrEqualTo(1);
        response.PrimaryTerm.Should().BeGreaterThan(0);
        response.SequenceNumber.Should().BeGreaterThan(0);
    }
}

public class DeleteNonExistentDocumentApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, DeleteResponse, IDeleteRequest,
        DeleteDescriptor<Project>, DeleteRequest<Project>>
{
    public DeleteNonExistentDocumentApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;
    protected override int ExpectStatusCode => 404;
    protected override HttpMethod HttpMethod => HttpMethod.DELETE;

    protected override Func<DeleteDescriptor<Project>, IDeleteRequest> Fluent => d => d.Routing(CallIsolatedValue);
    protected override DeleteRequest<Project> Initializer => new DeleteRequest<Project>(CallIsolatedValue) { Routing = CallIsolatedValue };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/project/_doc/{CallIsolatedValue}?routing={U(CallIsolatedValue)}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Delete(CallIsolatedValue, f),
        (client, f) => client.DeleteAsync(CallIsolatedValue, f),
        (client, r) => client.Delete(r),
        (client, r) => client.DeleteAsync(r)
    );

    protected override DeleteDescriptor<Project> NewDescriptor() => new DeleteDescriptor<Project>(CallIsolatedValue);

    protected override void ExpectResponse(DeleteResponse response)
    {
        response.ShouldNotBeValid();
        response.Result.Should().Be(Result.NotFound);
        response.Index.Should().Be("project");
        if (Cluster.ClusterConfiguration.Version < "2.0.0")
            response.Type.Should().Be("_doc");
        response.Id.Should().Be(CallIsolatedValue);
        response.Shards.Total.Should().BeGreaterOrEqualTo(1);
        response.Shards.Successful.Should().BeGreaterOrEqualTo(1);
        response.PrimaryTerm.Should().BeGreaterThan(0);
        response.SequenceNumber.Should().BeGreaterThan(0);
    }
}

public class DeleteNonExistentIndexDocumentApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, DeleteResponse, IDeleteRequest, DeleteDescriptor<Project>, DeleteRequest<Project>>
{
    public DeleteNonExistentIndexDocumentApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;
    protected override int ExpectStatusCode => 404;

    protected override HttpMethod HttpMethod => HttpMethod.DELETE;

    protected override Func<DeleteDescriptor<Project>, IDeleteRequest> Fluent => d => d.Index(BadIndex).Routing(CallIsolatedValue);
    protected override DeleteRequest<Project> Initializer => new DeleteRequest<Project>(BadIndex, CallIsolatedValue) { Routing = CallIsolatedValue };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/{BadIndex}/_doc/{CallIsolatedValue}?routing={U(CallIsolatedValue)}";

    private string BadIndex => CallIsolatedValue + "-bad-index";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Delete(CallIsolatedValue, f),
        (client, f) => client.DeleteAsync(CallIsolatedValue, f),
        (client, r) => client.Delete(r),
        (client, r) => client.DeleteAsync(r)
    );

    protected override DeleteDescriptor<Project> NewDescriptor() =>
        new DeleteDescriptor<Project>(index: CallIsolatedValue, id: CallIsolatedValue);

    protected override void ExpectResponse(DeleteResponse response)
    {
        response.ShouldNotBeValid();
        response.Result.Should().Be(Result.Error);
        response.ServerError.Should().NotBeNull();
        response.ServerError.Error.Reason.Should().StartWith("no such index");
    }
}
