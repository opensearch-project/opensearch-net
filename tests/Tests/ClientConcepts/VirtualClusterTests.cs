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
using OpenSearch.Net.VirtualizedCluster.Rules;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Xunit;

namespace Tests.ClientConcepts;

public class VirtualClusterTests
{
    [U]
    public async Task ThrowsExceptionWithNoRules()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(1)
            .StaticConnectionPool()
            .Settings(s => s.DisablePing().EnableDebugMode())
        );
        var e = await Assert.ThrowsAsync<UnexpectedOpenSearchClientException>(
            async () => await audit.TraceCalls(new ClientCall { }));

        e.Message.Should().Contain("No ClientCalls defined for the current VirtualCluster, so we do not know how to respond");
    }

    [U]
    public async Task ThrowsExceptionAfterDepleedingRules()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(1)
            .ClientCalls(r => r.Succeeds(TimesHelper.Once).ReturnResponse(new { x = 1 }))
            .StaticConnectionPool()
            .Settings(s => s.DisablePing().EnableDebugMode())
        );
        audit = await audit.TraceCalls(
            new ClientCall {

                { AuditEvent.HealthyResponse, 9200, response =>
                {
                    response.ApiCall.Success.Should().BeTrue();
                    response.ApiCall.HttpStatusCode.Should().Be(200);
                    response.ApiCall.DebugInformation.Should().Contain("x\":1");
                } },
            }
        );
        var e = await Assert.ThrowsAsync<UnexpectedOpenSearchClientException>(
            async () => await audit.TraceCalls(new ClientCall { }));

        e.Message.Should().Contain("No global or port specific ClientCalls rule (9200) matches any longer after 2 calls in to the cluster");
    }

    [U]
    public async Task AGlobalRuleStaysValidForever()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(1)
            .ClientCalls(c => c.SucceedAlways())
            .StaticConnectionPool()
            .Settings(s => s.DisablePing())
        );

        audit = await audit.TraceCalls(
            Enumerable.Range(0, 1000)
                .Select(i => new ClientCall { { AuditEvent.HealthyResponse, 9200 }, })
                .ToArray()
        );

    }

    [U]
    public async Task RulesAreIgnoredAfterBeingExecuted()
    {
        var audit = new Auditor(() => VirtualClusterWith
            .Nodes(1)
            .ClientCalls(r => r.Succeeds(TimesHelper.Once).ReturnResponse(new { x = 1 }))
            .ClientCalls(r => r.Fails(TimesHelper.Once, 500).ReturnResponse(new { x = 2 }))
            .ClientCalls(r => r.Fails(TimesHelper.Twice, 400).ReturnResponse(new { x = 3 }))
            .ClientCalls(r => r.Succeeds(TimesHelper.Once).ReturnResponse(new { x = 4 }))
            .StaticConnectionPool()
            .Settings(s => s.DisablePing().EnableDebugMode())
        );
        audit = await audit.TraceCalls(
            new ClientCall {

                { AuditEvent.HealthyResponse, 9200, response =>
                {
                    response.ApiCall.Success.Should().BeTrue();
                    response.ApiCall.HttpStatusCode.Should().Be(200);
                    response.ApiCall.DebugInformation.Should().Contain("x\":1");
                } },
            },
            new ClientCall {

                { AuditEvent.BadResponse, 9200, response =>
                {
                    response.ApiCall.Success.Should().BeFalse();
                    response.ApiCall.HttpStatusCode.Should().Be(500);
                    response.ApiCall.DebugInformation.Should().Contain("x\":2");
                } },
            },
            new ClientCall {

                { AuditEvent.BadResponse, 9200, response =>
                {
                    response.ApiCall.HttpStatusCode.Should().Be(400);
                    response.ApiCall.DebugInformation.Should().Contain("x\":3");
                } },
            },
            new ClientCall {

                { AuditEvent.BadResponse, 9200, response =>
                {
                    response.ApiCall.HttpStatusCode.Should().Be(400);
                    response.ApiCall.DebugInformation.Should().Contain("x\":3");
                } },
            },
            new ClientCall {

                { AuditEvent.HealthyResponse, 9200, response =>
                {
                    response.ApiCall.HttpStatusCode.Should().Be(200);
                    response.ApiCall.DebugInformation.Should().Contain("x\":4");
                } },
            }
        );
    }
}
