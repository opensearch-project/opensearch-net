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

public class Revival
{
    /**=== Ping on revival
		*
		* When a node is marked dead it will only be __put in the dog house__ for a certain amount of time.
		* Once it __comes out of the dog house__, or revived, a ping is scheduled before an actual API call, to ensure
		* that it's up and running. If it's still down, it's put _back in the dog house_ a little longer.
		*
		* Take a look at the <<request-timeout, Request timeouts>> for an explanation on what each timeout is.
		*/
    [U]
    public async Task PingAfterRevival()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(3)
            .ClientCalls(r => r.SucceedAlways())
            .ClientCalls(r => r.OnPort(9202).Fails(Once))
            .Ping(p => p.SucceedAlways())
            .StaticConnectionPool()
            .AllDefaults()
        );

        audit = await audit.TraceCalls(
            new ClientCall { { PingSuccess, 9200 }, { HealthyResponse, 9200 } },
            new ClientCall { { PingSuccess, 9201 }, { HealthyResponse, 9201 } },
            new ClientCall {
                { PingSuccess, 9202},
                { BadResponse, 9202},
                { HealthyResponse, 9200},
                { pool =>  pool.Nodes.Where(n=>!n.IsAlive).Should().HaveCount(1) }
            },
            new ClientCall { { HealthyResponse, 9201 } },
            new ClientCall { { HealthyResponse, 9200 } },
            new ClientCall { { HealthyResponse, 9201 } },
            new ClientCall {
                { HealthyResponse, 9200 },
                { pool => pool.Nodes.First(n=>!n.IsAlive).DeadUntil.Should().BeAfter(DateTime.UtcNow) }
            }
        );

        audit = await audit.TraceCalls(
            new ClientCall { { HealthyResponse, 9201 } },
            new ClientCall { { HealthyResponse, 9200 } },
            new ClientCall { { HealthyResponse, 9201 } }
        );

        audit.ChangeTime(d => d.AddMinutes(20));

        audit = await audit.TraceCalls(
            new ClientCall { { HealthyResponse, 9201 } },
            new ClientCall {
                { Resurrection, 9202 },
                { PingSuccess, 9202 },
                { HealthyResponse, 9202 }
            }
        );
    }
}
