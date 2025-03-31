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
*   http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/
// ███╗   ██╗ ██████╗ ████████╗██╗ ██████╗███████╗
// ████╗  ██║██╔═══██╗╚══██╔══╝██║██╔════╝██╔════╝
// ██╔██╗ ██║██║   ██║   ██║   ██║██║     █████╗
// ██║╚██╗██║██║   ██║   ██║   ██║██║     ██╔══╝
// ██║ ╚████║╚██████╔╝   ██║   ██║╚██████╗███████╗
// ╚═╝  ╚═══╝ ╚═════╝    ╚═╝   ╚═╝ ╚═════╝╚══════╝
// -----------------------------------------------
//
// This file is automatically generated
// Please do not edit these files manually
// Run the following in the root of the repos:
//
//      *NIX        :   ./build.sh codegen
//      Windows     :   build.bat codegen
//
// -----------------------------------------------

// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

// ReSharper disable once CheckNamespace
namespace OpenSearch.Net.Specification.TasksApi
{
    /// <summary>Request options for Cancel <para>https://opensearch.org/docs/latest/api-reference/tasks/#task-canceling</para></summary>
    public partial class CancelTasksRequestParameters
        : RequestParameters<CancelTasksRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => false;

        /// <summary>A comma-separated list of actions that should be returned. Keep empty to return all.</summary>
        public string[] Actions
        {
            get => Q<string[]>("actions");
            set => Q("actions", value);
        }

        /// <summary>
        /// A comma-separated list of node IDs or names used to limit the returned information. Use `_local` to return information from the node
        /// you're connecting to, specify the node name to get information from a specific node, or keep the parameter empty to get information from
        /// all nodes.
        /// </summary>
        public string[] Nodes
        {
            get => Q<string[]>("nodes");
            set => Q("nodes", value);
        }

        /// <summary>Returns tasks with a specified parent task ID (`node_id:task_number`). Keep empty or set to -1 to return all.</summary>
        public string ParentTaskId
        {
            get => Q<string>("parent_task_id");
            set => Q("parent_task_id", value);
        }

        /// <summary>Waits for the matching task to complete. When `true`, the request is blocked until the task has completed.</summary>
        public bool? WaitForCompletion
        {
            get => Q<bool?>("wait_for_completion");
            set => Q("wait_for_completion", value);
        }
    }

    /// <summary>Request options for GetTask <para>https://opensearch.org/docs/latest/api-reference/tasks/</para></summary>
    public partial class GetTaskRequestParameters : RequestParameters<GetTaskRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>The amount of time to wait for a response.</summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }

        /// <summary>Waits for the matching task to complete. When `true`, the request is blocked until the task has completed.</summary>
        public bool? WaitForCompletion
        {
            get => Q<bool?>("wait_for_completion");
            set => Q("wait_for_completion", value);
        }
    }

    /// <summary>Request options for List <para>https://opensearch.org/docs/latest/api-reference/tasks/</para></summary>
    public partial class ListTasksRequestParameters : RequestParameters<ListTasksRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>A comma-separated list of actions that should be returned. Keep empty to return all.</summary>
        public string[] Actions
        {
            get => Q<string[]>("actions");
            set => Q("actions", value);
        }

        /// <summary>When `true`, the response includes detailed information about shard recoveries.</summary>
        public bool? Detailed
        {
            get => Q<bool?>("detailed");
            set => Q("detailed", value);
        }

        /// <summary>Groups tasks by parent/child relationships or nodes.</summary>
        public TasksGroupBy? GroupBy
        {
            get => Q<TasksGroupBy?>("group_by");
            set => Q("group_by", value);
        }

        /// <summary>
        /// A comma-separated list of node IDs or names used to limit the returned information. Use `_local` to return information from the node
        /// you're connecting to, specify the node name to get information from a specific node, or keep the parameter empty to get information from
        /// all nodes.
        /// </summary>
        public string[] Nodes
        {
            get => Q<string[]>("nodes");
            set => Q("nodes", value);
        }

        /// <summary>Returns tasks with a specified parent task ID (`node_id:task_number`). Keep empty or set to -1 to return all.</summary>
        public string ParentTaskId
        {
            get => Q<string>("parent_task_id");
            set => Q("parent_task_id", value);
        }

        /// <summary>The amount of time to wait for a response.</summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }

        /// <summary>Waits for the matching task to complete. When `true`, the request is blocked until the task has completed.</summary>
        public bool? WaitForCompletion
        {
            get => Q<bool?>("wait_for_completion");
            set => Q("wait_for_completion", value);
        }
    }
}
