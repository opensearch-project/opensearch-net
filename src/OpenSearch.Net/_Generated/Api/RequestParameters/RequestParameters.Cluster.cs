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

// ReSharper disable once CheckNamespace
namespace OpenSearch.Net.Specification.ClusterApi
{
    ///<summary>Request options for AllocationExplain <para>https://opensearch.org/docs/latest/api-reference/cluster-api/cluster-allocation/</para></summary>
    public partial class ClusterAllocationExplainRequestParameters
        : RequestParameters<ClusterAllocationExplainRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;

        ///<summary>Return information about disk usage and shard sizes.</summary>
        public bool? IncludeDiskInfo
        {
            get => Q<bool?>("include_disk_info");
            set => Q("include_disk_info", value);
        }

        ///<summary>Return 'YES' decisions in explanation.</summary>
        public bool? IncludeYesDecisions
        {
            get => Q<bool?>("include_yes_decisions");
            set => Q("include_yes_decisions", value);
        }
    }

    ///<summary>Request options for DeleteComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class DeleteComponentTemplateRequestParameters
        : RequestParameters<DeleteComponentTemplateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;

        ///<summary>Operation timeout for connection to cluster-manager node.</summary>
        ///<remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        ///<summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }

        ///<summary>Operation timeout.</summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }
    }

    ///<summary>Request options for DeleteVotingConfigExclusions <para>https://opensearch.org/docs/latest</para></summary>
    public partial class DeleteVotingConfigExclusionsRequestParameters
        : RequestParameters<DeleteVotingConfigExclusionsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;

        ///<summary>Specifies whether to wait for all excluded nodes to be removed from the cluster before clearing the voting configuration exclusions list.</summary>
        public bool? WaitForRemoval
        {
            get => Q<bool?>("wait_for_removal");
            set => Q("wait_for_removal", value);
        }
    }

    ///<summary>Request options for ExistsComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class ExistsComponentTemplateRequestParameters
        : RequestParameters<ExistsComponentTemplateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.HEAD;
        public override bool SupportsBody => false;

        ///<summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        ///<summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }
    }

    ///<summary>Request options for GetComponentTemplate <para>https://opensearch.org/docs/latest</para></summary>
    public partial class GetComponentTemplateRequestParameters
        : RequestParameters<GetComponentTemplateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        ///<summary>Operation timeout for connection to cluster-manager node.</summary>
        ///<remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public TimeSpan ClusterManagerTimeout
        {
            get => Q<TimeSpan>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        ///<summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
        }

        ///<summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public TimeSpan MasterTimeout
        {
            get => Q<TimeSpan>("master_timeout");
            set => Q("master_timeout", value);
        }
    }
}
