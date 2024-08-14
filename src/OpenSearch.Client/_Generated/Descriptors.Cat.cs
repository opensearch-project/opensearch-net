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
using OpenSearch.Net.Specification.CatApi;
using OpenSearch.Net.Utf8Json;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
    /// <summary>Descriptor for AllPitSegments <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/</para></summary>
    public partial class CatAllPitSegmentsDescriptor
        : RequestDescriptorBase<
            CatAllPitSegmentsDescriptor,
            CatAllPitSegmentsRequestParameters,
            ICatAllPitSegmentsRequest
        >,
            ICatAllPitSegmentsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.CatAllPitSegments;

        // values part of the url path
        // Request parameters
        /// <summary>The unit in which to display byte values.</summary>
        public CatAllPitSegmentsDescriptor Bytes(Bytes? bytes) => Qs("bytes", bytes);

        /// <summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public CatAllPitSegmentsDescriptor Format(string format) => Qs("format", format);

        /// <summary>Comma-separated list of column names to display.</summary>
        public CatAllPitSegmentsDescriptor Headers(params string[] headers) => Qs("h", headers);

        /// <summary>Return help information.</summary>
        public CatAllPitSegmentsDescriptor Help(bool? help = true) => Qs("help", help);

        /// <summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public CatAllPitSegmentsDescriptor SortByColumns(params string[] sortbycolumns) =>
            Qs("s", sortbycolumns);

        /// <summary>Verbose mode. Display column headers.</summary>
        public CatAllPitSegmentsDescriptor Verbose(bool? verbose = true) => Qs("v", verbose);
    }

    /// <summary>Descriptor for PitSegments <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/</para></summary>
    public partial class CatPitSegmentsDescriptor
        : RequestDescriptorBase<
            CatPitSegmentsDescriptor,
            CatPitSegmentsRequestParameters,
            ICatPitSegmentsRequest
        >,
            ICatPitSegmentsRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.CatPitSegments;

        // values part of the url path
        // Request parameters
        /// <summary>The unit in which to display byte values.</summary>
        public CatPitSegmentsDescriptor Bytes(Bytes? bytes) => Qs("bytes", bytes);

        /// <summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public CatPitSegmentsDescriptor Format(string format) => Qs("format", format);

        /// <summary>Comma-separated list of column names to display.</summary>
        public CatPitSegmentsDescriptor Headers(params string[] headers) => Qs("h", headers);

        /// <summary>Return help information.</summary>
        public CatPitSegmentsDescriptor Help(bool? help = true) => Qs("help", help);

        /// <summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public CatPitSegmentsDescriptor SortByColumns(params string[] sortbycolumns) =>
            Qs("s", sortbycolumns);

        /// <summary>Verbose mode. Display column headers.</summary>
        public CatPitSegmentsDescriptor Verbose(bool? verbose = true) => Qs("v", verbose);
    }

    /// <summary>Descriptor for SegmentReplication <para>https://opensearch.org/docs/latest/api-reference/cat/cat-segment-replication/</para></summary>
    public partial class CatSegmentReplicationDescriptor
        : RequestDescriptorBase<
            CatSegmentReplicationDescriptor,
            CatSegmentReplicationRequestParameters,
            ICatSegmentReplicationRequest
        >,
            ICatSegmentReplicationRequest
    {
        internal override ApiUrls ApiUrls => ApiUrlsLookups.CatSegmentReplication;

        /// <summary>/_cat/segment_replication</summary>
        public CatSegmentReplicationDescriptor()
            : base() { }

        /// <summary>/_cat/segment_replication/{index}</summary>
        /// <param name="index">Optional, accepts null</param>
        public CatSegmentReplicationDescriptor(Indices index)
            : base(r => r.Optional("index", index)) { }

        // values part of the url path
        Indices ICatSegmentReplicationRequest.Index => Self.RouteValues.Get<Indices>("index");

        /// <summary>Comma-separated list or wildcard expression of index names to limit the returned information.</summary>
        public CatSegmentReplicationDescriptor Index(Indices index) =>
            Assign(index, (a, v) => a.RouteValues.Optional("index", v));

        /// <summary>a shortcut into calling Index(typeof(TOther))</summary>
        public CatSegmentReplicationDescriptor Index<TOther>()
            where TOther : class =>
            Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));

        /// <summary>A shortcut into calling Index(Indices.All)</summary>
        public CatSegmentReplicationDescriptor AllIndices() => Index(Indices.All);

        // Request parameters
        /// <summary>If `true`, the response only includes ongoing segment replication events.</summary>
        public CatSegmentReplicationDescriptor ActiveOnly(bool? activeonly = true) =>
            Qs("active_only", activeonly);

        /// <summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified).</summary>
        public CatSegmentReplicationDescriptor AllowNoIndices(bool? allownoindices = true) =>
            Qs("allow_no_indices", allownoindices);

        /// <summary>The unit in which to display byte values.</summary>
        public CatSegmentReplicationDescriptor Bytes(Bytes? bytes) => Qs("bytes", bytes);

        /// <summary>If `true`, the response only includes latest completed segment replication events.</summary>
        public CatSegmentReplicationDescriptor CompletedOnly(bool? completedonly = true) =>
            Qs("completed_only", completedonly);

        /// <summary>If `true`, the response includes detailed information about segment replications.</summary>
        public CatSegmentReplicationDescriptor Detailed(bool? detailed = true) =>
            Qs("detailed", detailed);

        /// <summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
        public CatSegmentReplicationDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) =>
            Qs("expand_wildcards", expandwildcards);

        /// <summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public CatSegmentReplicationDescriptor Format(string format) => Qs("format", format);

        /// <summary>Comma-separated list of column names to display.</summary>
        public CatSegmentReplicationDescriptor Headers(params string[] headers) => Qs("h", headers);

        /// <summary>Return help information.</summary>
        public CatSegmentReplicationDescriptor Help(bool? help = true) => Qs("help", help);

        /// <summary>Whether specified concrete, expanded or aliased indices should be ignored when throttled.</summary>
        public CatSegmentReplicationDescriptor IgnoreThrottled(bool? ignorethrottled = true) =>
            Qs("ignore_throttled", ignorethrottled);

        /// <summary>Whether specified concrete indices should be ignored when unavailable (missing or closed).</summary>
        public CatSegmentReplicationDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) =>
            Qs("ignore_unavailable", ignoreunavailable);

        /// <summary>Comma-separated list of shards to display.</summary>
        public CatSegmentReplicationDescriptor Shards(params string[] shards) =>
            Qs("shards", shards);

        /// <summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public CatSegmentReplicationDescriptor SortByColumns(params string[] sortbycolumns) =>
            Qs("s", sortbycolumns);

        /// <summary>Operation timeout.</summary>
        public CatSegmentReplicationDescriptor Timeout(Time timeout) => Qs("timeout", timeout);

        /// <summary>Verbose mode. Display column headers.</summary>
        public CatSegmentReplicationDescriptor Verbose(bool? verbose = true) => Qs("v", verbose);
    }
}
