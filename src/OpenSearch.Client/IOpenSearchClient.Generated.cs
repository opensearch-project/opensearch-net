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
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OpenSearch.Client.Specification.CatApi;
using OpenSearch.Client.Specification.ClusterApi;
using OpenSearch.Client.Specification.DanglingIndicesApi;
using OpenSearch.Client.Specification.IndicesApi;
using OpenSearch.Client.Specification.IngestApi;
using OpenSearch.Client.Specification.NodesApi;
using OpenSearch.Client.Specification.SnapshotApi;
using OpenSearch.Client.Specification.TasksApi;
using OpenSearch.Client;

namespace OpenSearch.Client
{
	///<summary>
	///OpenSearch high level client
	///</summary>
	public partial interface IOpenSearchClient
	{
		///<summary>Dangling Indices APIs</summary>
		DanglingIndicesNamespace DanglingIndices
		{
			get;
		}

		///<summary>Ingest APIs</summary>
		IngestNamespace Ingest
		{
			get;
		}

		///<summary>Nodes APIs</summary>
		NodesNamespace Nodes
		{
			get;
		}

		/// <summary>
		/// <c>POST</c> request to the <c>bulk</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/</a>
		/// </summary>
		BulkResponse Bulk(Func<BulkDescriptor, IBulkRequest> selector);
		/// <summary>
		/// <c>POST</c> request to the <c>bulk</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/</a>
		/// </summary>
		Task<BulkResponse> BulkAsync(Func<BulkDescriptor, IBulkRequest> selector, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>bulk</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/</a>
		/// </summary>
		BulkResponse Bulk(IBulkRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>bulk</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/</a>
		/// </summary>
		Task<BulkResponse> BulkAsync(IBulkRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>DELETE</c> request to the <c>clear_scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/scroll/">https://opensearch.org/docs/latest/opensearch/rest-api/scroll/</a>
		/// </summary>
		ClearScrollResponse ClearScroll(Func<ClearScrollDescriptor, IClearScrollRequest> selector = null);
		/// <summary>
		/// <c>DELETE</c> request to the <c>clear_scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/scroll/">https://opensearch.org/docs/latest/opensearch/rest-api/scroll/</a>
		/// </summary>
		Task<ClearScrollResponse> ClearScrollAsync(Func<ClearScrollDescriptor, IClearScrollRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>DELETE</c> request to the <c>clear_scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/scroll/">https://opensearch.org/docs/latest/opensearch/rest-api/scroll/</a>
		/// </summary>
		ClearScrollResponse ClearScroll(IClearScrollRequest request);
		/// <summary>
		/// <c>DELETE</c> request to the <c>clear_scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/scroll/">https://opensearch.org/docs/latest/opensearch/rest-api/scroll/</a>
		/// </summary>
		Task<ClearScrollResponse> ClearScrollAsync(IClearScrollRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>count</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/count/">https://opensearch.org/docs/latest/opensearch/rest-api/count/</a>
		/// </summary>
		CountResponse Count<TDocument>(Func<CountDescriptor<TDocument>, ICountRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>count</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/count/">https://opensearch.org/docs/latest/opensearch/rest-api/count/</a>
		/// </summary>
		Task<CountResponse> CountAsync<TDocument>(Func<CountDescriptor<TDocument>, ICountRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>count</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/count/">https://opensearch.org/docs/latest/opensearch/rest-api/count/</a>
		/// </summary>
		CountResponse Count(ICountRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>count</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/count/">https://opensearch.org/docs/latest/opensearch/rest-api/count/</a>
		/// </summary>
		Task<CountResponse> CountAsync(ICountRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>PUT</c> request to the <c>create</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</a>
		/// </summary>
		CreateResponse Create<TDocument>(TDocument document, Func<CreateDescriptor<TDocument>, ICreateRequest<TDocument>> selector)
			where TDocument : class;
		/// <summary>
		/// <c>PUT</c> request to the <c>create</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</a>
		/// </summary>
		Task<CreateResponse> CreateAsync<TDocument>(TDocument document, Func<CreateDescriptor<TDocument>, ICreateRequest<TDocument>> selector, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>PUT</c> request to the <c>create</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</a>
		/// </summary>
		CreateResponse Create<TDocument>(ICreateRequest<TDocument> request)
			where TDocument : class;
		/// <summary>
		/// <c>PUT</c> request to the <c>create</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</a>
		/// </summary>
		Task<CreateResponse> CreateAsync<TDocument>(ICreateRequest<TDocument> request, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>DELETE</c> request to the <c>delete</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/</a>
		/// </summary>
		DeleteResponse Delete<TDocument>(DocumentPath<TDocument> id, Func<DeleteDescriptor<TDocument>, IDeleteRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>DELETE</c> request to the <c>delete</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/</a>
		/// </summary>
		Task<DeleteResponse> DeleteAsync<TDocument>(DocumentPath<TDocument> id, Func<DeleteDescriptor<TDocument>, IDeleteRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>DELETE</c> request to the <c>delete</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/</a>
		/// </summary>
		DeleteResponse Delete(IDeleteRequest request);
		/// <summary>
		/// <c>DELETE</c> request to the <c>delete</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/</a>
		/// </summary>
		Task<DeleteResponse> DeleteAsync(IDeleteRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>delete_by_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</a>
		/// </summary>
		DeleteByQueryResponse DeleteByQuery<TDocument>(Func<DeleteByQueryDescriptor<TDocument>, IDeleteByQueryRequest> selector)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>delete_by_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</a>
		/// </summary>
		Task<DeleteByQueryResponse> DeleteByQueryAsync<TDocument>(Func<DeleteByQueryDescriptor<TDocument>, IDeleteByQueryRequest> selector, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>delete_by_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</a>
		/// </summary>
		DeleteByQueryResponse DeleteByQuery(IDeleteByQueryRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>delete_by_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</a>
		/// </summary>
		Task<DeleteByQueryResponse> DeleteByQueryAsync(IDeleteByQueryRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>delete_by_query_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</a>
		/// </summary>
		ListTasksResponse DeleteByQueryRethrottle(TaskId taskId, Func<DeleteByQueryRethrottleDescriptor, IDeleteByQueryRethrottleRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>delete_by_query_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</a>
		/// </summary>
		Task<ListTasksResponse> DeleteByQueryRethrottleAsync(TaskId taskId, Func<DeleteByQueryRethrottleDescriptor, IDeleteByQueryRethrottleRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>delete_by_query_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</a>
		/// </summary>
		ListTasksResponse DeleteByQueryRethrottle(IDeleteByQueryRethrottleRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>delete_by_query_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</a>
		/// </summary>
		Task<ListTasksResponse> DeleteByQueryRethrottleAsync(IDeleteByQueryRethrottleRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>DELETE</c> request to the <c>delete_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		DeleteScriptResponse DeleteScript(Id id, Func<DeleteScriptDescriptor, IDeleteScriptRequest> selector = null);
		/// <summary>
		/// <c>DELETE</c> request to the <c>delete_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<DeleteScriptResponse> DeleteScriptAsync(Id id, Func<DeleteScriptDescriptor, IDeleteScriptRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>DELETE</c> request to the <c>delete_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		DeleteScriptResponse DeleteScript(IDeleteScriptRequest request);
		/// <summary>
		/// <c>DELETE</c> request to the <c>delete_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<DeleteScriptResponse> DeleteScriptAsync(IDeleteScriptRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>HEAD</c> request to the <c>exists</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		ExistsResponse DocumentExists<TDocument>(DocumentPath<TDocument> id, Func<DocumentExistsDescriptor<TDocument>, IDocumentExistsRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>HEAD</c> request to the <c>exists</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		Task<ExistsResponse> DocumentExistsAsync<TDocument>(DocumentPath<TDocument> id, Func<DocumentExistsDescriptor<TDocument>, IDocumentExistsRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>HEAD</c> request to the <c>exists</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		ExistsResponse DocumentExists(IDocumentExistsRequest request);
		/// <summary>
		/// <c>HEAD</c> request to the <c>exists</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		Task<ExistsResponse> DocumentExistsAsync(IDocumentExistsRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>HEAD</c> request to the <c>exists_source</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		ExistsResponse SourceExists<TDocument>(DocumentPath<TDocument> id, Func<SourceExistsDescriptor<TDocument>, ISourceExistsRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>HEAD</c> request to the <c>exists_source</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		Task<ExistsResponse> SourceExistsAsync<TDocument>(DocumentPath<TDocument> id, Func<SourceExistsDescriptor<TDocument>, ISourceExistsRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>HEAD</c> request to the <c>exists_source</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		ExistsResponse SourceExists(ISourceExistsRequest request);
		/// <summary>
		/// <c>HEAD</c> request to the <c>exists_source</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		Task<ExistsResponse> SourceExistsAsync(ISourceExistsRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>explain</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/explain/">https://opensearch.org/docs/latest/opensearch/rest-api/explain/</a>
		/// </summary>
		ExplainResponse<TDocument> Explain<TDocument>(DocumentPath<TDocument> id, Func<ExplainDescriptor<TDocument>, IExplainRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>explain</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/explain/">https://opensearch.org/docs/latest/opensearch/rest-api/explain/</a>
		/// </summary>
		Task<ExplainResponse<TDocument>> ExplainAsync<TDocument>(DocumentPath<TDocument> id, Func<ExplainDescriptor<TDocument>, IExplainRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>explain</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/explain/">https://opensearch.org/docs/latest/opensearch/rest-api/explain/</a>
		/// </summary>
		ExplainResponse<TDocument> Explain<TDocument>(IExplainRequest request)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>explain</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/explain/">https://opensearch.org/docs/latest/opensearch/rest-api/explain/</a>
		/// </summary>
		Task<ExplainResponse<TDocument>> ExplainAsync<TDocument>(IExplainRequest request, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>field_caps</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		FieldCapabilitiesResponse FieldCapabilities(Indices index = null, Func<FieldCapabilitiesDescriptor, IFieldCapabilitiesRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>field_caps</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<FieldCapabilitiesResponse> FieldCapabilitiesAsync(Indices index = null, Func<FieldCapabilitiesDescriptor, IFieldCapabilitiesRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>field_caps</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		FieldCapabilitiesResponse FieldCapabilities(IFieldCapabilitiesRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>field_caps</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<FieldCapabilitiesResponse> FieldCapabilitiesAsync(IFieldCapabilitiesRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>GET</c> request to the <c>get</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		GetResponse<TDocument> Get<TDocument>(DocumentPath<TDocument> id, Func<GetDescriptor<TDocument>, IGetRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>GET</c> request to the <c>get</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		Task<GetResponse<TDocument>> GetAsync<TDocument>(DocumentPath<TDocument> id, Func<GetDescriptor<TDocument>, IGetRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>GET</c> request to the <c>get</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		GetResponse<TDocument> Get<TDocument>(IGetRequest request)
			where TDocument : class;
		/// <summary>
		/// <c>GET</c> request to the <c>get</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		Task<GetResponse<TDocument>> GetAsync<TDocument>(IGetRequest request, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>GET</c> request to the <c>get_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		GetScriptResponse GetScript(Id id, Func<GetScriptDescriptor, IGetScriptRequest> selector = null);
		/// <summary>
		/// <c>GET</c> request to the <c>get_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<GetScriptResponse> GetScriptAsync(Id id, Func<GetScriptDescriptor, IGetScriptRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>GET</c> request to the <c>get_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		GetScriptResponse GetScript(IGetScriptRequest request);
		/// <summary>
		/// <c>GET</c> request to the <c>get_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<GetScriptResponse> GetScriptAsync(IGetScriptRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>GET</c> request to the <c>get_source</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		SourceResponse<TDocument> Source<TDocument>(DocumentPath<TDocument> id, Func<SourceDescriptor<TDocument>, ISourceRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>GET</c> request to the <c>get_source</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		Task<SourceResponse<TDocument>> SourceAsync<TDocument>(DocumentPath<TDocument> id, Func<SourceDescriptor<TDocument>, ISourceRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>GET</c> request to the <c>get_source</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		SourceResponse<TDocument> Source<TDocument>(ISourceRequest request)
			where TDocument : class;
		/// <summary>
		/// <c>GET</c> request to the <c>get_source</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		Task<SourceResponse<TDocument>> SourceAsync<TDocument>(ISourceRequest request, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>PUT</c> request to the <c>index</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</a>
		/// </summary>
		IndexResponse Index<TDocument>(TDocument document, Func<IndexDescriptor<TDocument>, IIndexRequest<TDocument>> selector)
			where TDocument : class;
		/// <summary>
		/// <c>PUT</c> request to the <c>index</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</a>
		/// </summary>
		Task<IndexResponse> IndexAsync<TDocument>(TDocument document, Func<IndexDescriptor<TDocument>, IIndexRequest<TDocument>> selector, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>PUT</c> request to the <c>index</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</a>
		/// </summary>
		IndexResponse Index<TDocument>(IIndexRequest<TDocument> request)
			where TDocument : class;
		/// <summary>
		/// <c>PUT</c> request to the <c>index</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</a>
		/// </summary>
		Task<IndexResponse> IndexAsync<TDocument>(IIndexRequest<TDocument> request, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>GET</c> request to the <c>info</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/index/">https://opensearch.org/docs/latest/opensearch/index/</a>
		/// </summary>
		RootNodeInfoResponse RootNodeInfo(Func<RootNodeInfoDescriptor, IRootNodeInfoRequest> selector = null);
		/// <summary>
		/// <c>GET</c> request to the <c>info</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/index/">https://opensearch.org/docs/latest/opensearch/index/</a>
		/// </summary>
		Task<RootNodeInfoResponse> RootNodeInfoAsync(Func<RootNodeInfoDescriptor, IRootNodeInfoRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>GET</c> request to the <c>info</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/index/">https://opensearch.org/docs/latest/opensearch/index/</a>
		/// </summary>
		RootNodeInfoResponse RootNodeInfo(IRootNodeInfoRequest request);
		/// <summary>
		/// <c>GET</c> request to the <c>info</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/index/">https://opensearch.org/docs/latest/opensearch/index/</a>
		/// </summary>
		Task<RootNodeInfoResponse> RootNodeInfoAsync(IRootNodeInfoRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>mget</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/</a>
		/// </summary>
		MultiGetResponse MultiGet(Func<MultiGetDescriptor, IMultiGetRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>mget</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/</a>
		/// </summary>
		Task<MultiGetResponse> MultiGetAsync(Func<MultiGetDescriptor, IMultiGetRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>mget</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/</a>
		/// </summary>
		MultiGetResponse MultiGet(IMultiGetRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>mget</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/</a>
		/// </summary>
		Task<MultiGetResponse> MultiGetAsync(IMultiGetRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>msearch</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/">https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</a>
		/// </summary>
		MultiSearchResponse MultiSearch(Indices index = null, Func<MultiSearchDescriptor, IMultiSearchRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>msearch</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/">https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</a>
		/// </summary>
		Task<MultiSearchResponse> MultiSearchAsync(Indices index = null, Func<MultiSearchDescriptor, IMultiSearchRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>msearch</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/">https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</a>
		/// </summary>
		MultiSearchResponse MultiSearch(IMultiSearchRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>msearch</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/">https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</a>
		/// </summary>
		Task<MultiSearchResponse> MultiSearchAsync(IMultiSearchRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>msearch_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/">https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</a>
		/// </summary>
		MultiSearchResponse MultiSearchTemplate(Indices index = null, Func<MultiSearchTemplateDescriptor, IMultiSearchTemplateRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>msearch_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/">https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</a>
		/// </summary>
		Task<MultiSearchResponse> MultiSearchTemplateAsync(Indices index = null, Func<MultiSearchTemplateDescriptor, IMultiSearchTemplateRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>msearch_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/">https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</a>
		/// </summary>
		MultiSearchResponse MultiSearchTemplate(IMultiSearchTemplateRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>msearch_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/">https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</a>
		/// </summary>
		Task<MultiSearchResponse> MultiSearchTemplateAsync(IMultiSearchTemplateRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>mtermvectors</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		MultiTermVectorsResponse MultiTermVectors(Func<MultiTermVectorsDescriptor, IMultiTermVectorsRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>mtermvectors</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<MultiTermVectorsResponse> MultiTermVectorsAsync(Func<MultiTermVectorsDescriptor, IMultiTermVectorsRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>mtermvectors</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		MultiTermVectorsResponse MultiTermVectors(IMultiTermVectorsRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>mtermvectors</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<MultiTermVectorsResponse> MultiTermVectorsAsync(IMultiTermVectorsRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>HEAD</c> request to the <c>ping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/index/">https://opensearch.org/docs/latest/opensearch/index/</a>
		/// </summary>
		PingResponse Ping(Func<PingDescriptor, IPingRequest> selector = null);
		/// <summary>
		/// <c>HEAD</c> request to the <c>ping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/index/">https://opensearch.org/docs/latest/opensearch/index/</a>
		/// </summary>
		Task<PingResponse> PingAsync(Func<PingDescriptor, IPingRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>HEAD</c> request to the <c>ping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/index/">https://opensearch.org/docs/latest/opensearch/index/</a>
		/// </summary>
		PingResponse Ping(IPingRequest request);
		/// <summary>
		/// <c>HEAD</c> request to the <c>ping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/index/">https://opensearch.org/docs/latest/opensearch/index/</a>
		/// </summary>
		Task<PingResponse> PingAsync(IPingRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>PUT</c> request to the <c>put_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		PutScriptResponse PutScript(Id id, Func<PutScriptDescriptor, IPutScriptRequest> selector);
		/// <summary>
		/// <c>PUT</c> request to the <c>put_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<PutScriptResponse> PutScriptAsync(Id id, Func<PutScriptDescriptor, IPutScriptRequest> selector, CancellationToken ct = default);
		/// <summary>
		/// <c>PUT</c> request to the <c>put_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		PutScriptResponse PutScript(IPutScriptRequest request);
		/// <summary>
		/// <c>PUT</c> request to the <c>put_script</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<PutScriptResponse> PutScriptAsync(IPutScriptRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>reindex</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</a>
		/// </summary>
		ReindexOnServerResponse ReindexOnServer(Func<ReindexOnServerDescriptor, IReindexOnServerRequest> selector);
		/// <summary>
		/// <c>POST</c> request to the <c>reindex</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</a>
		/// </summary>
		Task<ReindexOnServerResponse> ReindexOnServerAsync(Func<ReindexOnServerDescriptor, IReindexOnServerRequest> selector, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>reindex</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</a>
		/// </summary>
		ReindexOnServerResponse ReindexOnServer(IReindexOnServerRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>reindex</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</a>
		/// </summary>
		Task<ReindexOnServerResponse> ReindexOnServerAsync(IReindexOnServerRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>reindex_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</a>
		/// </summary>
		ReindexRethrottleResponse ReindexRethrottle(TaskId taskId, Func<ReindexRethrottleDescriptor, IReindexRethrottleRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>reindex_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</a>
		/// </summary>
		Task<ReindexRethrottleResponse> ReindexRethrottleAsync(TaskId taskId, Func<ReindexRethrottleDescriptor, IReindexRethrottleRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>reindex_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</a>
		/// </summary>
		ReindexRethrottleResponse ReindexRethrottle(IReindexRethrottleRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>reindex_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</a>
		/// </summary>
		Task<ReindexRethrottleResponse> ReindexRethrottleAsync(IReindexRethrottleRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>render_search_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/search-template/">https://opensearch.org/docs/latest/opensearch/search-template/</a>
		/// </summary>
		RenderSearchTemplateResponse RenderSearchTemplate(Func<RenderSearchTemplateDescriptor, IRenderSearchTemplateRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>render_search_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/search-template/">https://opensearch.org/docs/latest/opensearch/search-template/</a>
		/// </summary>
		Task<RenderSearchTemplateResponse> RenderSearchTemplateAsync(Func<RenderSearchTemplateDescriptor, IRenderSearchTemplateRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>render_search_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/search-template/">https://opensearch.org/docs/latest/opensearch/search-template/</a>
		/// </summary>
		RenderSearchTemplateResponse RenderSearchTemplate(IRenderSearchTemplateRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>render_search_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/search-template/">https://opensearch.org/docs/latest/opensearch/search-template/</a>
		/// </summary>
		Task<RenderSearchTemplateResponse> RenderSearchTemplateAsync(IRenderSearchTemplateRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>scripts_painless_execute</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		ExecutePainlessScriptResponse<TResult> ExecutePainlessScript<TResult>(Func<ExecutePainlessScriptDescriptor, IExecutePainlessScriptRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>scripts_painless_execute</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<ExecutePainlessScriptResponse<TResult>> ExecutePainlessScriptAsync<TResult>(Func<ExecutePainlessScriptDescriptor, IExecutePainlessScriptRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>scripts_painless_execute</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		ExecutePainlessScriptResponse<TResult> ExecutePainlessScript<TResult>(IExecutePainlessScriptRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>scripts_painless_execute</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<ExecutePainlessScriptResponse<TResult>> ExecutePainlessScriptAsync<TResult>(IExecutePainlessScriptRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body">https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body</a>
		/// </summary>
		ISearchResponse<TDocument> Scroll<TInferDocument, TDocument>(Time scroll, string scrollId, Func<ScrollDescriptor<TInferDocument>, IScrollRequest> selector = null)
			where TInferDocument : class where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body">https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body</a>
		/// </summary>
		Task<ISearchResponse<TDocument>> ScrollAsync<TInferDocument, TDocument>(Time scroll, string scrollId, Func<ScrollDescriptor<TInferDocument>, IScrollRequest> selector = null, CancellationToken ct = default)
			where TInferDocument : class where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body">https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body</a>
		/// </summary>
		ISearchResponse<TDocument> Scroll<TDocument>(Time scroll, string scrollId, Func<ScrollDescriptor<TDocument>, IScrollRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body">https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body</a>
		/// </summary>
		Task<ISearchResponse<TDocument>> ScrollAsync<TDocument>(Time scroll, string scrollId, Func<ScrollDescriptor<TDocument>, IScrollRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body">https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body</a>
		/// </summary>
		ISearchResponse<TDocument> Scroll<TDocument>(IScrollRequest request)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>scroll</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body">https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body</a>
		/// </summary>
		Task<ISearchResponse<TDocument>> ScrollAsync<TDocument>(IScrollRequest request, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/">https://opensearch.org/docs/latest/opensearch/rest-api/search/</a>
		/// </summary>
		ISearchResponse<TDocument> Search<TInferDocument, TDocument>(Func<SearchDescriptor<TInferDocument>, ISearchRequest> selector = null)
			where TInferDocument : class where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/">https://opensearch.org/docs/latest/opensearch/rest-api/search/</a>
		/// </summary>
		Task<ISearchResponse<TDocument>> SearchAsync<TInferDocument, TDocument>(Func<SearchDescriptor<TInferDocument>, ISearchRequest> selector = null, CancellationToken ct = default)
			where TInferDocument : class where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/">https://opensearch.org/docs/latest/opensearch/rest-api/search/</a>
		/// </summary>
		ISearchResponse<TDocument> Search<TDocument>(Func<SearchDescriptor<TDocument>, ISearchRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/">https://opensearch.org/docs/latest/opensearch/rest-api/search/</a>
		/// </summary>
		Task<ISearchResponse<TDocument>> SearchAsync<TDocument>(Func<SearchDescriptor<TDocument>, ISearchRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/">https://opensearch.org/docs/latest/opensearch/rest-api/search/</a>
		/// </summary>
		ISearchResponse<TDocument> Search<TDocument>(ISearchRequest request)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/search/">https://opensearch.org/docs/latest/opensearch/rest-api/search/</a>
		/// </summary>
		Task<ISearchResponse<TDocument>> SearchAsync<TDocument>(ISearchRequest request, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search_shards</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/">https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/</a>
		/// </summary>
		SearchShardsResponse SearchShards<TDocument>(Func<SearchShardsDescriptor<TDocument>, ISearchShardsRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search_shards</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/">https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/</a>
		/// </summary>
		Task<SearchShardsResponse> SearchShardsAsync<TDocument>(Func<SearchShardsDescriptor<TDocument>, ISearchShardsRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search_shards</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/">https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/</a>
		/// </summary>
		SearchShardsResponse SearchShards(ISearchShardsRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>search_shards</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/">https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/</a>
		/// </summary>
		Task<SearchShardsResponse> SearchShardsAsync(ISearchShardsRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>search_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/search-template/">https://opensearch.org/docs/latest/opensearch/search-template/</a>
		/// </summary>
		ISearchResponse<TDocument> SearchTemplate<TDocument>(Func<SearchTemplateDescriptor<TDocument>, ISearchTemplateRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/search-template/">https://opensearch.org/docs/latest/opensearch/search-template/</a>
		/// </summary>
		Task<ISearchResponse<TDocument>> SearchTemplateAsync<TDocument>(Func<SearchTemplateDescriptor<TDocument>, ISearchTemplateRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/search-template/">https://opensearch.org/docs/latest/opensearch/search-template/</a>
		/// </summary>
		ISearchResponse<TDocument> SearchTemplate<TDocument>(ISearchTemplateRequest request)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>search_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/search-template/">https://opensearch.org/docs/latest/opensearch/search-template/</a>
		/// </summary>
		Task<ISearchResponse<TDocument>> SearchTemplateAsync<TDocument>(ISearchTemplateRequest request, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>termvectors</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		TermVectorsResponse TermVectors<TDocument>(Func<TermVectorsDescriptor<TDocument>, ITermVectorsRequest<TDocument>> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>termvectors</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<TermVectorsResponse> TermVectorsAsync<TDocument>(Func<TermVectorsDescriptor<TDocument>, ITermVectorsRequest<TDocument>> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>termvectors</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		TermVectorsResponse TermVectors<TDocument>(ITermVectorsRequest<TDocument> request)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>termvectors</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		Task<TermVectorsResponse> TermVectorsAsync<TDocument>(ITermVectorsRequest<TDocument> request, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>update</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/</a>
		/// </summary>
		UpdateResponse<TDocument> Update<TDocument, TPartialDocument>(DocumentPath<TDocument> id, Func<UpdateDescriptor<TDocument, TPartialDocument>, IUpdateRequest<TDocument, TPartialDocument>> selector)
			where TDocument : class where TPartialDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>update</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/</a>
		/// </summary>
		Task<UpdateResponse<TDocument>> UpdateAsync<TDocument, TPartialDocument>(DocumentPath<TDocument> id, Func<UpdateDescriptor<TDocument, TPartialDocument>, IUpdateRequest<TDocument, TPartialDocument>> selector, CancellationToken ct = default)
			where TDocument : class where TPartialDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>update</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/</a>
		/// </summary>
		UpdateResponse<TDocument> Update<TDocument>(DocumentPath<TDocument> id, Func<UpdateDescriptor<TDocument, TDocument>, IUpdateRequest<TDocument, TDocument>> selector)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>update</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/</a>
		/// </summary>
		Task<UpdateResponse<TDocument>> UpdateAsync<TDocument>(DocumentPath<TDocument> id, Func<UpdateDescriptor<TDocument, TDocument>, IUpdateRequest<TDocument, TDocument>> selector, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>update</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/</a>
		/// </summary>
		UpdateResponse<TDocument> Update<TDocument, TPartialDocument>(IUpdateRequest<TDocument, TPartialDocument> request)
			where TDocument : class where TPartialDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>update</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/</a>
		/// </summary>
		Task<UpdateResponse<TDocument>> UpdateAsync<TDocument, TPartialDocument>(IUpdateRequest<TDocument, TPartialDocument> request, CancellationToken ct = default)
			where TDocument : class where TPartialDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>update_by_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</a>
		/// </summary>
		UpdateByQueryResponse UpdateByQuery<TDocument>(Func<UpdateByQueryDescriptor<TDocument>, IUpdateByQueryRequest> selector = null)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>update_by_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</a>
		/// </summary>
		Task<UpdateByQueryResponse> UpdateByQueryAsync<TDocument>(Func<UpdateByQueryDescriptor<TDocument>, IUpdateByQueryRequest> selector = null, CancellationToken ct = default)
			where TDocument : class;
		/// <summary>
		/// <c>POST</c> request to the <c>update_by_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</a>
		/// </summary>
		UpdateByQueryResponse UpdateByQuery(IUpdateByQueryRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>update_by_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</a>
		/// </summary>
		Task<UpdateByQueryResponse> UpdateByQueryAsync(IUpdateByQueryRequest request, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>update_by_query_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</a>
		/// </summary>
		ListTasksResponse UpdateByQueryRethrottle(TaskId taskId, Func<UpdateByQueryRethrottleDescriptor, IUpdateByQueryRethrottleRequest> selector = null);
		/// <summary>
		/// <c>POST</c> request to the <c>update_by_query_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</a>
		/// </summary>
		Task<ListTasksResponse> UpdateByQueryRethrottleAsync(TaskId taskId, Func<UpdateByQueryRethrottleDescriptor, IUpdateByQueryRethrottleRequest> selector = null, CancellationToken ct = default);
		/// <summary>
		/// <c>POST</c> request to the <c>update_by_query_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</a>
		/// </summary>
		ListTasksResponse UpdateByQueryRethrottle(IUpdateByQueryRethrottleRequest request);
		/// <summary>
		/// <c>POST</c> request to the <c>update_by_query_rethrottle</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</a>
		/// </summary>
		Task<ListTasksResponse> UpdateByQueryRethrottleAsync(IUpdateByQueryRethrottleRequest request, CancellationToken ct = default);

		///<summary>Snapshot APIs</summary>
		SnapshotNamespace Snapshot
		{
			get;
		}

		///<summary>Tasks APIs</summary>
		TasksNamespace Tasks
		{
			get;
		}
	}
}
