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
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.NodesUsage;

public class NodesUsageApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, NodesUsageResponse, INodesUsageRequest, NodesUsageDescriptor, NodesUsageRequest>
{
    public NodesUsageApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;
    protected override string UrlPath => "/_nodes/usage";

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        var searchResponse = client.Search<Project>(s => s
            .Size(0)
            .Aggregations(a => a
                .Average("avg_commits", avg => avg
                    .Field(f => f.NumberOfCommits)
                )
            )
        );

        if (!searchResponse.IsValid)
            throw new Exception($"Exception when setting up {nameof(NodesUsageApiTests)}: {searchResponse.DebugInformation}");
    }

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Nodes.Usage(),
        (client, f) => client.Nodes.UsageAsync(),
        (client, r) => client.Nodes.Usage(r),
        (client, r) => client.Nodes.UsageAsync(r)
    );

    protected override void ExpectResponse(NodesUsageResponse response)
    {
        response.ClusterName.Should().NotBeEmpty();

        response.NodeStatistics.Should().NotBeNull();
        response.NodeStatistics.Total.Should().Be(1);
        response.NodeStatistics.Successful.Should().Be(1);
        response.NodeStatistics.Failed.Should().Be(0);

        response.Nodes.Should().NotBeNull();
        response.Nodes.Should().HaveCount(1);

        var firstNode = response.Nodes.First();
        firstNode.Value.Timestamp.Should().BeBefore(DateTimeOffset.UtcNow);
        firstNode.Value.Since.Should().BeBefore(DateTimeOffset.UtcNow);
        firstNode.Value.RestActions.Should().NotBeNull();
        firstNode.Value.Aggregations.Should().NotBeNull().And.ContainKey("avg");
    }
}
