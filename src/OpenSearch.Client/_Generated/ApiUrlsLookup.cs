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
namespace OpenSearch.Client
{
    internal static partial class ApiUrlsLookups
    {
        internal static readonly ApiUrls CatAliases =
            new(new[] { "_cat/aliases", "_cat/aliases/{name}" });

        internal static readonly ApiUrls CatAllocation =
            new(new[] { "_cat/allocation", "_cat/allocation/{node_id}" });

        internal static readonly ApiUrls CatCount =
            new(new[] { "_cat/count", "_cat/count/{index}" });

        internal static readonly ApiUrls CatFielddata =
            new(new[] { "_cat/fielddata", "_cat/fielddata/{fields}" });

        internal static readonly ApiUrls CatHealth = new(new[] { "_cat/health" });

        internal static readonly ApiUrls ClusterAllocationExplain =
            new(new[] { "_cluster/allocation/explain" });

        internal static readonly ApiUrls ClusterDeleteComponentTemplate =
            new(new[] { "_component_template/{name}" });

        internal static readonly ApiUrls ClusterDeleteVotingConfigExclusions =
            new(new[] { "_cluster/voting_config_exclusions" });

        internal static readonly ApiUrls ClusterComponentTemplateExists =
            new(new[] { "_component_template/{name}" });

        internal static readonly ApiUrls ClusterGetComponentTemplate =
            new(new[] { "_component_template", "_component_template/{name}" });

        internal static readonly ApiUrls ClusterGetSettings = new(new[] { "_cluster/settings" });

        internal static readonly ApiUrls ClusterHealth =
            new(new[] { "_cluster/health", "_cluster/health/{index}" });

        internal static readonly ApiUrls ClusterPendingTasks =
            new(new[] { "_cluster/pending_tasks" });

        internal static readonly ApiUrls ClusterPostVotingConfigExclusions =
            new(new[] { "_cluster/voting_config_exclusions" });

        internal static readonly ApiUrls ClusterPutComponentTemplate =
            new(new[] { "_component_template/{name}" });

        internal static readonly ApiUrls ClusterPutSettings = new(new[] { "_cluster/settings" });

        internal static readonly ApiUrls ClusterRemoteInfo = new(new[] { "_remote/info" });

        internal static readonly ApiUrls ClusterReroute = new(new[] { "_cluster/reroute" });

        internal static readonly ApiUrls ClusterState =
            new(
                new[]
                {
                    "_cluster/state",
                    "_cluster/state/{metric}",
                    "_cluster/state/{metric}/{index}"
                }
            );

        internal static readonly ApiUrls ClusterStats =
            new(new[] { "_cluster/stats", "_cluster/stats/nodes/{node_id}" });

        internal static readonly ApiUrls NoNamespaceCreatePit =
            new(new[] { "{index}/_search/point_in_time" });

        internal static readonly ApiUrls DanglingIndicesDeleteDanglingIndex =
            new(new[] { "_dangling/{index_uuid}" });

        internal static readonly ApiUrls DanglingIndicesImportDanglingIndex =
            new(new[] { "_dangling/{index_uuid}" });

        internal static readonly ApiUrls DanglingIndicesList = new(new[] { "_dangling" });

        internal static readonly ApiUrls NoNamespaceDeleteAllPits =
            new(new[] { "_search/point_in_time/_all" });

        internal static readonly ApiUrls NoNamespaceDeletePit =
            new(new[] { "_search/point_in_time" });

        internal static readonly ApiUrls NoNamespaceGetAllPits =
            new(new[] { "_search/point_in_time/_all" });

        internal static readonly ApiUrls IndicesDeleteComposableTemplate =
            new(new[] { "_index_template/{name}" });

        internal static readonly ApiUrls IndicesComposableTemplateExists =
            new(new[] { "_index_template/{name}" });

        internal static readonly ApiUrls IndicesGetComposableTemplate =
            new(new[] { "_index_template", "_index_template/{name}" });

        internal static readonly ApiUrls IndicesPutComposableTemplate =
            new(new[] { "_index_template/{name}" });

        internal static readonly ApiUrls IngestDeletePipeline =
            new(new[] { "_ingest/pipeline/{id}" });

        internal static readonly ApiUrls IngestGetPipeline =
            new(new[] { "_ingest/pipeline", "_ingest/pipeline/{id}" });

        internal static readonly ApiUrls IngestGrokProcessorPatterns =
            new(new[] { "_ingest/processor/grok" });

        internal static readonly ApiUrls IngestPutPipeline = new(new[] { "_ingest/pipeline/{id}" });

        internal static readonly ApiUrls IngestSimulatePipeline =
            new(new[] { "_ingest/pipeline/_simulate", "_ingest/pipeline/{id}/_simulate" });

        internal static readonly ApiUrls NodesHotThreads =
            new(new[] { "_nodes/hot_threads", "_nodes/{node_id}/hot_threads" });

        internal static readonly ApiUrls NodesInfo =
            new(
                new[]
                {
                    "_nodes",
                    "_nodes/{metric}",
                    "_nodes/{node_id}",
                    "_nodes/{node_id}/{metric}"
                }
            );

        internal static readonly ApiUrls NodesReloadSecureSettings =
            new(
                new[] { "_nodes/reload_secure_settings", "_nodes/{node_id}/reload_secure_settings" }
            );

        internal static readonly ApiUrls NodesStats =
            new(
                new[]
                {
                    "_nodes/stats",
                    "_nodes/stats/{metric}",
                    "_nodes/stats/{metric}/{index_metric}",
                    "_nodes/{node_id}/stats",
                    "_nodes/{node_id}/stats/{metric}",
                    "_nodes/{node_id}/stats/{metric}/{index_metric}"
                }
            );

        internal static readonly ApiUrls NodesUsage =
            new(
                new[]
                {
                    "_nodes/usage",
                    "_nodes/usage/{metric}",
                    "_nodes/{node_id}/usage",
                    "_nodes/{node_id}/usage/{metric}"
                }
            );

        internal static readonly ApiUrls SnapshotCleanupRepository =
            new(new[] { "_snapshot/{repository}/_cleanup" });

        internal static readonly ApiUrls SnapshotClone =
            new(new[] { "_snapshot/{repository}/{snapshot}/_clone/{target_snapshot}" });

        internal static readonly ApiUrls SnapshotSnapshot =
            new(new[] { "_snapshot/{repository}/{snapshot}" });

        internal static readonly ApiUrls SnapshotCreateRepository =
            new(new[] { "_snapshot/{repository}" });

        internal static readonly ApiUrls SnapshotDelete =
            new(new[] { "_snapshot/{repository}/{snapshot}" });

        internal static readonly ApiUrls SnapshotDeleteRepository =
            new(new[] { "_snapshot/{repository}" });

        internal static readonly ApiUrls SnapshotGet =
            new(new[] { "_snapshot/{repository}/{snapshot}" });

        internal static readonly ApiUrls SnapshotGetRepository =
            new(new[] { "_snapshot", "_snapshot/{repository}" });

        internal static readonly ApiUrls SnapshotRestore =
            new(new[] { "_snapshot/{repository}/{snapshot}/_restore" });

        internal static readonly ApiUrls SnapshotStatus =
            new(
                new[]
                {
                    "_snapshot/_status",
                    "_snapshot/{repository}/_status",
                    "_snapshot/{repository}/{snapshot}/_status"
                }
            );

        internal static readonly ApiUrls SnapshotVerifyRepository =
            new(new[] { "_snapshot/{repository}/_verify" });

        internal static readonly ApiUrls TasksCancel =
            new(new[] { "_tasks/_cancel", "_tasks/{task_id}/_cancel" });

        internal static readonly ApiUrls TasksGetTask = new(new[] { "_tasks/{task_id}" });

        internal static readonly ApiUrls TasksList = new(new[] { "_tasks" });
    }
}
