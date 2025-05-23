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
namespace OpenSearch.Net.Specification.ObservabilityApi
{
    /// <summary>Request options for CreateObject</summary>
    public partial class CreateObjectRequestParameters
        : RequestParameters<CreateObjectRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
        public override bool SupportsBody => true;
    }

    /// <summary>Request options for DeleteObject</summary>
    public partial class DeleteObjectRequestParameters
        : RequestParameters<DeleteObjectRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for DeleteObjects</summary>
    public partial class DeleteObjectsRequestParameters
        : RequestParameters<DeleteObjectsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
        public override bool SupportsBody => false;

        /// <summary>The ID of a single observability object to delete.</summary>
        public string ObjectId
        {
            get => Q<string>("objectId");
            set => Q("objectId", value);
        }

        /// <summary>A comma-separated list of observability object IDs to delete.</summary>
        public string ObjectIdList
        {
            get => Q<string>("objectIdList");
            set => Q("objectIdList", value);
        }
    }

    /// <summary>Request options for GetLocalstats</summary>
    public partial class GetLocalstatsRequestParameters
        : RequestParameters<GetLocalstatsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for GetObject</summary>
    public partial class GetObjectRequestParameters : RequestParameters<GetObjectRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for ListObjects</summary>
    public partial class ListObjectsRequestParameters
        : RequestParameters<ListObjectsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;
    }

    /// <summary>Request options for UpdateObject</summary>
    public partial class UpdateObjectRequestParameters
        : RequestParameters<UpdateObjectRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
        public override bool SupportsBody => true;
    }
}
