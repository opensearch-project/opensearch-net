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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.VirtualizedCluster;
using OpenSearch.Net.VirtualizedCluster.Audit;
using OpenSearch.OpenSearch.Xunit.Sdk;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client.Settings;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework;
using static OpenSearch.Net.AuditEvent;
using static OpenSearch.Net.VirtualizedCluster.Rules.TimesHelper;

namespace Tests.ClientConcepts.ConnectionPooling.Sniffing;

public class RoleDetection
{
    /**=== Sniffing role detection
		*
		* When we sniff the cluster state, we detect the role of each node, for example,
		* whether it's cluster_manager eligible, a node that holds data, etc.
		* We can then use this information when selecting a node to perform an API call on.
		*/
    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task DetectsClusterManagerNodes()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .Sniff(s => s.Fails(Always))
            .Sniff(s => s.OnPort(9202)
                .Succeeds(Always, VirtualClusterWith.Nodes(8).ClusterManagerEligible(9200, 9201, 9202))
            )
            .SniffingConnectionPool()
            .AllDefaults()
        )
        {
            AssertPoolBeforeStartup = (pool) =>
            {
                pool.Should().NotBeNull();
                pool.Nodes.Should().HaveCount(10);
                pool.Nodes.Where(n => n.ClusterManagerEligible).Should().HaveCount(10);
            },
            AssertPoolAfterStartup = (pool) =>
            {
                pool.Should().NotBeNull();
                pool.Nodes.Should().HaveCount(8);
                pool.Nodes.Where(n => n.ClusterManagerEligible).Should().HaveCount(3);
            }
        };

        await audit.TraceStartup();
    }

    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task DetectsDataNodes()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .Sniff(s => s.Fails(Always))
            .Sniff(s => s.OnPort(9202)
                .Succeeds(Always, VirtualClusterWith.Nodes(8).StoresNoData(9200, 9201, 9202))
            )
            .SniffingConnectionPool()
            .AllDefaults()
        )
        {
            AssertPoolBeforeStartup = (pool) =>
            {
                pool.Should().NotBeNull();
                pool.Nodes.Should().HaveCount(10);
                pool.Nodes.Where(n => n.HoldsData).Should().HaveCount(10);
            },

            AssertPoolAfterStartup = (pool) =>
            {
                pool.Should().NotBeNull();
                pool.Nodes.Should().HaveCount(8);
                pool.Nodes.Where(n => n.HoldsData).Should().HaveCount(5);
            }
        };
        await audit.TraceStartup();
    }

    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task SkipsNodesThatDisableHttp()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .Sniff(s => s.SucceedAlways()
                .Succeeds(Always, VirtualClusterWith.Nodes(8).StoresNoData(9200, 9201, 9202).HttpDisabled(9201))
            )
            .SniffingConnectionPool()
            .AllDefaults()
        )
        {
            AssertPoolBeforeStartup = (pool) =>
            {
                pool.Should().NotBeNull();
                pool.Nodes.Should().HaveCount(10);
                pool.Nodes.Where(n => n.HoldsData).Should().HaveCount(10);
                pool.Nodes.Where(n => n.HttpEnabled).Should().HaveCount(10);
                pool.Nodes.Should().OnlyContain(n => n.Uri.Host == "localhost");
            },

            AssertPoolAfterStartup = (pool) =>
            {
                pool.Should().NotBeNull();
                pool.Nodes.Should().HaveCount(7, "we filtered the node that has no http enabled");
                pool.Nodes.Should().NotContain(n => n.Uri.Port == 9201);
                pool.Nodes.Where(n => n.HoldsData).Should().HaveCount(5);
            }
        };
        await audit.TraceStartup();
    }

    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task SkipClusterManagerOnlyNodes()
    {
        /**
			 * In this example, We create a Virtual cluster with a Sniffing connection pool that seeds all the known cluster_manager nodes.
			 * When the client sniffs on startup, we see that the cluster is 20 nodes in total, with the cluster_manager eligible nodes
			 * storing no data.
			 */
        var clusterManagerNodes = new[] { 9200, 9201, 9202 };
        var totalNodesInTheCluster = 20;
        //
        var audit = new Auditor(() => VirtualClusterWith
            .ClusterManagerOnlyNodes(clusterManagerNodes.Length)
            .ClientCalls(r => r.SucceedAlways())
            .Sniff(s => s.SucceedAlways()
                .Succeeds(Always, VirtualClusterWith
                    .Nodes(totalNodesInTheCluster)
                    .StoresNoData(clusterManagerNodes)
                    .ClusterManagerEligible(clusterManagerNodes)
                    .ClientCalls(r => r.SucceedAlways())
                )
            )
            .SniffingConnectionPool()
            .Settings(s => s.DisablePing())
        )
        {
            AssertPoolBeforeStartup = pool => // <1> Before the sniff, assert we only see three cluster_manager only nodes
            {
                pool.Should().NotBeNull();
                pool.Nodes.Should().HaveCount(3, "we seeded 3 cluster_manager only nodes at the start of the application");
                pool.Nodes.Where(n => n.HoldsData).Should().HaveCount(0, "none of which hold data");
            },
            AssertPoolAfterStartup = (pool) => // <2> After the sniff, assert we now know about the existence of 20 nodes.
            {
                pool.Should().NotBeNull();
                var nodes = pool.CreateView().ToList();
                nodes.Count().Should().Be(20, "cluster_manager nodes are included in the registration of nodes since we still favor sniffing on them");
            }
        };

        /** After the sniff has happened on 9200 before the first API call, assert that the subsequent API
			 * call hits the first non cluster_manager eligible node
			 */
        audit = await audit.TraceStartup(new ClientCall
        {
            { SniffSuccess, 9200},
            { HealthyResponse, 9203} // <1> Healthy response from 9203, not a cluster_manager eligible node
			});

        /**
			 * To verify that the client behaves as we expect when making requests to the virtual cluster, make 1000 different
			 * client calls and assert that each is not sent to any of the known cluster_manager only nodes
			 */
        var seenNodes = new HashSet<int>();
        foreach (var _ in Enumerable.Range(0, 1000))
        {
            audit = await audit.TraceCalls(
                new ClientCall {{HealthyResponse, (a) =>
                {
                    var port = a.Node.Uri.Port;
                    clusterManagerNodes.Should().NotContain(port);
                    seenNodes.Add(port);
                }}}
            );
        }

        seenNodes.Should().HaveCount(totalNodesInTheCluster - clusterManagerNodes.Length); // <1> `seenNodes` is a hash set of all the ports we hit. assert that this is equal to `known total nodes - known cluster_manager only nodes`
    }

    /**
		* ==== Node predicates
		*
		* A predicate can be specified on `ConnectionSettings` that can be used to determine which nodes in the cluster API calls
		* can be executed on.
		*
		* As an example, Let's create a Virtual cluster with a Sniffing connection pool that seeds all 20 nodes to begin. When the client
		* sniffs on startup, we see the cluster is still 20 nodes in total, however we are now aware of the
		* actual configured settings for the nodes from the cluster response.
		*/
    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task RespectsCustomPredicate()
    {
        var totalNodesInTheCluster = 20;
        var setting = "node.attr.rack_id";
        var value = "rack_one";
        var nodesInRackOne = new[] { 9204, 9210, 9213 };

        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(totalNodesInTheCluster)
            .ClientCalls(r => r.SucceedAlways())
            .Sniff(s => s.SucceedAlways()
                .Succeeds(Always, VirtualClusterWith
                    .Nodes(totalNodesInTheCluster)
                    .HasSetting(setting, value, nodesInRackOne)
                    .ClientCalls(r => r.SucceedAlways())
                )
            )
            .SniffingConnectionPool()
            .Settings(s => s
                .DisablePing() // <1> for testing simplicity, disable pings
                .NodePredicate(node => // <2> We only want to execute API calls to nodes in rack_one
                    node.Settings.ContainsKey(setting) &&
                    node.Settings[setting].ToString() == value
                )
            )
        )
        {
            AssertPoolAfterStartup = pool => // <3> After sniffing on startup, assert that the pool of nodes that the client will execute API calls against only contains the three nodes that are in `rack_one`
            {
                pool.Should().NotBeNull();
                var nodes = pool.CreateView().ToList();
                nodes.Count(n => n.Settings.ContainsKey(setting)).Should().Be(3, "only three nodes are in rack_one");
            }
        };

        /**
			 * With the cluster set up, assert that the sniff happens on 9200 before the first API call
			 * and that API call hits the first node in `rack_one`
			 */
        audit = await audit.TraceStartup(new ClientCall
        {
            { SniffSuccess, 9200},
            { HealthyResponse, 9204}
        });

        /**
			 * To prove that the client is working as expected, do a 1000 different client calls and
			 * assert that each is sent to a node in `rack_one` only,
			 * respecting the node predicate on connection settings
			 */
        var seenNodes = new HashSet<int>();
        foreach (var _ in Enumerable.Range(0, 1000))
        {
            audit = await audit.TraceCalls(
                new ClientCall {{HealthyResponse, (a) =>
                {
                    var port = a.Node.Uri.Port;
                    nodesInRackOne.Should().Contain(port);
                    seenNodes.Add(port);
                }}}
            );
        }

        seenNodes.Should().HaveCount(nodesInRackOne.Length);
    }

    /**
		 * As another example of node predicates, let's set up a Virtual cluster with a _bad_ node predicate, i.e.
		 * predicate that filters out *all* nodes from being the targets of API calls from the client
		 *
		 *
		 */
    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task CustomPredicateYieldingNothingThrows()
    {
        var totalNodesInTheCluster = 20;

        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(totalNodesInTheCluster)
            .Sniff(s => s.SucceedAlways()
                .Succeeds(Always, VirtualClusterWith.Nodes(totalNodesInTheCluster))
            )
            .SniffingConnectionPool()
            .Settings(s => s
                .DisablePing()
                .NodePredicate(node => false) // <1> A _bad_ predicate that declines *all* nodes
            )
        );

        /**
			 * Now when making the client calls, the audit trail indicates that a sniff on startup succeeds, but the subsequent
			 * API call fails because the node predicate filters out all nodes as targets on which to execute API calls
			 */
        await audit.TraceUnexpectedOpenSearchException(new ClientCall
        {
            { SniffOnStartup }, // <1> The audit trail indicates a sniff for the very first time on startup
				{ SniffSuccess }, // <2> The sniff succeeds because the node predicate is ignored when sniffing
				{ NoNodesAttempted } // <3> when trying to do an actual API call however, the predicate prevents any nodes from being attempted
			}, e =>
        {
            e.FailureReason.Should().Be(PipelineFailure.Unexpected);

            Func<string> debug = () => e.DebugInformation;
            debug.Invoking(s => s.Invoke()).Should().NotThrow();
        });
        /**
			 * An example of the debug information that the client response returns for the previous exception
			 *
			 * ....
			 * # FailureReason: Unrecoverable/Unexpected NoNodesAttempted while attempting POST on default-index/project/_search on an empty node, likely a node predicate on ConnectionSettings not matching ANY nodes
			 *  - [1] SniffOnStartup: Took: 00:00:00
			 *  - [2] SniffSuccess: Node: http://localhost:9200/ Took: 00:00:00
			 *  - [3] NoNodesAttempted: Took: 00:00:00
			 * # Inner Exception: No nodes were attempted, this can happen when a node predicate does not match any nodes
			 * ....
			*/
    }

    // hide
    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task DetectsFqdn()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .Sniff(s => s.SucceedAlways()
                .Succeeds(Always, VirtualClusterWith
                    .Nodes(8)
                    .StoresNoData(9200, 9201, 9202)
                    .SniffShouldReturnFqdn())
            )
            .SniffingConnectionPool()
            .AllDefaults()
        )
        {
            AssertPoolBeforeStartup = pool =>
            {
                pool.Should().NotBeNull();
                pool.Nodes.Should().HaveCount(10);
                pool.Nodes.Where(n => n.HoldsData).Should().HaveCount(10);
                pool.Nodes.Should().OnlyContain(n => n.Uri.Host == "localhost");
            },

            AssertPoolAfterStartup = pool =>
            {
                pool.Should().NotBeNull();
                pool.Nodes.Should().HaveCount(8);
                pool.Nodes.Where(n => n.HoldsData).Should().HaveCount(5);
                pool.Nodes.Should().OnlyContain(n => n.Uri.Host.StartsWith("fqdn") && !n.Uri.Host.Contains("/"));
            }
        };

        await audit.TraceStartup();
    }
}

// hide
public class SniffRoleDetectionCluster : ClientTestClusterBase
{
    public SniffRoleDetectionCluster() : base(new ClientTestClusterConfiguration
    {
        DefaultNodeSettings =
        {
            {"node.data", "false"},
            {"node.cluster_manager", "true"},
            {"node.attr.rack_id", "rack_one"}
        }
    })
    { }
}

//hide
public class RealWorldRoleDetection : IClusterFixture<SniffRoleDetectionCluster>
{
    private readonly SniffRoleDetectionCluster _cluster;
    private IConnectionSettingsValues _settings;

    public RealWorldRoleDetection(SniffRoleDetectionCluster cluster) => _cluster = cluster;

    [I]
    public async Task SniffPicksUpRoles()
    {
        var node = SniffAndReturnNode();
        node.ClusterManagerEligible.Should().BeTrue();
        node.HoldsData.Should().BeFalse();
        node.Settings.Should().NotBeEmpty().And.Contain("node.attr.rack_id", "rack_one");

        node = await SniffAndReturnNodeAsync();
        node.ClusterManagerEligible.Should().BeTrue();
        node.HoldsData.Should().BeFalse();
        node.Settings.Should().NotBeEmpty().And.Contain("node.attr.rack_id", "rack_one");
    }

    private Node SniffAndReturnNode()
    {
        var pipeline = CreatePipeline();
        pipeline.Sniff();
        return AssertSniffResponse();
    }

    private async Task<Node> SniffAndReturnNodeAsync()
    {
        var pipeline = CreatePipeline();
        await pipeline.SniffAsync(default(CancellationToken));
        return AssertSniffResponse();
    }

    private RequestPipeline CreatePipeline()
    {
        var uri = TestConnectionSettings.CreateUri(_cluster.Nodes.First().Port ?? 9200);
        _settings = new ConnectionSettings(new SniffingConnectionPool(new[] { uri }));
        var pipeline = new RequestPipeline(_settings, DateTimeProvider.Default, new RecyclableMemoryStreamFactory(),
            new SearchRequestParameters());
        return pipeline;
    }

    private Node AssertSniffResponse()
    {
        var nodes = _settings.ConnectionPool.Nodes;
        nodes.Should().NotBeEmpty().And.HaveCount(1);
        var node = nodes.First();
        return node;
    }
}
