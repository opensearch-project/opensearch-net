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
namespace OpenSearch.Client.Specification.ClusterApi
{
    ///<summary>Descriptor for AllocationExplain <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/</para></summary>
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
        ///<summary>Return information about disk usage and shard sizes.</summary>
        public ClusterAllocationExplainDescriptor IncludeDiskInfo(bool? includediskinfo = true) =>
            Qs("include_disk_info", includediskinfo);

        ///<summary>Return 'YES' decisions in explanation.</summary>
        public ClusterAllocationExplainDescriptor IncludeYesDecisions(
            bool? includeyesdecisions = true
        ) => Qs("include_yes_decisions", includeyesdecisions);
    }

    ///<summary>Descriptor for DeleteVotingConfigExclusions <para>https://opensearch.org/docs/latest</para></summary>
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
        ///<summary>Specifies whether to wait for all excluded nodes to be removed from the cluster before clearing the voting configuration exclusions list.</summary>
        public DeleteVotingConfigExclusionsDescriptor WaitForRemoval(bool? waitforremoval = true) =>
            Qs("wait_for_removal", waitforremoval);
    }

    ///<summary>Descriptor for GetSettings <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-settings/</para></summary>
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
        ///<summary>Operation timeout for connection to cluster-manager node.</summary>
        ///<remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public ClusterGetSettingsDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        ///<summary>Return settings in flat format.</summary>
        public ClusterGetSettingsDescriptor FlatSettings(bool? flatsettings = true) =>
            Qs("flat_settings", flatsettings);

        ///<summary>Whether to return all default clusters setting.</summary>
        public ClusterGetSettingsDescriptor IncludeDefaults(bool? includedefaults = true) =>
            Qs("include_defaults", includedefaults);

        ///<summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public ClusterGetSettingsDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);

        ///<summary>Operation timeout.</summary>
        public ClusterGetSettingsDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
    }
}
