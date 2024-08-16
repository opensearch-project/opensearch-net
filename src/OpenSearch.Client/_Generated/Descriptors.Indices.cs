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
using OpenSearch.Net;
using OpenSearch.Net.Specification.IndicesApi;
using OpenSearch.Net.Utf8Json;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
    /// <summary>Descriptor for DeleteComposableTemplate <para>https://opensearch.org/docs/latest/im-plugin/index-templates/#delete-a-template</para></summary>
    public partial class DeleteComposableIndexTemplateDescriptor
         : RequestDescriptorBase<DeleteComposableIndexTemplateDescriptor, DeleteComposableIndexTemplateRequestParameters, IDeleteComposableIndexTemplateRequest>,
            IDeleteComposableIndexTemplateRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesDeleteComposableTemplate;
        /// <summary>/_index_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public DeleteComposableIndexTemplateDescriptor(Name name)
            : base(r => r.Required("name", name)) { }
        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected DeleteComposableIndexTemplateDescriptor()
            : base() { }
        // values part of the url path
        Name IDeleteComposableIndexTemplateRequest.Name => Self.RouteValues.Get<Name>("name");
        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public DeleteComposableIndexTemplateDescriptor ClusterManagerTimeout(Time clustermanagertimeout) => Qs("cluster_manager_timeout", clustermanagertimeout);
        /// <summary>Period to wait for a connection to the master node. If no response is received before the timeout expires, the request fails and returns an error.</summary>
        [Obsolete("Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead.")]
        public DeleteComposableIndexTemplateDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
        /// <summary>Period to wait for a response. If no response is received before the timeout expires, the request fails and returns an error.</summary>
        public DeleteComposableIndexTemplateDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
    }

    /// <summary>Descriptor for ComposableTemplateExists <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
    public partial class ComposableIndexTemplateExistsDescriptor
         : RequestDescriptorBase<ComposableIndexTemplateExistsDescriptor, ComposableIndexTemplateExistsRequestParameters, IComposableIndexTemplateExistsRequest>,
            IComposableIndexTemplateExistsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesComposableTemplateExists;
        /// <summary>/_index_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public ComposableIndexTemplateExistsDescriptor(Name name)
            : base(r => r.Required("name", name)) { }
        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected ComposableIndexTemplateExistsDescriptor()
            : base() { }
        // values part of the url path
        Name IComposableIndexTemplateExistsRequest.Name => Self.RouteValues.Get<Name>("name");
        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public ComposableIndexTemplateExistsDescriptor ClusterManagerTimeout(Time clustermanagertimeout) => Qs("cluster_manager_timeout", clustermanagertimeout);
        /// <summary>Return settings in flat format.</summary>
        public ComposableIndexTemplateExistsDescriptor FlatSettings(bool? flatsettings = true) => Qs("flat_settings", flatsettings);
        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public ComposableIndexTemplateExistsDescriptor Local(bool? local = true) => Qs("local", local);
        /// <summary>Period to wait for a connection to the master node. If no response is received before the timeout expires, the request fails and returns an error.</summary>
        [Obsolete("Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead.")]
        public ComposableIndexTemplateExistsDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
    }

    /// <summary>Descriptor for GetComposableTemplate <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
    public partial class GetComposableIndexTemplateDescriptor
         : RequestDescriptorBase<GetComposableIndexTemplateDescriptor, GetComposableIndexTemplateRequestParameters, IGetComposableIndexTemplateRequest>,
            IGetComposableIndexTemplateRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesGetComposableTemplate;
        /// <summary>/_index_template</summary>
        public GetComposableIndexTemplateDescriptor()
            : base() { }
        /// <summary>/_index_template/{name}</summary>
        /// <param name="name">Optional, accepts null</param>
        public GetComposableIndexTemplateDescriptor(Name name)
            : base(r => r.Optional("name", name)) { }
        // values part of the url path
        Name IGetComposableIndexTemplateRequest.Name => Self.RouteValues.Get<Name>("name");

        /// <summary>Name of the index template to retrieve. Wildcard (*) expressions are supported.</summary>
        public GetComposableIndexTemplateDescriptor Name(Name name) => Assign(name, (a, v) => a.RouteValues.Optional("name", v));
        // Request parameters
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public GetComposableIndexTemplateDescriptor ClusterManagerTimeout(Time clustermanagertimeout) => Qs("cluster_manager_timeout", clustermanagertimeout);
        /// <summary>If true, returns settings in flat format.</summary>
        public GetComposableIndexTemplateDescriptor FlatSettings(bool? flatsettings = true) => Qs("flat_settings", flatsettings);
        /// <summary>If true, the request retrieves information from the local node only. Defaults to false, which means information is retrieved from the master node.</summary>
        public GetComposableIndexTemplateDescriptor Local(bool? local = true) => Qs("local", local);
        /// <summary>Period to wait for a connection to the master node. If no response is received before the timeout expires, the request fails and returns an error.</summary>
        [Obsolete("Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead.")]
        public GetComposableIndexTemplateDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
    }

    /// <summary>Descriptor for PutComposableTemplate <para>https://opensearch.org/docs/latest/im-plugin/index-templates/</para></summary>
    public partial class PutComposableIndexTemplateDescriptor
         : RequestDescriptorBase<PutComposableIndexTemplateDescriptor, PutComposableIndexTemplateRequestParameters, IPutComposableIndexTemplateRequest>,
            IPutComposableIndexTemplateRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesPutComposableTemplate;
        /// <summary>/_index_template/{name}</summary>
        /// <param name="name">this parameter is required</param>
        public PutComposableIndexTemplateDescriptor(Name name)
            : base(r => r.Required("name", name)) { }
        /// <summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
        [SerializationConstructor]
        protected PutComposableIndexTemplateDescriptor()
            : base() { }
        // values part of the url path
        Name IPutComposableIndexTemplateRequest.Name => Self.RouteValues.Get<Name>("name");
        // Request parameters
        /// <summary>User defined reason for creating/updating the index template.</summary>
        public PutComposableIndexTemplateDescriptor Cause(string cause) => Qs("cause", cause);
        /// <summary>Operation timeout for connection to cluster-manager node.</summary>
        /// <remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public PutComposableIndexTemplateDescriptor ClusterManagerTimeout(Time clustermanagertimeout) => Qs("cluster_manager_timeout", clustermanagertimeout);
        /// <summary>If `true`, this request cannot replace or update existing index templates.</summary>
        public PutComposableIndexTemplateDescriptor Create(bool? create = true) => Qs("create", create);
        /// <summary>Operation timeout for connection to master node.</summary>
        [Obsolete("Deprecated as of: 2.0.0, reason: To promote inclusive language, use 'cluster_manager_timeout' instead.")]
        public PutComposableIndexTemplateDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
    }

    /// <summary>Descriptor for Stats <para>https://opensearch.org/docs/latest</para></summary>
    public partial class IndicesStatsDescriptor
         : RequestDescriptorBase<IndicesStatsDescriptor, IndicesStatsRequestParameters, IIndicesStatsRequest>,
            IIndicesStatsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesStats;
        /// <summary>/_stats</summary>
        public IndicesStatsDescriptor()
            : base() { }
        /// <summary>/{index}/_stats</summary>
        /// <param name="index">Optional, accepts null</param>
        public IndicesStatsDescriptor(Indices index)
            : base(r => r.Optional("index", index)) { }
        /// <summary>/{index}/_stats/{metric}</summary>
        /// <param name="index">Optional, accepts null</param>
        /// <param name="metric">Optional, accepts null</param>
        public IndicesStatsDescriptor(Indices index, Metrics metric)
            : base(r => r.Optional("index", index).Optional("metric", metric)) { }
        /// <summary>/_stats/{metric}</summary>
        /// <param name="metric">Optional, accepts null</param>
        public IndicesStatsDescriptor(Metrics metric)
            : base(r => r.Optional("metric", metric)) { }
        // values part of the url path
        Indices IIndicesStatsRequest.Index => Self.RouteValues.Get<Indices>("index");
        Metrics IIndicesStatsRequest.Metric => Self.RouteValues.Get<Metrics>("metric");

        /// <summary>A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices.</summary>
        public IndicesStatsDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));

        /// <summary>a shortcut into calling Index(typeof(TOther))</summary>
        public IndicesStatsDescriptor Index<TOther>() where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));

        /// <summary>A shortcut into calling Index(Indices.All)</summary>
        public IndicesStatsDescriptor AllIndices() => Index(Indices.All);

        /// <summary>Limit the information returned the specific metrics.</summary>
        public IndicesStatsDescriptor Metric(Metrics metric) => Assign(metric, (a, v) => a.RouteValues.Optional("metric", v));
        // Request parameters
        /// <summary>Comma-separated list or wildcard expressions of fields to include in fielddata and suggest statistics.</summary>
        public IndicesStatsDescriptor CompletionFields(Fields completionfields) => Qs("completion_fields", completionfields);

        /// <summary>Comma-separated list or wildcard expressions of fields to include in fielddata and suggest statistics.</summary>
        public IndicesStatsDescriptor CompletionFields<T>(params Expression<Func<T, object>>[] fields) where T : class => Qs("completion_fields", fields?.Select(e => (Field)e));
        /// <summary>Type of index that wildcard patterns can match. If the request can target data streams, this argument determines whether wildcard expressions match hidden data streams. Supports comma-separated values, such as `open,hidden`.</summary>
        public IndicesStatsDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
        /// <summary>Comma-separated list or wildcard expressions of fields to include in fielddata statistics.</summary>
        public IndicesStatsDescriptor FielddataFields(Fields fielddatafields) => Qs("fielddata_fields", fielddatafields);

        /// <summary>Comma-separated list or wildcard expressions of fields to include in fielddata statistics.</summary>
        public IndicesStatsDescriptor FielddataFields<T>(params Expression<Func<T, object>>[] fields) where T : class => Qs("fielddata_fields", fields?.Select(e => (Field)e));
        /// <summary>Comma-separated list or wildcard expressions of fields to include in the statistics.</summary>
        public IndicesStatsDescriptor Fields(Fields fields) => Qs("fields", fields);

        /// <summary>Comma-separated list or wildcard expressions of fields to include in the statistics.</summary>
        public IndicesStatsDescriptor Fields<T>(params Expression<Func<T, object>>[] fields) where T : class => Qs("fields", fields?.Select(e => (Field)e));
        /// <summary>If true, statistics are not collected from closed indices.</summary>
        public IndicesStatsDescriptor ForbidClosedIndices(bool? forbidclosedindices = true) => Qs("forbid_closed_indices", forbidclosedindices);
        /// <summary>Comma-separated list of search groups to include in the search statistics.</summary>
        public IndicesStatsDescriptor Groups(params string[] groups) => Qs("groups", groups);
        /// <summary>If true, the call reports the aggregated disk usage of each one of the Lucene index files (only applies if segment stats are requested).</summary>
        public IndicesStatsDescriptor IncludeSegmentFileSizes(bool? includesegmentfilesizes = true) => Qs("include_segment_file_sizes", includesegmentfilesizes);
        /// <summary>If true, the response includes information from segments that are not loaded into memory.</summary>
        public IndicesStatsDescriptor IncludeUnloadedSegments(bool? includeunloadedsegments = true) => Qs("include_unloaded_segments", includeunloadedsegments);
        /// <summary>Indicates whether statistics are aggregated at the cluster, index, or shard level.</summary>
        public IndicesStatsDescriptor Level(Level? level) => Qs("level", level);
    }

}
