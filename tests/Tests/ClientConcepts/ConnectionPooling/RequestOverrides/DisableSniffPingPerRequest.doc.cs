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
using OpenSearch.Net;
using OpenSearch.Net.VirtualizedCluster;
using OpenSearch.Net.VirtualizedCluster.Audit;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;
using static OpenSearch.Net.AuditEvent;

namespace Tests.ClientConcepts.ConnectionPooling.RequestOverrides;

public class DisableSniffPingPerRequest
{
    /**[[disable-sniff-ping-per-request]]
		 * === Disable sniffing and pinging per request
		*
		* Even if you are using a sniffing connection pool thats set up to sniff on start/failure
		* and pinging enabled, you can opt out of this behaviour on a _per request_ basis.
		*
		* In our first test we set up a cluster that pings and sniffs on startup
		* but we disable the sniffing on our first request so we only see the ping and the response
		*/
    [U]
    public async Task DisableSniff()
    {
        /** Let's set up the cluster and configure clients to **always** sniff on startup */
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .ClientCalls(r => r.SucceedAlways())
            .Sniff(c => c.SucceedAlways())
            .Ping(c => c.SucceedAlways())
            .SniffingConnectionPool()
            .Settings(s => s.SniffOnStartup()) // <1> sniff on startup
        );

        /** Now We disable sniffing on the request so even though it's our first call,
			 * we do not want to sniff on startup.
			 *
			 * Instead, the sniff on startup is deferred to the second call into the cluster that
			 * does not disable sniffing on a per request basis.
			 *
			 * And after that no sniff on startup will happen again
			 */
        audit = await audit.TraceCalls(
            new ClientCall(r => r.DisableSniffing()) // <1> disable sniffing
				{
                { PingSuccess, 9200 }, // <2> first call is a successful ping
					{ HealthyResponse, 9200 }
            },
            new ClientCall()
            {
                { SniffOnStartup }, // <3> sniff on startup call happens here, on the second call
					{ SniffSuccess, 9200 },
                { PingSuccess, 9200 },
                { HealthyResponse, 9200 }
            },
            new ClientCall()
            {
                { PingSuccess, 9201 }, // <4> No sniff on startup again
					{ HealthyResponse, 9201 }
            }
        );
    }

    /** Now, let's disable pinging on the request */
    [U]
    public async Task DisablePing()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .ClientCalls(r => r.SucceedAlways())
            .Sniff(c => c.SucceedAlways())
            .SniffingConnectionPool()
            .Settings(s => s.SniffOnStartup())
        );

        audit = await audit.TraceCall(
            new ClientCall(r => r.DisablePing()) // <1> disable ping
				{
                { SniffOnStartup },
                { SniffSuccess, 9200 }, // <2> No ping after sniffing
					{ HealthyResponse, 9200 }
            }
        );
    }

    /** Finally, let's demonstrate disabling both sniff and ping on the request */
    [U]
    public async Task DisableSniffAndPing()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .ClientCalls(r => r.SucceedAlways())
            .SniffingConnectionPool()
            .Settings(s => s.SniffOnStartup())
        );

        audit = await audit.TraceCall(
            new ClientCall(r => r.DisableSniffing().DisablePing()) // <1> disable ping and sniff
				{
                { HealthyResponse, 9200 } // <2> no ping or sniff before the call
				}
        );
    }
}
