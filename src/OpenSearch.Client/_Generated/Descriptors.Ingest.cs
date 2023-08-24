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
using OpenSearch.Net.Specification.IngestApi;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client.Specification.IngestApi
{
    ///<summary>Descriptor for DeletePipeline <para>https://opensearch.org/docs/latest/api-reference/ingest-apis/delete-ingest/</para></summary>
    public partial class DeletePipelineDescriptor
        : RequestDescriptorBase<
            DeletePipelineDescriptor,
            DeletePipelineRequestParameters,
            IDeletePipelineRequest
        >,
            IDeletePipelineRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IngestDeletePipeline;

        ///<summary>/_ingest/pipeline/{id}</summary>
        ///<param name="id">this parameter is required</param>
        public DeletePipelineDescriptor(Id id)
            : base(r => r.Required("id", id)) { }

        ///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected DeletePipelineDescriptor()
            : base() { }

        // values part of the url path
        Id IDeletePipelineRequest.Id => Self.RouteValues.Get<Id>("id");

        // Request parameters
        ///<summary>Operation timeout for connection to cluster-manager node.</summary>
        ///<remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public DeletePipelineDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        ///<summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public DeletePipelineDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);

        ///<summary>Operation timeout.</summary>
        public DeletePipelineDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
    }

    ///<summary>Descriptor for GetPipeline <para>https://opensearch.org/docs/latest/api-reference/ingest-apis/get-ingest/</para></summary>
    public partial class GetPipelineDescriptor
        : RequestDescriptorBase<
            GetPipelineDescriptor,
            GetPipelineRequestParameters,
            IGetPipelineRequest
        >,
            IGetPipelineRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IngestGetPipeline;

        ///<summary>/_ingest/pipeline</summary>
        public GetPipelineDescriptor()
            : base() { }

        ///<summary>/_ingest/pipeline/{id}</summary>
        ///<param name="id">Optional, accepts null</param>
        public GetPipelineDescriptor(Id id)
            : base(r => r.Optional("id", id)) { }

        // values part of the url path
        Id IGetPipelineRequest.Id => Self.RouteValues.Get<Id>("id");

        ///<summary>Comma-separated list of pipeline ids. Wildcards supported.</summary>
        public GetPipelineDescriptor Id(Id id) =>
            Assign(id, (a, v) => a.RouteValues.Optional("id", v));

        // Request parameters
        ///<summary>Operation timeout for connection to cluster-manager node.</summary>
        ///<remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public GetPipelineDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        ///<summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public GetPipelineDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);
    }

    ///<summary>Descriptor for GrokProcessorPatterns <para>https://opensearch.org/docs/latest</para></summary>
    public partial class GrokProcessorPatternsDescriptor
        : RequestDescriptorBase<
            GrokProcessorPatternsDescriptor,
            GrokProcessorPatternsRequestParameters,
            IGrokProcessorPatternsRequest
        >,
            IGrokProcessorPatternsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IngestGrokProcessorPatterns;
        // values part of the url path
        // Request parameters
    }

    ///<summary>Descriptor for PutPipeline <para>https://opensearch.org/docs/latest/api-reference/ingest-apis/create-update-ingest/</para></summary>
    public partial class PutPipelineDescriptor
        : RequestDescriptorBase<
            PutPipelineDescriptor,
            PutPipelineRequestParameters,
            IPutPipelineRequest
        >,
            IPutPipelineRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IngestPutPipeline;

        ///<summary>/_ingest/pipeline/{id}</summary>
        ///<param name="id">this parameter is required</param>
        public PutPipelineDescriptor(Id id)
            : base(r => r.Required("id", id)) { }

        ///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected PutPipelineDescriptor()
            : base() { }

        // values part of the url path
        Id IPutPipelineRequest.Id => Self.RouteValues.Get<Id>("id");

        // Request parameters
        ///<summary>Operation timeout for connection to cluster-manager node.</summary>
        ///<remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public PutPipelineDescriptor ClusterManagerTimeout(Time clustermanagertimeout) =>
            Qs("cluster_manager_timeout", clustermanagertimeout);

        ///<summary>Operation timeout for connection to master node.</summary>
        [Obsolete(
            "Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead."
        )]
        public PutPipelineDescriptor MasterTimeout(Time mastertimeout) =>
            Qs("master_timeout", mastertimeout);

        ///<summary>Operation timeout.</summary>
        public PutPipelineDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
    }

    ///<summary>Descriptor for SimulatePipeline <para>https://opensearch.org/docs/latest/api-reference/ingest-apis/simulate-ingest/</para></summary>
    public partial class SimulatePipelineDescriptor
        : RequestDescriptorBase<
            SimulatePipelineDescriptor,
            SimulatePipelineRequestParameters,
            ISimulatePipelineRequest
        >,
            ISimulatePipelineRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IngestSimulatePipeline;

        ///<summary>/_ingest/pipeline/_simulate</summary>
        public SimulatePipelineDescriptor()
            : base() { }

        ///<summary>/_ingest/pipeline/{id}/_simulate</summary>
        ///<param name="id">Optional, accepts null</param>
        public SimulatePipelineDescriptor(Id id)
            : base(r => r.Optional("id", id)) { }

        // values part of the url path
        Id ISimulatePipelineRequest.Id => Self.RouteValues.Get<Id>("id");

        ///<summary>Pipeline ID.</summary>
        public SimulatePipelineDescriptor Id(Id id) =>
            Assign(id, (a, v) => a.RouteValues.Optional("id", v));

        // Request parameters
        ///<summary>Verbose mode. Display data output for each processor in executed pipeline.</summary>
        public SimulatePipelineDescriptor Verbose(bool? verbose = true) => Qs("verbose", verbose);
    }
}
