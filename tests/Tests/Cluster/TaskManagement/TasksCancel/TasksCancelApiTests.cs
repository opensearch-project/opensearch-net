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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.TaskManagement.TasksCancel;

public class TasksCancelApiTests
    : ApiIntegrationTestBase<IntrusiveOperationCluster, CancelTasksResponse, ICancelTasksRequest, CancelTasksDescriptor, CancelTasksRequest>
{
    public TasksCancelApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;


    protected override Func<CancelTasksDescriptor, ICancelTasksRequest> Fluent => d => d
        .TaskId(TaskId);

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override CancelTasksRequest Initializer => new CancelTasksRequest(TaskId);
    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/_tasks/{U(TaskId.ToString())}/_cancel";
    private TaskId TaskId => RanIntegrationSetup ? ExtendedValue<TaskId>("taskId") : "foo:1";

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var index in values.Values)
        {
            client.IndexMany(Enumerable.Range(0, 10000).Select(i => new Test { Id = i + 1, Flag = "bar" }), index);
            client.Indices.Refresh(index);
        }
        foreach (var view in values.Views)
        {
            values.CurrentView = view;
            var index = values.Value;

            var reindex = client.ReindexOnServer(r => r
                .Source(s => s.Index(index))
                .Destination(s => s.Index($"{index}-clone"))
                .WaitForCompletion(false)
            );

            var taskId = reindex.Task;
            var taskInfo = client.Tasks.GetTask(taskId);
            taskInfo.ShouldBeValid();
            values.ExtendedValue("taskId", taskId);
        }
    }

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Tasks.Cancel(f),
        (client, f) => client.Tasks.CancelAsync(f),
        (client, r) => client.Tasks.Cancel(r),
        (client, r) => client.Tasks.CancelAsync(r)
    );

    protected override void ExpectResponse(CancelTasksResponse response)
    {
        response.NodeFailures.Should().BeNullOrEmpty();
        response.Nodes.Should().NotBeEmpty();
        var tasks = response.Nodes.First().Value.Tasks;
        tasks.Should().NotBeEmpty().And.ContainKey(TaskId);
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class Test
    {
        public string Flag { get; set; }
        public long Id { get; set; }
    }
}
