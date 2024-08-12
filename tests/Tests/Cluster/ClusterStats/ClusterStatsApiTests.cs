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

using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.ClusterStats;

public class ClusterStatsApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, ClusterStatsResponse, IClusterStatsRequest, ClusterStatsDescriptor, ClusterStatsRequest>
{
    public ClusterStatsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;
    protected override string UrlPath => "/_cluster/stats";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cluster.Stats(),
        (client, f) => client.Cluster.StatsAsync(),
        (client, r) => client.Cluster.Stats(r),
        (client, r) => client.Cluster.StatsAsync(r)
    );

    protected override void ExpectResponse(ClusterStatsResponse response)
    {
        response.ClusterName.Should().NotBeNullOrWhiteSpace();

        response.ClusterUUID.Should().NotBeNullOrWhiteSpace();

        response.NodeStatistics.Should().NotBeNull();
        response.Status.Should().NotBe(ClusterStatus.Red);
        response.Timestamp.Should().BeGreaterThan(0);
        Assert(response.Nodes);
        Assert(response.Indices);
    }

    protected void Assert(ClusterNodesStats nodes)
    {
        nodes.Should().NotBeNull();
        nodes.Count.Should().NotBeNull();
        if (Cluster.ClusterConfiguration.Version < "2.0.0")
            nodes.Count.Master.Should().BeGreaterOrEqualTo(1);
        else
            nodes.Count.ClusterManager.Should().BeGreaterOrEqualTo(1);

        nodes.FileSystem.Should().NotBeNull();
        nodes.FileSystem.AvailableInBytes.Should().BeGreaterThan(0);
        nodes.FileSystem.FreeInBytes.Should().BeGreaterThan(0);
        nodes.FileSystem.TotalInBytes.Should().BeGreaterThan(0);

        nodes.Jvm.Should().NotBeNull();
        nodes.Jvm.MaxUptimeInMilliseconds.Should().BeGreaterThan(0);
        nodes.Jvm.Threads.Should().BeGreaterThan(0);
        nodes.Jvm.Memory.Should().NotBeNull();
        nodes.Jvm.Memory.HeapMaxInBytes.Should().BeGreaterThan(0);
        nodes.Jvm.Memory.HeapUsedInBytes.Should().BeGreaterThan(0);

        nodes.Jvm.Versions.Should().NotBeEmpty();
        var version = nodes.Jvm.Versions.First();
        version.Count.Should().BeGreaterThan(0);
        version.Version.Should().NotBeNullOrWhiteSpace();
        version.VmName.Should().NotBeNullOrWhiteSpace();
        version.VmVendor.Should().NotBeNullOrWhiteSpace();
        version.VmVersion.Should().NotBeNullOrWhiteSpace();

        nodes.OperatingSystem.Should().NotBeNull();
        nodes.OperatingSystem.AvailableProcessors.Should().BeGreaterThan(0);
        nodes.OperatingSystem.AllocatedProcessors.Should().BeGreaterThan(0);
        nodes.OperatingSystem.Names.Should().NotBeEmpty();
        nodes.OperatingSystem.Memory.Should().NotBeNull();
        nodes.OperatingSystem.PrettyNames.Should().NotBeNull();

        var plugins = nodes.Plugins;
        plugins.Should().NotBeEmpty();

        var plugin = plugins.First();
        plugin.Name.Should().NotBeNullOrWhiteSpace();
        plugin.Description.Should().NotBeNullOrWhiteSpace();
        plugin.Version.Should().NotBeNullOrWhiteSpace();
        plugin.ClassName.Should().NotBeNullOrWhiteSpace();

        nodes.Process.Should().NotBeNull();
        nodes.Process.Cpu.Should().NotBeNull();
        nodes.Process.OpenFileDescriptors.Should().NotBeNull();
        nodes.Process.OpenFileDescriptors.Max.Should().NotBe(0);
        nodes.Process.OpenFileDescriptors.Min.Should().NotBe(0);

        nodes.Versions.Should().NotBeEmpty();

        nodes.Ingest.Should().NotBeNull();
    }

    protected void Assert(ClusterIndicesStats indices)
    {
        indices.Should().NotBeNull();
        indices.Count.Should().BeGreaterThan(0);

        indices.Documents.Should().NotBeNull();
        indices.Documents.Count.Should().BeGreaterThan(0);

        indices.Completion.Should().NotBeNull();
        indices.Fielddata.Should().NotBeNull();
        indices.QueryCache.Should().NotBeNull();

        indices.Segments.Should().NotBeNull();
        indices.Segments.Count.Should().BeGreaterThan(0);
        if (Cluster.ClusterConfiguration.Version < "2.0.0")
        {
            indices.Segments.DocValuesMemoryInBytes.Should().BeGreaterThan(0);
            indices.Segments.MemoryInBytes.Should().BeGreaterThan(0);
            indices.Segments.NormsMemoryInBytes.Should().BeGreaterThan(0);
            indices.Segments.StoredFieldsMemoryInBytes.Should().BeGreaterThan(0);
            indices.Segments.TermsMemoryInBytes.Should().BeGreaterThan(0);
        }

        indices.Shards.Should().NotBeNull();
        indices.Shards.Primaries.Should().BeGreaterThan(0);
        indices.Shards.Total.Should().BeGreaterThan(0);
        indices.Shards.Index.Primaries.Should().NotBeNull();
        indices.Shards.Index.Primaries.Avg.Should().BeGreaterThan(0);
        indices.Shards.Index.Primaries.Min.Should().BeGreaterThan(0);
        indices.Shards.Index.Primaries.Max.Should().BeGreaterThan(0);
        indices.Shards.Index.Replication.Should().NotBeNull();
        indices.Shards.Index.Shards.Should().NotBeNull();
        indices.Shards.Index.Shards.Avg.Should().BeGreaterThan(0);
        indices.Shards.Index.Shards.Min.Should().BeGreaterThan(0);
        indices.Shards.Index.Shards.Max.Should().BeGreaterThan(0);

        indices.Store.Should().NotBeNull();
        indices.Store.SizeInBytes.Should().BeGreaterThan(0);
    }
}
