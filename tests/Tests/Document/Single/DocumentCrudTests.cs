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

using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Document.Single;

public class DocumentCrudTests
    : CrudTestBase<WritableCluster, IndexResponse, GetResponse<Project>, UpdateResponse<Project>, DeleteResponse, ExistsResponse>
{
    public DocumentCrudTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool SupportsDeletes => true;

    protected override LazyResponses Exists() =>
        Calls<DocumentExistsDescriptor<Project>, DocumentExistsRequest<Project>, IDocumentExistsRequest, ExistsResponse>(
            id => new DocumentExistsRequest<Project>(id) { Routing = Project.Instance.Name },
            (id, d) => d.Routing(Project.Instance.Name),
            (s, c, f) => c.DocumentExists(s, f),
            (s, c, f) => c.DocumentExistsAsync(s, f),
            (s, c, r) => c.DocumentExists(r),
            (s, c, r) => c.DocumentExistsAsync(r)
        );

    protected override LazyResponses Create() => Calls<IndexDescriptor<Project>, IndexRequest<Project>, IIndexRequest<Project>, IndexResponse>(
        id => new IndexRequest<Project>(Project.Instance, id: id) { Routing = Project.Instance.Name },
        (id, d) => d.Id(id).Routing(Project.Instance.Name),
        (s, c, f) => c.Index(Project.Instance, f),
        (s, c, f) => c.IndexAsync(Project.Instance, f),
        (s, c, r) => c.Index(r),
        (s, c, r) => c.IndexAsync(r)
    );

    protected override LazyResponses Read() => Calls<GetDescriptor<Project>, GetRequest<Project>, IGetRequest, GetResponse<Project>>(
        id => new GetRequest<Project>(id) { Routing = Project.Instance.Name },
        (id, d) => d.Routing(Project.Instance.Name),
        (s, c, f) => c.Get(s, f),
        (s, c, f) => c.GetAsync(s, f),
        (s, c, r) => c.Get<Project>(r),
        (s, c, r) => c.GetAsync<Project>(r)
    );

    protected override LazyResponses Update() => Calls<
        UpdateDescriptor<Project, Project>,
        UpdateRequest<Project, Project>,
        IUpdateRequest<Project, Project>,
        UpdateResponse<Project>
    >(
        id => new UpdateRequest<Project, Project>(id)
        {
            Routing = Project.Instance.Name,
            Doc = new Project { Description = id + " updated" }
        },
        (id, d) => d
            .Routing(Project.Instance.Name)
            .Doc(new Project { Description = id + " updated" }),
        (s, c, f) => c.Update<Project, Project>(s, f),
        (s, c, f) => c.UpdateAsync<Project, Project>(s, f),
        (s, c, r) => c.Update(r),
        (s, c, r) => c.UpdateAsync(r)
    );

    protected override LazyResponses Delete() => Calls<DeleteDescriptor<Project>, DeleteRequest<Project>, IDeleteRequest, DeleteResponse>(
        id => new DeleteRequest<Project>(id) { Routing = Project.Instance.Name },
        (id, d) => d.Routing(Project.Instance.Name),
        (s, c, f) => c.Delete(s, f),
        (s, c, f) => c.DeleteAsync(s, f),
        (s, c, r) => c.Delete(r),
        (s, c, r) => c.DeleteAsync(r)
    );

    [I]
    protected async Task DocumentIsUpdated() => await AssertOnGetAfterUpdate(r =>
    {
        r.Source.Should().NotBeNull();
        r.Version.Should().BeGreaterThan(1);
        r.SequenceNumber.Should().BeGreaterOrEqualTo(1);
        r.PrimaryTerm.Should().BeGreaterThan(0);
        r.Source.Description.Should().EndWith("updated");
    });

    [I]
    protected async Task DocumentIsDeleted() => await AssertOnGetAfterDelete(r =>
        r.Found.Should().BeFalse()
    );

    [I]
    protected override async Task GetAfterDeleteIsValid() => await AssertOnGetAfterDelete(r =>
    {
        r.ShouldNotBeValid();
        r.Index.Should().NotBeNullOrEmpty();
        if (Cluster.ClusterConfiguration.Version < "2.0.0")
            r.Type.Should().NotBeNullOrEmpty();
        r.Id.Should().NotBeNullOrEmpty();
    });

    protected override void ExpectDeleteNotFoundResponse(DeleteResponse response)
    {
        response.Index.Should().NotBeNullOrEmpty();
        if (Cluster.ClusterConfiguration.Version < "2.0.0")
            response.Type.Should().NotBeNullOrEmpty();
        response.Id.Should().NotBeNullOrEmpty();
        response.Version.Should().BeGreaterThan(0);
        response.SequenceNumber.Should().BeGreaterThan(0);
        response.Result.Should().Be(Result.NotFound);
    }
}
