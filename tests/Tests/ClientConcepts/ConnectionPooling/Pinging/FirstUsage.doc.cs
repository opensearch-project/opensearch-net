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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.Net.VirtualizedCluster;
using OpenSearch.Net.VirtualizedCluster.Audit;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;
using static OpenSearch.Net.AuditEvent;
using static OpenSearch.Net.VirtualizedCluster.Rules.TimesHelper;

namespace Tests.ClientConcepts.ConnectionPooling.Pinging;

public class FirstUsage
{
    /**=== Ping on first usage
		*
		* Pinging is enabled by default for the <<static-connection-pool, Static>>, <<sniffing-connection-pool, Sniffing>>
		* and <<sticky-connection-pool, Sticky>> connection pools.
		* This means that the first time a node is used or resurrected, a ping is issued a with a small (configurable) timeout,
		* allowing the client to fail and fallover to a healthy node much faster than attempting a request, that may be heavier than a ping.
		*/

    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task PingFailsFallsOverToHealthyNodeWithoutPing()
    {
        /** Here's an example with a cluster with two nodes where the second node fails on ping */
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(2)
            .Ping(p => p.Succeeds(Always))
            .Ping(p => p.OnPort(9201).FailAlways())
            .ClientCalls(c => c.SucceedAlways())
            .StaticConnectionPool()
            .AllDefaults()
        );

        /** When making the calls, the first call goes to 9200 which succeeds,
			* and the 2nd call does a ping on 9201 because it's used for the first time.
			* The ping fails so we wrap over to node 9200 which we've already pinged.
			*
			* Finally we assert that the connectionpool has one node that is marked as dead
			*/
        await audit.TraceCalls(

            new ClientCall {
                { PingSuccess, 9200},
                { HealthyResponse, 9200},
                { pool =>
                {
                    pool.Nodes.Where(n=>!n.IsAlive).Should().HaveCount(0);
                } }
            },
            new ClientCall {
                { PingFailure, 9201},
                { HealthyResponse, 9200},
                { pool =>  pool.Nodes.Where(n=>!n.IsAlive).Should().HaveCount(1) }
            }
        );
    }
    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task PingFailsFallsOverMultipleTimesToHealthyNode()
    {
        /** A cluster with 4 nodes where the second and third pings fail */
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(4)
            .Ping(p => p.SucceedAlways())
            .Ping(p => p.OnPort(9201).FailAlways())
            .Ping(p => p.OnPort(9202).FailAlways())
            .ClientCalls(c => c.SucceedAlways())
            .StaticConnectionPool()
            .AllDefaults()
        );

        await audit.TraceCalls(
            new ClientCall {
                { PingSuccess, 9200}, // <1> The first call goes to 9200, which succeeds
					{ HealthyResponse, 9200},
                { pool =>
                    pool.Nodes.Where(n=>!n.IsAlive).Should().HaveCount(0)
                }
            },
            new ClientCall {
                { PingFailure, 9201}, // <2> The 2nd call does a ping on 9201 because its used for the first time. This fails
					{ PingFailure, 9202}, // <3> So we ping 9202. This _also_ fails
					{ PingSuccess, 9203}, // <4> We then ping 9203 because we haven't used it before and it succeeds
					{ HealthyResponse, 9203},
                { pool =>
                    pool.Nodes.Where(n=>!n.IsAlive).Should().HaveCount(2) // <5> Finally, we assert that the connection pool has two nodes that are marked as dead
					}
            }
        );
    }

    /**
		 * All nodes are pinged on first use, provided they are healthy
		 */
    [U, SuppressMessage("AsyncUsage", "AsyncFixer001:Unnecessary async/await usage", Justification = "Its a test")]
    public async Task AllNodesArePingedOnlyOnFirstUseProvidedTheyAreHealthy()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(4)
            .Ping(p => p.SucceedAlways()) // <1> Pings on nodes always succeed
            .ClientCalls(c => c.SucceedAlways())
            .StaticConnectionPool()
            .AllDefaults()
        );

        await audit.TraceCalls(
            new ClientCall { { PingSuccess, 9200 }, { HealthyResponse, 9200 } }, // <2> A successful ping on each node
            new ClientCall { { PingSuccess, 9201 }, { HealthyResponse, 9201 } },
            new ClientCall { { PingSuccess, 9202 }, { HealthyResponse, 9202 } },
            new ClientCall { { PingSuccess, 9203 }, { HealthyResponse, 9203 } },
            new ClientCall { { HealthyResponse, 9200 } },
            new ClientCall { { HealthyResponse, 9201 } },
            new ClientCall { { HealthyResponse, 9202 } },
            new ClientCall { { HealthyResponse, 9203 } },
            new ClientCall { { HealthyResponse, 9200 } }
        );
    }
}
