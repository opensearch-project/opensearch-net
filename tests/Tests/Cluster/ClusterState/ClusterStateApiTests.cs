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
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.ClusterState;

public class ClusterStateApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, ClusterStateResponse, IClusterStateRequest, ClusterStateDescriptor, ClusterStateRequest>
{
    public ClusterStateApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;
    protected override string UrlPath => "/_cluster/state";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cluster.State(),
        (client, f) => client.Cluster.StateAsync(),
        (client, r) => client.Cluster.State(r),
        (client, r) => client.Cluster.StateAsync(r)
    );

    protected override void ExpectResponse(ClusterStateResponse response)
    {
        var isPre20version = Cluster.ClusterConfiguration.Version < "2.0.0";

        response.ClusterName.Should().NotBeNullOrWhiteSpace();
        if (isPre20version)
            response.MasterNode.Should().NotBeNullOrWhiteSpace();
        else
            response.ClusterManagerNode.Should().NotBeNullOrWhiteSpace();
        response.StateUUID.Should().NotBeNullOrWhiteSpace();
        response.Version.Should().BeGreaterThan(0);

        var clusterManagerNode =
            isPre20version
                ? response.State["nodes"][response.MasterNode]
                : response.State["nodes"][response.ClusterManagerNode];
        var clusterManagerNodeName = clusterManagerNode["name"].Value as string;
        var transportAddress = clusterManagerNode["transport_address"].Value as string;
        clusterManagerNodeName.Should().NotBeNullOrWhiteSpace();
        transportAddress.Should().NotBeNullOrWhiteSpace();

        var getSyntax = response.Get<string>($"nodes.{(isPre20version ? response.MasterNode : response.ClusterManagerNode)}.transport_address");

        getSyntax.Should().NotBeNullOrWhiteSpace().And.Be(transportAddress);

        var badPath = response.Get<string>($"this.is.not.a.path.into.the.response.structure");
        badPath.Should().BeNull();

        var dict = response.Get<DynamicDictionary>($"nodes");

        dict.Count.Should().BeGreaterThan(0);
        var node = dict[(isPre20version ? response.MasterNode : response.ClusterManagerNode)].ToDictionary();
        node.Should().NotBeNull().And.ContainKey("name");

        object dictDoesNotExist = response.Get<DynamicDictionary>("nodes2");
        dictDoesNotExist.Should().BeNull();


        dynamic r = response.State;

        string lastCommittedConfig = r.metadata.cluster_coordination.last_committed_config[0];

        lastCommittedConfig.Should().NotBeNullOrWhiteSpace();




    }
}
