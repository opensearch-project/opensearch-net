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
using OpenSearch.Net.Specification.CatApi;
using OpenSearch.Net.Specification.ClusterApi;
using OpenSearch.Net.Specification.DanglingIndicesApi;
using OpenSearch.Net.Specification.FeaturesApi;
using OpenSearch.Net.Specification.IndicesApi;
using OpenSearch.Net.Specification.IngestApi;
using OpenSearch.Net.Specification.NodesApi;
using OpenSearch.Net.Specification.SnapshotApi;
using OpenSearch.Net.Specification.TasksApi;
using static OpenSearch.Net.HttpMethod;

// ReSharper disable InterpolatedStringExpressionIsNotIFormattable
// ReSharper disable RedundantExtendsListEntry
namespace OpenSearch.Net
{
	///<summary>
	///OpenSearch low level client
	///</summary>
	public partial class OpenSearchLowLevelClient : IOpenSearchLowLevelClient
	{
		public LowLevelCatNamespace Cat
		{
			get;
			private set;
		}

		public LowLevelDanglingIndicesNamespace DanglingIndices
		{
			get;
			private set;
		}

		public LowLevelFeaturesNamespace Features
		{
			get;
			private set;
		}

		public LowLevelIngestNamespace Ingest
		{
			get;
			private set;
		}

		public LowLevelNodesNamespace Nodes
		{
			get;
			private set;
		}

		public LowLevelSnapshotNamespace Snapshot
		{
			get;
			private set;
		}

		public LowLevelTasksNamespace Tasks
		{
			get;
			private set;
		}

		partial void SetupNamespaces()
		{
			Cat = new LowLevelCatNamespace(this);
			DanglingIndices = new LowLevelDanglingIndicesNamespace(this);
			Features = new LowLevelFeaturesNamespace(this);
			Ingest = new LowLevelIngestNamespace(this);
			Nodes = new LowLevelNodesNamespace(this);
			Snapshot = new LowLevelSnapshotNamespace(this);
			Tasks = new LowLevelTasksNamespace(this);
		}

		///<summary>POST on /_bulk <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/</para></summary>
		///<param name = "body">The operation definition and data (action-data pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Bulk<TResponse>(PostData body, BulkRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_bulk", body, RequestParams(requestParameters));
		///<summary>POST on /_bulk <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/</para></summary>
		///<param name = "body">The operation definition and data (action-data pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("bulk", "body")]
		public Task<TResponse> BulkAsync<TResponse>(PostData body, BulkRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_bulk", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_bulk <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/</para></summary>
		///<param name = "index">Default index for items which don&#x27;t provide one</param>
		///<param name = "body">The operation definition and data (action-data pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Bulk<TResponse>(string index, PostData body, BulkRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_bulk"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_bulk <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/</para></summary>
		///<param name = "index">Default index for items which don&#x27;t provide one</param>
		///<param name = "body">The operation definition and data (action-data pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("bulk", "index, body")]
		public Task<TResponse> BulkAsync<TResponse>(string index, PostData body, BulkRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_bulk"), ctx, body, RequestParams(requestParameters));
		///<summary>DELETE on /_search/scroll <para>https://opensearch.org/docs/latest/opensearch/rest-api/scroll/</para></summary>
		///<param name = "body">A comma-separated list of scroll IDs to clear if none was specified via the scroll_id parameter</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse ClearScroll<TResponse>(PostData body, ClearScrollRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(DELETE, "_search/scroll", body, RequestParams(requestParameters));
		///<summary>DELETE on /_search/scroll <para>https://opensearch.org/docs/latest/opensearch/rest-api/scroll/</para></summary>
		///<param name = "body">A comma-separated list of scroll IDs to clear if none was specified via the scroll_id parameter</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("clear_scroll", "body")]
		public Task<TResponse> ClearScrollAsync<TResponse>(PostData body, ClearScrollRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(DELETE, "_search/scroll", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_count <para>https://opensearch.org/docs/latest/opensearch/rest-api/count/</para></summary>
		///<param name = "body">A query to restrict the results specified with the Query DSL (optional)</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Count<TResponse>(PostData body, CountRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_count", body, RequestParams(requestParameters));
		///<summary>POST on /_count <para>https://opensearch.org/docs/latest/opensearch/rest-api/count/</para></summary>
		///<param name = "body">A query to restrict the results specified with the Query DSL (optional)</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("count", "body")]
		public Task<TResponse> CountAsync<TResponse>(PostData body, CountRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_count", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_count <para>https://opensearch.org/docs/latest/opensearch/rest-api/count/</para></summary>
		///<param name = "index">A comma-separated list of indices to restrict the results</param>
		///<param name = "body">A query to restrict the results specified with the Query DSL (optional)</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Count<TResponse>(string index, PostData body, CountRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_count"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_count <para>https://opensearch.org/docs/latest/opensearch/rest-api/count/</para></summary>
		///<param name = "index">A comma-separated list of indices to restrict the results</param>
		///<param name = "body">A query to restrict the results specified with the Query DSL (optional)</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("count", "index, body")]
		public Task<TResponse> CountAsync<TResponse>(string index, PostData body, CountRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_count"), ctx, body, RequestParams(requestParameters));
		///<summary>PUT on /{index}/_create/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">Document ID</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Create<TResponse>(string index, string id, PostData body, CreateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(PUT, Url($"{index:index}/_create/{id:id}"), body, RequestParams(requestParameters));
		///<summary>PUT on /{index}/_create/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">Document ID</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("create", "index, id, body")]
		public Task<TResponse> CreateAsync<TResponse>(string index, string id, PostData body, CreateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(PUT, Url($"{index:index}/_create/{id:id}"), ctx, body, RequestParams(requestParameters));
		///<summary>DELETE on /{index}/_doc/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Delete<TResponse>(string index, string id, DeleteRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(DELETE, Url($"{index:index}/_doc/{id:id}"), null, RequestParams(requestParameters));
		///<summary>DELETE on /{index}/_doc/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("delete", "index, id")]
		public Task<TResponse> DeleteAsync<TResponse>(string index, string id, DeleteRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(DELETE, Url($"{index:index}/_doc/{id:id}"), ctx, null, RequestParams(requestParameters));
		///<summary>POST on /{index}/_delete_by_query <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The search definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse DeleteByQuery<TResponse>(string index, PostData body, DeleteByQueryRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_delete_by_query"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_delete_by_query <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The search definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("delete_by_query", "index, body")]
		public Task<TResponse> DeleteByQueryAsync<TResponse>(string index, PostData body, DeleteByQueryRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_delete_by_query"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_delete_by_query/{task_id}/_rethrottle <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</para></summary>
		///<param name = "taskId">The task id to rethrottle</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse DeleteByQueryRethrottle<TResponse>(string taskId, DeleteByQueryRethrottleRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"_delete_by_query/{taskId:taskId}/_rethrottle"), null, RequestParams(requestParameters));
		///<summary>POST on /_delete_by_query/{task_id}/_rethrottle <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</para></summary>
		///<param name = "taskId">The task id to rethrottle</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("delete_by_query_rethrottle", "task_id")]
		public Task<TResponse> DeleteByQueryRethrottleAsync<TResponse>(string taskId, DeleteByQueryRethrottleRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"_delete_by_query/{taskId:taskId}/_rethrottle"), ctx, null, RequestParams(requestParameters));
		///<summary>DELETE on /_scripts/{id} <para></para></summary>
		///<param name = "id">Script ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse DeleteScript<TResponse>(string id, DeleteScriptRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(DELETE, Url($"_scripts/{id:id}"), null, RequestParams(requestParameters));
		///<summary>DELETE on /_scripts/{id} <para></para></summary>
		///<param name = "id">Script ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("delete_script", "id")]
		public Task<TResponse> DeleteScriptAsync<TResponse>(string id, DeleteScriptRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(DELETE, Url($"_scripts/{id:id}"), ctx, null, RequestParams(requestParameters));
		///<summary>HEAD on /{index}/_doc/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse DocumentExists<TResponse>(string index, string id, DocumentExistsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(HEAD, Url($"{index:index}/_doc/{id:id}"), null, RequestParams(requestParameters));
		///<summary>HEAD on /{index}/_doc/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("exists", "index, id")]
		public Task<TResponse> DocumentExistsAsync<TResponse>(string index, string id, DocumentExistsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(HEAD, Url($"{index:index}/_doc/{id:id}"), ctx, null, RequestParams(requestParameters));
		///<summary>HEAD on /{index}/_source/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse SourceExists<TResponse>(string index, string id, SourceExistsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(HEAD, Url($"{index:index}/_source/{id:id}"), null, RequestParams(requestParameters));
		///<summary>HEAD on /{index}/_source/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("exists_source", "index, id")]
		public Task<TResponse> SourceExistsAsync<TResponse>(string index, string id, SourceExistsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(HEAD, Url($"{index:index}/_source/{id:id}"), ctx, null, RequestParams(requestParameters));
		///<summary>POST on /{index}/_explain/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/explain/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "body">The query definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Explain<TResponse>(string index, string id, PostData body, ExplainRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_explain/{id:id}"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_explain/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/explain/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "body">The query definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("explain", "index, id, body")]
		public Task<TResponse> ExplainAsync<TResponse>(string index, string id, PostData body, ExplainRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_explain/{id:id}"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_field_caps <para></para></summary>
		///<param name = "body">An index filter specified with the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse FieldCapabilities<TResponse>(PostData body, FieldCapabilitiesRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_field_caps", body, RequestParams(requestParameters));
		///<summary>POST on /_field_caps <para></para></summary>
		///<param name = "body">An index filter specified with the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("field_caps", "body")]
		public Task<TResponse> FieldCapabilitiesAsync<TResponse>(PostData body, FieldCapabilitiesRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_field_caps", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_field_caps <para></para></summary>
		///<param name = "index">A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">An index filter specified with the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse FieldCapabilities<TResponse>(string index, PostData body, FieldCapabilitiesRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_field_caps"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_field_caps <para></para></summary>
		///<param name = "index">A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">An index filter specified with the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("field_caps", "index, body")]
		public Task<TResponse> FieldCapabilitiesAsync<TResponse>(string index, PostData body, FieldCapabilitiesRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_field_caps"), ctx, body, RequestParams(requestParameters));
		///<summary>GET on /{index}/_doc/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Get<TResponse>(string index, string id, GetRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"{index:index}/_doc/{id:id}"), null, RequestParams(requestParameters));
		///<summary>GET on /{index}/_doc/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("get", "index, id")]
		public Task<TResponse> GetAsync<TResponse>(string index, string id, GetRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"{index:index}/_doc/{id:id}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_scripts/{id} <para></para></summary>
		///<param name = "id">Script ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse GetScript<TResponse>(string id, GetScriptRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"_scripts/{id:id}"), null, RequestParams(requestParameters));
		///<summary>GET on /_scripts/{id} <para></para></summary>
		///<param name = "id">Script ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("get_script", "id")]
		public Task<TResponse> GetScriptAsync<TResponse>(string id, GetScriptRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"_scripts/{id:id}"), ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_script_context <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		public TResponse GetScriptContext<TResponse>(GetScriptContextRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_script_context", null, RequestParams(requestParameters));
		///<summary>GET on /_script_context <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		[MapsApi("get_script_context", "")]
		public Task<TResponse> GetScriptContextAsync<TResponse>(GetScriptContextRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_script_context", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /_script_language <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		public TResponse GetScriptLanguages<TResponse>(GetScriptLanguagesRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "_script_language", null, RequestParams(requestParameters));
		///<summary>GET on /_script_language <para></para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		[MapsApi("get_script_languages", "")]
		public Task<TResponse> GetScriptLanguagesAsync<TResponse>(GetScriptLanguagesRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "_script_language", ctx, null, RequestParams(requestParameters));
		///<summary>GET on /{index}/_source/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Source<TResponse>(string index, string id, SourceRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, Url($"{index:index}/_source/{id:id}"), null, RequestParams(requestParameters));
		///<summary>GET on /{index}/_source/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">The document ID</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("get_source", "index, id")]
		public Task<TResponse> SourceAsync<TResponse>(string index, string id, SourceRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, Url($"{index:index}/_source/{id:id}"), ctx, null, RequestParams(requestParameters));
		///<summary>PUT on /{index}/_doc/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">Document ID</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Index<TResponse>(string index, string id, PostData body, IndexRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(PUT, Url($"{index:index}/_doc/{id:id}"), body, RequestParams(requestParameters));
		///<summary>PUT on /{index}/_doc/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">Document ID</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("index", "index, id, body")]
		public Task<TResponse> IndexAsync<TResponse>(string index, string id, PostData body, IndexRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(PUT, Url($"{index:index}/_doc/{id:id}"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_doc <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Index<TResponse>(string index, PostData body, IndexRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_doc"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_doc <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("index", "index, body")]
		public Task<TResponse> IndexAsync<TResponse>(string index, PostData body, IndexRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_doc"), ctx, body, RequestParams(requestParameters));
		///<summary>GET on / <para>https://opensearch.org/docs/latest/opensearch/index/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse RootNodeInfo<TResponse>(RootNodeInfoRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(GET, "", null, RequestParams(requestParameters));
		///<summary>GET on / <para>https://opensearch.org/docs/latest/opensearch/index/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("info", "")]
		public Task<TResponse> RootNodeInfoAsync<TResponse>(RootNodeInfoRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(GET, "", ctx, null, RequestParams(requestParameters));
		///<summary>POST on /_mget <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/</para></summary>
		///<param name = "body">Document identifiers; can be either `docs` (containing full document information) or `ids` (when index and type is provided in the URL.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse MultiGet<TResponse>(PostData body, MultiGetRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_mget", body, RequestParams(requestParameters));
		///<summary>POST on /_mget <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/</para></summary>
		///<param name = "body">Document identifiers; can be either `docs` (containing full document information) or `ids` (when index and type is provided in the URL.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("mget", "body")]
		public Task<TResponse> MultiGetAsync<TResponse>(PostData body, MultiGetRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_mget", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_mget <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "body">Document identifiers; can be either `docs` (containing full document information) or `ids` (when index and type is provided in the URL.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse MultiGet<TResponse>(string index, PostData body, MultiGetRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_mget"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_mget <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "body">Document identifiers; can be either `docs` (containing full document information) or `ids` (when index and type is provided in the URL.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("mget", "index, body")]
		public Task<TResponse> MultiGetAsync<TResponse>(string index, PostData body, MultiGetRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_mget"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_msearch <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
		///<param name = "body">The request definitions (metadata-search request definition pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse MultiSearch<TResponse>(PostData body, MultiSearchRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_msearch", body, RequestParams(requestParameters));
		///<summary>POST on /_msearch <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
		///<param name = "body">The request definitions (metadata-search request definition pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("msearch", "body")]
		public Task<TResponse> MultiSearchAsync<TResponse>(PostData body, MultiSearchRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_msearch", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_msearch <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
		///<param name = "index">A comma-separated list of index names to use as default</param>
		///<param name = "body">The request definitions (metadata-search request definition pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse MultiSearch<TResponse>(string index, PostData body, MultiSearchRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_msearch"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_msearch <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
		///<param name = "index">A comma-separated list of index names to use as default</param>
		///<param name = "body">The request definitions (metadata-search request definition pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("msearch", "index, body")]
		public Task<TResponse> MultiSearchAsync<TResponse>(string index, PostData body, MultiSearchRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_msearch"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_msearch/template <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
		///<param name = "body">The request definitions (metadata-search request definition pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse MultiSearchTemplate<TResponse>(PostData body, MultiSearchTemplateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_msearch/template", body, RequestParams(requestParameters));
		///<summary>POST on /_msearch/template <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
		///<param name = "body">The request definitions (metadata-search request definition pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("msearch_template", "body")]
		public Task<TResponse> MultiSearchTemplateAsync<TResponse>(PostData body, MultiSearchTemplateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_msearch/template", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_msearch/template <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
		///<param name = "index">A comma-separated list of index names to use as default</param>
		///<param name = "body">The request definitions (metadata-search request definition pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse MultiSearchTemplate<TResponse>(string index, PostData body, MultiSearchTemplateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_msearch/template"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_msearch/template <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
		///<param name = "index">A comma-separated list of index names to use as default</param>
		///<param name = "body">The request definitions (metadata-search request definition pairs), separated by newlines</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("msearch_template", "index, body")]
		public Task<TResponse> MultiSearchTemplateAsync<TResponse>(string index, PostData body, MultiSearchTemplateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_msearch/template"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_mtermvectors <para></para></summary>
		///<param name = "body">Define ids, documents, parameters or a list of parameters per document here. You must at least provide a list of document ids. See documentation.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse MultiTermVectors<TResponse>(PostData body, MultiTermVectorsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_mtermvectors", body, RequestParams(requestParameters));
		///<summary>POST on /_mtermvectors <para></para></summary>
		///<param name = "body">Define ids, documents, parameters or a list of parameters per document here. You must at least provide a list of document ids. See documentation.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("mtermvectors", "body")]
		public Task<TResponse> MultiTermVectorsAsync<TResponse>(PostData body, MultiTermVectorsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_mtermvectors", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_mtermvectors <para></para></summary>
		///<param name = "index">The index in which the document resides.</param>
		///<param name = "body">Define ids, documents, parameters or a list of parameters per document here. You must at least provide a list of document ids. See documentation.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse MultiTermVectors<TResponse>(string index, PostData body, MultiTermVectorsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_mtermvectors"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_mtermvectors <para></para></summary>
		///<param name = "index">The index in which the document resides.</param>
		///<param name = "body">Define ids, documents, parameters or a list of parameters per document here. You must at least provide a list of document ids. See documentation.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("mtermvectors", "index, body")]
		public Task<TResponse> MultiTermVectorsAsync<TResponse>(string index, PostData body, MultiTermVectorsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_mtermvectors"), ctx, body, RequestParams(requestParameters));
		///<summary>HEAD on / <para>https://opensearch.org/docs/latest/opensearch/index/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Ping<TResponse>(PingRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(HEAD, "", null, RequestParams(requestParameters));
		///<summary>HEAD on / <para>https://opensearch.org/docs/latest/opensearch/index/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("ping", "")]
		public Task<TResponse> PingAsync<TResponse>(PingRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(HEAD, "", ctx, null, RequestParams(requestParameters));
		///<summary>PUT on /_scripts/{id} <para></para></summary>
		///<param name = "id">Script ID</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse PutScript<TResponse>(string id, PostData body, PutScriptRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(PUT, Url($"_scripts/{id:id}"), body, RequestParams(requestParameters));
		///<summary>PUT on /_scripts/{id} <para></para></summary>
		///<param name = "id">Script ID</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("put_script", "id, body")]
		public Task<TResponse> PutScriptAsync<TResponse>(string id, PostData body, PutScriptRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(PUT, Url($"_scripts/{id:id}"), ctx, body, RequestParams(requestParameters));
		///<summary>PUT on /_scripts/{id}/{context} <para></para></summary>
		///<param name = "id">Script ID</param>
		///<param name = "context">Script context</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse PutScript<TResponse>(string id, string context, PostData body, PutScriptRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(PUT, Url($"_scripts/{id:id}/{context:context}"), body, RequestParams(requestParameters));
		///<summary>PUT on /_scripts/{id}/{context} <para></para></summary>
		///<param name = "id">Script ID</param>
		///<param name = "context">Script context</param>
		///<param name = "body">The document</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("put_script", "id, context, body")]
		public Task<TResponse> PutScriptAsync<TResponse>(string id, string context, PostData body, PutScriptRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(PUT, Url($"_scripts/{id:id}/{context:context}"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_rank_eval <para></para></summary>
		///<param name = "body">The ranking evaluation search definition, including search requests, document ratings and ranking metric definition.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		public TResponse RankEval<TResponse>(PostData body, RankEvalRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_rank_eval", body, RequestParams(requestParameters));
		///<summary>POST on /_rank_eval <para></para></summary>
		///<param name = "body">The ranking evaluation search definition, including search requests, document ratings and ranking metric definition.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		[MapsApi("rank_eval", "body")]
		public Task<TResponse> RankEvalAsync<TResponse>(PostData body, RankEvalRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_rank_eval", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_rank_eval <para></para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The ranking evaluation search definition, including search requests, document ratings and ranking metric definition.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		public TResponse RankEval<TResponse>(string index, PostData body, RankEvalRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_rank_eval"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_rank_eval <para></para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The ranking evaluation search definition, including search requests, document ratings and ranking metric definition.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		[MapsApi("rank_eval", "index, body")]
		public Task<TResponse> RankEvalAsync<TResponse>(string index, PostData body, RankEvalRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_rank_eval"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_reindex <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</para></summary>
		///<param name = "body">The search definition using the Query DSL and the prototype for the index request.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse ReindexOnServer<TResponse>(PostData body, ReindexOnServerRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_reindex", body, RequestParams(requestParameters));
		///<summary>POST on /_reindex <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</para></summary>
		///<param name = "body">The search definition using the Query DSL and the prototype for the index request.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("reindex", "body")]
		public Task<TResponse> ReindexOnServerAsync<TResponse>(PostData body, ReindexOnServerRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_reindex", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_reindex/{task_id}/_rethrottle <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</para></summary>
		///<param name = "taskId">The task id to rethrottle</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse ReindexRethrottle<TResponse>(string taskId, ReindexRethrottleRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"_reindex/{taskId:taskId}/_rethrottle"), null, RequestParams(requestParameters));
		///<summary>POST on /_reindex/{task_id}/_rethrottle <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</para></summary>
		///<param name = "taskId">The task id to rethrottle</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("reindex_rethrottle", "task_id")]
		public Task<TResponse> ReindexRethrottleAsync<TResponse>(string taskId, ReindexRethrottleRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"_reindex/{taskId:taskId}/_rethrottle"), ctx, null, RequestParams(requestParameters));
		///<summary>POST on /_render/template <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
		///<param name = "body">The search definition template and its params</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse RenderSearchTemplate<TResponse>(PostData body, RenderSearchTemplateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_render/template", body, RequestParams(requestParameters));
		///<summary>POST on /_render/template <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
		///<param name = "body">The search definition template and its params</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("render_search_template", "body")]
		public Task<TResponse> RenderSearchTemplateAsync<TResponse>(PostData body, RenderSearchTemplateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_render/template", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_render/template/{id} <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
		///<param name = "id">The id of the stored search template</param>
		///<param name = "body">The search definition template and its params</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse RenderSearchTemplate<TResponse>(string id, PostData body, RenderSearchTemplateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"_render/template/{id:id}"), body, RequestParams(requestParameters));
		///<summary>POST on /_render/template/{id} <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
		///<param name = "id">The id of the stored search template</param>
		///<param name = "body">The search definition template and its params</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("render_search_template", "id, body")]
		public Task<TResponse> RenderSearchTemplateAsync<TResponse>(string id, PostData body, RenderSearchTemplateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"_render/template/{id:id}"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_scripts/painless/_execute <para></para></summary>
		///<param name = "body">The script to execute</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		public TResponse ExecutePainlessScript<TResponse>(PostData body, ExecutePainlessScriptRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_scripts/painless/_execute", body, RequestParams(requestParameters));
		///<summary>POST on /_scripts/painless/_execute <para></para></summary>
		///<param name = "body">The script to execute</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		///<remarks>Note: Experimental within the OpenSearch server, this functionality is Experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features. This functionality is subject to potential breaking changes within a minor version, meaning that your referencing code may break when this library is upgraded.</remarks>
		[MapsApi("scripts_painless_execute", "body")]
		public Task<TResponse> ExecutePainlessScriptAsync<TResponse>(PostData body, ExecutePainlessScriptRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_scripts/painless/_execute", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_search/scroll <para>https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body</para></summary>
		///<param name = "body">The scroll ID if not passed by URL or query parameter.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Scroll<TResponse>(PostData body, ScrollRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_search/scroll", body, RequestParams(requestParameters));
		///<summary>POST on /_search/scroll <para>https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body</para></summary>
		///<param name = "body">The scroll ID if not passed by URL or query parameter.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("scroll", "body")]
		public Task<TResponse> ScrollAsync<TResponse>(PostData body, ScrollRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_search/scroll", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_search <para>https://opensearch.org/docs/latest/opensearch/rest-api/search/</para></summary>
		///<param name = "body">The search definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Search<TResponse>(PostData body, SearchRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_search", body, RequestParams(requestParameters));
		///<summary>POST on /_search <para>https://opensearch.org/docs/latest/opensearch/rest-api/search/</para></summary>
		///<param name = "body">The search definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("search", "body")]
		public Task<TResponse> SearchAsync<TResponse>(PostData body, SearchRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_search", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_search <para>https://opensearch.org/docs/latest/opensearch/rest-api/search/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The search definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Search<TResponse>(string index, PostData body, SearchRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_search"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_search <para>https://opensearch.org/docs/latest/opensearch/rest-api/search/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The search definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("search", "index, body")]
		public Task<TResponse> SearchAsync<TResponse>(string index, PostData body, SearchRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_search"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_search_shards <para>https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse SearchShards<TResponse>(SearchShardsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_search_shards", null, RequestParams(requestParameters));
		///<summary>POST on /_search_shards <para>https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/</para></summary>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("search_shards", "")]
		public Task<TResponse> SearchShardsAsync<TResponse>(SearchShardsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_search_shards", ctx, null, RequestParams(requestParameters));
		///<summary>POST on /{index}/_search_shards <para>https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse SearchShards<TResponse>(string index, SearchShardsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_search_shards"), null, RequestParams(requestParameters));
		///<summary>POST on /{index}/_search_shards <para>https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("search_shards", "index")]
		public Task<TResponse> SearchShardsAsync<TResponse>(string index, SearchShardsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_search_shards"), ctx, null, RequestParams(requestParameters));
		///<summary>POST on /_search/template <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
		///<param name = "body">The search definition template and its params</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse SearchTemplate<TResponse>(PostData body, SearchTemplateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, "_search/template", body, RequestParams(requestParameters));
		///<summary>POST on /_search/template <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
		///<param name = "body">The search definition template and its params</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("search_template", "body")]
		public Task<TResponse> SearchTemplateAsync<TResponse>(PostData body, SearchTemplateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, "_search/template", ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_search/template <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The search definition template and its params</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse SearchTemplate<TResponse>(string index, PostData body, SearchTemplateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_search/template"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_search/template <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The search definition template and its params</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("search_template", "index, body")]
		public Task<TResponse> SearchTemplateAsync<TResponse>(string index, PostData body, SearchTemplateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_search/template"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_termvectors/{id} <para></para></summary>
		///<param name = "index">The index in which the document resides.</param>
		///<param name = "id">The id of the document, when not specified a doc param should be supplied.</param>
		///<param name = "body">Define parameters and or supply a document to get termvectors for. See documentation.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse TermVectors<TResponse>(string index, string id, PostData body, TermVectorsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_termvectors/{id:id}"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_termvectors/{id} <para></para></summary>
		///<param name = "index">The index in which the document resides.</param>
		///<param name = "id">The id of the document, when not specified a doc param should be supplied.</param>
		///<param name = "body">Define parameters and or supply a document to get termvectors for. See documentation.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("termvectors", "index, id, body")]
		public Task<TResponse> TermVectorsAsync<TResponse>(string index, string id, PostData body, TermVectorsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_termvectors/{id:id}"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_termvectors <para></para></summary>
		///<param name = "index">The index in which the document resides.</param>
		///<param name = "body">Define parameters and or supply a document to get termvectors for. See documentation.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse TermVectors<TResponse>(string index, PostData body, TermVectorsRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_termvectors"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_termvectors <para></para></summary>
		///<param name = "index">The index in which the document resides.</param>
		///<param name = "body">Define parameters and or supply a document to get termvectors for. See documentation.</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("termvectors", "index, body")]
		public Task<TResponse> TermVectorsAsync<TResponse>(string index, PostData body, TermVectorsRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_termvectors"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_update/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">Document ID</param>
		///<param name = "body">The request definition requires either `script` or partial `doc`</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse Update<TResponse>(string index, string id, PostData body, UpdateRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_update/{id:id}"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_update/{id} <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/</para></summary>
		///<param name = "index">The name of the index</param>
		///<param name = "id">Document ID</param>
		///<param name = "body">The request definition requires either `script` or partial `doc`</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("update", "index, id, body")]
		public Task<TResponse> UpdateAsync<TResponse>(string index, string id, PostData body, UpdateRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_update/{id:id}"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_update_by_query <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The search definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse UpdateByQuery<TResponse>(string index, PostData body, UpdateByQueryRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"{index:index}/_update_by_query"), body, RequestParams(requestParameters));
		///<summary>POST on /{index}/_update_by_query <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</para></summary>
		///<param name = "index">A comma-separated list of index names to search; use the special string `_all` or Indices.All to perform the operation on all indices</param>
		///<param name = "body">The search definition using the Query DSL</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("update_by_query", "index, body")]
		public Task<TResponse> UpdateByQueryAsync<TResponse>(string index, PostData body, UpdateByQueryRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"{index:index}/_update_by_query"), ctx, body, RequestParams(requestParameters));
		///<summary>POST on /_update_by_query/{task_id}/_rethrottle <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</para></summary>
		///<param name = "taskId">The task id to rethrottle</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		public TResponse UpdateByQueryRethrottle<TResponse>(string taskId, UpdateByQueryRethrottleRequestParameters requestParameters = null)
			where TResponse : class, IOpenSearchResponse, new() => DoRequest<TResponse>(POST, Url($"_update_by_query/{taskId:taskId}/_rethrottle"), null, RequestParams(requestParameters));
		///<summary>POST on /_update_by_query/{task_id}/_rethrottle <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</para></summary>
		///<param name = "taskId">The task id to rethrottle</param>
		///<param name = "requestParameters">Request specific configuration such as querystring parameters &amp; request specific connection settings.</param>
		[MapsApi("update_by_query_rethrottle", "task_id")]
		public Task<TResponse> UpdateByQueryRethrottleAsync<TResponse>(string taskId, UpdateByQueryRethrottleRequestParameters requestParameters = null, CancellationToken ctx = default)
			where TResponse : class, IOpenSearchResponse, new() => DoRequestAsync<TResponse>(POST, Url($"_update_by_query/{taskId:taskId}/_rethrottle"), ctx, null, RequestParams(requestParameters));
	}
}
