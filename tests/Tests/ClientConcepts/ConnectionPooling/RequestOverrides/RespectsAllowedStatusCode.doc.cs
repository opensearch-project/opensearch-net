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

namespace Tests.ClientConcepts.ConnectionPooling.RequestOverrides
{
    public class RespectsAllowedStatusCode
    {
        /**=== Allowed status codes
		*/

        [U]
        public async Task CanOverrideBadResponse()
        {
            var audit = new Auditor(() => VirtualClusterWith
                .Nodes(10)
                .ClientCalls(r => r.FailAlways(400))
                .StaticConnectionPool()
                .Settings(s => s.DisablePing().MaximumRetries(0))
            );

            audit = await audit.TraceCalls(
                new ClientCall {
                    { BadResponse, 9200 }
                },
                new ClientCall(r => r.AllowedStatusCodes(400)) {
                    { HealthyResponse, 9201 }
                }
            );
        }
    }
}
