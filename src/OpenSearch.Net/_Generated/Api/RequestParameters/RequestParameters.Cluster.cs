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
namespace OpenSearch.Net.Specification.ClusterApi
{
    /// <summary>Request options for AllocationExplain <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/</para></summary>
    public partial class ClusterAllocationExplainRequestParameters
        : RequestParameters<ClusterAllocationExplainRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;

        /// <summary>When `true`, returns information about disk usage and shard sizes.</summary>
        public bool? IncludeDiskInfo
        {
            get => Q<bool?>("include_disk_info");
            set => Q("include_disk_info", value);
        }

        /// <summary>When `true`, returns any `YES` decisions in the allocation explanation.</summary>
        public bool? IncludeYesDecisions
        {
            get => Q<bool?>("include_yes_decisions");
            set => Q("include_yes_decisions", value);
        }
    }

    /// <summary>Request options for DeleteComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class DeleteComponentTemplateRequestParameters
        : RequestParameters<DeleteComponentTemplateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;

        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }
    }

    /// <summary>Request options for DeleteDecommissionAwareness <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-decommission/#example-decommissioning-and-recommissioning-a-zone</para></summary>
    public partial class DeleteDecommissionAwarenessRequestParameters
        : RequestParameters<DeleteDecommissionAwarenessRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for DeleteVotingConfigExclusions <para>https://opensearch.org/docs/latest</para></summary>
    public partial class DeleteVotingConfigExclusionsRequestParameters
        : RequestParameters<DeleteVotingConfigExclusionsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;

        /// <summary>
        /// Specifies whether to wait for all excluded nodes to be removed from the cluster before clearing the voting configuration exclusions list.
        /// When `true`, all excluded nodes are removed from the cluster before this API takes any action. When `false`, the voting configuration
        /// exclusions list is cleared even if some excluded nodes are still in the cluster.
        /// </summary>
        public bool? WaitForRemoval
        {
            get => Q<bool?>("wait_for_removal");
            set => Q("wait_for_removal", value);
        }
    }

    /// <summary>Request options for DeleteWeightedRouting <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-awareness/#example-deleting-weights</para></summary>
    public partial class DeleteWeightedRoutingRequestParameters
        : RequestParameters<DeleteWeightedRoutingRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for ComponentTemplateExists <para>https://opensearch.org/docs/latest</para></summary>
    public partial class ComponentTemplateExistsRequestParameters
        : RequestParameters<ComponentTemplateExistsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.HEAD;
        public override bool SupportsBody => false;

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>
        /// When `true`, the request retrieves information from the local node only. When `false, information is retrieved from the cluster manager
        /// node.
        /// </summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }
    }

    /// <summary>Request options for GetComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class GetComponentTemplateRequestParameters
        : RequestParameters<GetComponentTemplateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>If `true`, returns settings in flat format.</summary>
        public bool? FlatSettings
        {
            get => Q<bool?>("flat_settings");
            set => Q("flat_settings", value);
        }

        /// <summary>
        /// When `true`, the request retrieves information from the local node only. When `false`, information is retrieved from the cluster manager
        /// node.
        /// </summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }
    }

    /// <summary>Request options for GetDecommissionAwareness <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-decommission/#example-getting-zone-decommission-status</para></summary>
    public partial class GetDecommissionAwarenessRequestParameters
        : RequestParameters<GetDecommissionAwarenessRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetSettings <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/</para></summary>
    public partial class ClusterGetSettingsRequestParameters
        : RequestParameters<ClusterGetSettingsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>When `true`, returns cluster settings in a flat format.</summary>
        public bool? FlatSettings
        {
            get => Q<bool?>("flat_settings");
            set => Q("flat_settings", value);
        }

        /// <summary>When `true`, returns default cluster settings from the local node.</summary>
        public bool? IncludeDefaults
        {
            get => Q<bool?>("include_defaults");
            set => Q("include_defaults", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }
    }

    /// <summary>Request options for GetWeightedRouting <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-awareness/#example-getting-weights-for-all-zones</para></summary>
    public partial class GetWeightedRoutingRequestParameters
        : RequestParameters<GetWeightedRoutingRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for Health <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/</para></summary>
    public partial class ClusterHealthRequestParameters
        : RequestParameters<ClusterHealthRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>
        /// The name of the awareness attribute for which to return the cluster health status (for example, `zone`). Applicable only if `level` is set
        /// to `awareness_attributes`.
        /// </summary>
        public string AwarenessAttribute
        {
            get => Q<string>("awareness_attribute");
            set => Q("awareness_attribute", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>Specifies the type of index that wildcard expressions can match. Supports comma-separated values.</summary>
        public ExpandWildcards? ExpandWildcards
        {
            get => Q<ExpandWildcards?>("expand_wildcards");
            set => Q("expand_wildcards", value);
        }
        public ClusterHealthLevel? Level
        {
            get => Q<ClusterHealthLevel?>("level");
            set => Q("level", value);
        }

        /// <summary>Whether to return information from the local node only instead of from the cluster manager node.</summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }

        /// <summary>Waits until the specified number of shards is active before returning a response. Use `all` for all shards.</summary>
        public string WaitForActiveShards
        {
            get => Q<string>("wait_for_active_shards");
            set => Q("wait_for_active_shards", value);
        }

        /// <summary>Waits until all currently queued events with the given priority are processed.</summary>
        public WaitForEvents? WaitForEvents
        {
            get => Q<WaitForEvents?>("wait_for_events");
            set => Q("wait_for_events", value);
        }

        /// <summary>
        /// Waits until the specified number of nodes (`N`) is available. Accepts `&gt;=N`, `&lt;=N`, `&gt;N`, and `&lt;N`. You can also use `ge(N)`,
        /// `le(N)`, `gt(N)`, and `lt(N)` notation.
        /// </summary>
        public string WaitForNodes
        {
            get => Q<string>("wait_for_nodes");
            set => Q("wait_for_nodes", value);
        }

        /// <summary>Whether to wait until there are no initializing shards in the cluster.</summary>
        public bool? WaitForNoInitializingShards
        {
            get => Q<bool?>("wait_for_no_initializing_shards");
            set => Q("wait_for_no_initializing_shards", value);
        }

        /// <summary>Whether to wait until there are no relocating shards in the cluster.</summary>
        public bool? WaitForNoRelocatingShards
        {
            get => Q<bool?>("wait_for_no_relocating_shards");
            set => Q("wait_for_no_relocating_shards", value);
        }

        /// <summary>Waits until the cluster health reaches the specified status or better.</summary>
        public HealthStatus? WaitForStatus
        {
            get => Q<HealthStatus?>("wait_for_status");
            set => Q("wait_for_status", value);
        }
    }

    /// <summary>Request options for PendingTasks <para>https://opensearch.org/docs/latest</para></summary>
    public partial class ClusterPendingTasksRequestParameters
        : RequestParameters<ClusterPendingTasksRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>
        /// When `true`, the request retrieves information from the local node only. When `false`, information is retrieved from the cluster manager
        /// node.
        /// </summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }
    }

    /// <summary>Request options for PostVotingConfigExclusions <para>https://opensearch.org/docs/latest</para></summary>
    public partial class PostVotingConfigExclusionsRequestParameters
        : RequestParameters<PostVotingConfigExclusionsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => false;

        /// <summary>
        /// A comma-separated list of node IDs to exclude from the voting configuration. When using this setting, you cannot also specify
        /// `node_names`.
        /// </summary>
        public string[] NodeIds
        {
            get => Q<string[]>("node_ids");
            set => Q("node_ids", value);
        }

        /// <summary>
        /// A comma-separated list of node names to exclude from the voting configuration. When using this setting, you cannot also specify
        /// `node_ids`.
        /// </summary>
        public string[] NodeNames
        {
            get => Q<string[]>("node_names");
            set => Q("node_names", value);
        }

        /// <summary>
        /// When adding a voting configuration exclusion, the API waits for the specified nodes to be excluded from the voting configuration before
        /// returning a response. If the timeout expires before the appropriate condition is satisfied, the request fails and returns an error.
        /// </summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }
    }

    /// <summary>Request options for PutComponentTemplate <para>https://opensearch.org/docs/latest/im-plugin/index-templates/#use-component-templates-to-create-an-index-template</para></summary>
    public partial class PutComponentTemplateRequestParameters
        : RequestParameters<PutComponentTemplateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>When `true`, this request cannot replace or update existing component templates.</summary>
        public bool? Create
        {
            get => Q<bool?>("create");
            set => Q("create", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }
    }

    /// <summary>Request options for PutDecommissionAwareness <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-decommission/#example-decommissioning-and-recommissioning-a-zone</para></summary>
    public partial class PutDecommissionAwarenessRequestParameters
        : RequestParameters<PutDecommissionAwarenessRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for PutSettings <para>https://opensearch.org/docs/latest/api-reference/cluster-settings/</para></summary>
    public partial class ClusterPutSettingsRequestParameters
        : RequestParameters<ClusterPutSettingsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>Returns settings in a flat format.</summary>
        public bool? FlatSettings
        {
            get => Q<bool?>("flat_settings");
            set => Q("flat_settings", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }
    }

    /// <summary>Request options for PutWeightedRouting <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-awareness/#example-weighted-round-robin-search</para></summary>
    public partial class PutWeightedRoutingRequestParameters
        : RequestParameters<PutWeightedRoutingRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for RemoteInfo <para>https://opensearch.org/docs/latest/api-reference/remote-info/</para></summary>
    public partial class RemoteInfoRequestParameters
        : RequestParameters<RemoteInfoRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for Reroute <para>https://opensearch.org/docs/latest</para></summary>
    public partial class ClusterRerouteRequestParameters
        : RequestParameters<ClusterRerouteRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>When `true`, the request simulates the operation and returns the resulting state.</summary>
        public bool? DryRun
        {
            get => Q<bool?>("dry_run");
            set => Q("dry_run", value);
        }

        /// <summary>When `true`, the response contains an explanation of why certain commands can or cannot be executed.</summary>
        public bool? Explain
        {
            get => Q<bool?>("explain");
            set => Q("explain", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }

        /// <summary>Limits the information returned to the specified metrics.</summary>
        public ClusterRerouteMetric? Metric
        {
            get => Q<ClusterRerouteMetric?>("metric");
            set => Q("metric", value);
        }

        /// <summary>When `true`, retries shard allocation if it was blocked because of too many subsequent failures.</summary>
        public bool? RetryFailed
        {
            get => Q<bool?>("retry_failed");
            set => Q("retry_failed", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }
    }

    /// <summary>Request options for State <para>https://opensearch.org/docs/latest</para></summary>
    public partial class ClusterStateRequestParameters
        : RequestParameters<ClusterStateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>
        /// Whether to ignore a wildcard index expression that resolves into no concrete indexes. This includes the `_all` string or when no indexes
        /// have been specified.
        /// </summary>
        public bool? AllowNoIndices
        {
            get => Q<bool?>("allow_no_indices");
            set => Q("allow_no_indices", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        /// <summary>Specifies the type of index that wildcard expressions can match. Supports comma-separated values.</summary>
        public ExpandWildcards? ExpandWildcards
        {
            get => Q<ExpandWildcards?>("expand_wildcards");
            set => Q("expand_wildcards", value);
        }

        /// <summary>Returns settings in a flat format.</summary>
        public bool? FlatSettings
        {
            get => Q<bool?>("flat_settings");
            set => Q("flat_settings", value);
        }

        /// <summary>Whether the specified concrete indexes should be ignored when unavailable (missing or closed).</summary>
        public bool? IgnoreUnavailable
        {
            get => Q<bool?>("ignore_unavailable");
            set => Q("ignore_unavailable", value);
        }

        /// <summary>Whether to return information from the local node only instead of from the cluster manager node.</summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        /// <summary>
        /// A duration. Units can be `nanos`, `micros`, `ms` (milliseconds), `s` (seconds), `m` (minutes), `h` (hours) and `d` (days). Also accepts
        /// "0" without a unit and "-1" to indicate an unspecified value.
        /// </summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use `cluster_manager_timeout` instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }

        /// <summary>Wait for the metadata version to be equal or greater than the specified metadata version.</summary>
        public long? WaitForMetadataVersion
        {
            get => Q<long?>("wait_for_metadata_version");
            set => Q("wait_for_metadata_version", value);
        }

        /// <summary>The maximum time to wait for `wait_for_metadata_version` before timing out.</summary>
        public TimeSpan WaitForTimeout
        {
            get => Q<TimeSpan>("wait_for_timeout");
            set => Q("wait_for_timeout", value);
        }
    }

    /// <summary>Request options for Stats <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-stats/</para></summary>
    public partial class ClusterStatsRequestParameters
        : RequestParameters<ClusterStatsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>When `true`, returns settings in a flat format.</summary>
        public bool? FlatSettings
        {
            get => Q<bool?>("flat_settings");
            set => Q("flat_settings", value);
        }

        /// <summary>
        /// The amount of time to wait for each node to respond. If a node does not respond before its timeout expires, the response does not include
        /// its stats. However, timed out nodes are included in the response's `_nodes.failed` property. Defaults to no timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }
    }
}
