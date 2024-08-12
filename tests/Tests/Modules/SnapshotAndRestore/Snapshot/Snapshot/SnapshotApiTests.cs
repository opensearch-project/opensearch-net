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
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Modules.SnapshotAndRestore.Snapshot.Snapshot;

public class SnapshotApiTests : ApiTestBase<ReadOnlyCluster, SnapshotResponse, ISnapshotRequest, SnapshotDescriptor, SnapshotRequest>
{
    private static readonly string _repos = "repository1";
    private static readonly string _snapshot = "snapshot1";

    public SnapshotApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson { get; } = new
    {
        indices = "project",
        include_global_state = true
    };

    protected override Func<SnapshotDescriptor, ISnapshotRequest> Fluent => d => d
        .Index<Project>()
        .IncludeGlobalState()
        .WaitForCompletion();

    protected override HttpMethod HttpMethod => HttpMethod.PUT;

    protected override SnapshotRequest Initializer => new SnapshotRequest(_repos, _snapshot)
    {
        Indices = Index<Project>(),
        IncludeGlobalState = true,
        WaitForCompletion = true
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/_snapshot/{_repos}/{_snapshot}?wait_for_completion=true";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Snapshot.Snapshot(_repos, _snapshot, f),
        (client, f) => client.Snapshot.SnapshotAsync(_repos, _snapshot, f),
        (client, r) => client.Snapshot.Snapshot(r),
        (client, r) => client.Snapshot.SnapshotAsync(r)
    );

    protected override SnapshotDescriptor NewDescriptor() => new SnapshotDescriptor(_repos, _snapshot);
}
