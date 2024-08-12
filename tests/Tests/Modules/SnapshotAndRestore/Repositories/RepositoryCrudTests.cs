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

using System.IO;
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Modules.SnapshotAndRestore.Repositories;

public class RepositoryCrudTests
    : CrudTestBase<IntrusiveOperationCluster, CreateRepositoryResponse, GetRepositoryResponse, CreateRepositoryResponse,
        DeleteRepositoryResponse>
{
    private readonly string _rootRepositoryPath;

    public RepositoryCrudTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) =>
        _rootRepositoryPath = cluster.FileSystem.RepositoryPath;

    protected override LazyResponses Create() =>
        Calls<CreateRepositoryDescriptor, CreateRepositoryRequest, ICreateRepositoryRequest, CreateRepositoryResponse>(
            CreateInitializer,
            CreateFluent,
            (s, c, f) => c.Snapshot.CreateRepository(s, f),
            (s, c, f) => c.Snapshot.CreateRepositoryAsync(s, f),
            (s, c, r) => c.Snapshot.CreateRepository(r),
            (s, c, r) => c.Snapshot.CreateRepositoryAsync(r)
        );

    private string GetRepositoryPath(string name) => Path.Combine(_rootRepositoryPath, name);

    protected CreateRepositoryRequest CreateInitializer(string name) =>
        new CreateRepositoryRequest(name)
        {
            Repository = new FileSystemRepository(
                new FileSystemRepositorySettings(GetRepositoryPath(name))
                {
                    ChunkSize = "64mb",
                    Compress = true
                }
            )
        };

    protected ICreateRepositoryRequest CreateFluent(string name, CreateRepositoryDescriptor d) => d
        .FileSystem(fs => fs
            .Settings(GetRepositoryPath(name), s => s
                .ChunkSize("64mb")
                .Compress()
            )
        );

    protected override LazyResponses Read() =>
        Calls<GetRepositoryDescriptor, GetRepositoryRequest, IGetRepositoryRequest, GetRepositoryResponse>(
            ReadInitializer,
            ReadFluent,
            (s, c, f) => c.Snapshot.GetRepository(f),
            (s, c, f) => c.Snapshot.GetRepositoryAsync(f),
            (s, c, r) => c.Snapshot.GetRepository(r),
            (s, c, r) => c.Snapshot.GetRepositoryAsync(r)
        );

    protected GetRepositoryRequest ReadInitializer(string name) => new GetRepositoryRequest(name);

    protected IGetRepositoryRequest ReadFluent(string name, GetRepositoryDescriptor d) => d
        .RepositoryName(name);

    protected override LazyResponses Update() =>
        Calls<CreateRepositoryDescriptor, CreateRepositoryRequest, ICreateRepositoryRequest, CreateRepositoryResponse>(
            UpdateInitializer,
            UpdateFluent,
            (s, c, f) => c.Snapshot.CreateRepository(s, f),
            (s, c, f) => c.Snapshot.CreateRepositoryAsync(s, f),
            (s, c, r) => c.Snapshot.CreateRepository(r),
            (s, c, r) => c.Snapshot.CreateRepositoryAsync(r)
        );

    protected CreateRepositoryRequest UpdateInitializer(string name) => new CreateRepositoryRequest(name)
    {
        Repository = new FileSystemRepository(new FileSystemRepositorySettings(GetRepositoryPath(name))
        {
            ChunkSize = "64mb",
            Compress = true,
            ConcurrentStreams = 5
        }
        )
    };

    protected ICreateRepositoryRequest UpdateFluent(string name, CreateRepositoryDescriptor d) => d
        .FileSystem(fs => fs
            .Settings(GetRepositoryPath(name), s => s
                .ChunkSize("64mb")
                .Compress()
                .ConcurrentStreams(5)
            )
        );

    protected override LazyResponses Delete() =>
        Calls<DeleteRepositoryDescriptor, DeleteRepositoryRequest, IDeleteRepositoryRequest, DeleteRepositoryResponse>(
            DeleteInitializer,
            DeleteFluent,
            (s, c, f) => c.Snapshot.DeleteRepository(s),
            (s, c, f) => c.Snapshot.DeleteRepositoryAsync(s),
            (s, c, r) => c.Snapshot.DeleteRepository(r),
            (s, c, r) => c.Snapshot.DeleteRepositoryAsync(r)
        );

    protected DeleteRepositoryRequest DeleteInitializer(string name) => new DeleteRepositoryRequest(name);

    protected IDeleteRepositoryRequest DeleteFluent(string name, DeleteRepositoryDescriptor d) => null;

    protected override void ExpectAfterCreate(GetRepositoryResponse response)
    {
        response.Repositories.Should().NotBeNull().And.HaveCount(1);
        var name = response.Repositories.Keys.First();
        var repository = response.FileSystem(name);
        repository.Should().NotBeNull();
        repository.Type.Should().Be("fs");
        repository.Settings.Should().NotBeNull();
        repository.Settings.ChunkSize.Should().Be("64mb");
        repository.Settings.Compress.Should().BeTrue();
    }

    protected override void ExpectAfterUpdate(GetRepositoryResponse response)
    {
        response.Repositories.Should().NotBeNull().And.HaveCount(1);
        var name = response.Repositories.Keys.First();
        var repository = response.FileSystem(name);
        repository.Should().NotBeNull();
        repository.Type.Should().Be("fs");
        repository.Settings.Should().NotBeNull();
        repository.Settings.ChunkSize.Should().Be("64mb");
        repository.Settings.Compress.Should().BeTrue();
        repository.Settings.ConcurrentStreams.Should().Be(5);
    }

    protected override void ExpectDeleteNotFoundResponse(DeleteRepositoryResponse response)
    {
        response.ServerError.Should().NotBeNull();
        response.ServerError.Status.Should().Be(404);
        response.ServerError.Error.Reason.Should().Contain("missing");
    }
}
