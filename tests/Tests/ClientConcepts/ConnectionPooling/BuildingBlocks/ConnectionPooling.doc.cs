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
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Configuration;
using Tests.Framework;

namespace Tests.ClientConcepts.ConnectionPooling.BuildingBlocks;

public class ConnectionPooling
{
    /**[[connection-pooling]]
		 * === Connection pools
		 * Connection pooling is the internal mechanism that takes care of registering what nodes there are in the cluster and which
		 * OSC can use to issue client calls on.
		 *
		 * [IMPORTANT]
		 * --
		 * Despite the name, a connection pool in OSC is **not** like connection pooling that you may be familiar with from
		 * https://msdn.microsoft.com/en-us/library/bb399543(v=vs.110).aspx[interacting with a database using ADO.Net]; for example,
		 * a connection pool in OSC is **not** responsible for managing an underlying pool of TCP connections to OpenSearch,
		 * this is https://blogs.msdn.microsoft.com/adarshk/2005/01/02/understanding-system-net-connection-management-and-servicepointmanager/[handled by the ServicePointManager in Desktop CLR].
		 * --
		 *
		 * So, what is a connection pool in OSC responsible for? It is responsible for managing the nodes in an OpenSearch
		 * cluster to which a connection can be made and there is one instance of an `IConnectionPool` associated with an
		 * instance of `ConnectionSettings`. Since a <<lifetimes,single client and connection settings instance is recommended for the
		 * life of the application>>, the lifetime of a single connection pool instance will also be bound to the lifetime
		 * of the application.
		 *
		 * There are five types of connection pool
		 *
		 * - <<single-node-connection-pool,SingleNodeConnectionPool>>
		 * - <<cloud-connection-pool,CloudConnectionPool>>
		 * - <<static-connection-pool,StaticConnectionPool>>
		 * - <<sniffing-connection-pool,SniffingConnectionPool>>
		 * - <<sticky-connection-pool,StickyConnectionPool>>
		 */

    /**
		* [[single-node-connection-pool]]
		* ==== SingleNodeConnectionPool
		*
		* The simplest of all connection pools and the default if no connection pool is explicitly passed to the `ConnectionSettings` constructor.
		* It takes a single `Uri` and uses that to connect to OpenSearch for all the calls. Single node connection pool doesn't opt in to
		* sniffing or pinging behavior and will never mark nodes dead or alive. The one `Uri` it holds is always ready to go.
		*
		* Single node connection pool is the pool to use if your cluster contains only a single node or you are interacting with
		* your cluster through a single load balancer instance.
		*/
    [U]
    public void SingleNode()
    {
        var uri = new Uri("http://localhost:9201");
        var pool = new SingleNodeConnectionPool(uri);
        var client = new OpenSearchClient(new ConnectionSettings(pool));

        /** This type of pool is hardwired to opt out of reseeding (<<sniffing-behaviour, sniffing>>) as well as <<pinging-behaviour, pinging>> */
        // hide
        {
            pool.Nodes.Should().HaveCount(1);
            var node = pool.Nodes.First();
            node.Uri.Port.Should().Be(9201);
            pool.SupportsReseeding.Should().BeFalse();
            pool.SupportsPinging.Should().BeFalse();
            client.ConnectionSettings.ConnectionPool
                .Should()
                .BeOfType<SingleNodeConnectionPool>();
        }

        /** When you use the low ceremony `OpenSearchClient` constructor that takes a single `Uri`,
			* internally a `SingleNodeConnectionPool` is used
			*/
        client = new OpenSearchClient(uri);

        /** However we encourage you to pass connection settings explicitly.
			*/
        // hide
        client.ConnectionSettings.ConnectionPool
            .Should().BeOfType<SingleNodeConnectionPool>();
    }

    /**
		* [[cloud-connection-pool]]
		* ==== CloudConnectionPool
		*
		* A specialized subclass of `SingleNodeConnectionPool` that accepts a Cloud Id and credentials.
		* When used the client will also pick OpenSearch Cloud optimized defaults for the connection settings.
		 *
		 * A Cloud Id for your cluster can be fetched from your OpenSearch Cloud cluster administration console.
		 *
		 * A Cloud Id should be in the form of `cluster_name:base_64_data` where `base_64_data` are the UUIDs for the services in this cloud instance e.g
		 *
		 * `host_name$opensearch_uuid$opensearchDashboards_uuid$apm_uuid`
		 *
		 * Out of these, only `host_name` and `opensearch_uuid` are always available.
		 *
		*/
    [U]
    public void CloudConnectionPool()
    {
        // hide
        string ToBase64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        // hide
        var hostName = "cloud-endpoint.example";
        // hide
        var opensearchUuid = "3dadf823f05388497ea684236d918a1a";
        // hide
        var services = $"{hostName}${opensearchUuid}$3f26e1609cf54a0f80137a80de560da4";
        // hide
        var cloudId = $"my_cluster:{ToBase64(services)}";

        /**
			 * A cloud connection pool can be created using credentials and a `cloudId`
			 */
        var credentials = new BasicAuthenticationCredentials("username", "password"); // <1> a username and password that can access OpenSearch service on OpenSearch Cloud
        var pool = new CloudConnectionPool(cloudId, credentials); // <2> `cloudId` is a value that can be retrieved from the OpenSearch Cloud web console
        var client = new OpenSearchClient(new ConnectionSettings(pool));

        // hide
        {
            pool.UsingSsl.Should().BeTrue();
            pool.Nodes.Should().HaveCount(1);
            var node = pool.Nodes.First();
            node.Uri.Port.Should().Be(443);
            node.Uri.Host.Should().Be($"{opensearchUuid}.{hostName}");
            node.Uri.Scheme.Should().Be("https");
        }

        /** This type of pool, like its parent the `SingleNodeConnectionPool`, is hardwired to opt out of
			 * reseeding (<<sniffing-behaviour, sniffing>>) as well as <<pinging-behaviour, pinging>>.
			 */
        // hide
        {
            pool.SupportsReseeding.Should().BeFalse();
            pool.SupportsPinging.Should().BeFalse();
        }

        /**
			 * You can also directly create a cloud enabled connection using the `OpenSearchClient`'s constructor
			*/
        client = new OpenSearchClient(cloudId, credentials);

        // hide
        {
            client.ConnectionSettings.ConnectionPool
                .Should()
                .BeOfType<CloudConnectionPool>();
        }

        // hide
        {
            client = new OpenSearchClient(new ConnectionSettings(pool));
            client.ConnectionSettings.ConnectionPool.Should().BeOfType<CloudConnectionPool>();
            client.ConnectionSettings.EnableHttpCompression.Should().BeTrue();
            client.ConnectionSettings.BasicAuthenticationCredentials.Should().NotBeNull();
            client.ConnectionSettings.BasicAuthenticationCredentials.Username.Should().Be("username");
        }

        //hide
        {

            //make sure we can deal with trailing dollar sign separators.
            foreach (var dollars in Enumerable.Range(0, 5).Select(i => new string('$', i)))
            {
                Func<IOpenSearchClient> doesNotThrowWhenEndsWithDollar = () =>
                    new OpenSearchClient($"my_cluster:{ToBase64($"hostname$guid{dollars}")}", credentials);

                var validClient = doesNotThrowWhenEndsWithDollar.Should().NotThrow().Subject;
                validClient.ConnectionSettings.ConnectionPool.Nodes.First().Uri.Should().Be("https://guid.hostname");
            }

            var badCloudIds = new[]
            {
                "",
                "my_cluster",
                "my_cluster:",
                $"my_cluster:{ToBase64("hostname")}",
                $"my_cluster:{ToBase64("hostname$")}"
            };

            foreach (var id in badCloudIds)
            {
                Action create = () => new OpenSearchClient(id, credentials);

                create.Should()
                    .Throw<ArgumentException>()
                    .And.Message.Should()
                    .Contain("should be a string in the form of cluster_name:base_64_data");
            }
        }
    }

    /**[[static-connection-pool]]
		* ==== StaticConnectionPool
		*
		* The static connection pool is great if you have a known small sized cluster and do no want to enable
		* sniffing to find out the cluster topology.
		*/
    [U]
    public void Static()
    {
        /** Given a collection of `Uri` */
        var uris = Enumerable.Range(9200, 5)
            .Select(port => new Uri($"http://localhost:{port}"));

        /** a connection pool can be seeded with this collection */
        var pool = new StaticConnectionPool(uris);
        var client = new OpenSearchClient(new ConnectionSettings(pool));

        /** Or using an enumerable of `Node` */
        var nodes = uris.Select(u => new Node(u));
        pool = new StaticConnectionPool(nodes);
        client = new OpenSearchClient(new ConnectionSettings(pool));

        /** This type of pool is hardwired to opt out of reseeding
			 * (<<sniffing-behaviour, sniffing>>) but supports <<pinging-behaviour, pinging>> when enabled.
			 */
        //hide
        {
            pool.SupportsReseeding.Should().BeFalse();
            pool.SupportsPinging.Should().BeTrue();
            client.ConnectionSettings.ConnectionPool
                .Should().BeOfType<StaticConnectionPool>();
        }
    }

    //hide
    private class SeededRandomConectionPool : StaticConnectionPool
    {
        public SeededRandomConectionPool(IEnumerable<Node> nodes, int seed)
            : base(nodes, randomize: true, randomizeSeed: seed, dateTimeProvider: null)
        { }
    }

    // hide
    [U]
    public void RandomizedInitialNodes()
    {
        IEnumerable<StaticConnectionPool> CreateSeededPools(int nodeCount, int pools)
        {
            var seed = TestConfiguration.Instance.Seed;
            var nodes = Enumerable.Range(1, nodeCount)
                .Select(i => new Node(new Uri($"https://10.0.0.{i}:9200/")))
                .ToList();
            for (var i = 0; i < nodeCount; i++)
                yield return new SeededRandomConectionPool(nodes, seed + i);
        }

        var connectionPools = CreateSeededPools(100, 100).ToList();
        connectionPools.Should().HaveCount(100);
        connectionPools
            .Select(p => p.CreateView().First().Uri.ToString())
            .All(uri => uri == "https://10.0.0.1:9200/")
            .Should()
            .BeFalse();
    }

    /**[[sniffing-connection-pool]]
		* ==== SniffingConnectionPool
		*
		* A pool derived from `StaticConnectionPool`, a sniffing connection pool allows itself to be reseeded at run time.
		* It comes with the very minor overhead of a `ReaderWriterLockSlim` to ensure thread safety.
		*/
    [U]
    public void Sniffing()
    {
        /** Given a collection of `Uri` */
        var uris = Enumerable.Range(9200, 5)
            .Select(port => new Uri($"http://localhost:{port}"));

        /** a connection pool can be seeded using an enumerable of `Uri` */
        var pool = new SniffingConnectionPool(uris);
        var client = new OpenSearchClient(new ConnectionSettings(pool));

        /** Or using an enumerable of `Node`. A major benefit in using nodes is that you can include
			* known node roles when seeding, which OSC can then use to favour particular API requests. For example,
			* sniffing on cluster_manager eligible nodes first, and take cluster_manager only nodes out of rotation for issuing client calls on.
			*/
        var nodes = uris.Select(u => new Node(u));
        pool = new SniffingConnectionPool(nodes);
        client = new OpenSearchClient(new ConnectionSettings(pool));

        /** This type of pool is hardwired to opt in to reseeding (<<sniffing-behaviour, sniffing>>), and <<pinging-behaviour, pinging>> */
        //hide
        {
            pool.SupportsReseeding.Should().BeTrue();
            pool.SupportsPinging.Should().BeTrue();
            client.ConnectionSettings.ConnectionPool
                .Should()
                .BeOfType<SniffingConnectionPool>();
        }
    }

    /**[[sticky-connection-pool]]
		* ==== StickyConnectionPool
		*
		* A type of connection pool that returns the first live node to issue a request against, such that the node is _sticky_ between requests.
		* It uses https://msdn.microsoft.com/en-us/library/system.threading.interlocked(v=vs.110).aspx[`System.Threading.Interlocked`]
		* to keep an _indexer_ to the last live node in a thread safe manner.
		*/
    [U]
    public void Sticky()
    {
        /** Given a collection of `Uri` */
        var uris = Enumerable.Range(9200, 5)
            .Select(port => new Uri($"http://localhost:{port}"));

        /** a connection pool can be seeded using an enumerable of `Uri` */
        var pool = new StickyConnectionPool(uris);
        var client = new OpenSearchClient(new ConnectionSettings(pool));

        /** Or using an enumerable of `Node`, similar to `SniffingConnectionPool`
			*/
        var nodes = uris.Select(u => new Node(u));
        pool = new StickyConnectionPool(nodes);
        client = new OpenSearchClient(new ConnectionSettings(pool));

        /** This type of pool is hardwired to opt out of reseeding (<<sniffing-behaviour, sniffing>>), but does support <<pinging-behaviour, pinging>>. */
        // hide
        {
            pool.SupportsReseeding.Should().BeFalse();
            pool.SupportsPinging.Should().BeTrue();
            client.ConnectionSettings.ConnectionPool
                .Should()
                .BeOfType<StickyConnectionPool>();
        }
    }

    /**[[sticky-sniffing-connection-pool]]
		* ==== Sticky Sniffing Connection Pool
		*
		* A type of connection pool that returns the first live node to issue a request against, such that the node is _sticky_ between requests.
		* This implementation supports sniffing and sorting so that each instance of your application can favour a node. For example,
		* a node in the same rack, based on node attributes.
		*/
    [U]
    public void SniffingSortedSticky()
    {
        /** Given a collection of `Uri` */
        var uris = Enumerable.Range(9200, 5)
            .Select(port => new Uri($"http://localhost:{port}"));

        /** a sniffing sorted sticky pool takes a second parameter, a delegate of `Func<Node, float>`, that takes a Node and returns a weight.
			* Nodes will be sorted in descending order by weight. In the following example, nodes are scored so that client nodes
			* in rack_id `rack_one` score the highest
			*/
        var pool = new StickySniffingConnectionPool(uris, node =>
        {
            var weight = 0f;

            if (node.ClientNode)
                weight += 10;

            if (node.Settings.TryGetValue("node.attr.rack_id", out var rackId) && rackId.ToString() == "rack_one")
                weight += 10;

            return weight;
        });

        var client = new OpenSearchClient(new ConnectionSettings(pool));

        // hide
        {
            pool.SupportsReseeding.Should().BeTrue();
            pool.SupportsPinging.Should().BeTrue();
            client.ConnectionSettings.ConnectionPool
                .Should()
                .BeOfType<StickySniffingConnectionPool>();
        }
    }
}
