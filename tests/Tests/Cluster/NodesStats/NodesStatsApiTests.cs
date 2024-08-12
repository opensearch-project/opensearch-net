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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Core.ManagedOpenSearch.NodeSeeders;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Cluster.NodesStats;

//TODO: re-evaluate which numerics are safe to assert greater then 0 and add better error messages so we can see which numeric assertion fails on CI.
public class NodesStatsApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, NodesStatsResponse, INodesStatsRequest, NodesStatsDescriptor, NodesStatsRequest>
{
    public NodesStatsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;
    protected override string UrlPath => "/_nodes/stats";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Nodes.Stats(),
        (client, f) => client.Nodes.StatsAsync(),
        (client, r) => client.Nodes.Stats(r),
        (client, r) => client.Nodes.StatsAsync(r)
    );

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        // performing a bunch of search actions to make sure some stats have been gathered
        // even if this api is tested in isolation
        for (var i = 0; i < 5; i++)
        {
            var searchResult = client.MultiSearch(AllIndices, m => m
                .Search<Project>(s => s.MatchAll().Size(0))
                .Search<Project>(s => s.MatchAll().Size(0))
                .Search<Project>(s => s.MatchAll().Size(0))
                .Search<Project>(s => s.MatchAll().Size(0))
            );
            searchResult.ShouldBeValid();
        }
    }

    protected override void ExpectResponse(NodesStatsResponse response)
    {
        response.ClusterName.Should().NotBeNullOrWhiteSpace();
        response.NodeStatistics.Should().NotBeNull();
        response.Nodes.Should().NotBeEmpty().And.HaveCount(1);
        var kv = response.Nodes.First();
        kv.Key.Should().NotBeNullOrWhiteSpace();
        var node = kv.Value;
        Assert(node);
        Assert(node.OperatingSystem);
        Assert(node.Process);
        Assert(node.Transport);
        Assert(node.Script);
        Assert(node.Http);
        Assert(node.Breakers);
        Assert(node.FileSystem);
        Assert(node.ThreadPool);
        Assert(node.Jvm);
        Assert(node.AdaptiveSelection);
        Assert(node.Ingest);
        Assert(node.ScriptCache);
        Assert(node.IndexingPressure);
    }

    protected void Assert(NodeIngestStats nodeIngestStats)
    {
        nodeIngestStats.Should().NotBeNull();
        nodeIngestStats.Total.Should().NotBeNull();

        nodeIngestStats.Pipelines.Should().NotBeNull();
        nodeIngestStats.Pipelines.Should().ContainKey(DefaultSeeder.PipelineName);

        var pipelineStats = nodeIngestStats.Pipelines[DefaultSeeder.PipelineName];

        pipelineStats.Should().NotBeNull();
        pipelineStats.Processors.Should().NotBeNull().And.HaveCount(1);

        var processorStats = pipelineStats.Processors.First();

        processorStats.Type.Should().Be("set");
        processorStats.Statistics.Should().NotBeNull();
    }

    protected void Assert(IReadOnlyDictionary<string, AdaptiveSelectionStats> adaptiveSelectionStats) =>
        adaptiveSelectionStats.Should().NotBeNull();

    protected void Assert(NodeStats node)
    {
        node.Name.Should().NotBeNullOrWhiteSpace();
        //node.Timestamp.Should().BeGreaterThan(0);
        node.TransportAddress.Should().NotBeNullOrWhiteSpace();
        node.Host.Should().NotBeNullOrWhiteSpace();
        node.Ip.Should().NotBeEmpty();
        node.Roles.Should().NotBeNullOrEmpty();
    }

    protected void Assert(OperatingSystemStats os)
    {
        os.Should().NotBeNull();

        //os.Timestamp.Should().BeGreaterThan(0);
        //os.LoadAverage.Should().NotBe(0);
        //os.CpuPercent.Should().NotBe(0);

        os.Memory.Should().NotBeNull();
        //os.Memory.TotalInBytes.Should().BeGreaterThan(0);
        //os.Memory.FreeInBytes.Should().BeGreaterThan(0);
        //os.Memory.UsedInBytes.Should().BeGreaterThan(0);
        //os.Memory.FreePercent.Should().BeGreaterThan(0);
        //os.Memory.UsedPercent.Should().BeGreaterThan(0);

        os.Swap.Should().NotBeNull();
        //os.Swap.TotalInBytes.Should().BeGreaterThan(0);
        //os.Swap.FreeInBytes.Should().BeGreaterThan(0);
        //os.Swap.UsedInBytes.Should().BeGreaterThan(0);
    }

    protected void Assert(ProcessStats process)
    {
        process.Should().NotBeNull();

        //process.Timestamp.Should().BeGreaterThan(0);
        //process.OpenFileDescriptors.Should().NotBe(0);

        process.CPU.Should().NotBeNull();
        //process.CPU.TotalInMilliseconds.Should().BeGreaterThan(0);
        process.Memory.Should().NotBeNull();
        //process.Memory.TotalVirtualInBytes.Should().BeGreaterThan(0);
    }

    protected void Assert(ScriptStats script) => script.Should().NotBeNull();

    protected void Assert(ScriptCacheStats scriptCache) => scriptCache?.Sum.Should().NotBeNull();

    protected void Assert(TransportStats transport) => transport.Should().NotBeNull();

    protected void Assert(HttpStats http) => http.Should().NotBeNull();

    protected void Assert(IReadOnlyDictionary<string, BreakerStats> breakers)
    {
        breakers.Should().NotBeEmpty().And.ContainKey("request");
        // ReSharper disable once UnusedVariable
        var requestBreaker = breakers["request"];
        //requestBreaker.LimitSizeInBytes.Should().BeGreaterThan(0);
        //requestBreaker.Overhead.Should().BeGreaterThan(0);
    }

    protected void Assert(FileSystemStats fileSystem)
    {
        fileSystem.Should().NotBeNull();
        //fileSystem.Timestamp.Should().BeGreaterThan(0);
        fileSystem.Total.Should().NotBeNull();
        //fileSystem.Total.AvailableInBytes.Should().BeGreaterThan(0);
        //fileSystem.Total.FreeInBytes.Should().BeGreaterThan(0);
        //fileSystem.Total.TotalInBytes.Should().BeGreaterThan(0);

        fileSystem.Data.Should().NotBeEmpty();
        var path = fileSystem.Data.First();
        //path.AvailableInBytes.Should().BeGreaterThan(0);
        //path.FreeInBytes.Should().BeGreaterThan(0);
        //path.TotalInBytes.Should().BeGreaterThan(0);
        path.Mount.Should().NotBeNullOrWhiteSpace();
        path.Path.Should().NotBeNullOrWhiteSpace();
        path.Type.Should().NotBeNullOrWhiteSpace();
    }

    protected void Assert(IReadOnlyDictionary<string, ThreadCountStats> threadPools)
    {
        threadPools.Should().NotBeEmpty().And.ContainKey("management");
        // ReSharper disable once UnusedVariable
        var threadPool = threadPools["management"];
        //threadPool.Completed.Should().BeGreaterThan(0);
    }

    protected void Assert(NodeJvmStats jvm)
    {
        jvm.Should().NotBeNull();

        //jvm.Timestamp.Should().BeGreaterThan(0);
        //jvm.UptimeInMilliseconds.Should().BeGreaterThan(0);

        jvm.BufferPools.Should().NotBeEmpty().And.ContainKey("direct");
        var bufferPool = jvm.BufferPools["direct"];
        //bufferPool.Count.Should().BeGreaterThan(0);
        //bufferPool.TotalCapacityInBytes.Should().BeGreaterThan(0);
        //bufferPool.UsedInBytes.Should().BeGreaterThan(0);

        jvm.Classes.Should().NotBeNull();
        //jvm.Classes.CurrentLoadedCount.Should().BeGreaterThan(0);
        //jvm.Classes.TotalLoadedCount.Should().BeGreaterThan(0);
        //jvm.Classes.TotalUnloadedCount.Should().BeGreaterOrEqualTo(0);

        jvm.GarbageCollection.Should().NotBeNull();
        jvm.GarbageCollection.Collectors.Should().NotBeEmpty().And.ContainKey("young");
        var youngGc = jvm.GarbageCollection.Collectors["young"];
        youngGc.Should().NotBeNull();
        //youngGc.CollectionCount.Should().BeGreaterThan(0);
        //youngGc.CollectionTimeInMilliseconds.Should().BeGreaterThan(0);

        jvm.Memory.Should().NotBeNull();
        //jvm.Memory.HeapCommittedInBytes.Should().BeGreaterThan(0);
        //jvm.Memory.HeapMaxInBytes.Should().BeGreaterThan(0);
        //jvm.Memory.HeapUsedInBytes.Should().BeGreaterThan(0);
        //jvm.Memory.HeapUsedPercent.Should().BeGreaterThan(0);
        //jvm.Memory.NonHeapCommittedInBytes.Should().BeGreaterThan(0);
        //jvm.Memory.NonHeapUsedInBytes.Should().BeGreaterThan(0);

        jvm.Memory.Pools.Should().NotBeEmpty().And.ContainKey("young");
        var youngMemoryPool = jvm.Memory.Pools["young"];
        youngMemoryPool.Should().NotBeNull();
        //youngMemoryPool.MaxInBytes.Should().BeGreaterThan(0);
        //youngMemoryPool.PeakMaxInBytes.Should().BeGreaterThan(0);
        //youngMemoryPool.PeakUsedInBytes.Should().BeGreaterThan(0);

        jvm.Threads.Should().NotBeNull();
        //jvm.Threads.Count.Should().BeGreaterThan(0);
        //jvm.Threads.PeakCount.Should().BeGreaterThan(0);
    }

    protected void Assert(IndexingPressureStats indexingPressureStats)
    {
        indexingPressureStats.Memory.LimitInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Limit.Should().NotBeNull();

        indexingPressureStats.Memory.Current.CombinedCoordinatingAndPrimaryInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Current.CombinedCoordinatingAndPrimary.Should().NotBeNull();
        indexingPressureStats.Memory.Current.CoordinatingInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Current.Coordinating.Should().NotBeNull();
        indexingPressureStats.Memory.Current.PrimaryInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Current.Primary.Should().NotBeNull();
        indexingPressureStats.Memory.Current.ReplicaInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Current.Replica.Should().NotBeNull();
        indexingPressureStats.Memory.Current.AllInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Current.All.Should().NotBeNull();

        indexingPressureStats.Memory.Total.CombinedCoordinatingAndPrimaryInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Total.CombinedCoordinatingAndPrimary.Should().NotBeNull();
        indexingPressureStats.Memory.Total.CoordinatingInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Total.Coordinating.Should().NotBeNull();
        indexingPressureStats.Memory.Total.PrimaryInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Total.Primary.Should().NotBeNull();
        indexingPressureStats.Memory.Total.ReplicaInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Total.Replica.Should().NotBeNull();
        indexingPressureStats.Memory.Total.AllInBytes.Should().BeGreaterOrEqualTo(0);
        //indexingPressureStats.Memory.Total.All.Should().NotBeNull();
        indexingPressureStats.Memory.Total.CoordinatingRejections.Should().BeGreaterOrEqualTo(0);
        indexingPressureStats.Memory.Total.PrimaryRejections.Should().BeGreaterOrEqualTo(0);
        indexingPressureStats.Memory.Total.ReplicaRejections.Should().BeGreaterOrEqualTo(0);
    }
}
