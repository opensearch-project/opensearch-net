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

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Cluster.RemoteInfo;

public class RemoteInfoApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, RemoteInfoResponse, IRemoteInfoRequest, RemoteInfoDescriptor, RemoteInfoRequest>
{
    public RemoteInfoApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;
    protected override string UrlPath => "/_remote/info";

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        var enableRemoteClusters = client.Cluster.PutSettings(s => s
            .Transient(t => t
                .Add("cluster.remote.cluster_one.seeds", new[] { "127.0.0.1:9300", "127.0.0.1:9301" })
                .Add("cluster.remote.cluster_one.skip_unavailable", true)
                .Add("cluster.remote.cluster_two.seeds", new[] { "127.0.0.1:9300" })
                .Add("cluster.remote.cluster_two.skip_unavailable", true)));
        enableRemoteClusters.ShouldBeValid();

        var remoteSearch = client.Search<Project>(s => s.Index(Index<Project>("cluster_one").And<Project>("cluster_two")));
        remoteSearch.ShouldBeValid();
    }

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cluster.RemoteInfo(),
        (client, f) => client.Cluster.RemoteInfoAsync(),
        (client, r) => client.Cluster.RemoteInfo(r),
        (client, r) => client.Cluster.RemoteInfoAsync(r)
    );

    protected override void ExpectResponse(RemoteInfoResponse response)
    {
        response.Remotes.Should()
            .NotBeEmpty()
            .And.ContainKey("cluster_one")
            .And.ContainKey("cluster_two");

        foreach (var (name, remote) in response.Remotes)
        {
            if (!name.StartsWith("cluster_")) continue;
            remote.Connected.Should().BeTrue();
            remote.Seeds.Should().NotBeNullOrEmpty();
            remote.InitialConnectTimeout.Should().NotBeNull().And.Be("30s");
            remote.MaxConnectionsPerCluster.Should().BeGreaterThan(0, "max_connections_per_cluster");
            remote.NumNodesConnected.Should().BeGreaterThan(0, "num_nodes_connected");
        }
    }
}
