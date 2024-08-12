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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Core.ManagedOpenSearch.NodeSeeders;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.TaskManagement.TasksList;

public class TasksListApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, ListTasksResponse, IListTasksRequest, ListTasksDescriptor, ListTasksRequest>
{
    public TasksListApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;

    protected override Func<ListTasksDescriptor, IListTasksRequest> Fluent => s => s
        .Actions("*lists*");

    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override ListTasksRequest Initializer => new ListTasksRequest
    {
        Actions = new[] { "*lists*" }
    };

    protected override string UrlPath => "/_tasks?actions=%2Alists%2A";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Tasks.List(f),
        (client, f) => client.Tasks.ListAsync(f),
        (client, r) => client.Tasks.List(r),
        (client, r) => client.Tasks.ListAsync(r)
    );

    protected override void ExpectResponse(ListTasksResponse response)
    {
        response.Nodes.Should().NotBeEmpty();
        var taskExecutingNode = response.Nodes.First().Value;
        taskExecutingNode.Host.Should().NotBeNullOrWhiteSpace();
        taskExecutingNode.Ip.Should().NotBeNullOrWhiteSpace();
        taskExecutingNode.Name.Should().NotBeNullOrWhiteSpace();
        taskExecutingNode.TransportAddress.Should().NotBeNullOrWhiteSpace();
        taskExecutingNode.Tasks.Should().NotBeEmpty();
        taskExecutingNode.Tasks.Count().Should().BeGreaterOrEqualTo(2);

        var task = taskExecutingNode.Tasks.Values.First(p => p.ParentTaskId != null);
        task.Action.Should().NotBeNullOrWhiteSpace();
        task.Type.Should().NotBeNullOrWhiteSpace();
        task.Id.Should().BePositive();
        task.Node.Should().NotBeNullOrWhiteSpace();
        task.RunningTimeInNanoSeconds.Should().BeGreaterThan(0);
        task.StartTimeInMilliseconds.Should().BeGreaterThan(0);
        task.ParentTaskId.Should().NotBeNull();

        var parentTask = taskExecutingNode.Tasks[task.ParentTaskId];
        parentTask.Should().NotBeNull();
        parentTask.ParentTaskId.Should().BeNull();
    }
}

public class TasksListDetailedApiTests
    : ApiIntegrationTestBase<IntrusiveOperationCluster, ListTasksResponse, IListTasksRequest, ListTasksDescriptor, ListTasksRequest>
{
    private static TaskId _taskId = new TaskId("fakeid:1");

    public TasksListDetailedApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;

    protected override Func<ListTasksDescriptor, IListTasksRequest> Fluent => s => s
        .Detailed();

    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override ListTasksRequest Initializer => new ListTasksRequest()
    {
        Detailed = true
    };

    protected override string UrlPath => $"/_tasks?detailed=true";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Tasks.List(f),
        (client, f) => client.Tasks.ListAsync(f),
        (client, r) => client.Tasks.List(r),
        (client, r) => client.Tasks.ListAsync(r)
    );

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        // get a suitable load of projects in order to get a decent task status out
        var sourceIndex = "project-list-detailed";
        var targetIndex = "tasks-lists-detailed";
        var bulkResponse = client.IndexMany(Project.Generator.Generate(10000), sourceIndex);
        if (!bulkResponse.IsValid)
            throw new Exception($"failure in setting up integration {bulkResponse.ServerError}");

        client.Indices.Refresh(sourceIndex);

        var createIndex = client.Indices.Create(targetIndex, i => i
            .Settings(settings => settings.Analysis(DefaultSeeder.ProjectAnalysisSettings))
            .Map<Project>(DefaultSeeder.ProjectTypeMappings)
        );
        createIndex.ShouldBeValid();

        var response = client.ReindexOnServer(r => r
            .Source(s => s.Index(sourceIndex))
            .Destination(d => d
                .Index(targetIndex)
                .OpType(OpType.Create)
            )
            .Conflicts(Conflicts.Proceed)
            .WaitForCompletion(false)
            .Refresh()
        );

        _taskId = response.Task;
    }

    protected override void ExpectResponse(ListTasksResponse response)
    {
        response.Nodes.Should().NotBeEmpty();
        var taskExecutingNode = response.Nodes.First().Value;
        taskExecutingNode.Host.Should().NotBeNullOrWhiteSpace();
        taskExecutingNode.Ip.Should().NotBeNullOrWhiteSpace();
        taskExecutingNode.Name.Should().NotBeNullOrWhiteSpace();
        taskExecutingNode.TransportAddress.Should().NotBeNullOrWhiteSpace();
        taskExecutingNode.Tasks.Should().NotBeEmpty();
        taskExecutingNode.Tasks.Count().Should().BeGreaterOrEqualTo(1);

        var task = taskExecutingNode.Tasks[_taskId];
        task.Action.Should().NotBeNullOrWhiteSpace();
        task.Type.Should().NotBeNullOrWhiteSpace();
        task.Id.Should().BePositive();
        task.Node.Should().NotBeNullOrWhiteSpace();
        task.RunningTimeInNanoSeconds.Should().BeGreaterThan(0);
        task.StartTimeInMilliseconds.Should().BeGreaterThan(0);

        var status = task.Status;
        status.Should().NotBeNull();
        status.Total.Should().BeGreaterOrEqualTo(0);
        status.Batches.Should().BeGreaterOrEqualTo(0);
    }
}
