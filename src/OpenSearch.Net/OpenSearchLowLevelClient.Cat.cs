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
namespace OpenSearch.Net.Specification.CatApi
{
	///<summary>
	/// Cat APIs.
	/// <para>Not intended to be instantiated directly. Use the <see cref = "IOpenSearchLowLevelClient.Cat"/> property
	/// on <see cref = "IOpenSearchLowLevelClient"/>.
	///</para>
	///</summary>
	public partial class LowLevelCatNamespace : NamespacedClientProxy
	{
		///<summary>GET on /_cat/pending_tasks <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-pending-tasks/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse PendingTasks<TResponse>(CatPendingTasksRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/pending_tasks", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/pending_tasks <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-pending-tasks/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.pending_tasks", "")]
		public Task<TResponse> PendingTasksAsync<TResponse>(CatPendingTasksRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/pending_tasks", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/plugins <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-plugins/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Plugins<TResponse>(CatPluginsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/plugins", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/plugins <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-plugins/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.plugins", "")]
		public Task<TResponse> PluginsAsync<TResponse>(CatPluginsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/plugins", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/recovery <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-recovery/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Recovery<TResponse>(CatRecoveryRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/recovery", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/recovery <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-recovery/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.recovery", "")]
		public Task<TResponse> RecoveryAsync<TResponse>(CatRecoveryRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/recovery", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/recovery/{index} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-recovery/</para></summary>
		///<param name = "index">Comma-separated list or wildcard expression of index names to limit the returned information</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Recovery<TResponse>(string index, CatRecoveryRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cat/recovery/{index:index}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cat/recovery/{index} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-recovery/</para></summary>
		///<param name = "index">Comma-separated list or wildcard expression of index names to limit the returned information</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.recovery", "index")]
		public Task<TResponse> RecoveryAsync<TResponse>(string index, CatRecoveryRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cat/recovery/{index:index}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/repositories <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-repositories/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Repositories<TResponse>(CatRepositoriesRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/repositories", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/repositories <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-repositories/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.repositories", "")]
		public Task<TResponse> RepositoriesAsync<TResponse>(CatRepositoriesRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/repositories", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/segments <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-segments/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Segments<TResponse>(CatSegmentsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/segments", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/segments <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-segments/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.segments", "")]
		public Task<TResponse> SegmentsAsync<TResponse>(CatSegmentsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/segments", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/segments/{index} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-segments/</para></summary>
		///<param name = "index">A comma-separated list of index names to limit the returned information</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Segments<TResponse>(string index, CatSegmentsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cat/segments/{index:index}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cat/segments/{index} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-segments/</para></summary>
		///<param name = "index">A comma-separated list of index names to limit the returned information</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.segments", "index")]
		public Task<TResponse> SegmentsAsync<TResponse>(string index, CatSegmentsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cat/segments/{index:index}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/shards <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-shards/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Shards<TResponse>(CatShardsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/shards", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/shards <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-shards/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.shards", "")]
		public Task<TResponse> ShardsAsync<TResponse>(CatShardsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/shards", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/shards/{index} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-shards/</para></summary>
		///<param name = "index">A comma-separated list of index names to limit the returned information</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Shards<TResponse>(string index, CatShardsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cat/shards/{index:index}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cat/shards/{index} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-shards/</para></summary>
		///<param name = "index">A comma-separated list of index names to limit the returned information</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.shards", "index")]
		public Task<TResponse> ShardsAsync<TResponse>(string index, CatShardsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cat/shards/{index:index}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/snapshots <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-snapshots/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Snapshots<TResponse>(CatSnapshotsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/snapshots", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/snapshots <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-snapshots/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.snapshots", "")]
		public Task<TResponse> SnapshotsAsync<TResponse>(CatSnapshotsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/snapshots", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/snapshots/{repository} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-snapshots/</para></summary>
		///<param name = "repository">Name of repository from which to fetch the snapshot information</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Snapshots<TResponse>(string repository, CatSnapshotsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cat/snapshots/{repository:repository}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cat/snapshots/{repository} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-snapshots/</para></summary>
		///<param name = "repository">Name of repository from which to fetch the snapshot information</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.snapshots", "repository")]
		public Task<TResponse> SnapshotsAsync<TResponse>(string repository, CatSnapshotsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cat/snapshots/{repository:repository}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/tasks <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-tasks/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		public TResponse Tasks<TResponse>(CatTasksRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/tasks", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/tasks <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-tasks/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		[MapsApi("cat.tasks", "")]
		public Task<TResponse> TasksAsync<TResponse>(CatTasksRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/tasks", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/templates <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Templates<TResponse>(CatTemplatesRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/templates", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/templates <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.templates", "")]
		public Task<TResponse> TemplatesAsync<TResponse>(CatTemplatesRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/templates", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/templates/{name} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
		///<param name = "name">A pattern that returned template names must match</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Templates<TResponse>(string name, CatTemplatesRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cat/templates/{name:name}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cat/templates/{name} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
		///<param name = "name">A pattern that returned template names must match</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.templates", "name")]
		public Task<TResponse> TemplatesAsync<TResponse>(string name, CatTemplatesRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cat/templates/{name:name}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/thread_pool <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-thread-pool/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse ThreadPool<TResponse>(CatThreadPoolRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_cat/thread_pool", null, RequestParams(requestParameters));
		///<summary>GET on /_cat/thread_pool <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-thread-pool/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.thread_pool", "")]
		public Task<TResponse> ThreadPoolAsync<TResponse>(CatThreadPoolRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_cat/thread_pool", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_cat/thread_pool/{thread_pool_patterns} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-thread-pool/</para></summary>
		///<param name = "threadPoolPatterns">A comma-separated list of regular-expressions to filter the thread pools in the output</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse ThreadPool<TResponse>(string threadPoolPatterns, CatThreadPoolRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_cat/thread_pool/{threadPoolPatterns:threadPoolPatterns}"), null, RequestParams(requestParameters));
		///<summary>GET on /_cat/thread_pool/{thread_pool_patterns} <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-thread-pool/</para></summary>
		///<param name = "threadPoolPatterns">A comma-separated list of regular-expressions to filter the thread pools in the output</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("cat.thread_pool", "thread_pool_patterns")]
		public Task<TResponse> ThreadPoolAsync<TResponse>(string threadPoolPatterns, CatThreadPoolRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_cat/thread_pool/{threadPoolPatterns:threadPoolPatterns}"), ctx, null, RequestParams(requestParameters));
	}
}
