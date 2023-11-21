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

// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;
using static OpenSearch.Net.HttpMethod;

// ReSharper disable InterpolatedStringExpressionIsNotIFormattable
// ReSharper disable once CheckNamespace
// ReSharper disable InterpolatedStringExpressionIsNotIFormattable
// ReSharper disable RedundantExtendsListEntry
namespace OpenSearch.Net.Specification.ClusterApi
{
	///<summary>
	/// Cluster APIs.
	/// <para>Not intended to be instantiated directly. Use the <see cref = "IOpenSearchLowLevelClient.Cluster"/> property
	/// on <see cref = "IOpenSearchLowLevelClient"/>.
	///</para>
	///</summary>
	public partial class LowLevelClusterNamespace : NamespacedClientProxy
	{
		///<summary>POST on /_cluster/allocation/explain <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-allocation/</para></summary>
		///<param name = "body">The index, shard, and primary flag to explain. Empty means &#x27;explain the first unassigned shard&#x27;</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse AllocationExplain<TResponse>(PostData body, ClusterAllocationExplainRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_cluster/allocation/explain", body, RequestParams(requestParameters));
		///<summary>POST on /_cluster/allocation/explain <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-allocation/</para></summary>
		///<param name = "body">The index, shard, and primary flag to explain. Empty means &#x27;explain the first unassigned shard&#x27;</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.allocation_explain", "body")]
		public Task<TResponse> AllocationExplainAsync<TResponse>(PostData body, ClusterAllocationExplainRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_cluster/allocation/explain", ctx, body, RequestParams(requestParameters));
		///<summary>DELETE on /_cluster/voting_config_exclusions <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse DeleteVotingConfigExclusions<TResponse>(DeleteVotingConfigExclusionsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(DELETE, "_cluster/voting_config_exclusions", null, RequestParams(requestParameters));
		///<summary>DELETE on /_cluster/voting_config_exclusions <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.delete_voting_config_exclusions", "")]
		public Task<TResponse> DeleteVotingConfigExclusionsAsync<TResponse>(DeleteVotingConfigExclusionsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(DELETE, "_cluster/voting_config_exclusions", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/settings <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-settings/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse GetSettings<TResponse>(ClusterGetSettingsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cluster/settings", null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/settings <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-settings/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.get_settings", "")]
		public Task<TResponse> GetSettingsAsync<TResponse>(ClusterGetSettingsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cluster/settings", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/health <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-health/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Health<TResponse>(ClusterHealthRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cluster/health", null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/health <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-health/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.health", "")]
		public Task<TResponse> HealthAsync<TResponse>(ClusterHealthRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cluster/health", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/health/{index} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-health/</para></summary>
		///<param name = "index">Limit the information returned to a specific index</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Health<TResponse>(string index, ClusterHealthRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cluster/health/{index:index}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/health/{index} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-health/</para></summary>
		///<param name = "index">Limit the information returned to a specific index</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.health", "index")]
		public Task<TResponse> HealthAsync<TResponse>(string index, ClusterHealthRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cluster/health/{index:index}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/pending_tasks <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-pending-tasks/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse PendingTasks<TResponse>(ClusterPendingTasksRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cluster/pending_tasks", null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/pending_tasks <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-pending-tasks/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.pending_tasks", "")]
		public Task<TResponse> PendingTasksAsync<TResponse>(ClusterPendingTasksRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cluster/pending_tasks", ctx, null, RequestParams(requestParameters));
		///<summary>POST on /_cluster/voting_config_exclusions <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse PostVotingConfigExclusions<TResponse>(PostVotingConfigExclusionsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_cluster/voting_config_exclusions", null, RequestParams(requestParameters));
		///<summary>POST on /_cluster/voting_config_exclusions <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.post_voting_config_exclusions", "")]
		public Task<TResponse> PostVotingConfigExclusionsAsync<TResponse>(PostVotingConfigExclusionsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_cluster/voting_config_exclusions", ctx, null, RequestParams(requestParameters));
		///<summary>PUT on /_cluster/settings <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-settings/</para></summary>
		///<param name = "body">The settings to be updated. Can be either `transient` or `persistent` (survives cluster restart).</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse PutSettings<TResponse>(PostData body, ClusterPutSettingsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(PUT, "_cluster/settings", body, RequestParams(requestParameters));
		///<summary>PUT on /_cluster/settings <para>https://opensearch.org/docs/latest/opensearch/rest-api/cluster-settings/</para></summary>
		///<param name = "body">The settings to be updated. Can be either `transient` or `persistent` (survives cluster restart).</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.put_settings", "body")]
		public Task<TResponse> PutSettingsAsync<TResponse>(PostData body, ClusterPutSettingsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(PUT, "_cluster/settings", ctx, body, RequestParams(requestParameters));
		///<summary>GET on /_remote/info <para>https://opensearch.org/docs/latest/opensearch/rest-api/remote-info/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse RemoteInfo<TResponse>(RemoteInfoRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_remote/info", null, RequestParams(requestParameters));
		///<summary>GET on /_remote/info <para>https://opensearch.org/docs/latest/opensearch/rest-api/remote-info/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.remote_info", "")]
		public Task<TResponse> RemoteInfoAsync<TResponse>(RemoteInfoRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_remote/info", ctx, null, RequestParams(requestParameters));
		///<summary>POST on /_cluster/reroute <para></para></summary>
		///<param name = "body">The definition of `commands` to perform (`move`, `cancel`, `allocate`)</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Reroute<TResponse>(PostData body, ClusterRerouteRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_cluster/reroute", body, RequestParams(requestParameters));
		///<summary>POST on /_cluster/reroute <para></para></summary>
		///<param name = "body">The definition of `commands` to perform (`move`, `cancel`, `allocate`)</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.reroute", "body")]
		public Task<TResponse> RerouteAsync<TResponse>(PostData body, ClusterRerouteRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_cluster/reroute", ctx, body, RequestParams(requestParameters));
		///<summary>GET on /_cluster/state <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse State<TResponse>(ClusterStateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cluster/state", null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/state <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.state", "")]
		public Task<TResponse> StateAsync<TResponse>(ClusterStateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cluster/state", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/state/{metric} <para></para></summary>
		///<param name = "metric">Limit the information returned to the specified metrics</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse State<TResponse>(string metric, ClusterStateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cluster/state/{metric:metric}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/state/{metric} <para></para></summary>
		///<param name = "metric">Limit the information returned to the specified metrics</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.state", "metric")]
		public Task<TResponse> StateAsync<TResponse>(string metric, ClusterStateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cluster/state/{metric:metric}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/state/{metric}/{index} <para></para></summary>
		///<param name = "metric">Limit the information returned to the specified metrics</param>
		///<param name = "index">A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse State<TResponse>(string metric, string index, ClusterStateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cluster/state/{metric:metric}/{index:index}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/state/{metric}/{index} <para></para></summary>
		///<param name = "metric">Limit the information returned to the specified metrics</param>
		///<param name = "index">A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.state", "metric, index")]
		public Task<TResponse> StateAsync<TResponse>(string metric, string index, ClusterStateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cluster/state/{metric:metric}/{index:index}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/stats <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Stats<TResponse>(ClusterStatsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cluster/stats", null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/stats <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.stats", "")]
		public Task<TResponse> StatsAsync<TResponse>(ClusterStatsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cluster/stats", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/stats/nodes/{node_id} <para></para></summary>
		///<param name = "nodeId">A comma-separated list of node IDs or names to limit the returned information; use `_local` to return information from the node you&#x27;re connecting to, leave empty to get information from all nodes</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Stats<TResponse>(string nodeId, ClusterStatsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cluster/stats/nodes/{nodeId:nodeId}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cluster/stats/nodes/{node_id} <para></para></summary>
		///<param name = "nodeId">A comma-separated list of node IDs or names to limit the returned information; use `_local` to return information from the node you&#x27;re connecting to, leave empty to get information from all nodes</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cluster.stats", "node_id")]
		public Task<TResponse> StatsAsync<TResponse>(string nodeId, ClusterStatsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cluster/stats/nodes/{nodeId:nodeId}"), ctx, null, RequestParams(requestParameters));
	}
}
