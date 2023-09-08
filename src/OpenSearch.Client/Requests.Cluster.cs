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
using OpenSearch.Net.Specification.ClusterApi;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
	[InterfaceDataContract]
	public partial interface IClusterStateRequest : IRequest<ClusterStateRequestParameters>
	{
		[IgnoreDataMember]
		Metrics Metric
		{
			get;
		}

		[IgnoreDataMember]
		Indices Index
		{
			get;
		}
	}

	///<summary>Request for State <para></para></summary>
	public partial class ClusterStateRequest : PlainRequestBase<ClusterStateRequestParameters>, IClusterStateRequest
	{
		protected IClusterStateRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterState;
		///<summary>/_cluster/state</summary>
		public ClusterStateRequest(): base()
		{
		}

		///<summary>/_cluster/state/{metric}</summary>
		///<param name = "metric">Optional, accepts null</param>
		public ClusterStateRequest(Metrics metric): base(r => r.Optional("metric", metric))
		{
		}

		///<summary>/_cluster/state/{metric}/{index}</summary>
		///<param name = "metric">Optional, accepts null</param>
		///<param name = "index">Optional, accepts null</param>
		public ClusterStateRequest(Metrics metric, Indices index): base(r => r.Optional("metric", metric).Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Metrics IClusterStateRequest.Metric => Self.RouteValues.Get<Metrics>("metric");
		[IgnoreDataMember]
		Indices IClusterStateRequest.Index => Self.RouteValues.Get<Indices>("index");
		// Request parameters
		///<summary>
		/// Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have
		/// been specified)
		///</summary>
		public bool? AllowNoIndices
		{
			get => Q<bool? >("allow_no_indices");
			set => Q("allow_no_indices", value);
		}

		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public ExpandWildcards? ExpandWildcards
		{
			get => Q<ExpandWildcards? >("expand_wildcards");
			set => Q("expand_wildcards", value);
		}

		///<summary>Return settings in flat format (default: false)</summary>
		public bool? FlatSettings
		{
			get => Q<bool? >("flat_settings");
			set => Q("flat_settings", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public Time MasterTimeout
		{
			get => Q<Time>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public Time ClusterManagerTimeout
		{
			get => Q<Time>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Wait for the metadata version to be equal or greater than the specified metadata version</summary>
		public long? WaitForMetadataVersion
		{
			get => Q<long? >("wait_for_metadata_version");
			set => Q("wait_for_metadata_version", value);
		}

		///<summary>The maximum time to wait for wait_for_metadata_version before timing out</summary>
		public Time WaitForTimeout
		{
			get => Q<Time>("wait_for_timeout");
			set => Q("wait_for_timeout", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IClusterStatsRequest : IRequest<ClusterStatsRequestParameters>
	{
		[IgnoreDataMember]
		NodeIds NodeId
		{
			get;
		}
	}

	///<summary>Request for Stats <para></para></summary>
	public partial class ClusterStatsRequest : PlainRequestBase<ClusterStatsRequestParameters>, IClusterStatsRequest
	{
		protected IClusterStatsRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.ClusterStats;
		///<summary>/_cluster/stats</summary>
		public ClusterStatsRequest(): base()
		{
		}

		///<summary>/_cluster/stats/nodes/{node_id}</summary>
		///<param name = "nodeId">Optional, accepts null</param>
		public ClusterStatsRequest(NodeIds nodeId): base(r => r.Optional("node_id", nodeId))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		NodeIds IClusterStatsRequest.NodeId => Self.RouteValues.Get<NodeIds>("node_id");
		// Request parameters
		///<summary>Return settings in flat format (default: false)</summary>
		public bool? FlatSettings
		{
			get => Q<bool? >("flat_settings");
			set => Q("flat_settings", value);
		}

		///<summary>Explicit operation timeout</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}
	}
}
