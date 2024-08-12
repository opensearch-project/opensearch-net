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

namespace Tests.ClientConcepts.ConnectionPooling.MaxRetries;

public class RespectsMaxRetry
{
    /**[[retries]]
		*=== Retries
		* By default, OSC will retry a request as many times as there are nodes in the cluster, that the client knows about.
		*
		* Retries still respects the request timeout however, meaning if you have a 100 node cluster
		* and a request timeout of 20 seconds, the client will retry as many times as it can before
		* giving up at the request timeout of 20 seconds.
		*/
    [U]
    public async Task DefaultMaxIsNumberOfNodes()
    {
        /**
			 * Retry behaviour can be demonstrated using OSC's Virtual cluster test framework. In the following
			 * example, a ten node cluster is defined that always fails on all client calls, except on port 9209
			 */
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .ClientCalls(r => r.FailAlways())
            .ClientCalls(r => r.OnPort(9209).SucceedAlways())
            .StaticConnectionPool()
            .Settings(s => s.DisablePing())
        );

        /**
			 * The trace of a client call shows that a bad response is received from nodes 9200 to 9208,
			 * finally returning a healthy response from the node on port 9209
			 */
        audit = await audit.TraceCall(
            new ClientCall {
                { BadResponse, 9200 },
                { BadResponse, 9201 },
                { BadResponse, 9202 },
                { BadResponse, 9203 },
                { BadResponse, 9204 },
                { BadResponse, 9205 },
                { BadResponse, 9206 },
                { BadResponse, 9207 },
                { BadResponse, 9208 },
                { HealthyResponse, 9209 }
            }
        );
    }

    /**==== Maximum number of retries
		 *
		* When you have a 100 node cluster for example, you might want to ensure that retries occur only
		* a _fixed_ number of times. This can be done using `MaximumRetries(n)` on `ConnectionSettings`
		*
		* IMPORTANT: the actual number of requests is `initial attempt + set number of retries`
		*/

    [U]
    public async Task FixedMaximumNumberOfRetries()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .ClientCalls(r => r.FailAlways())
            .ClientCalls(r => r.OnPort(9209).SucceedAlways())
            .StaticConnectionPool()
            .Settings(s => s.DisablePing().MaximumRetries(3)) // <1> Set the maximum number of retries to 3
        );

        audit = await audit.TraceCall(
            new ClientCall {
                { BadResponse, 9200 },
                { BadResponse, 9201 },
                { BadResponse, 9202 },
                { BadResponse, 9203 },
                { MaxRetriesReached } // <2> The client call trace returns an `MaxRetriesReached` audit after the initial attempt and the number of retries allowed
				}
        );
    }
    /**
		* In our previous example we simulated very fast failures, but in the real world, a call might take upwards of a second.
		*
		* In this next example, we simulate a particularly heavy search that takes 10 seconds to fail, and set a request timeout of 20 seconds.
		* We see that the request is tried twice and gives up before a third call is attempted, since the call takes 10 seconds and thus can be
		* tried twice (initial call and one retry) _before_ the request timeout.
		*/
    [U]
    public async Task RespectsOveralRequestTimeout()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .ClientCalls(r => r.FailAlways().Takes(TimeSpan.FromSeconds(10)))
            .ClientCalls(r => r.OnPort(9209).SucceedAlways())
            .StaticConnectionPool()
            .Settings(s => s.DisablePing().RequestTimeout(TimeSpan.FromSeconds(20)))
        );

        audit = await audit.TraceCall(
            new ClientCall {
                { BadResponse, 9200 },
                { BadResponse, 9201 },
                { MaxTimeoutReached }
            }
        );
    }

    /**
		 * ==== Maximum retry timeout
		* If you set a smaller request timeout you might not want it to also affect the retry timeout.
		* In cases like this, you can configure the `MaxRetryTimeout` separately.
		* Here we simulate calls taking 3 seconds, a request timeout of 2 seconds and a max retry timeout of 10 seconds.
		* We should see 5 attempts to perform this query, testing that our request timeout cuts the query off short and that
		* our max retry timeout of 10 seconds wins over the configured request timeout
		*/
    [U]
    public async Task RespectsMaxRetryTimeoutOverRequestTimeout()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .ClientCalls(r => r.FailAlways().Takes(TimeSpan.FromSeconds(3)))
            .ClientCalls(r => r.OnPort(9209).FailAlways())
            .StaticConnectionPool()
            .Settings(s => s.DisablePing().RequestTimeout(TimeSpan.FromSeconds(2)).MaxRetryTimeout(TimeSpan.FromSeconds(10)))
        );

        audit = await audit.TraceCall(
            new ClientCall {
                { BadResponse, 9200 },
                { BadResponse, 9201 },
                { BadResponse, 9202 },
                { BadResponse, 9203 },
                { BadResponse, 9204 },
                { MaxTimeoutReached }
            }
        );

    }
    /**
		* If your retry policy expands beyond the number of available nodes, the client **won't** retry the same node twice
		*/
    [U]
    public async Task RetriesAreLimitedByNodesInPool()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(2)
            .ClientCalls(r => r.FailAlways().Takes(TimeSpan.FromSeconds(3)))
            .ClientCalls(r => r.OnPort(9209).SucceedAlways())
            .StaticConnectionPool()
            .Settings(s => s.DisablePing().RequestTimeout(TimeSpan.FromSeconds(2)).MaxRetryTimeout(TimeSpan.FromSeconds(10)))
        );

        audit = await audit.TraceCall(
            new ClientCall {
                { BadResponse, 9200 },
                { BadResponse, 9201 },
                { MaxRetriesReached },
                { FailedOverAllNodes }
            }
        );
    }

    /**
		* This makes setting any retry setting on a single node connection pool a no-op by design!
		* Connection pooling and failover is all about trying to fail sanely whilst still utilizing the available resources and
		* not giving up on the fail fast principle; **It is NOT a mechanism for forcing requests to succeed.**
		*/
    [U]
    public async Task DoesNotRetryOnSingleNodeConnectionPool()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(10)
            .ClientCalls(r => r.FailAlways().Takes(TimeSpan.FromSeconds(3)))
            .ClientCalls(r => r.OnPort(9209).SucceedAlways())
            .SingleNodeConnection()
            .Settings(s => s.DisablePing().MaximumRetries(10))
        );

        audit = await audit.TraceCall(
            new ClientCall {
                { BadResponse, 9200 }
            }
        );
    }
}
