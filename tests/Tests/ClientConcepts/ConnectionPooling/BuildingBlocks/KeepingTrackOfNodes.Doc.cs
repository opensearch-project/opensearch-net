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
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;

namespace Tests.ClientConcepts.ConnectionPooling.BuildingBlocks;

public class KeepingTrackOfNodes
{

    /**=== Keeping track of nodes
		 */
    [U]
    public void Creating()
    {
        /** ==== Creating a Node
			* A `Node` can be instantiated by passing it a `Uri`
			*/
        var node = new Node(new Uri("http://localhost:9200"));
        node.Uri.Should().NotBeNull();
        node.Uri.Port.Should().Be(9200);

        /** By default cluster_manager eligible and holds data is presumed to be true **/
        node.ClusterManagerEligible.Should().BeTrue();
        node.HoldsData.Should().BeTrue();

        /** Is resurrected is true on first usage, hints to the transport that a ping might be useful */
        node.IsResurrected.Should().BeTrue();
        /**
			* When instantiating your connection pool you could switch these to false to initialize the client to
			* a known cluster topology.
			*/
    }
    [U]
    public void BuildingPaths()
    {
        /** ==== Building a Node path
			* passing a node with a path should be preserved.
			* Sometimes an OpenSearch node lives behind a proxy
			*/
        var node = new Node(new Uri("http://test.example/opensearch"));

        node.Uri.Port.Should().Be(80);
        node.Uri.AbsolutePath.Should().Be("/opensearch/");

        /** *We force paths to end with a forward slash* so that they can later be safely combined */
        var combinedPath = new Uri(node.Uri, "index/type/_search");
        combinedPath.AbsolutePath.Should().Be("/opensearch/index/type/_search");

        /** which is exactly what the `CreatePath` method does on `Node` */
        combinedPath = node.CreatePath("index/type/_search");
        combinedPath.AbsolutePath.Should().Be("/opensearch/index/type/_search");
    }

    /** ==== Marking Nodes */
    [U]
    public void MarkNodes()
    {
        var node = new Node(new Uri("http://localhost:9200"));
        node.FailedAttempts.Should().Be(0);
        node.IsAlive.Should().BeTrue();
        /**
			* every time a node is marked dead, the number of attempts should increase
			* and the passed datetime should be exposed.
			*/
        for (var i = 0; i < 10; i++)
        {
            var deadUntil = DateTime.Now.AddMinutes(1);
            node.MarkDead(deadUntil);
            node.FailedAttempts.Should().Be(i + 1);
            node.IsAlive.Should().BeFalse();
            node.DeadUntil.Should().Be(deadUntil);
        }
        /** however when marking a node alive, the `DeadUntil` property should be reset and `FailedAttempts` reset to 0*/
        node.MarkAlive();
        node.FailedAttempts.Should().Be(0);
        node.DeadUntil.Should().Be(default(DateTime));
        node.IsAlive.Should().BeTrue();
    }

    [U]
    public void Equality()
    {
        /** ==== Node Equality
			* Nodes are considered equal if they have the same endpoint, no matter what other metadata is associated */
        var node = new Node(new Uri("http://localhost:9200")) { ClusterManagerEligible = false };
        var nodeAsClusterManager = new Node(new Uri("http://localhost:9200")) { ClusterManagerEligible = true };

        (node == nodeAsClusterManager).Should().BeTrue();
        (node != nodeAsClusterManager).Should().BeFalse();

        var uri = new Uri("http://localhost:9200");
        (node == uri).Should().BeTrue();

        var differentUri = new Uri("http://localhost:9201");
        (node != differentUri).Should().BeTrue();

        node.Should().Be(nodeAsClusterManager);
    }
}
