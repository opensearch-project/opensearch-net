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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Modules.SnapshotAndRestore.Restore;

public class RestoreApiTests : ApiTestBase<IntrusiveOperationCluster, RestoreResponse, IRestoreRequest, RestoreDescriptor, RestoreRequest>
{
    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    public RestoreApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage)
    {
        if (!TestClient.Configuration.RunIntegrationTests) return;

        var createRepository = Client.Snapshot.CreateRepository(RepositoryName, r => r
            .FileSystem(fs => fs
                .Settings(Path.Combine(cluster.FileSystem.RepositoryPath, RepositoryName))
            )
        );
        if (!createRepository.IsValid)
            throw new Exception("Setup: failed to create snapshot repository");

        var getSnapshotResponse = Client.Snapshot.Get(RepositoryName, SnapshotName);

        if (!getSnapshotResponse.IsValid && getSnapshotResponse.ApiCall.HttpStatusCode == 404 ||
            !getSnapshotResponse.Snapshots.Any())
        {
            var snapshot = Client.Snapshot.Snapshot(RepositoryName, SnapshotName, s => s
                .WaitForCompletion()
            );

            if (!snapshot.IsValid)
                throw new Exception($"Setup: snapshot failed. {snapshot.OriginalException}. {snapshot.ServerError?.Error}");
        }
    }

    protected override object ExpectJson { get; } = new
    {
        rename_pattern = "osc-(.+)",
        rename_replacement = "osc-restored-$1",
    };

    protected override Func<RestoreDescriptor, IRestoreRequest> Fluent => d => d
        .RenamePattern("osc-(.+)")
        .RenameReplacement("osc-restored-$1");

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override RestoreRequest Initializer => new RestoreRequest(RepositoryName, SnapshotName)
    {
        RenamePattern = "osc-(.+)",
        RenameReplacement = "osc-restored-$1"
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/_snapshot/{RepositoryName}/{SnapshotName}/_restore";

    private static string RepositoryName { get; } = RandomString();
    private static string SnapshotName { get; } = RandomString();

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Snapshot.Restore(RepositoryName, SnapshotName, f),
        (client, f) => client.Snapshot.RestoreAsync(RepositoryName, SnapshotName, f),
        (client, r) => client.Snapshot.Restore(r),
        (client, r) => client.Snapshot.RestoreAsync(r)
    );

    protected override RestoreDescriptor NewDescriptor() => new RestoreDescriptor(RepositoryName, SnapshotName);
}
