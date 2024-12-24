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
namespace OpenSearch.Net.Specification.RollupsApi
{
    /// <summary>Request options for Delete <para>https://opensearch.org/docs/latest/im-plugin/index-rollups/rollup-api/#delete-an-index-rollup-job</para></summary>
    public partial class DeleteRequestParameters : RequestParameters<DeleteRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for Explain <para>https://opensearch.org/docs/latest/im-plugin/index-rollups/rollup-api/#explain-an-index-rollup-job</para></summary>
    public partial class ExplainRequestParameters : RequestParameters<ExplainRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for Get <para>https://opensearch.org/docs/latest/im-plugin/index-rollups/rollup-api/#get-an-index-rollup-job</para></summary>
    public partial class GetRequestParameters : RequestParameters<GetRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for Put <para>https://opensearch.org/docs/latest/im-plugin/index-rollups/rollup-api/#create-or-update-an-index-rollup-job</para></summary>
    public partial class PutRequestParameters : RequestParameters<PutRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;

        /// <summary>Only perform the operation if the document has this primary term.</summary>
        public long? IfPrimaryTerm
        {
            get => Q<long?>("if_primary_term");
            set => Q("if_primary_term", value);
        }

        /// <summary>Only perform the operation if the document has this sequence number.</summary>
        public long? IfSequenceNumber
        {
            get => Q<long?>("if_seq_no");
            set => Q("if_seq_no", value);
        }
    }

    /// <summary>Request options for Start <para>https://opensearch.org/docs/latest/im-plugin/index-rollups/rollup-api/#start-or-stop-an-index-rollup-job</para></summary>
    public partial class StartRequestParameters : RequestParameters<StartRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for Stop <para>https://opensearch.org/docs/latest/im-plugin/index-rollups/rollup-api/#start-or-stop-an-index-rollup-job</para></summary>
    public partial class StopRequestParameters : RequestParameters<StopRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => false;
    }
}