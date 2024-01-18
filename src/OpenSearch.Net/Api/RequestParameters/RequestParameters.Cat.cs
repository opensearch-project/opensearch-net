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

// ReSharper disable once CheckNamespace
namespace OpenSearch.Net.Specification.CatApi
{
	///<summary>Request options for Segments <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-segments/</para></summary>
	public class CatSegmentsRequestParameters : RequestParameters<CatSegmentsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
		///<summary>The unit in which to display byte values</summary>
		public Bytes? Bytes
		{
			get => Q<Bytes? >("bytes");
			set => Q("bytes", value);
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

	///<summary>Request options for Shards <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-shards/</para></summary>
	public class CatShardsRequestParameters : RequestParameters<CatShardsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
		///<summary>The unit in which to display byte values</summary>
		public Bytes? Bytes
		{
			get => Q<Bytes? >("bytes");
			set => Q("bytes", value);
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

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Explicit operation timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public TimeSpan MasterTimeout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public TimeSpan ClusterManagerTimeout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
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

	///<summary>Request options for Snapshots <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-snapshots/</para></summary>
	public class CatSnapshotsRequestParameters : RequestParameters<CatSnapshotsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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
		public TimeSpan MasterTimeout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public TimeSpan ClusterManagerTimeout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
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

	///<summary>Request options for Tasks <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-tasks/</para></summary>
	public class CatTasksRequestParameters : RequestParameters<CatTasksRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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

	///<summary>Request options for Templates <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public class CatTemplatesRequestParameters : RequestParameters<CatTemplatesRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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
		public TimeSpan MasterTimeout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public TimeSpan ClusterManagerTimeout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
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

	///<summary>Request options for ThreadPool <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-thread-pool/</para></summary>
	public class CatThreadPoolRequestParameters : RequestParameters<CatThreadPoolRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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
		public TimeSpan MasterTimeout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public TimeSpan ClusterManagerTimeout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
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
