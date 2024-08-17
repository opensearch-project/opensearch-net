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
namespace OpenSearch.Net.Specification.CatApi
{
    /// <summary>Request options for AllPitSegments <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/</para></summary>
    public partial class CatAllPitSegmentsRequestParameters
        : RequestParameters<CatAllPitSegmentsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>The unit in which to display byte values.</summary>
        public Bytes? Bytes
        {
            get => Q<Bytes?>("bytes");
            set => Q("bytes", value);
        }

        /// <summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public string Format
        {
            get => Q<string>("format");
            set
            {
                Q("format", value);
                SetAcceptHeader(value);
            }
        }

        /// <summary>Comma-separated list of column names to display.</summary>
        public string[] Headers
        {
            get => Q<string[]>("h");
            set => Q("h", value);
        }

        /// <summary>Return help information.</summary>
        public bool? Help
        {
            get => Q<bool?>("help");
            set => Q("help", value);
        }

        /// <summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public string[] SortByColumns
        {
            get => Q<string[]>("s");
            set => Q("s", value);
        }

        /// <summary>Verbose mode. Display column headers.</summary>
        public bool? Verbose
        {
            get => Q<bool?>("v");
            set => Q("v", value);
        }
    }

    /// <summary>Request options for PitSegments <para>https://opensearch.org/docs/latest/search-plugins/point-in-time-api/</para></summary>
    public partial class CatPitSegmentsRequestParameters
        : RequestParameters<CatPitSegmentsRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => true;

        /// <summary>The unit in which to display byte values.</summary>
        public Bytes? Bytes
        {
            get => Q<Bytes?>("bytes");
            set => Q("bytes", value);
        }

        /// <summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public string Format
        {
            get => Q<string>("format");
            set
            {
                Q("format", value);
                SetAcceptHeader(value);
            }
        }

        /// <summary>Comma-separated list of column names to display.</summary>
        public string[] Headers
        {
            get => Q<string[]>("h");
            set => Q("h", value);
        }

        /// <summary>Return help information.</summary>
        public bool? Help
        {
            get => Q<bool?>("help");
            set => Q("help", value);
        }

        /// <summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public string[] SortByColumns
        {
            get => Q<string[]>("s");
            set => Q("s", value);
        }

        /// <summary>Verbose mode. Display column headers.</summary>
        public bool? Verbose
        {
            get => Q<bool?>("v");
            set => Q("v", value);
        }
    }

    /// <summary>Request options for SegmentReplication <para>https://opensearch.org/docs/latest/api-reference/cat/cat-segment-replication/</para></summary>
    public partial class CatSegmentReplicationRequestParameters
        : RequestParameters<CatSegmentReplicationRequestParameters>
    {
        public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
        public override bool SupportsBody => false;

        /// <summary>If `true`, the response only includes ongoing segment replication events.</summary>
        public bool? ActiveOnly
        {
            get => Q<bool?>("active_only");
            set => Q("active_only", value);
        }

        /// <summary>
        /// Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have
        /// been specified).
        /// </summary>
        public bool? AllowNoIndices
        {
            get => Q<bool?>("allow_no_indices");
            set => Q("allow_no_indices", value);
        }

        /// <summary>The unit in which to display byte values.</summary>
        public Bytes? Bytes
        {
            get => Q<Bytes?>("bytes");
            set => Q("bytes", value);
        }

        /// <summary>If `true`, the response only includes latest completed segment replication events.</summary>
        public bool? CompletedOnly
        {
            get => Q<bool?>("completed_only");
            set => Q("completed_only", value);
        }

        /// <summary>If `true`, the response includes detailed information about segment replications.</summary>
        public bool? Detailed
        {
            get => Q<bool?>("detailed");
            set => Q("detailed", value);
        }

        /// <summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
        public ExpandWildcards? ExpandWildcards
        {
            get => Q<ExpandWildcards?>("expand_wildcards");
            set => Q("expand_wildcards", value);
        }

        /// <summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public string Format
        {
            get => Q<string>("format");
            set
            {
                Q("format", value);
                SetAcceptHeader(value);
            }
        }

        /// <summary>Comma-separated list of column names to display.</summary>
        public string[] Headers
        {
            get => Q<string[]>("h");
            set => Q("h", value);
        }

        /// <summary>Return help information.</summary>
        public bool? Help
        {
            get => Q<bool?>("help");
            set => Q("help", value);
        }

        /// <summary>Whether specified concrete, expanded or aliased indices should be ignored when throttled.</summary>
        public bool? IgnoreThrottled
        {
            get => Q<bool?>("ignore_throttled");
            set => Q("ignore_throttled", value);
        }

        /// <summary>Whether specified concrete indices should be ignored when unavailable (missing or closed).</summary>
        public bool? IgnoreUnavailable
        {
            get => Q<bool?>("ignore_unavailable");
            set => Q("ignore_unavailable", value);
        }

        /// <summary>Comma-separated list or wildcard expression of index names to limit the returned information.</summary>
        public string[] Index
        {
            get => Q<string[]>("index");
            set => Q("index", value);
        }

        /// <summary>Comma-separated list of shards to display.</summary>
        public string[] Shards
        {
            get => Q<string[]>("shards");
            set => Q("shards", value);
        }

        /// <summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public string[] SortByColumns
        {
            get => Q<string[]>("s");
            set => Q("s", value);
        }

        /// <summary>Operation timeout.</summary>
        public TimeSpan Timeout
        {
            get => Q<TimeSpan>("timeout");
            set => Q("timeout", value);
        }

        /// <summary>Verbose mode. Display column headers.</summary>
        public bool? Verbose
        {
            get => Q<bool?>("v");
            set => Q("v", value);
        }
    }
}
