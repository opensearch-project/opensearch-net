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
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.Specification.IndicesApi;

// ReSharper disable once CheckNamespace
// ReSharper disable RedundantTypeArgumentsOfMethod
namespace OpenSearch.Client.Specification.IndicesApi
{
	///<summary>
	/// Indices APIs.
	/// <para>Not intended to be instantiated directly. Use the <see cref = "IOpenSearchClient.Indices"/> property
	/// on <see cref = "IOpenSearchClient"/>.
	///</para>
	///</summary>
	public partial class IndicesNamespace : NamespacedClientProxy
	{
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.add_block</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public AddIndexBlockResponse AddBlock(Indices index, IndexBlock block, Func<AddIndexBlockDescriptor, IAddIndexBlockRequest> selector = null) => AddBlock(selector.InvokeOrDefault(new AddIndexBlockDescriptor(index: index, block: block)));
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.add_block</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<AddIndexBlockResponse> AddBlockAsync(Indices index, IndexBlock block, Func<AddIndexBlockDescriptor, IAddIndexBlockRequest> selector = null, CancellationToken ct = default) => AddBlockAsync(selector.InvokeOrDefault(new AddIndexBlockDescriptor(index: index, block: block)), ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.add_block</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public AddIndexBlockResponse AddBlock(IAddIndexBlockRequest request) => DoRequest<IAddIndexBlockRequest, AddIndexBlockResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.add_block</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<AddIndexBlockResponse> AddBlockAsync(IAddIndexBlockRequest request, CancellationToken ct = default) => DoRequestAsync<IAddIndexBlockRequest, AddIndexBlockResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.analyze</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public AnalyzeResponse Analyze(Func<AnalyzeDescriptor, IAnalyzeRequest> selector = null) => Analyze(selector.InvokeOrDefault(new AnalyzeDescriptor()));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.analyze</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<AnalyzeResponse> AnalyzeAsync(Func<AnalyzeDescriptor, IAnalyzeRequest> selector = null, CancellationToken ct = default) => AnalyzeAsync(selector.InvokeOrDefault(new AnalyzeDescriptor()), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.analyze</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public AnalyzeResponse Analyze(IAnalyzeRequest request) => DoRequest<IAnalyzeRequest, AnalyzeResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.analyze</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<AnalyzeResponse> AnalyzeAsync(IAnalyzeRequest request, CancellationToken ct = default) => DoRequestAsync<IAnalyzeRequest, AnalyzeResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.clear_cache</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public ClearCacheResponse ClearCache(Indices index = null, Func<ClearCacheDescriptor, IClearCacheRequest> selector = null) => ClearCache(selector.InvokeOrDefault(new ClearCacheDescriptor().Index(index: index)));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.clear_cache</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<ClearCacheResponse> ClearCacheAsync(Indices index = null, Func<ClearCacheDescriptor, IClearCacheRequest> selector = null, CancellationToken ct = default) => ClearCacheAsync(selector.InvokeOrDefault(new ClearCacheDescriptor().Index(index: index)), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.clear_cache</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public ClearCacheResponse ClearCache(IClearCacheRequest request) => DoRequest<IClearCacheRequest, ClearCacheResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.clear_cache</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<ClearCacheResponse> ClearCacheAsync(IClearCacheRequest request, CancellationToken ct = default) => DoRequestAsync<IClearCacheRequest, ClearCacheResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.clone</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/</a>
		/// </summary>
		public CloneIndexResponse Clone(IndexName index, IndexName target, Func<CloneIndexDescriptor, ICloneIndexRequest> selector = null) => Clone(selector.InvokeOrDefault(new CloneIndexDescriptor(index: index, target: target)));
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.clone</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/</a>
		/// </summary>
		public Task<CloneIndexResponse> CloneAsync(IndexName index, IndexName target, Func<CloneIndexDescriptor, ICloneIndexRequest> selector = null, CancellationToken ct = default) => CloneAsync(selector.InvokeOrDefault(new CloneIndexDescriptor(index: index, target: target)), ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.clone</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/</a>
		/// </summary>
		public CloneIndexResponse Clone(ICloneIndexRequest request) => DoRequest<ICloneIndexRequest, CloneIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.clone</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/</a>
		/// </summary>
		public Task<CloneIndexResponse> CloneAsync(ICloneIndexRequest request, CancellationToken ct = default) => DoRequestAsync<ICloneIndexRequest, CloneIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.close</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</a>
		/// </summary>
		public CloseIndexResponse Close(Indices index, Func<CloseIndexDescriptor, ICloseIndexRequest> selector = null) => Close(selector.InvokeOrDefault(new CloseIndexDescriptor(index: index)));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.close</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</a>
		/// </summary>
		public Task<CloseIndexResponse> CloseAsync(Indices index, Func<CloseIndexDescriptor, ICloseIndexRequest> selector = null, CancellationToken ct = default) => CloseAsync(selector.InvokeOrDefault(new CloseIndexDescriptor(index: index)), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.close</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</a>
		/// </summary>
		public CloseIndexResponse Close(ICloseIndexRequest request) => DoRequest<ICloseIndexRequest, CloseIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.close</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</a>
		/// </summary>
		public Task<CloseIndexResponse> CloseAsync(ICloseIndexRequest request, CancellationToken ct = default) => DoRequestAsync<ICloseIndexRequest, CloseIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.create</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/</a>
		/// </summary>
		public CreateIndexResponse Create(IndexName index, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null) => Create(selector.InvokeOrDefault(new CreateIndexDescriptor(index: index)));
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.create</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/</a>
		/// </summary>
		public Task<CreateIndexResponse> CreateAsync(IndexName index, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null, CancellationToken ct = default) => CreateAsync(selector.InvokeOrDefault(new CreateIndexDescriptor(index: index)), ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.create</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/</a>
		/// </summary>
		public CreateIndexResponse Create(ICreateIndexRequest request) => DoRequest<ICreateIndexRequest, CreateIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.create</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/</a>
		/// </summary>
		public Task<CreateIndexResponse> CreateAsync(ICreateIndexRequest request, CancellationToken ct = default) => DoRequestAsync<ICreateIndexRequest, CreateIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/</a>
		/// </summary>
		public DeleteIndexResponse Delete(Indices index, Func<DeleteIndexDescriptor, IDeleteIndexRequest> selector = null) => Delete(selector.InvokeOrDefault(new DeleteIndexDescriptor(index: index)));
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/</a>
		/// </summary>
		public Task<DeleteIndexResponse> DeleteAsync(Indices index, Func<DeleteIndexDescriptor, IDeleteIndexRequest> selector = null, CancellationToken ct = default) => DeleteAsync(selector.InvokeOrDefault(new DeleteIndexDescriptor(index: index)), ct);
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/</a>
		/// </summary>
		public DeleteIndexResponse Delete(IDeleteIndexRequest request) => DoRequest<IDeleteIndexRequest, DeleteIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/</a>
		/// </summary>
		public Task<DeleteIndexResponse> DeleteAsync(IDeleteIndexRequest request, CancellationToken ct = default) => DoRequestAsync<IDeleteIndexRequest, DeleteIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public DeleteAliasResponse DeleteAlias(Indices index, Names name, Func<DeleteAliasDescriptor, IDeleteAliasRequest> selector = null) => DeleteAlias(selector.InvokeOrDefault(new DeleteAliasDescriptor(index: index, name: name)));
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<DeleteAliasResponse> DeleteAliasAsync(Indices index, Names name, Func<DeleteAliasDescriptor, IDeleteAliasRequest> selector = null, CancellationToken ct = default) => DeleteAliasAsync(selector.InvokeOrDefault(new DeleteAliasDescriptor(index: index, name: name)), ct);
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public DeleteAliasResponse DeleteAlias(IDeleteAliasRequest request) => DoRequest<IDeleteAliasRequest, DeleteAliasResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<DeleteAliasResponse> DeleteAliasAsync(IDeleteAliasRequest request, CancellationToken ct = default) => DoRequestAsync<IDeleteAliasRequest, DeleteAliasResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public DeleteIndexTemplateResponse DeleteTemplate(Name name, Func<DeleteIndexTemplateDescriptor, IDeleteIndexTemplateRequest> selector = null) => DeleteTemplate(selector.InvokeOrDefault(new DeleteIndexTemplateDescriptor(name: name)));
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public Task<DeleteIndexTemplateResponse> DeleteTemplateAsync(Name name, Func<DeleteIndexTemplateDescriptor, IDeleteIndexTemplateRequest> selector = null, CancellationToken ct = default) => DeleteTemplateAsync(selector.InvokeOrDefault(new DeleteIndexTemplateDescriptor(name: name)), ct);
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public DeleteIndexTemplateResponse DeleteTemplate(IDeleteIndexTemplateRequest request) => DoRequest<IDeleteIndexTemplateRequest, DeleteIndexTemplateResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>DELETE</c> request to the <c>indices.delete_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public Task<DeleteIndexTemplateResponse> DeleteTemplateAsync(IDeleteIndexTemplateRequest request, CancellationToken ct = default) => DoRequestAsync<IDeleteIndexTemplateRequest, DeleteIndexTemplateResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</a>
		/// </summary>
		public ExistsResponse Exists(Indices index, Func<IndexExistsDescriptor, IIndexExistsRequest> selector = null) => Exists(selector.InvokeOrDefault(new IndexExistsDescriptor(index: index)));
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</a>
		/// </summary>
		public Task<ExistsResponse> ExistsAsync(Indices index, Func<IndexExistsDescriptor, IIndexExistsRequest> selector = null, CancellationToken ct = default) => ExistsAsync(selector.InvokeOrDefault(new IndexExistsDescriptor(index: index)), ct);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</a>
		/// </summary>
		public ExistsResponse Exists(IIndexExistsRequest request) => DoRequest<IIndexExistsRequest, ExistsResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</a>
		/// </summary>
		public Task<ExistsResponse> ExistsAsync(IIndexExistsRequest request, CancellationToken ct = default) => DoRequestAsync<IIndexExistsRequest, ExistsResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public ExistsResponse AliasExists(Names name, Func<AliasExistsDescriptor, IAliasExistsRequest> selector = null) => AliasExists(selector.InvokeOrDefault(new AliasExistsDescriptor(name: name)));
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<ExistsResponse> AliasExistsAsync(Names name, Func<AliasExistsDescriptor, IAliasExistsRequest> selector = null, CancellationToken ct = default) => AliasExistsAsync(selector.InvokeOrDefault(new AliasExistsDescriptor(name: name)), ct);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public ExistsResponse AliasExists(IAliasExistsRequest request) => DoRequest<IAliasExistsRequest, ExistsResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<ExistsResponse> AliasExistsAsync(IAliasExistsRequest request, CancellationToken ct = default) => DoRequestAsync<IAliasExistsRequest, ExistsResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public ExistsResponse TemplateExists(Names name, Func<IndexTemplateExistsDescriptor, IIndexTemplateExistsRequest> selector = null) => TemplateExists(selector.InvokeOrDefault(new IndexTemplateExistsDescriptor(name: name)));
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public Task<ExistsResponse> TemplateExistsAsync(Names name, Func<IndexTemplateExistsDescriptor, IIndexTemplateExistsRequest> selector = null, CancellationToken ct = default) => TemplateExistsAsync(selector.InvokeOrDefault(new IndexTemplateExistsDescriptor(name: name)), ct);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public ExistsResponse TemplateExists(IIndexTemplateExistsRequest request) => DoRequest<IIndexTemplateExistsRequest, ExistsResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public Task<ExistsResponse> TemplateExistsAsync(IIndexTemplateExistsRequest request, CancellationToken ct = default) => DoRequestAsync<IIndexTemplateExistsRequest, ExistsResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_type</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</a>
		/// </summary>
		/// <remarks>Deprecated as of OpenSearch 2.0</remarks>
		public ExistsResponse TypeExists(Indices index, Names type, Func<TypeExistsDescriptor, ITypeExistsRequest> selector = null) => TypeExists(selector.InvokeOrDefault(new TypeExistsDescriptor(index: index, type: type)));
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_type</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</a>
		/// </summary>
		/// <remarks>Deprecated as of OpenSearch 2.0</remarks>
		public Task<ExistsResponse> TypeExistsAsync(Indices index, Names type, Func<TypeExistsDescriptor, ITypeExistsRequest> selector = null, CancellationToken ct = default) => TypeExistsAsync(selector.InvokeOrDefault(new TypeExistsDescriptor(index: index, type: type)), ct);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_type</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</a>
		/// </summary>
		/// <remarks>Deprecated as of OpenSearch 2.0</remarks>
		public ExistsResponse TypeExists(ITypeExistsRequest request) => DoRequest<ITypeExistsRequest, ExistsResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>HEAD</c> request to the <c>indices.exists_type</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</a>
		/// </summary>
		/// <remarks>Deprecated as of OpenSearch 2.0</remarks>
		public Task<ExistsResponse> TypeExistsAsync(ITypeExistsRequest request, CancellationToken ct = default) => DoRequestAsync<ITypeExistsRequest, ExistsResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.flush</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public FlushResponse Flush(Indices index = null, Func<FlushDescriptor, IFlushRequest> selector = null) => Flush(selector.InvokeOrDefault(new FlushDescriptor().Index(index: index)));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.flush</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<FlushResponse> FlushAsync(Indices index = null, Func<FlushDescriptor, IFlushRequest> selector = null, CancellationToken ct = default) => FlushAsync(selector.InvokeOrDefault(new FlushDescriptor().Index(index: index)), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.flush</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public FlushResponse Flush(IFlushRequest request) => DoRequest<IFlushRequest, FlushResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.flush</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<FlushResponse> FlushAsync(IFlushRequest request, CancellationToken ct = default) => DoRequestAsync<IFlushRequest, FlushResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.forcemerge</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public ForceMergeResponse ForceMerge(Indices index = null, Func<ForceMergeDescriptor, IForceMergeRequest> selector = null) => ForceMerge(selector.InvokeOrDefault(new ForceMergeDescriptor().Index(index: index)));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.forcemerge</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<ForceMergeResponse> ForceMergeAsync(Indices index = null, Func<ForceMergeDescriptor, IForceMergeRequest> selector = null, CancellationToken ct = default) => ForceMergeAsync(selector.InvokeOrDefault(new ForceMergeDescriptor().Index(index: index)), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.forcemerge</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public ForceMergeResponse ForceMerge(IForceMergeRequest request) => DoRequest<IForceMergeRequest, ForceMergeResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.forcemerge</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<ForceMergeResponse> ForceMergeAsync(IForceMergeRequest request, CancellationToken ct = default) => DoRequestAsync<IForceMergeRequest, ForceMergeResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/</a>
		/// </summary>
		public GetIndexResponse Get(Indices index, Func<GetIndexDescriptor, IGetIndexRequest> selector = null) => Get(selector.InvokeOrDefault(new GetIndexDescriptor(index: index)));
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/</a>
		/// </summary>
		public Task<GetIndexResponse> GetAsync(Indices index, Func<GetIndexDescriptor, IGetIndexRequest> selector = null, CancellationToken ct = default) => GetAsync(selector.InvokeOrDefault(new GetIndexDescriptor(index: index)), ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/</a>
		/// </summary>
		public GetIndexResponse Get(IGetIndexRequest request) => DoRequest<IGetIndexRequest, GetIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/</a>
		/// </summary>
		public Task<GetIndexResponse> GetAsync(IGetIndexRequest request, CancellationToken ct = default) => DoRequestAsync<IGetIndexRequest, GetIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public GetAliasResponse GetAlias(Indices index = null, Func<GetAliasDescriptor, IGetAliasRequest> selector = null) => GetAlias(selector.InvokeOrDefault(new GetAliasDescriptor().Index(index: index)));
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<GetAliasResponse> GetAliasAsync(Indices index = null, Func<GetAliasDescriptor, IGetAliasRequest> selector = null, CancellationToken ct = default) => GetAliasAsync(selector.InvokeOrDefault(new GetAliasDescriptor().Index(index: index)), ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public GetAliasResponse GetAlias(IGetAliasRequest request) => DoRequest<IGetAliasRequest, GetAliasResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<GetAliasResponse> GetAliasAsync(IGetAliasRequest request, CancellationToken ct = default) => DoRequestAsync<IGetAliasRequest, GetAliasResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_field_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public GetFieldMappingResponse GetFieldMapping<TDocument>(Fields fields, Func<GetFieldMappingDescriptor<TDocument>, IGetFieldMappingRequest> selector = null)
			where TDocument : class => GetFieldMapping(selector.InvokeOrDefault(new GetFieldMappingDescriptor<TDocument>(fields: fields)));
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_field_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public Task<GetFieldMappingResponse> GetFieldMappingAsync<TDocument>(Fields fields, Func<GetFieldMappingDescriptor<TDocument>, IGetFieldMappingRequest> selector = null, CancellationToken ct = default)
			where TDocument : class => GetFieldMappingAsync(selector.InvokeOrDefault(new GetFieldMappingDescriptor<TDocument>(fields: fields)), ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_field_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public GetFieldMappingResponse GetFieldMapping(IGetFieldMappingRequest request) => DoRequest<IGetFieldMappingRequest, GetFieldMappingResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_field_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public Task<GetFieldMappingResponse> GetFieldMappingAsync(IGetFieldMappingRequest request, CancellationToken ct = default) => DoRequestAsync<IGetFieldMappingRequest, GetFieldMappingResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public GetMappingResponse GetMapping<TDocument>(Func<GetMappingDescriptor<TDocument>, IGetMappingRequest> selector = null)
			where TDocument : class => GetMapping(selector.InvokeOrDefault(new GetMappingDescriptor<TDocument>()));
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public Task<GetMappingResponse> GetMappingAsync<TDocument>(Func<GetMappingDescriptor<TDocument>, IGetMappingRequest> selector = null, CancellationToken ct = default)
			where TDocument : class => GetMappingAsync(selector.InvokeOrDefault(new GetMappingDescriptor<TDocument>()), ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public GetMappingResponse GetMapping(IGetMappingRequest request) => DoRequest<IGetMappingRequest, GetMappingResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public Task<GetMappingResponse> GetMappingAsync(IGetMappingRequest request, CancellationToken ct = default) => DoRequestAsync<IGetMappingRequest, GetMappingResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public GetIndexSettingsResponse GetSettings(Indices index = null, Func<GetIndexSettingsDescriptor, IGetIndexSettingsRequest> selector = null) => GetSettings(selector.InvokeOrDefault(new GetIndexSettingsDescriptor().Index(index: index)));
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<GetIndexSettingsResponse> GetSettingsAsync(Indices index = null, Func<GetIndexSettingsDescriptor, IGetIndexSettingsRequest> selector = null, CancellationToken ct = default) => GetSettingsAsync(selector.InvokeOrDefault(new GetIndexSettingsDescriptor().Index(index: index)), ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public GetIndexSettingsResponse GetSettings(IGetIndexSettingsRequest request) => DoRequest<IGetIndexSettingsRequest, GetIndexSettingsResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<GetIndexSettingsResponse> GetSettingsAsync(IGetIndexSettingsRequest request, CancellationToken ct = default) => DoRequestAsync<IGetIndexSettingsRequest, GetIndexSettingsResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public GetIndexTemplateResponse GetTemplate(Names name = null, Func<GetIndexTemplateDescriptor, IGetIndexTemplateRequest> selector = null) => GetTemplate(selector.InvokeOrDefault(new GetIndexTemplateDescriptor().Name(name: name)));
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public Task<GetIndexTemplateResponse> GetTemplateAsync(Names name = null, Func<GetIndexTemplateDescriptor, IGetIndexTemplateRequest> selector = null, CancellationToken ct = default) => GetTemplateAsync(selector.InvokeOrDefault(new GetIndexTemplateDescriptor().Name(name: name)), ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public GetIndexTemplateResponse GetTemplate(IGetIndexTemplateRequest request) => DoRequest<IGetIndexTemplateRequest, GetIndexTemplateResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.get_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public Task<GetIndexTemplateResponse> GetTemplateAsync(IGetIndexTemplateRequest request, CancellationToken ct = default) => DoRequestAsync<IGetIndexTemplateRequest, GetIndexTemplateResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.open</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</a>
		/// </summary>
		public OpenIndexResponse Open(Indices index, Func<OpenIndexDescriptor, IOpenIndexRequest> selector = null) => Open(selector.InvokeOrDefault(new OpenIndexDescriptor(index: index)));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.open</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</a>
		/// </summary>
		public Task<OpenIndexResponse> OpenAsync(Indices index, Func<OpenIndexDescriptor, IOpenIndexRequest> selector = null, CancellationToken ct = default) => OpenAsync(selector.InvokeOrDefault(new OpenIndexDescriptor(index: index)), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.open</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</a>
		/// </summary>
		public OpenIndexResponse Open(IOpenIndexRequest request) => DoRequest<IOpenIndexRequest, OpenIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.open</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</a>
		/// </summary>
		public Task<OpenIndexResponse> OpenAsync(IOpenIndexRequest request, CancellationToken ct = default) => DoRequestAsync<IOpenIndexRequest, OpenIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public PutAliasResponse PutAlias(Indices index, Name name, Func<PutAliasDescriptor, IPutAliasRequest> selector = null) => PutAlias(selector.InvokeOrDefault(new PutAliasDescriptor(index: index, name: name)));
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<PutAliasResponse> PutAliasAsync(Indices index, Name name, Func<PutAliasDescriptor, IPutAliasRequest> selector = null, CancellationToken ct = default) => PutAliasAsync(selector.InvokeOrDefault(new PutAliasDescriptor(index: index, name: name)), ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public PutAliasResponse PutAlias(IPutAliasRequest request) => DoRequest<IPutAliasRequest, PutAliasResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_alias</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<PutAliasResponse> PutAliasAsync(IPutAliasRequest request, CancellationToken ct = default) => DoRequestAsync<IPutAliasRequest, PutAliasResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public PutMappingResponse PutMapping<TDocument>(Func<PutMappingDescriptor<TDocument>, IPutMappingRequest> selector)
			where TDocument : class => PutMapping(selector.InvokeOrDefault(new PutMappingDescriptor<TDocument>()));
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public Task<PutMappingResponse> PutMappingAsync<TDocument>(Func<PutMappingDescriptor<TDocument>, IPutMappingRequest> selector, CancellationToken ct = default)
			where TDocument : class => PutMappingAsync(selector.InvokeOrDefault(new PutMappingDescriptor<TDocument>()), ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public PutMappingResponse PutMapping(IPutMappingRequest request) => DoRequest<IPutMappingRequest, PutMappingResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_mapping</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/">https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</a>
		/// </summary>
		public Task<PutMappingResponse> PutMappingAsync(IPutMappingRequest request, CancellationToken ct = default) => DoRequestAsync<IPutMappingRequest, PutMappingResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public UpdateIndexSettingsResponse UpdateSettings(Indices index, Func<UpdateIndexSettingsDescriptor, IUpdateIndexSettingsRequest> selector) => UpdateSettings(selector.InvokeOrDefault(new UpdateIndexSettingsDescriptor().Index(index: index)));
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<UpdateIndexSettingsResponse> UpdateSettingsAsync(Indices index, Func<UpdateIndexSettingsDescriptor, IUpdateIndexSettingsRequest> selector, CancellationToken ct = default) => UpdateSettingsAsync(selector.InvokeOrDefault(new UpdateIndexSettingsDescriptor().Index(index: index)), ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public UpdateIndexSettingsResponse UpdateSettings(IUpdateIndexSettingsRequest request) => DoRequest<IUpdateIndexSettingsRequest, UpdateIndexSettingsResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<UpdateIndexSettingsResponse> UpdateSettingsAsync(IUpdateIndexSettingsRequest request, CancellationToken ct = default) => DoRequestAsync<IUpdateIndexSettingsRequest, UpdateIndexSettingsResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public PutIndexTemplateResponse PutTemplate(Name name, Func<PutIndexTemplateDescriptor, IPutIndexTemplateRequest> selector) => PutTemplate(selector.InvokeOrDefault(new PutIndexTemplateDescriptor(name: name)));
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public Task<PutIndexTemplateResponse> PutTemplateAsync(Name name, Func<PutIndexTemplateDescriptor, IPutIndexTemplateRequest> selector, CancellationToken ct = default) => PutTemplateAsync(selector.InvokeOrDefault(new PutIndexTemplateDescriptor(name: name)), ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public PutIndexTemplateResponse PutTemplate(IPutIndexTemplateRequest request) => DoRequest<IPutIndexTemplateRequest, PutIndexTemplateResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.put_template</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</a>
		/// </summary>
		public Task<PutIndexTemplateResponse> PutTemplateAsync(IPutIndexTemplateRequest request, CancellationToken ct = default) => DoRequestAsync<IPutIndexTemplateRequest, PutIndexTemplateResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.recovery</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public RefreshResponse Refresh(Indices index = null, Func<RefreshDescriptor, IRefreshRequest> selector = null) => Refresh(selector.InvokeOrDefault(new RefreshDescriptor().Index(index: index)));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.refresh</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		public Task<RefreshResponse> RefreshAsync(Indices index = null, Func<RefreshDescriptor, IRefreshRequest> selector = null, CancellationToken ct = default) => RefreshAsync(selector.InvokeOrDefault(new RefreshDescriptor().Index(index: index)), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.refresh</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		public RefreshResponse Refresh(IRefreshRequest request) => DoRequest<IRefreshRequest, RefreshResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.refresh</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/">https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</a>
		/// </summary>
		public Task<RefreshResponse> RefreshAsync(IRefreshRequest request, CancellationToken ct = default) => DoRequestAsync<IRefreshRequest, RefreshResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.resolve_index</c> API, read more about this API online:
		/// <para></para>
		/// </summary>
		public ResolveIndexResponse Resolve(Names name, Func<ResolveIndexDescriptor, IResolveIndexRequest> selector = null) => Resolve(selector.InvokeOrDefault(new ResolveIndexDescriptor(name: name)));
		/// <summary>
		/// <c>GET</c> request to the <c>indices.resolve_index</c> API, read more about this API online:
		/// <para></para>
		/// </summary>
		public Task<ResolveIndexResponse> ResolveAsync(Names name, Func<ResolveIndexDescriptor, IResolveIndexRequest> selector = null, CancellationToken ct = default) => ResolveAsync(selector.InvokeOrDefault(new ResolveIndexDescriptor(name: name)), ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.resolve_index</c> API, read more about this API online:
		/// <para></para>
		/// </summary>
		public ResolveIndexResponse Resolve(IResolveIndexRequest request) => DoRequest<IResolveIndexRequest, ResolveIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.resolve_index</c> API, read more about this API online:
		/// <para></para>
		/// </summary>
		public Task<ResolveIndexResponse> ResolveAsync(IResolveIndexRequest request, CancellationToken ct = default) => DoRequestAsync<IResolveIndexRequest, ResolveIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.rollover</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream">https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream</a>
		/// </summary>
		public RolloverIndexResponse Rollover(Name alias, Func<RolloverIndexDescriptor, IRolloverIndexRequest> selector = null) => Rollover(selector.InvokeOrDefault(new RolloverIndexDescriptor(alias: alias)));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.rollover</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream">https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream</a>
		/// </summary>
		public Task<RolloverIndexResponse> RolloverAsync(Name alias, Func<RolloverIndexDescriptor, IRolloverIndexRequest> selector = null, CancellationToken ct = default) => RolloverAsync(selector.InvokeOrDefault(new RolloverIndexDescriptor(alias: alias)), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.rollover</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream">https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream</a>
		/// </summary>
		public RolloverIndexResponse Rollover(IRolloverIndexRequest request) => DoRequest<IRolloverIndexRequest, RolloverIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.rollover</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream">https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream</a>
		/// </summary>
		public Task<RolloverIndexResponse> RolloverAsync(IRolloverIndexRequest request, CancellationToken ct = default) => DoRequestAsync<IRolloverIndexRequest, RolloverIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>indices.segments</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-segments/">https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-segments/</a>
		/// </summary>
		public ShrinkIndexResponse Shrink(IndexName index, IndexName target, Func<ShrinkIndexDescriptor, IShrinkIndexRequest> selector = null) => Shrink(selector.InvokeOrDefault(new ShrinkIndexDescriptor(index: index, target: target)));
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.shrink</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/shrink-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/shrink-index/</a>
		/// </summary>
		public Task<ShrinkIndexResponse> ShrinkAsync(IndexName index, IndexName target, Func<ShrinkIndexDescriptor, IShrinkIndexRequest> selector = null, CancellationToken ct = default) => ShrinkAsync(selector.InvokeOrDefault(new ShrinkIndexDescriptor(index: index, target: target)), ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.shrink</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/shrink-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/shrink-index/</a>
		/// </summary>
		public ShrinkIndexResponse Shrink(IShrinkIndexRequest request) => DoRequest<IShrinkIndexRequest, ShrinkIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.shrink</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/shrink-index/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/shrink-index/</a>
		/// </summary>
		public Task<ShrinkIndexResponse> ShrinkAsync(IShrinkIndexRequest request, CancellationToken ct = default) => DoRequestAsync<IShrinkIndexRequest, ShrinkIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.split</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/</a>
		/// </summary>
		public SplitIndexResponse Split(IndexName index, IndexName target, Func<SplitIndexDescriptor, ISplitIndexRequest> selector = null) => Split(selector.InvokeOrDefault(new SplitIndexDescriptor(index: index, target: target)));
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.split</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/</a>
		/// </summary>
		public Task<SplitIndexResponse> SplitAsync(IndexName index, IndexName target, Func<SplitIndexDescriptor, ISplitIndexRequest> selector = null, CancellationToken ct = default) => SplitAsync(selector.InvokeOrDefault(new SplitIndexDescriptor(index: index, target: target)), ct);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.split</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/</a>
		/// </summary>
		public SplitIndexResponse Split(ISplitIndexRequest request) => DoRequest<ISplitIndexRequest, SplitIndexResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>PUT</c> request to the <c>indices.split</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/">https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/</a>
		/// </summary>
		public Task<SplitIndexResponse> SplitAsync(ISplitIndexRequest request, CancellationToken ct = default) => DoRequestAsync<ISplitIndexRequest, SplitIndexResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.update_aliases</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public BulkAliasResponse BulkAlias(Func<BulkAliasDescriptor, IBulkAliasRequest> selector) => BulkAlias(selector.InvokeOrDefault(new BulkAliasDescriptor()));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.update_aliases</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<BulkAliasResponse> BulkAliasAsync(Func<BulkAliasDescriptor, IBulkAliasRequest> selector, CancellationToken ct = default) => BulkAliasAsync(selector.InvokeOrDefault(new BulkAliasDescriptor()), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.update_aliases</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public BulkAliasResponse BulkAlias(IBulkAliasRequest request) => DoRequest<IBulkAliasRequest, BulkAliasResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.update_aliases</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/rest-api/alias/">https://opensearch.org/docs/latest/opensearch/rest-api/alias/</a>
		/// </summary>
		public Task<BulkAliasResponse> BulkAliasAsync(IBulkAliasRequest request, CancellationToken ct = default) => DoRequestAsync<IBulkAliasRequest, BulkAliasResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.validate_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public ValidateQueryResponse ValidateQuery<TDocument>(Func<ValidateQueryDescriptor<TDocument>, IValidateQueryRequest> selector = null)
			where TDocument : class => ValidateQuery(selector.InvokeOrDefault(new ValidateQueryDescriptor<TDocument>()));
		/// <summary>
		/// <c>POST</c> request to the <c>indices.validate_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<ValidateQueryResponse> ValidateQueryAsync<TDocument>(Func<ValidateQueryDescriptor<TDocument>, IValidateQueryRequest> selector = null, CancellationToken ct = default)
			where TDocument : class => ValidateQueryAsync(selector.InvokeOrDefault(new ValidateQueryDescriptor<TDocument>()), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.validate_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public ValidateQueryResponse ValidateQuery(IValidateQueryRequest request) => DoRequest<IValidateQueryRequest, ValidateQueryResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>indices.validate_query</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<ValidateQueryResponse> ValidateQueryAsync(IValidateQueryRequest request, CancellationToken ct = default) => DoRequestAsync<IValidateQueryRequest, ValidateQueryResponse>(request, request.RequestParameters, ct);
	}
}
