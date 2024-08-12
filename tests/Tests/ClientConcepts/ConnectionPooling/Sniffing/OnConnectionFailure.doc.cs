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
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.Net.VirtualizedCluster;
using OpenSearch.Net.VirtualizedCluster.Audit;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;
using Tests.Framework.Extensions;
using static OpenSearch.Net.AuditEvent;
using static OpenSearch.Net.VirtualizedCluster.Rules.TimesHelper;

namespace Tests.ClientConcepts.ConnectionPooling.Sniffing;

public class OnConnectionFailure
{
    /**=== Sniffing on connection failure
		*
		* Sniffing on connection is enabled by default when using a connection pool that allows reseeding.
		* The only connection pool we ship with that allows this is the <<sniffing-connection-pool, Sniffing connection pool>>.
		*
		* This can be very handy to force a refresh of the connection pool's known healthy nodes by asking the OpenSearch cluster itself, and
		* a sniff tries to get the nodes by asking each node it currently knows about, until one responds.
		*/

    [U]
    public async Task DoesASniffAfterConnectionFailure()
    {
        /**
			* Here we seed our connection with 5 known nodes on ports 9200-9204, of which we think
			* 9202, 9203, 9204 are cluster_manager eligible nodes. Our virtualized cluster will throw once when doing
			* a search on 9201. This should cause a sniff to be kicked off.
			*/
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(5)
            .ClusterManagerEligible(9202, 9203, 9204)
            .ClientCalls(r => r.SucceedAlways())
            .ClientCalls(r => r.OnPort(9201).Fails(Once)) // <1> When the call fails on 9201, the following sniff succeeds and returns a new cluster state of healthy nodes. This cluster only has 3 nodes and the known masters are 9200 and 9202. A search on 9201 is setup to still fail once
            .Sniff(p => p.SucceedAlways(VirtualClusterWith
                .Nodes(3)
                .ClusterManagerEligible(9200, 9202)
                .ClientCalls(r => r.OnPort(9201).Fails(Once))
                .ClientCalls(r => r.SucceedAlways())
                .Sniff(s => s.SucceedAlways(VirtualClusterWith // <2> After this second failure on 9201, another sniff will happen which returns a cluster state that no longer fails but looks completely different; It's now three nodes on ports 9210 - 9212, with 9210 and 9212 being cluster_manager eligible.
                    .Nodes(3, 9210)
                    .ClusterManagerEligible(9210, 9212)
                    .ClientCalls(r => r.SucceedAlways())
                    .Sniff(r => r.SucceedAlways())
                ))
            ))
            .SniffingConnectionPool()
            .Settings(s => s.DisablePing().SniffOnStartup(false))
        );

        audit = await audit.TraceCalls(
            /** */
            new ClientCall {
                { HealthyResponse, 9200 },
                { pool =>  pool.Nodes.Count.Should().Be(5) }
            },
            new ClientCall {
                { BadResponse, 9201},
                { SniffOnFail },
                { SniffSuccess, 9202}, // <3> We assert we do a sniff on our first known cluster_manager node 9202 after the failed call on 9201
					{ HealthyResponse, 9200},
                { pool =>  pool.Nodes.Count.Should().Be(3) } // <4> Our pool should now have three nodes
				},
            new ClientCall {
                { BadResponse, 9201},
                { SniffOnFail }, // <5> We assert we do a sniff on the first cluster_manager node in our updated cluster
					{ SniffSuccess, 9200},
                { HealthyResponse, 9210},
                { pool =>  pool.Nodes.Count.Should().Be(3) }
            },
            new ClientCall { { HealthyResponse, 9211 } },
            new ClientCall { { HealthyResponse, 9212 } },
            new ClientCall { { HealthyResponse, 9210 } },
            new ClientCall { { HealthyResponse, 9211 } },
            new ClientCall { { HealthyResponse, 9212 } },
            new ClientCall { { HealthyResponse, 9210 } },
            new ClientCall { { HealthyResponse, 9211 } },
            new ClientCall { { HealthyResponse, 9212 } },
            new ClientCall { { HealthyResponse, 9210 } }
        );
    }

    /**==== Sniffing after ping failure
		 *
		 */
    [U]
    public async Task DoesASniffAfterConnectionFailureOnPing()
    {
        /** Here we set up our cluster exactly the same as the previous setup
			* Only we enable pinging (default is `true`) and make the ping fail
			*/
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(5)
            .ClusterManagerEligible(9202, 9203, 9204)
            .Ping(r => r.OnPort(9201).Fails(Once))
            .Ping(r => r.SucceedAlways())
            .ClientCalls(c => c.SucceedAlways())
            .Sniff(p => p.SucceedAlways(VirtualClusterWith
                .Nodes(3)
                .ClusterManagerEligible(9200, 9202)
                .Ping(r => r.OnPort(9201).Fails(Once))
                .Ping(r => r.SucceedAlways())
                .ClientCalls(c => c.SucceedAlways())
                .Sniff(s => s.SucceedAlways(VirtualClusterWith
                    .Nodes(3, 9210)
                    .ClusterManagerEligible(9210, 9211)
                    .Ping(r => r.SucceedAlways())
                    .Sniff(r => r.SucceedAlways())
                    .ClientCalls(c => c.SucceedAlways())
                ))
            ))
            .SniffingConnectionPool()
            .Settings(s => s.SniffOnStartup(false))
        );

        audit = await audit.TraceCalls(
            new ClientCall {
                { PingSuccess, 9200 },
                { HealthyResponse, 9200 },
                { pool =>  pool.Nodes.Count.Should().Be(5) }
            },
            new ClientCall {
                { PingFailure, 9201},
                { SniffOnFail }, // <1> We assert we do a sniff on our first known cluster_manager node 9202
					{ SniffSuccess, 9202},
                { PingSuccess, 9200},
                { HealthyResponse, 9200},
                { pool =>  pool.Nodes.Count.Should().Be(3) } // <2> Our pool should now have three nodes
				},
            new ClientCall {
                { PingFailure, 9201},
                { SniffOnFail }, // <3> We assert we do a sniff on the first cluster_manager node in our updated cluster
					{ SniffSuccess, 9200},
                { PingSuccess, 9210},
                { HealthyResponse, 9210},
                { pool =>  pool.Nodes.Count.Should().Be(3) }
            },
            new ClientCall {
                { PingSuccess, 9211 },
                { HealthyResponse, 9211 }
            },
            new ClientCall {
                { PingSuccess, 9212 },
                { HealthyResponse, 9212 }
            },
            new ClientCall { { HealthyResponse, 9210 } }, // <4> 9210 was already pinged after the sniff returned the new nodes
            new ClientCall { { HealthyResponse, 9211 } },
            new ClientCall { { HealthyResponse, 9212 } },
            new ClientCall { { HealthyResponse, 9210 } }
        );
    }

    /**==== Client uses publish address
		 *
		 */
    [U]
    public async Task UsesPublishAddress()
    {
        var audit = new Auditor(() => VirtualClusterWith
                .Nodes(2)
                .ClusterManagerEligible(9200)
                .ClientCalls(c => c.SucceedAlways())
                .Ping(r => r.OnPort(9200).Fails(Once))
                .Ping(c => c.SucceedAlways())
                .Sniff(p => p.SucceedAlways(VirtualClusterWith
                    .Nodes(10)
                    .ClusterManagerEligible(9200, 9202, 9201)
                    .PublishAddress("10.0.12.1")
                    .ClientCalls(c => c.SucceedAlways())
                    .Ping(c => c.SucceedAlways())
                ))
                .SniffingConnectionPool()
                .Settings(s => s.SniffOnStartup(false))
        );

        void HostAssert(Audit a, string host, int expectedPort)
        {
            a.Node.Uri.Host.Should().Be(host);
            a.Node.Uri.Port.Should().Be(expectedPort);
        }
        void SniffUrlAssert(Audit a, string host, int expectedPort)
        {
            HostAssert(a, host, expectedPort);
            var sniffUri = new UriBuilder(a.Node.Uri)
            {
                Path = RequestPipeline.SniffPath,
                Query = "flat_settings=true&timeout=2s"
            }.Uri;
            sniffUri.PathEquals(a.Path, nameof(SniffUrlAssert));
        }

        audit = await audit.TraceCalls(
            new ClientCall {
                { PingFailure, a => HostAssert(a, "localhost", 9200)},
                { SniffOnFail },
                { SniffSuccess, a => SniffUrlAssert(a, "localhost", 9200)},
                { PingSuccess, a => HostAssert(a, "10.0.12.1", 9200)},
                { HealthyResponse,  a => HostAssert(a, "10.0.12.1", 9200)},
                { pool =>  pool.Nodes.Count.Should().Be(10) } // <1> Our pool should now have 10 nodes
				}
        );
    }
}
