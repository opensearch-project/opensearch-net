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
namespace OpenSearch.Net.Specification.FlowFrameworkApi
{
    /// <summary>Request options for Create <para>https://opensearch.org/docs/latest/automating-configurations/api/create-workflow/</para></summary>
    public partial class CreateRequestParameters : RequestParameters<CreateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
        public bool? Provision
        {
            get => Q<bool?>("provision");
            set => Q("provision", value);
        }

        /// <remarks>Supported by OpenSearch servers of version 2.17.0 or greater.</remarks>
        public bool? Reprovision
        {
            get => Q<bool?>("reprovision");
            set => Q("reprovision", value);
        }
        public bool? UpdateFields
        {
            get => Q<bool?>("update_fields");
            set => Q("update_fields", value);
        }

        /// <summary>To use a workflow template, specify it in the use_case query parameter when creating a workflow.</summary>
        public string UseCase
        {
            get => Q<string>("use_case");
            set => Q("use_case", value);
        }
        public string Validation
        {
            get => Q<string>("validation");
            set => Q("validation", value);
        }
    }

    /// <summary>Request options for Delete <para>https://opensearch.org/docs/latest/automating-configurations/api/delete-workflow/</para></summary>
    public partial class DeleteRequestParameters : RequestParameters<DeleteRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
        public bool? ClearStatus
        {
            get => Q<bool?>("clear_status");
            set => Q("clear_status", value);
        }
    }

    /// <summary>Request options for Deprovision <para>https://opensearch.org/docs/latest/automating-configurations/api/deprovision-workflow/</para></summary>
    public partial class DeprovisionRequestParameters
        : RequestParameters<DeprovisionRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => false;
        public string AllowDelete
        {
            get => Q<string>("allow_delete");
            set => Q("allow_delete", value);
        }
    }

    /// <summary>Request options for Get <para>https://opensearch.org/docs/latest/automating-configurations/api/get-workflow/</para></summary>
    public partial class GetRequestParameters : RequestParameters<GetRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetStatus <para>https://opensearch.org/docs/latest/automating-configurations/api/get-workflow-status/</para></summary>
    public partial class GetStatusRequestParameters : RequestParameters<GetStatusRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>The all parameter specifies whether the response should return all fields.</summary>
        public bool? All
        {
            get => Q<bool?>("all");
            set => Q("all", value);
        }
    }

    /// <summary>Request options for GetSteps <para>https://opensearch.org/docs/latest/automating-configurations/api/get-workflow-steps/</para></summary>
    public partial class GetStepsRequestParameters : RequestParameters<GetStepsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
        public string WorkflowStep
        {
            get => Q<string>("workflow_step");
            set => Q("workflow_step", value);
        }
    }

    /// <summary>Request options for Provision <para>https://opensearch.org/docs/latest/automating-configurations/api/provision-workflow/</para></summary>
    public partial class ProvisionRequestParameters : RequestParameters<ProvisionRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for Search <para>https://opensearch.org/docs/latest/automating-configurations/api/provision-workflow/</para></summary>
    public partial class SearchRequestParameters : RequestParameters<SearchRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for SearchState <para>https://opensearch.org/docs/latest/automating-configurations/api/search-workflow-state/</para></summary>
    public partial class SearchStateRequestParameters
        : RequestParameters<SearchStateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for Update <para>https://opensearch.org/docs/latest/automating-configurations/api/create-workflow/</para></summary>
    public partial class UpdateRequestParameters : RequestParameters<UpdateRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;
        public bool? Provision
        {
            get => Q<bool?>("provision");
            set => Q("provision", value);
        }

        /// <remarks>Supported by OpenSearch servers of version 2.17.0 or greater.</remarks>
        public bool? Reprovision
        {
            get => Q<bool?>("reprovision");
            set => Q("reprovision", value);
        }
        public bool? UpdateFields
        {
            get => Q<bool?>("update_fields");
            set => Q("update_fields", value);
        }

        /// <summary>To use a workflow template, specify it in the use_case query parameter when creating a workflow.</summary>
        public string UseCase
        {
            get => Q<string>("use_case");
            set => Q("use_case", value);
        }
        public string Validation
        {
            get => Q<string>("validation");
            set => Q("validation", value);
        }
    }
}
