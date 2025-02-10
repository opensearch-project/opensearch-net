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
namespace OpenSearch.Net.Specification.SmApi
{
    /// <summary>Request options for CreatePolicy</summary>
    public partial class CreatePolicyRequestParameters
        : RequestParameters<CreatePolicyRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for DeletePolicy</summary>
    public partial class DeletePolicyRequestParameters
        : RequestParameters<DeletePolicyRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for ExplainPolicy</summary>
    public partial class ExplainPolicyRequestParameters
        : RequestParameters<ExplainPolicyRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetPolicies</summary>
    public partial class GetPoliciesRequestParameters
        : RequestParameters<GetPoliciesRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>The starting index from which to retrieve snapshot management policies.</summary>
        public int? From
        {
            get => Q<int?>("from");
            set => Q("from", value);
        }

        /// <summary>The query string to filter the returned snapshot management policies.</summary>
        public string QueryStringParam
        {
            get => Q<string>("queryString");
            set => Q("queryString", value);
        }

        /// <summary>The number of snapshot management policies to return.</summary>
        public int? Size
        {
            get => Q<int?>("size");
            set => Q("size", value);
        }

        /// <summary>The name of the field to sort the snapshot management policies by.</summary>
        public string SortField
        {
            get => Q<string>("sortField");
            set => Q("sortField", value);
        }

        /// <summary>The order to sort the snapshot management policies.</summary>
        public string SortOrder
        {
            get => Q<string>("sortOrder");
            set => Q("sortOrder", value);
        }
    }

    /// <summary>Request options for GetPolicy</summary>
    public partial class GetPolicyRequestParameters : RequestParameters<GetPolicyRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for StartPolicy</summary>
    public partial class StartPolicyRequestParameters
        : RequestParameters<StartPolicyRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for StopPolicy</summary>
    public partial class StopPolicyRequestParameters
        : RequestParameters<StopPolicyRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for UpdatePolicy</summary>
    public partial class UpdatePolicyRequestParameters
        : RequestParameters<UpdatePolicyRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;

        /// <summary>The primary term of the policy to update.</summary>
        public int? IfPrimaryTerm
        {
            get => Q<int?>("if_primary_term");
            set => Q("if_primary_term", value);
        }

        /// <summary>The sequence number of the policy to update.</summary>
        public int? IfSequenceNumber
        {
            get => Q<int?>("if_seq_no");
            set => Q("if_seq_no", value);
        }
    }
}
