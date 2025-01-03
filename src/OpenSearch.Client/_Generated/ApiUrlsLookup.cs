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
        internal static readonly ApiUrls NoNamespaceBulk = new(["_bulk", "{index}/_bulk"]);

        internal static readonly ApiUrls CatAliases = new(["_cat/aliases", "_cat/aliases/{name}"]);

        internal static readonly ApiUrls CatAllPitSegments = new(["_cat/pit_segments/_all"]);

        internal static readonly ApiUrls CatAllocation = new(
            ["_cat/allocation", "_cat/allocation/{node_id}"]
        );

        internal static readonly ApiUrls CatClusterManager = new(["_cat/cluster_manager"]);

        internal static readonly ApiUrls CatCount = new(["_cat/count", "_cat/count/{index}"]);

        internal static readonly ApiUrls CatFielddata = new(
            ["_cat/fielddata", "_cat/fielddata/{fields}"]
        );

        internal static readonly ApiUrls CatHealth = new(["_cat/health"]);

        internal static readonly ApiUrls CatHelp = new(["_cat"]);

        internal static readonly ApiUrls CatIndices = new(["_cat/indices", "_cat/indices/{index}"]);

        internal static readonly ApiUrls CatMaster = new(["_cat/master"]);

        internal static readonly ApiUrls CatNodeAttributes = new(["_cat/nodeattrs"]);

        internal static readonly ApiUrls CatNodes = new(["_cat/nodes"]);

        internal static readonly ApiUrls CatPendingTasks = new(["_cat/pending_tasks"]);

        internal static readonly ApiUrls CatPitSegments = new(["_cat/pit_segments"]);

        internal static readonly ApiUrls CatPlugins = new(["_cat/plugins"]);

        internal static readonly ApiUrls CatRecovery = new(
            ["_cat/recovery", "_cat/recovery/{index}"]
        );

        internal static readonly ApiUrls CatRepositories = new(["_cat/repositories"]);

        internal static readonly ApiUrls CatSegmentReplication = new(
            ["_cat/segment_replication", "_cat/segment_replication/{index}"]
        );

        internal static readonly ApiUrls CatSegments = new(
            ["_cat/segments", "_cat/segments/{index}"]
        );

        internal static readonly ApiUrls CatShards = new(["_cat/shards", "_cat/shards/{index}"]);

        internal static readonly ApiUrls CatSnapshots = new(
            ["_cat/snapshots", "_cat/snapshots/{repository}"]
        );

        internal static readonly ApiUrls CatTasks = new(["_cat/tasks"]);

        internal static readonly ApiUrls CatTemplates = new(
            ["_cat/templates", "_cat/templates/{name}"]
        );

        internal static readonly ApiUrls CatThreadPool = new(
            ["_cat/thread_pool", "_cat/thread_pool/{thread_pool_patterns}"]
        );

        internal static readonly ApiUrls NoNamespaceClearScroll = new(["_search/scroll"]);

        internal static readonly ApiUrls ClusterAllocationExplain = new(
            ["_cluster/allocation/explain"]
        );

        internal static readonly ApiUrls ClusterDeleteComponentTemplate = new(
            ["_component_template/{name}"]
        );

        internal static readonly ApiUrls ClusterDeleteVotingConfigExclusions = new(
            ["_cluster/voting_config_exclusions"]
        );

        internal static readonly ApiUrls ClusterComponentTemplateExists = new(
            ["_component_template/{name}"]
        );

        internal static readonly ApiUrls ClusterGetComponentTemplate = new(
            ["_component_template", "_component_template/{name}"]
        );

        internal static readonly ApiUrls ClusterGetSettings = new(["_cluster/settings"]);

        internal static readonly ApiUrls ClusterHealth = new(
            ["_cluster/health", "_cluster/health/{index}"]
        );

        internal static readonly ApiUrls ClusterPendingTasks = new(["_cluster/pending_tasks"]);

        internal static readonly ApiUrls ClusterPostVotingConfigExclusions = new(
            ["_cluster/voting_config_exclusions"]
        );

        internal static readonly ApiUrls ClusterPutComponentTemplate = new(
            ["_component_template/{name}"]
        );

        internal static readonly ApiUrls ClusterPutSettings = new(["_cluster/settings"]);

        internal static readonly ApiUrls ClusterRemoteInfo = new(["_remote/info"]);

        internal static readonly ApiUrls ClusterReroute = new(["_cluster/reroute"]);

        internal static readonly ApiUrls ClusterState = new(
            ["_cluster/state", "_cluster/state/{metric}", "_cluster/state/{metric}/{index}"]
        );

        internal static readonly ApiUrls ClusterStats = new(
            [
                "_cluster/stats",
                "_cluster/stats/{metric}/{index_metric}/nodes/{node_id}",
                "_cluster/stats/{metric}/nodes/{node_id}",
                "_cluster/stats/nodes/{node_id}",
            ]
        );

        internal static readonly ApiUrls NoNamespaceCount = new(["_count", "{index}/_count"]);

        internal static readonly ApiUrls NoNamespaceCreate = new(["{index}/_create/{id}"]);

        internal static readonly ApiUrls NoNamespaceCreatePit = new(
            ["{index}/_search/point_in_time"]
        );

        internal static readonly ApiUrls DanglingIndicesDeleteDanglingIndex = new(
            ["_dangling/{index_uuid}"]
        );

        internal static readonly ApiUrls DanglingIndicesImportDanglingIndex = new(
            ["_dangling/{index_uuid}"]
        );

        internal static readonly ApiUrls DanglingIndicesList = new(["_dangling"]);

        internal static readonly ApiUrls NoNamespaceDelete = new(["{index}/_doc/{id}"]);

        internal static readonly ApiUrls NoNamespaceDeleteAllPits = new(
            ["_search/point_in_time/_all"]
        );

        internal static readonly ApiUrls NoNamespaceDeleteByQuery = new(
            ["{index}/_delete_by_query"]
        );

        internal static readonly ApiUrls NoNamespaceDeleteByQueryRethrottle = new(
            ["_delete_by_query/{task_id}/_rethrottle"]
        );

        internal static readonly ApiUrls NoNamespaceDeletePit = new(["_search/point_in_time"]);

        internal static readonly ApiUrls NoNamespaceDeleteScript = new(["_scripts/{id}"]);

        internal static readonly ApiUrls NoNamespaceDocumentExists = new(["{index}/_doc/{id}"]);

        internal static readonly ApiUrls NoNamespaceSourceExists = new(["{index}/_source/{id}"]);

        internal static readonly ApiUrls NoNamespaceExplain = new(["{index}/_explain/{id}"]);

        internal static readonly ApiUrls NoNamespaceFieldCapabilities = new(
            ["_field_caps", "{index}/_field_caps"]
        );

        internal static readonly ApiUrls NoNamespaceGet = new(["{index}/_doc/{id}"]);

        internal static readonly ApiUrls NoNamespaceGetAllPits = new(
            ["_search/point_in_time/_all"]
        );

        internal static readonly ApiUrls NoNamespaceGetScript = new(["_scripts/{id}"]);

        internal static readonly ApiUrls NoNamespaceSource = new(["{index}/_source/{id}"]);

        internal static readonly ApiUrls NoNamespaceIndex = new(
            ["{index}/_doc", "{index}/_doc/{id}"]
        );

        internal static readonly ApiUrls IndicesAddBlock = new(["{index}/_block/{block}"]);

        internal static readonly ApiUrls IndicesAnalyze = new(["_analyze", "{index}/_analyze"]);

        internal static readonly ApiUrls IndicesClearCache = new(
            ["_cache/clear", "{index}/_cache/clear"]
        );

        internal static readonly ApiUrls IndicesClone = new(["{index}/_clone/{target}"]);

        internal static readonly ApiUrls IndicesClose = new(["{index}/_close"]);

        internal static readonly ApiUrls IndicesCreate = new(["{index}"]);

        internal static readonly ApiUrls IndicesDelete = new(["{index}"]);

        internal static readonly ApiUrls IndicesDeleteAlias = new(["{index}/_alias/{name}"]);

        internal static readonly ApiUrls IndicesDeleteComposableTemplate = new(
            ["_index_template/{name}"]
        );

        internal static readonly ApiUrls IndicesDeleteTemplate = new(["_template/{name}"]);

        internal static readonly ApiUrls IndicesExists = new(["{index}"]);

        internal static readonly ApiUrls IndicesAliasExists = new(
            ["{index}/_alias/{name}", "_alias/{name}"]
        );

        internal static readonly ApiUrls IndicesComposableTemplateExists = new(
            ["_index_template/{name}"]
        );

        internal static readonly ApiUrls IndicesTemplateExists = new(["_template/{name}"]);

        internal static readonly ApiUrls IndicesFlush = new(["_flush", "{index}/_flush"]);

        internal static readonly ApiUrls IndicesForceMerge = new(
            ["_forcemerge", "{index}/_forcemerge"]
        );

        internal static readonly ApiUrls IndicesGet = new(["{index}"]);

        internal static readonly ApiUrls IndicesGetAlias = new(
            ["_alias", "{index}/_alias", "{index}/_alias/{name}", "_alias/{name}"]
        );

        internal static readonly ApiUrls IndicesGetFieldMapping = new(
            ["_mapping/field/{fields}", "{index}/_mapping/field/{fields}"]
        );

        internal static readonly ApiUrls IndicesGetComposableTemplate = new(
            ["_index_template", "_index_template/{name}"]
        );

        internal static readonly ApiUrls IndicesGetMapping = new(["_mapping", "{index}/_mapping"]);

        internal static readonly ApiUrls IndicesGetSettings = new(
            ["_settings", "{index}/_settings", "{index}/_settings/{name}", "_settings/{name}"]
        );

        internal static readonly ApiUrls IndicesGetTemplate = new(
            ["_template", "_template/{name}"]
        );

        internal static readonly ApiUrls IndicesOpen = new(["{index}/_open"]);

        internal static readonly ApiUrls IndicesPutAlias = new(
            ["_alias", "{index}/_alias", "{index}/_alias/{name}", "_alias/{name}"]
        );

        internal static readonly ApiUrls IndicesPutComposableTemplate = new(
            ["_index_template/{name}"]
        );

        internal static readonly ApiUrls IndicesPutMapping = new(["{index}/_mapping"]);

        internal static readonly ApiUrls IndicesUpdateSettings = new(
            ["_settings", "{index}/_settings"]
        );

        internal static readonly ApiUrls IndicesPutTemplate = new(["_template/{name}"]);

        internal static readonly ApiUrls IndicesRefresh = new(["_refresh", "{index}/_refresh"]);

        internal static readonly ApiUrls IndicesResolve = new(["_resolve/index/{name}"]);

        internal static readonly ApiUrls IndicesRollover = new(
            ["{alias}/_rollover", "{alias}/_rollover/{new_index}"]
        );

        internal static readonly ApiUrls IndicesShrink = new(["{index}/_shrink/{target}"]);

        internal static readonly ApiUrls IndicesSplit = new(["{index}/_split/{target}"]);

        internal static readonly ApiUrls IndicesStats = new(
            ["_stats", "{index}/_stats", "{index}/_stats/{metric}", "_stats/{metric}"]
        );

        internal static readonly ApiUrls IndicesBulkAlias = new(["_aliases"]);

        internal static readonly ApiUrls IndicesValidateQuery = new(
            ["_validate/query", "{index}/_validate/query"]
        );

        internal static readonly ApiUrls NoNamespaceRootNodeInfo = new([""]);

        internal static readonly ApiUrls IngestDeletePipeline = new(["_ingest/pipeline/{id}"]);

        internal static readonly ApiUrls IngestGetPipeline = new(
            ["_ingest/pipeline", "_ingest/pipeline/{id}"]
        );

        internal static readonly ApiUrls IngestGrokProcessorPatterns = new(
            ["_ingest/processor/grok"]
        );

        internal static readonly ApiUrls IngestPutPipeline = new(["_ingest/pipeline/{id}"]);

        internal static readonly ApiUrls IngestSimulatePipeline = new(
            ["_ingest/pipeline/_simulate", "_ingest/pipeline/{id}/_simulate"]
        );

        internal static readonly ApiUrls NoNamespaceMultiGet = new(["_mget", "{index}/_mget"]);

        internal static readonly ApiUrls NoNamespaceMultiSearch = new(
            ["_msearch", "{index}/_msearch"]
        );

        internal static readonly ApiUrls NoNamespaceMultiSearchTemplate = new(
            ["_msearch/template", "{index}/_msearch/template"]
        );

        internal static readonly ApiUrls NoNamespaceMultiTermVectors = new(
            ["_mtermvectors", "{index}/_mtermvectors"]
        );

        internal static readonly ApiUrls NodesHotThreads = new(
            ["_nodes/hot_threads", "_nodes/{node_id}/hot_threads"]
        );

        internal static readonly ApiUrls NodesInfo = new(
            ["_nodes", "_nodes/{metric}", "_nodes/{node_id}", "_nodes/{node_id}/{metric}"]
        );

        internal static readonly ApiUrls NodesReloadSecureSettings = new(
            ["_nodes/reload_secure_settings", "_nodes/{node_id}/reload_secure_settings"]
        );

        internal static readonly ApiUrls NodesStats = new(
            [
                "_nodes/stats",
                "_nodes/stats/{metric}",
                "_nodes/stats/{metric}/{index_metric}",
                "_nodes/{node_id}/stats",
                "_nodes/{node_id}/stats/{metric}",
                "_nodes/{node_id}/stats/{metric}/{index_metric}",
            ]
        );

        internal static readonly ApiUrls NodesUsage = new(
            [
                "_nodes/usage",
                "_nodes/usage/{metric}",
                "_nodes/{node_id}/usage",
                "_nodes/{node_id}/usage/{metric}",
            ]
        );

        internal static readonly ApiUrls NoNamespacePing = new([""]);

        internal static readonly ApiUrls NoNamespacePutScript = new(
            ["_scripts/{id}", "_scripts/{id}/{context}"]
        );

        internal static readonly ApiUrls NoNamespaceReindexOnServer = new(["_reindex"]);

        internal static readonly ApiUrls NoNamespaceReindexRethrottle = new(
            ["_reindex/{task_id}/_rethrottle"]
        );

        internal static readonly ApiUrls NoNamespaceRenderSearchTemplate = new(
            ["_render/template", "_render/template/{id}"]
        );

        internal static readonly ApiUrls NoNamespaceExecutePainlessScript = new(
            ["_scripts/painless/_execute"]
        );

        internal static readonly ApiUrls NoNamespaceScroll = new(["_search/scroll"]);

        internal static readonly ApiUrls NoNamespaceSearch = new(["_search", "{index}/_search"]);

        internal static readonly ApiUrls NoNamespaceSearchShards = new(
            ["_search_shards", "{index}/_search_shards"]
        );

        internal static readonly ApiUrls NoNamespaceSearchTemplate = new(
            ["_search/template", "{index}/_search/template"]
        );

        internal static readonly ApiUrls SnapshotCleanupRepository = new(
            ["_snapshot/{repository}/_cleanup"]
        );

        internal static readonly ApiUrls SnapshotClone = new(
            ["_snapshot/{repository}/{snapshot}/_clone/{target_snapshot}"]
        );

        internal static readonly ApiUrls SnapshotSnapshot = new(
            ["_snapshot/{repository}/{snapshot}"]
        );

        internal static readonly ApiUrls SnapshotCreateRepository = new(["_snapshot/{repository}"]);

        internal static readonly ApiUrls SnapshotDelete = new(
            ["_snapshot/{repository}/{snapshot}"]
        );

        internal static readonly ApiUrls SnapshotDeleteRepository = new(["_snapshot/{repository}"]);

        internal static readonly ApiUrls SnapshotGet = new(["_snapshot/{repository}/{snapshot}"]);

        internal static readonly ApiUrls SnapshotGetRepository = new(
            ["_snapshot", "_snapshot/{repository}"]
        );

        internal static readonly ApiUrls SnapshotRestore = new(
            ["_snapshot/{repository}/{snapshot}/_restore"]
        );

        internal static readonly ApiUrls SnapshotStatus = new(
            [
                "_snapshot/_status",
                "_snapshot/{repository}/_status",
                "_snapshot/{repository}/{snapshot}/_status",
            ]
        );

        internal static readonly ApiUrls SnapshotVerifyRepository = new(
            ["_snapshot/{repository}/_verify"]
        );

        internal static readonly ApiUrls TasksCancel = new(
            ["_tasks/_cancel", "_tasks/{task_id}/_cancel"]
        );

        internal static readonly ApiUrls TasksGetTask = new(["_tasks/{task_id}"]);

        internal static readonly ApiUrls TasksList = new(["_tasks"]);

        internal static readonly ApiUrls NoNamespaceTermVectors = new(
            ["{index}/_termvectors", "{index}/_termvectors/{id}"]
        );

        internal static readonly ApiUrls NoNamespaceUpdate = new(["{index}/_update/{id}"]);

        internal static readonly ApiUrls NoNamespaceUpdateByQuery = new(
            ["{index}/_update_by_query"]
        );

        internal static readonly ApiUrls NoNamespaceUpdateByQueryRethrottle = new(
            ["_update_by_query/{task_id}/_rethrottle"]
        );
    }
}
