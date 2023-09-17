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
using System.Runtime.Serialization;
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
    [InterfaceDataContract]
    public partial interface ICatAliasesRequest : IRequest<CatAliasesRequestParameters>
    {
        [IgnoreDataMember]
        Names Name { get; }
    }

    /// <summary>Request for Aliases <para>https://opensearch.org/docs/latest/api-reference/cat/cat-aliases/</para></summary>
    public partial class CatAliasesRequest
        : PlainRequestBase<CatAliasesRequestParameters>,
            ICatAliasesRequest
    {
        protected ICatAliasesRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.CatAliases;

        /// <summary>/_cat/aliases</summary>
        public CatAliasesRequest()
            : base() { }

        /// <summary>/_cat/aliases/{name}</summary>
        /// <param name="name">Optional, accepts null</param>
        public CatAliasesRequest(Names name)
            : base(r => r.Optional("name", name)) { }

        // values part of the url path
        [IgnoreDataMember]
        Names ICatAliasesRequest.Name => Self.RouteValues.Get<Names>("name");

        // Request parameters
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

        /// <summary>Return local information, do not retrieve the state from cluster-manager node.</summary>
        public bool? Local
        {
            get => Q<bool?>("local");
            set => Q("local", value);
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

    [InterfaceDataContract]
    public partial interface ICatAllocationRequest : IRequest<CatAllocationRequestParameters>
    {
        [IgnoreDataMember]
        NodeIds NodeId { get; }
    }

    ///<summary>Request for Allocation <para>https://opensearch.org/docs/latest/api-reference/cat/cat-allocation/</para></summary>
    public partial class CatAllocationRequest
        : PlainRequestBase<CatAllocationRequestParameters>,
            ICatAllocationRequest
    {
        protected ICatAllocationRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.CatAllocation;

        ///<summary>/_cat/allocation</summary>
        public CatAllocationRequest()
            : base() { }

        ///<summary>/_cat/allocation/{node_id}</summary>
        ///<param name="nodeId">Optional, accepts null</param>
        public CatAllocationRequest(NodeIds nodeId)
            : base(r => r.Optional("node_id", nodeId)) { }

        // values part of the url path
        [IgnoreDataMember]
        NodeIds ICatAllocationRequest.NodeId => Self.RouteValues.Get<NodeIds>("node_id");

        // Request parameters
        ///<summary>The unit in which to display byte values.</summary>
        public Bytes? Bytes
        {
            get => Q<Bytes?>("bytes");
            set => Q("bytes", value);
        }

        ///<summary>Operation timeout for connection to cluster-manager node.</summary>
        ///<remarks>Supported by OpenSearch servers of version 2.0.0 or greater.</remarks>
        public Time ClusterManagerTimeout
        {
            get => Q<Time>("cluster_manager_timeout");
            set => Q("cluster_manager_timeout", value);
        }

        ///<summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public string Format
        {
            get => Q<string>("format");
            set
            {
                Q("format", value);
                SetAcceptHeader(value);
            }
        }

        ///<summary>Comma-separated list of column names to display.</summary>
        public string[] Headers
        {
            get => Q<string[]>("h");
            set => Q("h", value);
        }

        ///<summary>Return help information.</summary>
        public bool? Help
        {
            get => Q<bool?>("help");
            set => Q("help", value);
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
        public Time MasterTimeout
        {
            get => Q<Time>("master_timeout");
            set => Q("master_timeout", value);
        }

        ///<summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public string[] SortByColumns
        {
            get => Q<string[]>("s");
            set => Q("s", value);
        }

        ///<summary>Verbose mode. Display column headers.</summary>
        public bool? Verbose
        {
            get => Q<bool?>("v");
            set => Q("v", value);
        }
    }

    [InterfaceDataContract]
    public partial interface ICatCountRequest : IRequest<CatCountRequestParameters>
    {
        [IgnoreDataMember]
        Indices Index { get; }
    }

    ///<summary>Request for Count <para>https://opensearch.org/docs/latest/api-reference/cat/cat-count/</para></summary>
    public partial class CatCountRequest
        : PlainRequestBase<CatCountRequestParameters>,
            ICatCountRequest
    {
        protected ICatCountRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.CatCount;

        ///<summary>/_cat/count</summary>
        public CatCountRequest()
            : base() { }

        ///<summary>/_cat/count/{index}</summary>
        ///<param name="index">Optional, accepts null</param>
        public CatCountRequest(Indices index)
            : base(r => r.Optional("index", index)) { }

        // values part of the url path
        [IgnoreDataMember]
        Indices ICatCountRequest.Index => Self.RouteValues.Get<Indices>("index");

        // Request parameters
        ///<summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public string Format
        {
            get => Q<string>("format");
            set
            {
                Q("format", value);
                SetAcceptHeader(value);
            }
        }

        ///<summary>Comma-separated list of column names to display.</summary>
        public string[] Headers
        {
            get => Q<string[]>("h");
            set => Q("h", value);
        }

        ///<summary>Return help information.</summary>
        public bool? Help
        {
            get => Q<bool?>("help");
            set => Q("help", value);
        }

        ///<summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public string[] SortByColumns
        {
            get => Q<string[]>("s");
            set => Q("s", value);
        }

        ///<summary>Verbose mode. Display column headers.</summary>
        public bool? Verbose
        {
            get => Q<bool?>("v");
            set => Q("v", value);
        }
    }

    [InterfaceDataContract]
    public partial interface ICatFielddataRequest : IRequest<CatFielddataRequestParameters>
    {
        [IgnoreDataMember]
        Fields Fields { get; }
    }

    ///<summary>Request for Fielddata <para>https://opensearch.org/docs/latest/api-reference/cat/cat-field-data/</para></summary>
    public partial class CatFielddataRequest
        : PlainRequestBase<CatFielddataRequestParameters>,
            ICatFielddataRequest
    {
        protected ICatFielddataRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.CatFielddata;

        ///<summary>/_cat/fielddata</summary>
        public CatFielddataRequest()
            : base() { }

        ///<summary>/_cat/fielddata/{fields}</summary>
        ///<param name="fields">Optional, accepts null</param>
        public CatFielddataRequest(Fields fields)
            : base(r => r.Optional("fields", fields)) { }

        // values part of the url path
        [IgnoreDataMember]
        Fields ICatFielddataRequest.Fields => Self.RouteValues.Get<Fields>("fields");

        // Request parameters
        ///<summary>The unit in which to display byte values.</summary>
        public Bytes? Bytes
        {
            get => Q<Bytes?>("bytes");
            set => Q("bytes", value);
        }

        ///<summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public string Format
        {
            get => Q<string>("format");
            set
            {
                Q("format", value);
                SetAcceptHeader(value);
            }
        }

        ///<summary>Comma-separated list of column names to display.</summary>
        public string[] Headers
        {
            get => Q<string[]>("h");
            set => Q("h", value);
        }

        ///<summary>Return help information.</summary>
        public bool? Help
        {
            get => Q<bool?>("help");
            set => Q("help", value);
        }

        ///<summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public string[] SortByColumns
        {
            get => Q<string[]>("s");
            set => Q("s", value);
        }

        ///<summary>Verbose mode. Display column headers.</summary>
        public bool? Verbose
        {
            get => Q<bool?>("v");
            set => Q("v", value);
        }
    }

    [InterfaceDataContract]
    public partial interface ICatHealthRequest : IRequest<CatHealthRequestParameters> { }

    ///<summary>Request for Health <para>https://opensearch.org/docs/latest/api-reference/cat/cat-health/</para></summary>
    public partial class CatHealthRequest
        : PlainRequestBase<CatHealthRequestParameters>,
            ICatHealthRequest
    {
        protected ICatHealthRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.CatHealth;

        // values part of the url path

        // Request parameters
        ///<summary>A short version of the Accept header, e.g. json, yaml.</summary>
        public string Format
        {
            get => Q<string>("format");
            set
            {
                Q("format", value);
                SetAcceptHeader(value);
            }
        }

        ///<summary>Comma-separated list of column names to display.</summary>
        public string[] Headers
        {
            get => Q<string[]>("h");
            set => Q("h", value);
        }

        ///<summary>Return help information.</summary>
        public bool? Help
        {
            get => Q<bool?>("help");
            set => Q("help", value);
        }

        ///<summary>Set to false to disable timestamping.</summary>
        public bool? IncludeTimestamp
        {
            get => Q<bool?>("ts");
            set => Q("ts", value);
        }

        ///<summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public string[] SortByColumns
        {
            get => Q<string[]>("s");
            set => Q("s", value);
        }

        ///<summary>Verbose mode. Display column headers.</summary>
        public bool? Verbose
        {
            get => Q<bool?>("v");
            set => Q("v", value);
        }
    }

    [InterfaceDataContract]
    public partial interface ICatHelpRequest : IRequest<CatHelpRequestParameters> { }

    ///<summary>Request for Help <para>https://opensearch.org/docs/latest/api-reference/cat/index/</para></summary>
    public partial class CatHelpRequest
        : PlainRequestBase<CatHelpRequestParameters>,
            ICatHelpRequest
    {
        protected ICatHelpRequest Self => this;
        internal override ApiUrls ApiUrls => ApiUrlsLookups.CatHelp;

        // values part of the url path

        // Request parameters
        ///<summary>Return help information.</summary>
        public bool? Help
        {
            get => Q<bool?>("help");
            set => Q("help", value);
        }

        ///<summary>Comma-separated list of column names or column aliases to sort by.</summary>
        public string[] SortByColumns
        {
            get => Q<string[]>("s");
            set => Q("s", value);
        }
    }
}
