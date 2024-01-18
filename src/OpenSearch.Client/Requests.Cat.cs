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
* 	http://www.apache.org/licenses/LICENSE-2.0
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
// 		*NIX 		:	./build.sh codegen
// 		Windows 	:	build.bat codegen
//
// -----------------------------------------------
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Specification.CatApi;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
	[InterfaceDataContract]
	public partial interface ICatSnapshotsRequest : IRequest<CatSnapshotsRequestParameters>
	{
		[IgnoreDataMember]
		Names RepositoryName
		{
			get;
		}
	}

	///<summary>Request for Snapshots <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-snapshots/</para></summary>
	public partial class CatSnapshotsRequest : PlainRequestBase<CatSnapshotsRequestParameters>, ICatSnapshotsRequest
	{
		protected ICatSnapshotsRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.CatSnapshots;
		///<summary>/_cat/snapshots</summary>
		public CatSnapshotsRequest(): base()
		{
		}

		///<summary>/_cat/snapshots/{repository}</summary>
		///<param name = "repository">Optional, accepts null</param>
		public CatSnapshotsRequest(Names repository): base(r => r.Optional("repository", repository))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Names ICatSnapshotsRequest.RepositoryName => Self.RouteValues.Get<Names>("repository");
		// Request parameters
		///<summary>a short version of the Accept header, e.g. json, yaml</summary>
		public string Format
		{
			get => Q<string>("format");
			set
			{
				Q("format", value);
				SetAcceptHeader(value);
			}
		}

		///<summary>Comma-separated list of column names to display</summary>
		public string[] Headers
		{
			get => Q<string[]>("h");
			set => Q("h", value);
		}

		///<summary>Return help information</summary>
		public bool? Help
		{
			get => Q<bool? >("help");
			set => Q("help", value);
		}

		///<summary>Set to true to ignore unavailable snapshots</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Explicit operation timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public Time MasterTimeout
		{
			get => Q<Time>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public Time ClusterManagerTimeout
		{
			get => Q<Time>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Comma-separated list of column names or column aliases to sort by</summary>
		public string[] SortByColumns
		{
			get => Q<string[]>("s");
			set => Q("s", value);
		}

		///<summary>Verbose mode. Display column headers</summary>
		public bool? Verbose
		{
			get => Q<bool? >("v");
			set => Q("v", value);
		}
	}

	[InterfaceDataContract]
	public partial interface ICatTasksRequest : IRequest<CatTasksRequestParameters>
	{
	}

	///<summary>Request for Tasks <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-tasks/</para></summary>
	///<remarks>Note: Experimental within the OpenSearch server, this functionality is experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features.</remarks>
	public partial class CatTasksRequest : PlainRequestBase<CatTasksRequestParameters>, ICatTasksRequest
	{
		protected ICatTasksRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.CatTasks;
		// values part of the url path
		// Request parameters
		///<summary>A comma-separated list of actions that should be returned. Leave empty to return all.</summary>
		public string[] Actions
		{
			get => Q<string[]>("actions");
			set => Q("actions", value);
		}

		///<summary>Return detailed task information (default: false)</summary>
		public bool? Detailed
		{
			get => Q<bool? >("detailed");
			set => Q("detailed", value);
		}

		///<summary>a short version of the Accept header, e.g. json, yaml</summary>
		public string Format
		{
			get => Q<string>("format");
			set
			{
				Q("format", value);
				SetAcceptHeader(value);
			}
		}

		///<summary>Comma-separated list of column names to display</summary>
		public string[] Headers
		{
			get => Q<string[]>("h");
			set => Q("h", value);
		}

		///<summary>Return help information</summary>
		public bool? Help
		{
			get => Q<bool? >("help");
			set => Q("help", value);
		}

		///<summary>
		/// A comma-separated list of node IDs or names to limit the returned information; use `_local` to return information from the node you're
		/// connecting to, leave empty to get information from all nodes
		///</summary>
		public string[] Nodes
		{
			get => Q<string[]>("nodes");
			set => Q("nodes", value);
		}

		///<summary>Return tasks with specified parent task id (node_id:task_number). Set to -1 to return all.</summary>
		public string ParentTaskId
		{
			get => Q<string>("parent_task_id");
			set => Q("parent_task_id", value);
		}

		///<summary>Comma-separated list of column names or column aliases to sort by</summary>
		public string[] SortByColumns
		{
			get => Q<string[]>("s");
			set => Q("s", value);
		}

		///<summary>Verbose mode. Display column headers</summary>
		public bool? Verbose
		{
			get => Q<bool? >("v");
			set => Q("v", value);
		}
	}

	[InterfaceDataContract]
	public partial interface ICatTemplatesRequest : IRequest<CatTemplatesRequestParameters>
	{
		[IgnoreDataMember]
		Name Name
		{
			get;
		}
	}

	///<summary>Request for Templates <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public partial class CatTemplatesRequest : PlainRequestBase<CatTemplatesRequestParameters>, ICatTemplatesRequest
	{
		protected ICatTemplatesRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.CatTemplates;
		///<summary>/_cat/templates</summary>
		public CatTemplatesRequest(): base()
		{
		}

		///<summary>/_cat/templates/{name}</summary>
		///<param name = "name">Optional, accepts null</param>
		public CatTemplatesRequest(Name name): base(r => r.Optional("name", name))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Name ICatTemplatesRequest.Name => Self.RouteValues.Get<Name>("name");
		// Request parameters
		///<summary>a short version of the Accept header, e.g. json, yaml</summary>
		public string Format
		{
			get => Q<string>("format");
			set
			{
				Q("format", value);
				SetAcceptHeader(value);
			}
		}

		///<summary>Comma-separated list of column names to display</summary>
		public string[] Headers
		{
			get => Q<string[]>("h");
			set => Q("h", value);
		}

		///<summary>Return help information</summary>
		public bool? Help
		{
			get => Q<bool? >("help");
			set => Q("help", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Explicit operation timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public Time MasterTimeout
		{
			get => Q<Time>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public Time ClusterManagerTimeout
		{
			get => Q<Time>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Comma-separated list of column names or column aliases to sort by</summary>
		public string[] SortByColumns
		{
			get => Q<string[]>("s");
			set => Q("s", value);
		}

		///<summary>Verbose mode. Display column headers</summary>
		public bool? Verbose
		{
			get => Q<bool? >("v");
			set => Q("v", value);
		}
	}

	[InterfaceDataContract]
	public partial interface ICatThreadPoolRequest : IRequest<CatThreadPoolRequestParameters>
	{
		[IgnoreDataMember]
		Names ThreadPoolPatterns
		{
			get;
		}
	}

	///<summary>Request for ThreadPool <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-thread-pool/</para></summary>
	public partial class CatThreadPoolRequest : PlainRequestBase<CatThreadPoolRequestParameters>, ICatThreadPoolRequest
	{
		protected ICatThreadPoolRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.CatThreadPool;
		///<summary>/_cat/thread_pool</summary>
		public CatThreadPoolRequest(): base()
		{
		}

		///<summary>/_cat/thread_pool/{thread_pool_patterns}</summary>
		///<param name = "threadPoolPatterns">Optional, accepts null</param>
		public CatThreadPoolRequest(Names threadPoolPatterns): base(r => r.Optional("thread_pool_patterns", threadPoolPatterns))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Names ICatThreadPoolRequest.ThreadPoolPatterns => Self.RouteValues.Get<Names>("thread_pool_patterns");
		// Request parameters
		///<summary>a short version of the Accept header, e.g. json, yaml</summary>
		public string Format
		{
			get => Q<string>("format");
			set
			{
				Q("format", value);
				SetAcceptHeader(value);
			}
		}

		///<summary>Comma-separated list of column names to display</summary>
		public string[] Headers
		{
			get => Q<string[]>("h");
			set => Q("h", value);
		}

		///<summary>Return help information</summary>
		public bool? Help
		{
			get => Q<bool? >("help");
			set => Q("help", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Explicit operation timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public Time MasterTimeout
		{
			get => Q<Time>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public Time ClusterManagerTimeout
		{
			get => Q<Time>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Comma-separated list of column names or column aliases to sort by</summary>
		public string[] SortByColumns
		{
			get => Q<string[]>("s");
			set => Q("s", value);
		}

		///<summary>Verbose mode. Display column headers</summary>
		public bool? Verbose
		{
			get => Q<bool? >("v");
			set => Q("v", value);
		}
	}
}
