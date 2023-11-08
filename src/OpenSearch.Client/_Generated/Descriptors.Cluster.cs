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
using System.Text;
using System.Linq.Expressions;

using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Specification.ClusterApi;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
    /// <summary>Descriptor for AllocationExplain <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/</para></summary>
    public partial class ClusterAllocationExplainDescriptor
        : RequestDescriptorBase<
            ClusterAllocationExplainDescriptor,
            ClusterAllocationExplainRequestParameters,
            IClusterAllocationExplainRequest
        >,
            IClusterAllocationExplainRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterAllocationExplain;

        // values part of the url path
        // Request parameters
        /// <summary>Return information about disk usage and shard sizes.</summary>
        public ClusterAllocationExplainDescriptor IncludeDiskInfo(bool? includediskinfo = true) =>
            Qs("include_disk_info", includediskinfo);

        /// <summary>Return 'YES' decisions in explanation.</summary>
        public ClusterAllocationExplainDescriptor IncludeYesDecisions(
            bool? includeyesdecisions = true
        ) => Qs("include_yes_decisions", includeyesdecisions);
    }

    /// <summary>Descriptor for DeleteComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class DeleteComponentTemplateDescriptor
        : RequestDescriptorBase<
            DeleteComponentTemplateDescriptor,
            DeleteComponentTemplateRequestParameters,
            IDeleteComponentTemplateRequest
        >,
            IDeleteComponentTemplateRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterDeleteComponentTemplate;

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public DeleteComponentTemplateDescriptor(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected DeleteComponentTemplateDescriptor()
            : base() { }

        // values part of the url path
        Name IDeleteComponentTemplateRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public DeleteComponentTemplateDescriptor ClusterManagerTimeout(
            Time clustermanagertimeout
        ) => Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public DeleteComponentTemplateDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);

        /// <summary>Operation timeout.</summary>
        public DeleteComponentTemplateDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
    }

    /// <summary>Descriptor for DeleteVotingConfigExclusions <para>https://opensearch.org/docs/latest</para></summary>
    public partial class DeleteVotingConfigExclusionsDescriptor
        : RequestDescriptorBase<
            DeleteVotingConfigExclusionsDescriptor,
            DeleteVotingConfigExclusionsRequestParameters,
            IDeleteVotingConfigExclusionsRequest
        >,
            IDeleteVotingConfigExclusionsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterDeleteVotingConfigExclusions;

        // values part of the url path
        // Request parameters
        /// <summary>Specifies whether to wait for all excluded nodes to be removed from the cluster before clearing the voting configuration exclusions list.</summary>
        public DeleteVotingConfigExclusionsDescriptor WaitForRemoval(bool? waitforremoval = true) =>
            Qs("wait_for_removal", waitforremoval);
    }

    /// <summary>Descriptor for ComponentTemplateExists <para>https://opensearch.org/docs/latest</para></summary>
    public partial class ComponentTemplateExistsDescriptor
        : RequestDescriptorBase<
            ComponentTemplateExistsDescriptor,
            ComponentTemplateExistsRequestParameters,
            IComponentTemplateExistsRequest
        >,
            IComponentTemplateExistsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterComponentTemplateExists;

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public ComponentTemplateExistsDescriptor(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected ComponentTemplateExistsDescriptor()
            : base() { }

        // values part of the url path
        Name IComponentTemplateExistsRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public ComponentTemplateExistsDescriptor ClusterManagerTimeout(
            Time clustermanagertimeout
        ) => Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public ComponentTemplateExistsDescriptor Local(bool? local = true) => Qs("local", local);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public ComponentTemplateExistsDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);
    }

    /// <summary>Descriptor for GetComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class GetComponentTemplateDescriptor
        : RequestDescriptorBase<
            GetComponentTemplateDescriptor,
            GetComponentTemplateRequestParameters,
            IGetComponentTemplateRequest
        >,
            IGetComponentTemplateRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterGetComponentTemplate;

        /// <summary>/_component_template</summary>
        public GetComponentTemplateDescriptor()
            : base() { }

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">Optional, accepts null</param>
        public GetComponentTemplateDescriptor(Names name)
            : base(r => r.Optional("name", name)) { }

        // values part of the url path
        Names IGetComponentTemplateRequest.Name => Self.RouteValues.Get<Names>("name");

        /// <summary>The Comma-separated names of the component templates.</summary>
        public GetComponentTemplateDescriptor Name(Names name) =>
            Assign(name, (a, v) => a.RouteValues.Optional("name", v));

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public GetComponentTemplateDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public GetComponentTemplateDescriptor Local(bool? local = true) => Qs("local", local);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public GetComponentTemplateDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);
    }

    /// <summary>Descriptor for GetSettings <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/</para></summary>
    public partial class ClusterGetSettingsDescriptor
        : RequestDescriptorBase<
            ClusterGetSettingsDescriptor,
            ClusterGetSettingsRequestParameters,
            IClusterGetSettingsRequest
        >,
            IClusterGetSettingsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterGetSettings;

        // values part of the url path
        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public ClusterGetSettingsDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Return settings in flat format.</summary>
        public ClusterGetSettingsDescriptor FlatSettings(bool? flatsettings = true) =>
            Qs("flat_settings", flatsettings);

        /// <summary>Whether to return all default clusters setting.</summary>
        public ClusterGetSettingsDescriptor IncludeDefaults(bool? includedefaults = true) =>
            Qs("include_defaults", includedefaults);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public ClusterGetSettingsDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);

        /// <summary>Operation timeout.</summary>
        public ClusterGetSettingsDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
    }

    /// <summary>Descriptor for Health <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-health/</para></summary>
    public partial class ClusterHealthDescriptor
        : RequestDescriptorBase<
            ClusterHealthDescriptor,
            ClusterHealthRequestParameters,
            IClusterHealthRequest
        >,
            IClusterHealthRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterHealth;

        /// <summary>/_cluster/health</summary>
        public ClusterHealthDescriptor()
            : base() { }

        /// <summary>/_cluster/health/{index}</summary>
        /// <param name="index">Optional, accepts null</param>
        public ClusterHealthDescriptor(Indices index)
            : base(r => r.Optional("index", index)) { }

        // values part of the url path
        Indices IClusterHealthRequest.Index => Self.RouteValues.Get<Indices>("index");

        /// <summary>Limit the information returned to specific indicies.</summary>
        public ClusterHealthDescriptor Index(Indices index) =>
            Assign(index, (a, v) => a.RouteValues.Optional("index", v));

        /// <summary>a shortcut into calling Index(typeof(TOther))</summary>
        public ClusterHealthDescriptor Index<TOther>()
            where TOther : class =>
            Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));

        /// <summary>A shortcut into calling Index(Indices.All)</summary>
        public ClusterHealthDescriptor AllIndices() => Index(Indices.All);

        // Request parameters
        /// <summary>The awareness attribute for which the health is required.</summary>
        public ClusterHealthDescriptor AwarenessAttribute(string awarenessattribute) =>
            Qs("awareness_attribute", awarenessattribute);

        /// <summary>Specify the level of detail for returned information.</summary>
        public ClusterHealthDescriptor ClusterHealthLevel(ClusterHealthLevel? clusterhealthlevel) =>
            Qs("level", clusterhealthlevel);

        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public ClusterHealthDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
        public ClusterHealthDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) =>
            Qs("expand_wildcards", expandwildcards);

        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public ClusterHealthDescriptor Local(bool? local = true) => Qs("local", local);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public ClusterHealthDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);

        /// <summary>Operation timeout.</summary>
        public ClusterHealthDescriptor Timeout(Time timeout) => Qs("timeout", timeout);

        /// <summary>Wait until the specified number of shards is active.</summary>
        public ClusterHealthDescriptor WaitForActiveShards(string waitforactiveshards) =>
            Qs("wait_for_active_shards", waitforactiveshards);

        /// <summary>Wait until all currently queued events with the given priority are processed.</summary>
        public ClusterHealthDescriptor WaitForEvents(WaitForEvents? waitforevents) =>
            Qs("wait_for_events", waitforevents);

        /// <summary>Wait until the specified number of nodes is available.</summary>
        public ClusterHealthDescriptor WaitForNodes(string waitfornodes) =>
            Qs("wait_for_nodes", waitfornodes);

        /// <summary>Whether to wait until there are no initializing shards in the cluster.</summary>
        public ClusterHealthDescriptor WaitForNoInitializingShards(
            bool? waitfornoinitializingshards = true
        ) => Qs("wait_for_no_initializing_shards", waitfornoinitializingshards);

        /// <summary>Whether to wait until there are no relocating shards in the cluster.</summary>
        public ClusterHealthDescriptor WaitForNoRelocatingShards(
            bool? waitfornorelocatingshards = true
        ) => Qs("wait_for_no_relocating_shards", waitfornorelocatingshards);

        /// <summary>Wait until cluster is in a specific state.</summary>
        public ClusterHealthDescriptor WaitForStatus(WaitForStatus? waitforstatus) =>
            Qs("wait_for_status", waitforstatus);
    }

    /// <summary>Descriptor for PendingTasks <para>https://opensearch.org/docs/latest</para></summary>
    public partial class ClusterPendingTasksDescriptor
        : RequestDescriptorBase<
            ClusterPendingTasksDescriptor,
            ClusterPendingTasksRequestParameters,
            IClusterPendingTasksRequest
        >,
            IClusterPendingTasksRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterPendingTasks;

        // values part of the url path
        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public ClusterPendingTasksDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public ClusterPendingTasksDescriptor Local(bool? local = true) => Qs("local", local);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public ClusterPendingTasksDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);
    }

    /// <summary>Descriptor for PutComponentTemplate</summary>
    public partial class PutComponentTemplateDescriptor
        : RequestDescriptorBase<
            PutComponentTemplateDescriptor,
            PutComponentTemplateRequestParameters,
            IPutComponentTemplateRequest
        >,
            IPutComponentTemplateRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterPutComponentTemplate;

        /// <summary>/_component_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public PutComponentTemplateDescriptor(Name name)
            : base(r => r.Required("name", name)) { }

        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected PutComponentTemplateDescriptor()
            : base() { }

        // values part of the url path
        Name IPutComponentTemplateRequest.Name => Self.RouteValues.Get<Name>("name");

        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public PutComponentTemplateDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        /// <summary>Whether the index template should only be added if new or can also replace an existing one.</summary>
        public PutComponentTemplateDescriptor Create(bool? create = true) => Qs("create", create);

        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public PutComponentTemplateDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);

        /// <summary>Operation timeout.</summary>
        public PutComponentTemplateDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
    }
}
