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
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client
{
	[InterfaceDataContract]
	public partial interface IBulkRequest : IRequest<BulkRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}
	}

	///<summary>Request for Bulk <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/bulk/</para></summary>
	public partial class BulkRequest : PlainRequestBase<BulkRequestParameters>, IBulkRequest
	{
		protected IBulkRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceBulk;
		///<summary>/_bulk</summary>
		public BulkRequest(): base()
		{
		}

		///<summary>/{index}/_bulk</summary>
		///<param name = "index">Optional, accepts null</param>
		public BulkRequest(IndexName index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName IBulkRequest.Index => Self.RouteValues.Get<IndexName>("index");
		// Request parameters
		///<summary>The pipeline id to preprocess incoming documents with</summary>
		public string Pipeline
		{
			get => Q<string>("pipeline");
			set => Q("pipeline", value);
		}

		///<summary>
		/// If `true` then refresh the affected shards to make this operation visible to search, if `wait_for` then wait for a refresh to make this
		/// operation visible to search, if `false` (the default) then do nothing with refreshes.
		///</summary>
		public Refresh? Refresh
		{
			get => Q<Refresh? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>Sets require_alias for all incoming documents. Defaults to unset (false)</summary>
		public bool? RequireAlias
		{
			get => Q<bool? >("require_alias");
			set => Q("require_alias", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>Default list of fields to exclude from the returned _source field, can be overridden on each sub-request</summary>
		public Fields SourceExcludes
		{
			get => Q<Fields>("_source_excludes");
			set => Q("_source_excludes", value);
		}

		///<summary>Default list of fields to extract and return from the _source field, can be overridden on each sub-request</summary>
		public Fields SourceIncludes
		{
			get => Q<Fields>("_source_includes");
			set => Q("_source_includes", value);
		}

		///<summary>Explicit operation timeout</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Default document type for items which don't provide one</summary>
		public string TypeQueryString
		{
			get => Q<string>("type");
			set => Q("type", value);
		}

		///<summary>
		/// Sets the number of shard copies that must be active before proceeding with the bulk operation. Defaults to 1, meaning the primary shard
		/// only. Set to `all` for all shard copies, otherwise set to any non-negative value less than or equal to the total number of copies for the
		/// shard (number of replicas + 1)
		///</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IClearScrollRequest : IRequest<ClearScrollRequestParameters>
	{
	}

	///<summary>Request for ClearScroll <para>https://opensearch.org/docs/latest/opensearch/rest-api/scroll/</para></summary>
	public partial class ClearScrollRequest : PlainRequestBase<ClearScrollRequestParameters>, IClearScrollRequest
	{
		protected IClearScrollRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceClearScroll;
	// values part of the url path
	// Request parameters
	}

	[InterfaceDataContract]
	public partial interface ICountRequest : IRequest<CountRequestParameters>
	{
		[IgnoreDataMember]
		Indices Index
		{
			get;
		}
	}

	public partial interface ICountRequest<TDocument> : ICountRequest
	{
	}

	///<summary>Request for Count <para>https://opensearch.org/docs/latest/opensearch/rest-api/count/</para></summary>
	public partial class CountRequest : PlainRequestBase<CountRequestParameters>, ICountRequest
	{
		protected ICountRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceCount;
		///<summary>/_count</summary>
		public CountRequest(): base()
		{
		}

		///<summary>/{index}/_count</summary>
		///<param name = "index">Optional, accepts null</param>
		public CountRequest(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Indices ICountRequest.Index => Self.RouteValues.Get<Indices>("index");
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

		///<summary>Specify whether wildcard and prefix queries should be analyzed (default: false)</summary>
		public bool? AnalyzeWildcard
		{
			get => Q<bool? >("analyze_wildcard");
			set => Q("analyze_wildcard", value);
		}

		///<summary>The analyzer to use for the query string</summary>
		public string Analyzer
		{
			get => Q<string>("analyzer");
			set => Q("analyzer", value);
		}

		///<summary>The default operator for query string query (AND or OR)</summary>
		public DefaultOperator? DefaultOperator
		{
			get => Q<DefaultOperator? >("default_operator");
			set => Q("default_operator", value);
		}

		///<summary>The field to use as default where no field prefix is given in the query string</summary>
		public string Df
		{
			get => Q<string>("df");
			set => Q("df", value);
		}

		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public ExpandWildcards? ExpandWildcards
		{
			get => Q<ExpandWildcards? >("expand_wildcards");
			set => Q("expand_wildcards", value);
		}

		///<summary>Whether specified concrete, expanded or aliased indices should be ignored when throttled</summary>
		public bool? IgnoreThrottled
		{
			get => Q<bool? >("ignore_throttled");
			set => Q("ignore_throttled", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Specify whether format-based query failures (such as providing text to a numeric field) should be ignored</summary>
		public bool? Lenient
		{
			get => Q<bool? >("lenient");
			set => Q("lenient", value);
		}

		///<summary>Include only documents with a specific `_score` value in the result</summary>
		public double? MinScore
		{
			get => Q<double? >("min_score");
			set => Q("min_score", value);
		}

		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Query in the Lucene query string syntax</summary>
		public string QueryOnQueryString
		{
			get => Q<string>("q");
			set => Q("q", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>The maximum count for each shard, upon reaching which the query execution will terminate early</summary>
		public long? TerminateAfter
		{
			get => Q<long? >("terminate_after");
			set => Q("terminate_after", value);
		}
	}

	public partial class CountRequest<TDocument> : CountRequest, ICountRequest<TDocument>
	{
		protected ICountRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_count</summary>
		public CountRequest(): base(typeof(TDocument))
		{
		}

		///<summary>/{index}/_count</summary>
		///<param name = "index">Optional, accepts null</param>
		public CountRequest(Indices index): base(index)
		{
		}
	}

	[InterfaceDataContract]
	public partial interface ICreateRequest<TDocument> : IRequest<CreateRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	///<summary>Request for Create <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</para></summary>
	public partial class CreateRequest<TDocument> : PlainRequestBase<CreateRequestParameters>, ICreateRequest<TDocument>
	{
		protected ICreateRequest<TDocument> Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceCreate;
		///<summary>/{index}/_create/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public CreateRequest(IndexName index, Id id): base(r => r.Required("index", index).Required("id", id))
		{
		}

		///<summary>/{index}/_create/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public CreateRequest(Id id): this(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_create/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public CreateRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected CreateRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName ICreateRequest<TDocument>.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id ICreateRequest<TDocument>.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>The pipeline id to preprocess incoming documents with</summary>
		public string Pipeline
		{
			get => Q<string>("pipeline");
			set => Q("pipeline", value);
		}

		///<summary>
		/// If `true` then refresh the affected shards to make this operation visible to search, if `wait_for` then wait for a refresh to make this
		/// operation visible to search, if `false` (the default) then do nothing with refreshes.
		///</summary>
		public Refresh? Refresh
		{
			get => Q<Refresh? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Explicit operation timeout</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Explicit version number for concurrency control</summary>
		public long? Version
		{
			get => Q<long? >("version");
			set => Q("version", value);
		}

		///<summary>Specific version type</summary>
		public VersionType? VersionType
		{
			get => Q<VersionType? >("version_type");
			set => Q("version_type", value);
		}

		///<summary>
		/// Sets the number of shard copies that must be active before proceeding with the index operation. Defaults to 1, meaning the primary shard
		/// only. Set to `all` for all shard copies, otherwise set to any non-negative value less than or equal to the total number of copies for the
		/// shard (number of replicas + 1)
		///</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IDeleteRequest : IRequest<DeleteRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	public partial interface IDeleteRequest<TDocument> : IDeleteRequest
	{
	}

	///<summary>Request for Delete <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-document/</para></summary>
	public partial class DeleteRequest : PlainRequestBase<DeleteRequestParameters>, IDeleteRequest
	{
		protected IDeleteRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceDelete;
		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public DeleteRequest(IndexName index, Id id): base(r => r.Required("index", index).Required("id", id))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DeleteRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName IDeleteRequest.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id IDeleteRequest.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>only perform the delete operation if the last operation that has changed the document has the specified primary term</summary>
		public long? IfPrimaryTerm
		{
			get => Q<long? >("if_primary_term");
			set => Q("if_primary_term", value);
		}

		///<summary>only perform the delete operation if the last operation that has changed the document has the specified sequence number</summary>
		public long? IfSequenceNumber
		{
			get => Q<long? >("if_seq_no");
			set => Q("if_seq_no", value);
		}

		///<summary>
		/// If `true` then refresh the affected shards to make this operation visible to search, if `wait_for` then wait for a refresh to make this
		/// operation visible to search, if `false` (the default) then do nothing with refreshes.
		///</summary>
		public Refresh? Refresh
		{
			get => Q<Refresh? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Explicit operation timeout</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Explicit version number for concurrency control</summary>
		public long? Version
		{
			get => Q<long? >("version");
			set => Q("version", value);
		}

		///<summary>Specific version type</summary>
		public VersionType? VersionType
		{
			get => Q<VersionType? >("version_type");
			set => Q("version_type", value);
		}

		///<summary>
		/// Sets the number of shard copies that must be active before proceeding with the delete operation. Defaults to 1, meaning the primary shard
		/// only. Set to `all` for all shard copies, otherwise set to any non-negative value less than or equal to the total number of copies for the
		/// shard (number of replicas + 1)
		///</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	public partial class DeleteRequest<TDocument> : DeleteRequest, IDeleteRequest<TDocument>
	{
		protected IDeleteRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public DeleteRequest(IndexName index, Id id): base(index, id)
		{
		}

		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public DeleteRequest(Id id): base(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public DeleteRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DeleteRequest(): base()
		{
		}
	}

	[InterfaceDataContract]
	public partial interface IDeleteByQueryRequest : IRequest<DeleteByQueryRequestParameters>
	{
		[IgnoreDataMember]
		Indices Index
		{
			get;
		}
	}

	public partial interface IDeleteByQueryRequest<TDocument> : IDeleteByQueryRequest
	{
	}

	///<summary>Request for DeleteByQuery <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</para></summary>
	public partial class DeleteByQueryRequest : PlainRequestBase<DeleteByQueryRequestParameters>, IDeleteByQueryRequest
	{
		protected IDeleteByQueryRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceDeleteByQuery;
		///<summary>/{index}/_delete_by_query</summary>
		///<param name = "index">this parameter is required</param>
		public DeleteByQueryRequest(Indices index): base(r => r.Required("index", index))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DeleteByQueryRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Indices IDeleteByQueryRequest.Index => Self.RouteValues.Get<Indices>("index");
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

		///<summary>Specify whether wildcard and prefix queries should be analyzed (default: false)</summary>
		public bool? AnalyzeWildcard
		{
			get => Q<bool? >("analyze_wildcard");
			set => Q("analyze_wildcard", value);
		}

		///<summary>The analyzer to use for the query string</summary>
		public string Analyzer
		{
			get => Q<string>("analyzer");
			set => Q("analyzer", value);
		}

		///<summary>What to do when the delete by query hits version conflicts?</summary>
		public Conflicts? Conflicts
		{
			get => Q<Conflicts? >("conflicts");
			set => Q("conflicts", value);
		}

		///<summary>The default operator for query string query (AND or OR)</summary>
		public DefaultOperator? DefaultOperator
		{
			get => Q<DefaultOperator? >("default_operator");
			set => Q("default_operator", value);
		}

		///<summary>The field to use as default where no field prefix is given in the query string</summary>
		public string Df
		{
			get => Q<string>("df");
			set => Q("df", value);
		}

		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public ExpandWildcards? ExpandWildcards
		{
			get => Q<ExpandWildcards? >("expand_wildcards");
			set => Q("expand_wildcards", value);
		}

		///<summary>Starting offset (default: 0)</summary>
		public long? From
		{
			get => Q<long? >("from");
			set => Q("from", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Specify whether format-based query failures (such as providing text to a numeric field) should be ignored</summary>
		public bool? Lenient
		{
			get => Q<bool? >("lenient");
			set => Q("lenient", value);
		}

		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Query in the Lucene query string syntax</summary>
		public string QueryOnQueryString
		{
			get => Q<string>("q");
			set => Q("q", value);
		}

		///<summary>Should the effected indexes be refreshed?</summary>
		public bool? Refresh
		{
			get => Q<bool? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>Specify if request cache should be used for this request or not, defaults to index level setting</summary>
		public bool? RequestCache
		{
			get => Q<bool? >("request_cache");
			set => Q("request_cache", value);
		}

		///<summary>The throttle for this request in sub-requests per second. -1 means no throttle.</summary>
		public long? RequestsPerSecond
		{
			get => Q<long? >("requests_per_second");
			set => Q("requests_per_second", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Specify how long a consistent view of the index should be maintained for scrolled search</summary>
		public Time Scroll
		{
			get => Q<Time>("scroll");
			set => Q("scroll", value);
		}

		///<summary>Size on the scroll request powering the delete by query</summary>
		public long? ScrollSize
		{
			get => Q<long? >("scroll_size");
			set => Q("scroll_size", value);
		}

		///<summary>Explicit timeout for each search request. Defaults to no timeout.</summary>
		public Time SearchTimeout
		{
			get => Q<Time>("search_timeout");
			set => Q("search_timeout", value);
		}

		///<summary>Search operation type</summary>
		public SearchType? SearchType
		{
			get => Q<SearchType? >("search_type");
			set => Q("search_type", value);
		}

		///<summary>The number of slices this task should be divided into. Defaults to 1, meaning the task isn't sliced into subtasks.</summary>
		public long? Slices
		{
			get => Q<long? >("slices");
			set => Q("slices", value);
		}

		///<summary>A comma-separated list of &lt;field&gt;:&lt;direction&gt; pairs</summary>
		public string[] Sort
		{
			get => Q<string[]>("sort");
			set => Q("sort", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>A list of fields to exclude from the returned _source field</summary>
		public Fields SourceExcludes
		{
			get => Q<Fields>("_source_excludes");
			set => Q("_source_excludes", value);
		}

		///<summary>A list of fields to extract and return from the _source field</summary>
		public Fields SourceIncludes
		{
			get => Q<Fields>("_source_includes");
			set => Q("_source_includes", value);
		}

		///<summary>Specific 'tag' of the request for logging and statistical purposes</summary>
		public string[] Stats
		{
			get => Q<string[]>("stats");
			set => Q("stats", value);
		}

		///<summary>The maximum number of documents to collect for each shard, upon reaching which the query execution will terminate early.</summary>
		public long? TerminateAfter
		{
			get => Q<long? >("terminate_after");
			set => Q("terminate_after", value);
		}

		///<summary>Time each individual bulk request should wait for shards that are unavailable.</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Specify whether to return document version as part of a hit</summary>
		public bool? Version
		{
			get => Q<bool? >("version");
			set => Q("version", value);
		}

		///<summary>
		/// Sets the number of shard copies that must be active before proceeding with the delete by query operation. Defaults to 1, meaning the
		/// primary shard only. Set to `all` for all shard copies, otherwise set to any non-negative value less than or equal to the total number of
		/// copies for the shard (number of replicas + 1)
		///</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}

		///<summary>Should the request should block until the delete by query is complete.</summary>
		public bool? WaitForCompletion
		{
			get => Q<bool? >("wait_for_completion");
			set => Q("wait_for_completion", value);
		}
	}

	public partial class DeleteByQueryRequest<TDocument> : DeleteByQueryRequest, IDeleteByQueryRequest<TDocument>
	{
		protected IDeleteByQueryRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_delete_by_query</summary>
		///<param name = "index">this parameter is required</param>
		public DeleteByQueryRequest(Indices index): base(index)
		{
		}

		///<summary>/{index}/_delete_by_query</summary>
		public DeleteByQueryRequest(): base(typeof(TDocument))
		{
		}
	}

	[InterfaceDataContract]
	public partial interface IDeleteByQueryRethrottleRequest : IRequest<DeleteByQueryRethrottleRequestParameters>
	{
		[IgnoreDataMember]
		TaskId TaskId
		{
			get;
		}
	}

	///<summary>Request for DeleteByQueryRethrottle <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/delete-by-query/</para></summary>
	public partial class DeleteByQueryRethrottleRequest : PlainRequestBase<DeleteByQueryRethrottleRequestParameters>, IDeleteByQueryRethrottleRequest
	{
		protected IDeleteByQueryRethrottleRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceDeleteByQueryRethrottle;
		///<summary>/_delete_by_query/{task_id}/_rethrottle</summary>
		///<param name = "taskId">this parameter is required</param>
		public DeleteByQueryRethrottleRequest(TaskId taskId): base(r => r.Required("task_id", taskId))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DeleteByQueryRethrottleRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		TaskId IDeleteByQueryRethrottleRequest.TaskId => Self.RouteValues.Get<TaskId>("task_id");
		// Request parameters
		///<summary>The throttle to set on this request in floating sub-requests per second. -1 means set no throttle.</summary>
		public long? RequestsPerSecond
		{
			get => Q<long? >("requests_per_second");
			set => Q("requests_per_second", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IDeleteScriptRequest : IRequest<DeleteScriptRequestParameters>
	{
		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	///<summary>Request for DeleteScript <para></para></summary>
	public partial class DeleteScriptRequest : PlainRequestBase<DeleteScriptRequestParameters>, IDeleteScriptRequest
	{
		protected IDeleteScriptRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceDeleteScript;
		///<summary>/_scripts/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public DeleteScriptRequest(Id id): base(r => r.Required("id", id))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DeleteScriptRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Id IDeleteScriptRequest.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
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

		///<summary>Explicit operation timeout</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IDocumentExistsRequest : IRequest<DocumentExistsRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	public partial interface IDocumentExistsRequest<TDocument> : IDocumentExistsRequest
	{
	}

	///<summary>Request for DocumentExists <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
	public partial class DocumentExistsRequest : PlainRequestBase<DocumentExistsRequestParameters>, IDocumentExistsRequest
	{
		protected IDocumentExistsRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceDocumentExists;
		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public DocumentExistsRequest(IndexName index, Id id): base(r => r.Required("index", index).Required("id", id))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DocumentExistsRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName IDocumentExistsRequest.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id IDocumentExistsRequest.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Specify whether to perform the operation in realtime or search mode</summary>
		public bool? Realtime
		{
			get => Q<bool? >("realtime");
			set => Q("realtime", value);
		}

		///<summary>Refresh the shard containing the document before performing the operation</summary>
		public bool? Refresh
		{
			get => Q<bool? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>A list of fields to exclude from the returned _source field</summary>
		public Fields SourceExcludes
		{
			get => Q<Fields>("_source_excludes");
			set => Q("_source_excludes", value);
		}

		///<summary>A list of fields to extract and return from the _source field</summary>
		public Fields SourceIncludes
		{
			get => Q<Fields>("_source_includes");
			set => Q("_source_includes", value);
		}

		///<summary>A comma-separated list of stored fields to return in the response</summary>
		public Fields StoredFields
		{
			get => Q<Fields>("stored_fields");
			set => Q("stored_fields", value);
		}

		///<summary>Explicit version number for concurrency control</summary>
		public long? Version
		{
			get => Q<long? >("version");
			set => Q("version", value);
		}

		///<summary>Specific version type</summary>
		public VersionType? VersionType
		{
			get => Q<VersionType? >("version_type");
			set => Q("version_type", value);
		}
	}

	public partial class DocumentExistsRequest<TDocument> : DocumentExistsRequest, IDocumentExistsRequest<TDocument>
	{
		protected IDocumentExistsRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public DocumentExistsRequest(IndexName index, Id id): base(index, id)
		{
		}

		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public DocumentExistsRequest(Id id): base(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public DocumentExistsRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DocumentExistsRequest(): base()
		{
		}
	}

	[InterfaceDataContract]
	public partial interface ISourceExistsRequest : IRequest<SourceExistsRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	public partial interface ISourceExistsRequest<TDocument> : ISourceExistsRequest
	{
	}

	///<summary>Request for SourceExists <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
	public partial class SourceExistsRequest : PlainRequestBase<SourceExistsRequestParameters>, ISourceExistsRequest
	{
		protected ISourceExistsRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceSourceExists;
		///<summary>/{index}/_source/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public SourceExistsRequest(IndexName index, Id id): base(r => r.Required("index", index).Required("id", id))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected SourceExistsRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName ISourceExistsRequest.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id ISourceExistsRequest.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Specify whether to perform the operation in realtime or search mode</summary>
		public bool? Realtime
		{
			get => Q<bool? >("realtime");
			set => Q("realtime", value);
		}

		///<summary>Refresh the shard containing the document before performing the operation</summary>
		public bool? Refresh
		{
			get => Q<bool? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>A list of fields to exclude from the returned _source field</summary>
		public Fields SourceExcludes
		{
			get => Q<Fields>("_source_excludes");
			set => Q("_source_excludes", value);
		}

		///<summary>A list of fields to extract and return from the _source field</summary>
		public Fields SourceIncludes
		{
			get => Q<Fields>("_source_includes");
			set => Q("_source_includes", value);
		}

		///<summary>Explicit version number for concurrency control</summary>
		public long? Version
		{
			get => Q<long? >("version");
			set => Q("version", value);
		}

		///<summary>Specific version type</summary>
		public VersionType? VersionType
		{
			get => Q<VersionType? >("version_type");
			set => Q("version_type", value);
		}
	}

	public partial class SourceExistsRequest<TDocument> : SourceExistsRequest, ISourceExistsRequest<TDocument>
	{
		protected ISourceExistsRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_source/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public SourceExistsRequest(IndexName index, Id id): base(index, id)
		{
		}

		///<summary>/{index}/_source/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public SourceExistsRequest(Id id): base(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_source/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public SourceExistsRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected SourceExistsRequest(): base()
		{
		}
	}

	[InterfaceDataContract]
	public partial interface IExplainRequest : IRequest<ExplainRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}

		[DataMember(Name = "stored_fields")]
		Fields StoredFields
		{
			get;
			set;
		}
	}

	public partial interface IExplainRequest<TDocument> : IExplainRequest
	{
	}

	///<summary>Request for Explain <para>https://opensearch.org/docs/latest/opensearch/rest-api/explain/</para></summary>
	public partial class ExplainRequest : PlainRequestBase<ExplainRequestParameters>, IExplainRequest
	{
		protected IExplainRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceExplain;
		///<summary>/{index}/_explain/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public ExplainRequest(IndexName index, Id id): base(r => r.Required("index", index).Required("id", id))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected ExplainRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName IExplainRequest.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id IExplainRequest.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>Specify whether wildcards and prefix queries in the query string query should be analyzed (default: false)</summary>
		public bool? AnalyzeWildcard
		{
			get => Q<bool? >("analyze_wildcard");
			set => Q("analyze_wildcard", value);
		}

		///<summary>The analyzer for the query string query</summary>
		public string Analyzer
		{
			get => Q<string>("analyzer");
			set => Q("analyzer", value);
		}

		///<summary>The default operator for query string query (AND or OR)</summary>
		public DefaultOperator? DefaultOperator
		{
			get => Q<DefaultOperator? >("default_operator");
			set => Q("default_operator", value);
		}

		///<summary>The default field for query string query (default: _all)</summary>
		public string Df
		{
			get => Q<string>("df");
			set => Q("df", value);
		}

		///<summary>Specify whether format-based query failures (such as providing text to a numeric field) should be ignored</summary>
		public bool? Lenient
		{
			get => Q<bool? >("lenient");
			set => Q("lenient", value);
		}

		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Query in the Lucene query string syntax</summary>
		public string QueryOnQueryString
		{
			get => Q<string>("q");
			set => Q("q", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>A list of fields to exclude from the returned _source field</summary>
		public Fields SourceExcludes
		{
			get => Q<Fields>("_source_excludes");
			set => Q("_source_excludes", value);
		}

		///<summary>A list of fields to extract and return from the _source field</summary>
		public Fields SourceIncludes
		{
			get => Q<Fields>("_source_includes");
			set => Q("_source_includes", value);
		}
	}

	public partial class ExplainRequest<TDocument> : ExplainRequest, IExplainRequest<TDocument>
	{
		protected IExplainRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_explain/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public ExplainRequest(IndexName index, Id id): base(index, id)
		{
		}

		///<summary>/{index}/_explain/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public ExplainRequest(Id id): base(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_explain/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public ExplainRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected ExplainRequest(): base()
		{
		}
	}

	[InterfaceDataContract]
	public partial interface IFieldCapabilitiesRequest : IRequest<FieldCapabilitiesRequestParameters>
	{
		[IgnoreDataMember]
		Indices Index
		{
			get;
		}
	}

	///<summary>Request for FieldCapabilities <para></para></summary>
	public partial class FieldCapabilitiesRequest : PlainRequestBase<FieldCapabilitiesRequestParameters>, IFieldCapabilitiesRequest
	{
		protected IFieldCapabilitiesRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceFieldCapabilities;
		///<summary>/_field_caps</summary>
		public FieldCapabilitiesRequest(): base()
		{
		}

		///<summary>/{index}/_field_caps</summary>
		///<param name = "index">Optional, accepts null</param>
		public FieldCapabilitiesRequest(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Indices IFieldCapabilitiesRequest.Index => Self.RouteValues.Get<Indices>("index");
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

		///<summary>A comma-separated list of field names</summary>
		public Fields Fields
		{
			get => Q<Fields>("fields");
			set => Q("fields", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Indicates whether unmapped fields should be included in the response.</summary>
		public bool? IncludeUnmapped
		{
			get => Q<bool? >("include_unmapped");
			set => Q("include_unmapped", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IGetRequest : IRequest<GetRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	public partial interface IGetRequest<TDocument> : IGetRequest
	{
	}

	///<summary>Request for Get <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
	public partial class GetRequest : PlainRequestBase<GetRequestParameters>, IGetRequest
	{
		protected IGetRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceGet;
		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public GetRequest(IndexName index, Id id): base(r => r.Required("index", index).Required("id", id))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected GetRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName IGetRequest.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id IGetRequest.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Specify whether to perform the operation in realtime or search mode</summary>
		public bool? Realtime
		{
			get => Q<bool? >("realtime");
			set => Q("realtime", value);
		}

		///<summary>Refresh the shard containing the document before performing the operation</summary>
		public bool? Refresh
		{
			get => Q<bool? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>A list of fields to exclude from the returned _source field</summary>
		public Fields SourceExcludes
		{
			get => Q<Fields>("_source_excludes");
			set => Q("_source_excludes", value);
		}

		///<summary>A list of fields to extract and return from the _source field</summary>
		public Fields SourceIncludes
		{
			get => Q<Fields>("_source_includes");
			set => Q("_source_includes", value);
		}

		///<summary>A comma-separated list of stored fields to return in the response</summary>
		public Fields StoredFields
		{
			get => Q<Fields>("stored_fields");
			set => Q("stored_fields", value);
		}

		///<summary>Explicit version number for concurrency control</summary>
		public long? Version
		{
			get => Q<long? >("version");
			set => Q("version", value);
		}

		///<summary>Specific version type</summary>
		public VersionType? VersionType
		{
			get => Q<VersionType? >("version_type");
			set => Q("version_type", value);
		}
	}

	public partial class GetRequest<TDocument> : GetRequest, IGetRequest<TDocument>
	{
		protected IGetRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public GetRequest(IndexName index, Id id): base(index, id)
		{
		}

		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public GetRequest(Id id): base(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public GetRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected GetRequest(): base()
		{
		}
	}

	[InterfaceDataContract]
	public partial interface IGetScriptRequest : IRequest<GetScriptRequestParameters>
	{
		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	///<summary>Request for GetScript <para></para></summary>
	public partial class GetScriptRequest : PlainRequestBase<GetScriptRequestParameters>, IGetScriptRequest
	{
		protected IGetScriptRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceGetScript;
		///<summary>/_scripts/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public GetScriptRequest(Id id): base(r => r.Required("id", id))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected GetScriptRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Id IGetScriptRequest.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
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
	}

	[InterfaceDataContract]
	public partial interface ISourceRequest : IRequest<SourceRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	public partial interface ISourceRequest<TDocument> : ISourceRequest
	{
	}

	///<summary>Request for Source <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
	public partial class SourceRequest : PlainRequestBase<SourceRequestParameters>, ISourceRequest
	{
		protected ISourceRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceSource;
		///<summary>/{index}/_source/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public SourceRequest(IndexName index, Id id): base(r => r.Required("index", index).Required("id", id))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected SourceRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName ISourceRequest.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id ISourceRequest.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Specify whether to perform the operation in realtime or search mode</summary>
		public bool? Realtime
		{
			get => Q<bool? >("realtime");
			set => Q("realtime", value);
		}

		///<summary>Refresh the shard containing the document before performing the operation</summary>
		public bool? Refresh
		{
			get => Q<bool? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>A list of fields to exclude from the returned _source field</summary>
		public Fields SourceExcludes
		{
			get => Q<Fields>("_source_excludes");
			set => Q("_source_excludes", value);
		}

		///<summary>A list of fields to extract and return from the _source field</summary>
		public Fields SourceIncludes
		{
			get => Q<Fields>("_source_includes");
			set => Q("_source_includes", value);
		}

		///<summary>Explicit version number for concurrency control</summary>
		public long? Version
		{
			get => Q<long? >("version");
			set => Q("version", value);
		}

		///<summary>Specific version type</summary>
		public VersionType? VersionType
		{
			get => Q<VersionType? >("version_type");
			set => Q("version_type", value);
		}
	}

	public partial class SourceRequest<TDocument> : SourceRequest, ISourceRequest<TDocument>
	{
		protected ISourceRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_source/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public SourceRequest(IndexName index, Id id): base(index, id)
		{
		}

		///<summary>/{index}/_source/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public SourceRequest(Id id): base(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_source/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public SourceRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected SourceRequest(): base()
		{
		}
	}

	[InterfaceDataContract]
	public partial interface IIndexRequest<TDocument> : IRequest<IndexRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	///<summary>Request for Index <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/index-document/</para></summary>
	public partial class IndexRequest<TDocument> : PlainRequestBase<IndexRequestParameters>, IIndexRequest<TDocument>
	{
		protected IIndexRequest<TDocument> Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceIndex;
		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">Optional, accepts null</param>
		public IndexRequest(IndexName index, Id id): base(r => r.Required("index", index).Optional("id", id))
		{
		}

		///<summary>/{index}/_doc</summary>
		///<param name = "index">this parameter is required</param>
		public IndexRequest(IndexName index): base(r => r.Required("index", index))
		{
		}

		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "id">Optional, accepts null</param>
		public IndexRequest(Id id): this(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_doc</summary>
		public IndexRequest(): this(typeof(TDocument))
		{
		}

		///<summary>/{index}/_doc/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public IndexRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		// values part of the url path
		[IgnoreDataMember]
		IndexName IIndexRequest<TDocument>.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id IIndexRequest<TDocument>.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>only perform the index operation if the last operation that has changed the document has the specified primary term</summary>
		public long? IfPrimaryTerm
		{
			get => Q<long? >("if_primary_term");
			set => Q("if_primary_term", value);
		}

		///<summary>only perform the index operation if the last operation that has changed the document has the specified sequence number</summary>
		public long? IfSequenceNumber
		{
			get => Q<long? >("if_seq_no");
			set => Q("if_seq_no", value);
		}

		///<summary>
		/// Explicit operation type. Defaults to `index` for requests with an explicit document ID, and to `create`for requests without an explicit
		/// document ID
		///</summary>
		public OpType? OpType
		{
			get => Q<OpType? >("op_type");
			set => Q("op_type", value);
		}

		///<summary>The pipeline id to preprocess incoming documents with</summary>
		public string Pipeline
		{
			get => Q<string>("pipeline");
			set => Q("pipeline", value);
		}

		///<summary>
		/// If `true` then refresh the affected shards to make this operation visible to search, if `wait_for` then wait for a refresh to make this
		/// operation visible to search, if `false` (the default) then do nothing with refreshes.
		///</summary>
		public Refresh? Refresh
		{
			get => Q<Refresh? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>When true, requires destination to be an alias. Default is false</summary>
		public bool? RequireAlias
		{
			get => Q<bool? >("require_alias");
			set => Q("require_alias", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Explicit operation timeout</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Explicit version number for concurrency control</summary>
		public long? Version
		{
			get => Q<long? >("version");
			set => Q("version", value);
		}

		///<summary>Specific version type</summary>
		public VersionType? VersionType
		{
			get => Q<VersionType? >("version_type");
			set => Q("version_type", value);
		}

		///<summary>
		/// Sets the number of shard copies that must be active before proceeding with the index operation. Defaults to 1, meaning the primary shard
		/// only. Set to `all` for all shard copies, otherwise set to any non-negative value less than or equal to the total number of copies for the
		/// shard (number of replicas + 1)
		///</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IRootNodeInfoRequest : IRequest<RootNodeInfoRequestParameters>
	{
	}

	///<summary>Request for RootNodeInfo <para>https://opensearch.org/docs/latest/opensearch/index/</para></summary>
	public partial class RootNodeInfoRequest : PlainRequestBase<RootNodeInfoRequestParameters>, IRootNodeInfoRequest
	{
		protected IRootNodeInfoRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceRootNodeInfo;
	// values part of the url path
	// Request parameters
	}

	[InterfaceDataContract]
	public partial interface IMultiGetRequest : IRequest<MultiGetRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[DataMember(Name = "stored_fields")]
		Fields StoredFields
		{
			get;
			set;
		}
	}

	///<summary>Request for MultiGet <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/multi-get/</para></summary>
	public partial class MultiGetRequest : PlainRequestBase<MultiGetRequestParameters>, IMultiGetRequest
	{
		protected IMultiGetRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceMultiGet;
		///<summary>/_mget</summary>
		public MultiGetRequest(): base()
		{
		}

		///<summary>/{index}/_mget</summary>
		///<param name = "index">Optional, accepts null</param>
		public MultiGetRequest(IndexName index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName IMultiGetRequest.Index => Self.RouteValues.Get<IndexName>("index");
		// Request parameters
		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Specify whether to perform the operation in realtime or search mode</summary>
		public bool? Realtime
		{
			get => Q<bool? >("realtime");
			set => Q("realtime", value);
		}

		///<summary>Refresh the shard containing the document before performing the operation</summary>
		public bool? Refresh
		{
			get => Q<bool? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>A list of fields to exclude from the returned _source field</summary>
		public Fields SourceExcludes
		{
			get => Q<Fields>("_source_excludes");
			set => Q("_source_excludes", value);
		}

		///<summary>A list of fields to extract and return from the _source field</summary>
		public Fields SourceIncludes
		{
			get => Q<Fields>("_source_includes");
			set => Q("_source_includes", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IMultiSearchRequest : IRequest<MultiSearchRequestParameters>
	{
		[IgnoreDataMember]
		Indices Index
		{
			get;
		}
	}

	///<summary>Request for MultiSearch <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
	public partial class MultiSearchRequest : PlainRequestBase<MultiSearchRequestParameters>, IMultiSearchRequest
	{
		protected IMultiSearchRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceMultiSearch;
		///<summary>/_msearch</summary>
		public MultiSearchRequest(): base()
		{
		}

		///<summary>/{index}/_msearch</summary>
		///<param name = "index">Optional, accepts null</param>
		public MultiSearchRequest(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Indices IMultiSearchRequest.Index => Self.RouteValues.Get<Indices>("index");
		// Request parameters
		///<summary>Indicates whether network round-trips should be minimized as part of cross-cluster search requests execution</summary>
		public bool? CcsMinimizeRoundtrips
		{
			get => Q<bool? >("ccs_minimize_roundtrips");
			set => Q("ccs_minimize_roundtrips", value);
		}

		///<summary>Controls the maximum number of concurrent searches the multi search api will execute</summary>
		public long? MaxConcurrentSearches
		{
			get => Q<long? >("max_concurrent_searches");
			set => Q("max_concurrent_searches", value);
		}

		///<summary>
		/// The number of concurrent shard requests each sub search executes concurrently per node. This value should be used to limit the impact of
		/// the search on the cluster in order to limit the number of concurrent shard requests
		///</summary>
		public long? MaxConcurrentShardRequests
		{
			get => Q<long? >("max_concurrent_shard_requests");
			set => Q("max_concurrent_shard_requests", value);
		}

		///<summary>
		/// A threshold that enforces a pre-filter roundtrip to prefilter search shards based on query rewriting if the number of shards the search
		/// request expands to exceeds the threshold. This filter roundtrip can limit the number of shards significantly if for instance a shard can
		/// not match any documents based on its rewrite method ie. if date filters are mandatory to match but the shard bounds and the query are
		/// disjoint.
		///</summary>
		public long? PreFilterShardSize
		{
			get => Q<long? >("pre_filter_shard_size");
			set => Q("pre_filter_shard_size", value);
		}

		///<summary>Search operation type</summary>
		public SearchType? SearchType
		{
			get => Q<SearchType? >("search_type");
			set => Q("search_type", value);
		}

		///<summary>Indicates whether hits.total should be rendered as an integer or an object in the rest search response</summary>
		public bool? TotalHitsAsInteger
		{
			get => Q<bool? >("rest_total_hits_as_int");
			set => Q("rest_total_hits_as_int", value);
		}

		///<summary>Specify whether aggregation and suggester names should be prefixed by their respective types in the response</summary>
		public bool? TypedKeys
		{
			get => Q<bool? >("typed_keys");
			set => Q("typed_keys", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IMultiSearchTemplateRequest : IRequest<MultiSearchTemplateRequestParameters>
	{
		[IgnoreDataMember]
		Indices Index
		{
			get;
		}
	}

	///<summary>Request for MultiSearchTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/multi-search/</para></summary>
	public partial class MultiSearchTemplateRequest : PlainRequestBase<MultiSearchTemplateRequestParameters>, IMultiSearchTemplateRequest
	{
		protected IMultiSearchTemplateRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceMultiSearchTemplate;
		///<summary>/_msearch/template</summary>
		public MultiSearchTemplateRequest(): base()
		{
		}

		///<summary>/{index}/_msearch/template</summary>
		///<param name = "index">Optional, accepts null</param>
		public MultiSearchTemplateRequest(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Indices IMultiSearchTemplateRequest.Index => Self.RouteValues.Get<Indices>("index");
		// Request parameters
		///<summary>Indicates whether network round-trips should be minimized as part of cross-cluster search requests execution</summary>
		public bool? CcsMinimizeRoundtrips
		{
			get => Q<bool? >("ccs_minimize_roundtrips");
			set => Q("ccs_minimize_roundtrips", value);
		}

		///<summary>Controls the maximum number of concurrent searches the multi search api will execute</summary>
		public long? MaxConcurrentSearches
		{
			get => Q<long? >("max_concurrent_searches");
			set => Q("max_concurrent_searches", value);
		}

		///<summary>Search operation type</summary>
		public SearchType? SearchType
		{
			get => Q<SearchType? >("search_type");
			set => Q("search_type", value);
		}

		///<summary>Indicates whether hits.total should be rendered as an integer or an object in the rest search response</summary>
		public bool? TotalHitsAsInteger
		{
			get => Q<bool? >("rest_total_hits_as_int");
			set => Q("rest_total_hits_as_int", value);
		}

		///<summary>Specify whether aggregation and suggester names should be prefixed by their respective types in the response</summary>
		public bool? TypedKeys
		{
			get => Q<bool? >("typed_keys");
			set => Q("typed_keys", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IMultiTermVectorsRequest : IRequest<MultiTermVectorsRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}
	}

	///<summary>Request for MultiTermVectors <para></para></summary>
	public partial class MultiTermVectorsRequest : PlainRequestBase<MultiTermVectorsRequestParameters>, IMultiTermVectorsRequest
	{
		protected IMultiTermVectorsRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceMultiTermVectors;
		///<summary>/_mtermvectors</summary>
		public MultiTermVectorsRequest(): base()
		{
		}

		///<summary>/{index}/_mtermvectors</summary>
		///<param name = "index">Optional, accepts null</param>
		public MultiTermVectorsRequest(IndexName index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName IMultiTermVectorsRequest.Index => Self.RouteValues.Get<IndexName>("index");
		// Request parameters
		///<summary>
		/// Specifies if document count, sum of document frequencies and sum of total term frequencies should be returned. Applies to all returned
		/// documents unless otherwise specified in body "params" or "docs".
		///</summary>
		public bool? FieldStatistics
		{
			get => Q<bool? >("field_statistics");
			set => Q("field_statistics", value);
		}

		///<summary>A comma-separated list of fields to return. Applies to all returned documents unless otherwise specified in body "params" or "docs".</summary>
		public Fields Fields
		{
			get => Q<Fields>("fields");
			set => Q("fields", value);
		}

		///<summary>Specifies if term offsets should be returned. Applies to all returned documents unless otherwise specified in body "params" or "docs".</summary>
		public bool? Offsets
		{
			get => Q<bool? >("offsets");
			set => Q("offsets", value);
		}

		///<summary>Specifies if term payloads should be returned. Applies to all returned documents unless otherwise specified in body "params" or "docs".</summary>
		public bool? Payloads
		{
			get => Q<bool? >("payloads");
			set => Q("payloads", value);
		}

		///<summary>Specifies if term positions should be returned. Applies to all returned documents unless otherwise specified in body "params" or "docs".</summary>
		public bool? Positions
		{
			get => Q<bool? >("positions");
			set => Q("positions", value);
		}

		///<summary>
		/// Specify the node or shard the operation should be performed on (default: random) .Applies to all returned documents unless otherwise
		/// specified in body "params" or "docs".
		///</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Specifies if requests are real-time as opposed to near-real-time (default: true).</summary>
		public bool? Realtime
		{
			get => Q<bool? >("realtime");
			set => Q("realtime", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>
		/// Specifies if total term frequency and document frequency should be returned. Applies to all returned documents unless otherwise specified
		/// in body "params" or "docs".
		///</summary>
		public bool? TermStatistics
		{
			get => Q<bool? >("term_statistics");
			set => Q("term_statistics", value);
		}

		///<summary>Explicit version number for concurrency control</summary>
		public long? Version
		{
			get => Q<long? >("version");
			set => Q("version", value);
		}

		///<summary>Specific version type</summary>
		public VersionType? VersionType
		{
			get => Q<VersionType? >("version_type");
			set => Q("version_type", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IPingRequest : IRequest<PingRequestParameters>
	{
	}

	///<summary>Request for Ping <para>https://opensearch.org/docs/latest/opensearch/index/</para></summary>
	public partial class PingRequest : PlainRequestBase<PingRequestParameters>, IPingRequest
	{
		protected IPingRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespacePing;
	// values part of the url path
	// Request parameters
	}

	[InterfaceDataContract]
	public partial interface IPutScriptRequest : IRequest<PutScriptRequestParameters>
	{
		[IgnoreDataMember]
		Id Id
		{
			get;
		}

		[IgnoreDataMember]
		Name Context
		{
			get;
		}
	}

	///<summary>Request for PutScript <para></para></summary>
	public partial class PutScriptRequest : PlainRequestBase<PutScriptRequestParameters>, IPutScriptRequest
	{
		protected IPutScriptRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespacePutScript;
		///<summary>/_scripts/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public PutScriptRequest(Id id): base(r => r.Required("id", id))
		{
		}

		///<summary>/_scripts/{id}/{context}</summary>
		///<param name = "id">this parameter is required</param>
		///<param name = "context">Optional, accepts null</param>
		public PutScriptRequest(Id id, Name context): base(r => r.Required("id", id).Optional("context", context))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected PutScriptRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Id IPutScriptRequest.Id => Self.RouteValues.Get<Id>("id");
		[IgnoreDataMember]
		Name IPutScriptRequest.Context => Self.RouteValues.Get<Name>("context");
		// Request parameters
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

		///<summary>Explicit operation timeout</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IReindexOnServerRequest : IRequest<ReindexOnServerRequestParameters>
	{
	}

	///<summary>Request for ReindexOnServer <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</para></summary>
	public partial class ReindexOnServerRequest : PlainRequestBase<ReindexOnServerRequestParameters>, IReindexOnServerRequest
	{
		protected IReindexOnServerRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceReindexOnServer;
		// values part of the url path
		// Request parameters
		///<summary>Should the affected indexes be refreshed?</summary>
		public bool? Refresh
		{
			get => Q<bool? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>The throttle to set on this request in sub-requests per second. -1 means no throttle.</summary>
		public long? RequestsPerSecond
		{
			get => Q<long? >("requests_per_second");
			set => Q("requests_per_second", value);
		}

		///<summary>Control how long to keep the search context alive</summary>
		public Time Scroll
		{
			get => Q<Time>("scroll");
			set => Q("scroll", value);
		}

		///<summary>The number of slices this task should be divided into. Defaults to 1, meaning the task isn't sliced into subtasks. Can be set to `auto`.</summary>
		public long? Slices
		{
			get => Q<long? >("slices");
			set => Q("slices", value);
		}

		///<summary>Time each individual bulk request should wait for shards that are unavailable.</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}

		///<summary>
		/// Sets the number of shard copies that must be active before proceeding with the reindex operation. Defaults to 1, meaning the primary shard
		/// only. Set to `all` for all shard copies, otherwise set to any non-negative value less than or equal to the total number of copies for the
		/// shard (number of replicas + 1)
		///</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}

		///<summary>Should the request should block until the reindex is complete.</summary>
		public bool? WaitForCompletion
		{
			get => Q<bool? >("wait_for_completion");
			set => Q("wait_for_completion", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IReindexRethrottleRequest : IRequest<ReindexRethrottleRequestParameters>
	{
		[IgnoreDataMember]
		TaskId TaskId
		{
			get;
		}
	}

	///<summary>Request for ReindexRethrottle <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/reindex/</para></summary>
	public partial class ReindexRethrottleRequest : PlainRequestBase<ReindexRethrottleRequestParameters>, IReindexRethrottleRequest
	{
		protected IReindexRethrottleRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceReindexRethrottle;
		///<summary>/_reindex/{task_id}/_rethrottle</summary>
		///<param name = "taskId">this parameter is required</param>
		public ReindexRethrottleRequest(TaskId taskId): base(r => r.Required("task_id", taskId))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected ReindexRethrottleRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		TaskId IReindexRethrottleRequest.TaskId => Self.RouteValues.Get<TaskId>("task_id");
		// Request parameters
		///<summary>The throttle to set on this request in floating sub-requests per second. -1 means set no throttle.</summary>
		public long? RequestsPerSecond
		{
			get => Q<long? >("requests_per_second");
			set => Q("requests_per_second", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IRenderSearchTemplateRequest : IRequest<RenderSearchTemplateRequestParameters>
	{
		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	///<summary>Request for RenderSearchTemplate <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
	public partial class RenderSearchTemplateRequest : PlainRequestBase<RenderSearchTemplateRequestParameters>, IRenderSearchTemplateRequest
	{
		protected IRenderSearchTemplateRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceRenderSearchTemplate;
		///<summary>/_render/template</summary>
		public RenderSearchTemplateRequest(): base()
		{
		}

		///<summary>/_render/template/{id}</summary>
		///<param name = "id">Optional, accepts null</param>
		public RenderSearchTemplateRequest(Id id): base(r => r.Optional("id", id))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Id IRenderSearchTemplateRequest.Id => Self.RouteValues.Get<Id>("id");
	// Request parameters
	}

	[InterfaceDataContract]
	public partial interface IExecutePainlessScriptRequest : IRequest<ExecutePainlessScriptRequestParameters>
	{
	}

	///<summary>Request for ExecutePainlessScript <para></para></summary>
	///<remarks>Note: Experimental within the OpenSearch server, this functionality is experimental and may be changed or removed completely in a future release. OpenSearch will take a best effort approach to fix any issues, but experimental features are not subject to the support SLA of official GA features.</remarks>
	public partial class ExecutePainlessScriptRequest : PlainRequestBase<ExecutePainlessScriptRequestParameters>, IExecutePainlessScriptRequest
	{
		protected IExecutePainlessScriptRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceExecutePainlessScript;
	// values part of the url path
	// Request parameters
	}

	[InterfaceDataContract]
	public partial interface IScrollRequest : IRequest<ScrollRequestParameters>
	{
	}

	///<summary>Request for Scroll <para>https://opensearch.org/docs/latest/opensearch/rest-api/search/#request-body</para></summary>
	public partial class ScrollRequest : PlainRequestBase<ScrollRequestParameters>, IScrollRequest
	{
		protected IScrollRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceScroll;
		// values part of the url path
		// Request parameters
		///<summary>Indicates whether hits.total should be rendered as an integer or an object in the rest search response</summary>
		public bool? TotalHitsAsInteger
		{
			get => Q<bool? >("rest_total_hits_as_int");
			set => Q("rest_total_hits_as_int", value);
		}
	}

	[InterfaceDataContract]
	public partial interface ISearchRequest : IRequest<SearchRequestParameters>
	{
		[IgnoreDataMember]
		Indices Index
		{
			get;
		}

		[DataMember(Name = "docvalue_fields")]
		Fields DocValueFields
		{
			get;
			set;
		}

		[DataMember(Name = "stored_fields")]
		Fields StoredFields
		{
			get;
			set;
		}

		[DataMember(Name = "track_total_hits")]
		TrackTotalHits TrackTotalHits
		{
			get;
			set;
		}
	}

	public partial interface ISearchRequest<TInferDocument> : ISearchRequest
	{
	}

	///<summary>Request for Search <para>https://opensearch.org/docs/latest/opensearch/rest-api/search/</para></summary>
	public partial class SearchRequest : PlainRequestBase<SearchRequestParameters>, ISearchRequest
	{
		protected ISearchRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceSearch;
		///<summary>/_search</summary>
		public SearchRequest(): base()
		{
		}

		///<summary>/{index}/_search</summary>
		///<param name = "index">Optional, accepts null</param>
		public SearchRequest(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Indices ISearchRequest.Index => Self.RouteValues.Get<Indices>("index");
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

		///<summary>Indicate if an error should be returned if there is a partial search failure or timeout</summary>
		public bool? AllowPartialSearchResults
		{
			get => Q<bool? >("allow_partial_search_results");
			set => Q("allow_partial_search_results", value);
		}

		///<summary>Specify whether wildcard and prefix queries should be analyzed (default: false)</summary>
		public bool? AnalyzeWildcard
		{
			get => Q<bool? >("analyze_wildcard");
			set => Q("analyze_wildcard", value);
		}

		///<summary>The analyzer to use for the query string</summary>
		public string Analyzer
		{
			get => Q<string>("analyzer");
			set => Q("analyzer", value);
		}

		///<summary>
		/// The number of shard results that should be reduced at once on the coordinating node. This value should be used as a protection mechanism
		/// to reduce the memory overhead per search request if the potential number of shards in the request can be large.
		///</summary>
		public long? BatchedReduceSize
		{
			get => Q<long? >("batched_reduce_size");
			set => Q("batched_reduce_size", value);
		}

		///<summary>Indicates whether network round-trips should be minimized as part of cross-cluster search requests execution</summary>
		public bool? CcsMinimizeRoundtrips
		{
			get => Q<bool? >("ccs_minimize_roundtrips");
			set => Q("ccs_minimize_roundtrips", value);
		}

		///<summary>The default operator for query string query (AND or OR)</summary>
		public DefaultOperator? DefaultOperator
		{
			get => Q<DefaultOperator? >("default_operator");
			set => Q("default_operator", value);
		}

		///<summary>The field to use as default where no field prefix is given in the query string</summary>
		public string Df
		{
			get => Q<string>("df");
			set => Q("df", value);
		}

		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public ExpandWildcards? ExpandWildcards
		{
			get => Q<ExpandWildcards? >("expand_wildcards");
			set => Q("expand_wildcards", value);
		}

		///<summary>Whether specified concrete, expanded or aliased indices should be ignored when throttled</summary>
		public bool? IgnoreThrottled
		{
			get => Q<bool? >("ignore_throttled");
			set => Q("ignore_throttled", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Specify whether format-based query failures (such as providing text to a numeric field) should be ignored</summary>
		public bool? Lenient
		{
			get => Q<bool? >("lenient");
			set => Q("lenient", value);
		}

		///<summary>
		/// The number of concurrent shard requests per node this search executes concurrently. This value should be used to limit the impact of the
		/// search on the cluster in order to limit the number of concurrent shard requests
		///</summary>
		public long? MaxConcurrentShardRequests
		{
			get => Q<long? >("max_concurrent_shard_requests");
			set => Q("max_concurrent_shard_requests", value);
		}

		///<summary>
		/// A threshold that enforces a pre-filter roundtrip to prefilter search shards based on query rewriting if the number of shards the search
		/// request expands to exceeds the threshold. This filter roundtrip can limit the number of shards significantly if for instance a shard can
		/// not match any documents based on its rewrite method ie. if date filters are mandatory to match but the shard bounds and the query are
		/// disjoint.
		///</summary>
		public long? PreFilterShardSize
		{
			get => Q<long? >("pre_filter_shard_size");
			set => Q("pre_filter_shard_size", value);
		}

		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Query in the Lucene query string syntax</summary>
		public string QueryOnQueryString
		{
			get => Q<string>("q");
			set => Q("q", value);
		}

		///<summary>Specify if request cache should be used for this request or not, defaults to index level setting</summary>
		public bool? RequestCache
		{
			get => Q<bool? >("request_cache");
			set => Q("request_cache", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Specify how long a consistent view of the index should be maintained for scrolled search</summary>
		public Time Scroll
		{
			get => Q<Time>("scroll");
			set => Q("scroll", value);
		}

		///<summary>Search operation type</summary>
		public SearchType? SearchType
		{
			get => Q<SearchType? >("search_type");
			set => Q("search_type", value);
		}

		///<summary>Specify whether to return sequence number and primary term of the last modification of each hit</summary>
		public bool? SequenceNumberPrimaryTerm
		{
			get => Q<bool? >("seq_no_primary_term");
			set => Q("seq_no_primary_term", value);
		}

		///<summary>Specific 'tag' of the request for logging and statistical purposes</summary>
		public string[] Stats
		{
			get => Q<string[]>("stats");
			set => Q("stats", value);
		}

		///<summary>Specify which field to use for suggestions</summary>
		public Field SuggestField
		{
			get => Q<Field>("suggest_field");
			set => Q("suggest_field", value);
		}

		///<summary>Specify suggest mode</summary>
		public SuggestMode? SuggestMode
		{
			get => Q<SuggestMode? >("suggest_mode");
			set => Q("suggest_mode", value);
		}

		///<summary>How many suggestions to return in response</summary>
		public long? SuggestSize
		{
			get => Q<long? >("suggest_size");
			set => Q("suggest_size", value);
		}

		///<summary>The source text for which the suggestions should be returned</summary>
		public string SuggestText
		{
			get => Q<string>("suggest_text");
			set => Q("suggest_text", value);
		}

		///<summary>Indicates whether hits.total should be rendered as an integer or an object in the rest search response</summary>
		public bool? TotalHitsAsInteger
		{
			get => Q<bool? >("rest_total_hits_as_int");
			set => Q("rest_total_hits_as_int", value);
		}

		///<summary>Specify whether aggregation and suggester names should be prefixed by their respective types in the response</summary>
		public bool? TypedKeys
		{
			get => Q<bool? >("typed_keys");
			set => Q("typed_keys", value);
		}
	}

	public partial class SearchRequest<TInferDocument> : SearchRequest, ISearchRequest<TInferDocument>
	{
		protected ISearchRequest<TInferDocument> TypedSelf => this;
		///<summary>/{index}/_search</summary>
		public SearchRequest(): base(typeof(TInferDocument))
		{
		}

		///<summary>/{index}/_search</summary>
		///<param name = "index">Optional, accepts null</param>
		public SearchRequest(Indices index): base(index)
		{
		}
	}

	[InterfaceDataContract]
	public partial interface ISearchShardsRequest : IRequest<SearchShardsRequestParameters>
	{
		[IgnoreDataMember]
		Indices Index
		{
			get;
		}
	}

	public partial interface ISearchShardsRequest<TDocument> : ISearchShardsRequest
	{
	}

	///<summary>Request for SearchShards <para>https://opensearch.org/docs/latest/security-plugin/access-control/cross-cluster-search/</para></summary>
	public partial class SearchShardsRequest : PlainRequestBase<SearchShardsRequestParameters>, ISearchShardsRequest
	{
		protected ISearchShardsRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceSearchShards;
		///<summary>/_search_shards</summary>
		public SearchShardsRequest(): base()
		{
		}

		///<summary>/{index}/_search_shards</summary>
		///<param name = "index">Optional, accepts null</param>
		public SearchShardsRequest(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Indices ISearchShardsRequest.Index => Self.RouteValues.Get<Indices>("index");
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

		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}
	}

	public partial class SearchShardsRequest<TDocument> : SearchShardsRequest, ISearchShardsRequest<TDocument>
	{
		protected ISearchShardsRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_search_shards</summary>
		public SearchShardsRequest(): base(typeof(TDocument))
		{
		}

		///<summary>/{index}/_search_shards</summary>
		///<param name = "index">Optional, accepts null</param>
		public SearchShardsRequest(Indices index): base(index)
		{
		}
	}

	[InterfaceDataContract]
	public partial interface ISearchTemplateRequest : IRequest<SearchTemplateRequestParameters>
	{
		[IgnoreDataMember]
		Indices Index
		{
			get;
		}
	}

	///<summary>Request for SearchTemplate <para>https://opensearch.org/docs/latest/opensearch/search-template/</para></summary>
	public partial class SearchTemplateRequest : PlainRequestBase<SearchTemplateRequestParameters>, ISearchTemplateRequest
	{
		protected ISearchTemplateRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceSearchTemplate;
		///<summary>/_search/template</summary>
		public SearchTemplateRequest(): base()
		{
		}

		///<summary>/{index}/_search/template</summary>
		///<param name = "index">Optional, accepts null</param>
		public SearchTemplateRequest(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Indices ISearchTemplateRequest.Index => Self.RouteValues.Get<Indices>("index");
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

		///<summary>Indicates whether network round-trips should be minimized as part of cross-cluster search requests execution</summary>
		public bool? CcsMinimizeRoundtrips
		{
			get => Q<bool? >("ccs_minimize_roundtrips");
			set => Q("ccs_minimize_roundtrips", value);
		}

		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public ExpandWildcards? ExpandWildcards
		{
			get => Q<ExpandWildcards? >("expand_wildcards");
			set => Q("expand_wildcards", value);
		}

		///<summary>Specify whether to return detailed information about score computation as part of a hit</summary>
		public bool? Explain
		{
			get => Q<bool? >("explain");
			set => Q("explain", value);
		}

		///<summary>Whether specified concrete, expanded or aliased indices should be ignored when throttled</summary>
		public bool? IgnoreThrottled
		{
			get => Q<bool? >("ignore_throttled");
			set => Q("ignore_throttled", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Specify whether to profile the query execution</summary>
		public bool? Profile
		{
			get => Q<bool? >("profile");
			set => Q("profile", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Specify how long a consistent view of the index should be maintained for scrolled search</summary>
		public Time Scroll
		{
			get => Q<Time>("scroll");
			set => Q("scroll", value);
		}

		///<summary>Search operation type</summary>
		public SearchType? SearchType
		{
			get => Q<SearchType? >("search_type");
			set => Q("search_type", value);
		}

		///<summary>Indicates whether hits.total should be rendered as an integer or an object in the rest search response</summary>
		public bool? TotalHitsAsInteger
		{
			get => Q<bool? >("rest_total_hits_as_int");
			set => Q("rest_total_hits_as_int", value);
		}

		///<summary>Specify whether aggregation and suggester names should be prefixed by their respective types in the response</summary>
		public bool? TypedKeys
		{
			get => Q<bool? >("typed_keys");
			set => Q("typed_keys", value);
		}
	}

	[InterfaceDataContract]
	public partial interface ITermVectorsRequest<TDocument> : IRequest<TermVectorsRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	///<summary>Request for TermVectors <para></para></summary>
	public partial class TermVectorsRequest<TDocument> : PlainRequestBase<TermVectorsRequestParameters>, ITermVectorsRequest<TDocument>
	{
		protected ITermVectorsRequest<TDocument> Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceTermVectors;
		///<summary>/{index}/_termvectors/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">Optional, accepts null</param>
		public TermVectorsRequest(IndexName index, Id id): base(r => r.Required("index", index).Optional("id", id))
		{
		}

		///<summary>/{index}/_termvectors</summary>
		///<param name = "index">this parameter is required</param>
		public TermVectorsRequest(IndexName index): base(r => r.Required("index", index))
		{
		}

		///<summary>/{index}/_termvectors/{id}</summary>
		///<param name = "id">Optional, accepts null</param>
		public TermVectorsRequest(Id id): this(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_termvectors</summary>
		public TermVectorsRequest(): this(typeof(TDocument))
		{
		}

		///<summary>/{index}/_termvectors/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public TermVectorsRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		// values part of the url path
		[IgnoreDataMember]
		IndexName ITermVectorsRequest<TDocument>.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id ITermVectorsRequest<TDocument>.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>Specifies if document count, sum of document frequencies and sum of total term frequencies should be returned.</summary>
		public bool? FieldStatistics
		{
			get => Q<bool? >("field_statistics");
			set => Q("field_statistics", value);
		}

		///<summary>A comma-separated list of fields to return.</summary>
		public Fields Fields
		{
			get => Q<Fields>("fields");
			set => Q("fields", value);
		}

		///<summary>Specifies if term offsets should be returned.</summary>
		public bool? Offsets
		{
			get => Q<bool? >("offsets");
			set => Q("offsets", value);
		}

		///<summary>Specifies if term payloads should be returned.</summary>
		public bool? Payloads
		{
			get => Q<bool? >("payloads");
			set => Q("payloads", value);
		}

		///<summary>Specifies if term positions should be returned.</summary>
		public bool? Positions
		{
			get => Q<bool? >("positions");
			set => Q("positions", value);
		}

		///<summary>Specify the node or shard the operation should be performed on (default: random).</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Specifies if request is real-time as opposed to near-real-time (default: true).</summary>
		public bool? Realtime
		{
			get => Q<bool? >("realtime");
			set => Q("realtime", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Specifies if total term frequency and document frequency should be returned.</summary>
		public bool? TermStatistics
		{
			get => Q<bool? >("term_statistics");
			set => Q("term_statistics", value);
		}

		///<summary>Explicit version number for concurrency control</summary>
		public long? Version
		{
			get => Q<long? >("version");
			set => Q("version", value);
		}

		///<summary>Specific version type</summary>
		public VersionType? VersionType
		{
			get => Q<VersionType? >("version_type");
			set => Q("version_type", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IUpdateRequest<TDocument, TPartialDocument> : IRequest<UpdateRequestParameters>
	{
		[IgnoreDataMember]
		IndexName Index
		{
			get;
		}

		[IgnoreDataMember]
		Id Id
		{
			get;
		}
	}

	///<summary>Request for Update <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-document/</para></summary>
	public partial class UpdateRequest<TDocument, TPartialDocument> : PlainRequestBase<UpdateRequestParameters>, IUpdateRequest<TDocument, TPartialDocument>
	{
		protected IUpdateRequest<TDocument, TPartialDocument> Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceUpdate;
		///<summary>/{index}/_update/{id}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "id">this parameter is required</param>
		public UpdateRequest(IndexName index, Id id): base(r => r.Required("index", index).Required("id", id))
		{
		}

		///<summary>/{index}/_update/{id}</summary>
		///<param name = "id">this parameter is required</param>
		public UpdateRequest(Id id): this(typeof(TDocument), id)
		{
		}

		///<summary>/{index}/_update/{id}</summary>
		///<param name = "id">The document used to resolve the path from</param>
		public UpdateRequest(TDocument documentWithId, IndexName index = null, Id id = null): this(index ?? typeof(TDocument), id ?? Id.From(documentWithId)) => DocumentFromPath(documentWithId);
		partial void DocumentFromPath(TDocument document);
		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected UpdateRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		IndexName IUpdateRequest<TDocument, TPartialDocument>.Index => Self.RouteValues.Get<IndexName>("index");
		[IgnoreDataMember]
		Id IUpdateRequest<TDocument, TPartialDocument>.Id => Self.RouteValues.Get<Id>("id");
		// Request parameters
		///<summary>only perform the update operation if the last operation that has changed the document has the specified primary term</summary>
		public long? IfPrimaryTerm
		{
			get => Q<long? >("if_primary_term");
			set => Q("if_primary_term", value);
		}

		///<summary>only perform the update operation if the last operation that has changed the document has the specified sequence number</summary>
		public long? IfSequenceNumber
		{
			get => Q<long? >("if_seq_no");
			set => Q("if_seq_no", value);
		}

		///<summary>The script language (default: painless)</summary>
		public string Lang
		{
			get => Q<string>("lang");
			set => Q("lang", value);
		}

		///<summary>
		/// If `true` then refresh the affected shards to make this operation visible to search, if `wait_for` then wait for a refresh to make this
		/// operation visible to search, if `false` (the default) then do nothing with refreshes.
		///</summary>
		public Refresh? Refresh
		{
			get => Q<Refresh? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>When true, requires destination is an alias. Default is false</summary>
		public bool? RequireAlias
		{
			get => Q<bool? >("require_alias");
			set => Q("require_alias", value);
		}

		///<summary>Specify how many times should the operation be retried when a conflict occurs (default: 0)</summary>
		public long? RetryOnConflict
		{
			get => Q<long? >("retry_on_conflict");
			set => Q("retry_on_conflict", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>Explicit operation timeout</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}

		///<summary>
		/// Sets the number of shard copies that must be active before proceeding with the update operation. Defaults to 1, meaning the primary shard
		/// only. Set to `all` for all shard copies, otherwise set to any non-negative value less than or equal to the total number of copies for the
		/// shard (number of replicas + 1)
		///</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	[InterfaceDataContract]
	public partial interface IUpdateByQueryRequest : IRequest<UpdateByQueryRequestParameters>
	{
		[IgnoreDataMember]
		Indices Index
		{
			get;
		}
	}

	public partial interface IUpdateByQueryRequest<TDocument> : IUpdateByQueryRequest
	{
	}

	///<summary>Request for UpdateByQuery <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</para></summary>
	public partial class UpdateByQueryRequest : PlainRequestBase<UpdateByQueryRequestParameters>, IUpdateByQueryRequest
	{
		protected IUpdateByQueryRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceUpdateByQuery;
		///<summary>/{index}/_update_by_query</summary>
		///<param name = "index">this parameter is required</param>
		public UpdateByQueryRequest(Indices index): base(r => r.Required("index", index))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected UpdateByQueryRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		Indices IUpdateByQueryRequest.Index => Self.RouteValues.Get<Indices>("index");
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

		///<summary>Specify whether wildcard and prefix queries should be analyzed (default: false)</summary>
		public bool? AnalyzeWildcard
		{
			get => Q<bool? >("analyze_wildcard");
			set => Q("analyze_wildcard", value);
		}

		///<summary>The analyzer to use for the query string</summary>
		public string Analyzer
		{
			get => Q<string>("analyzer");
			set => Q("analyzer", value);
		}

		///<summary>What to do when the update by query hits version conflicts?</summary>
		public Conflicts? Conflicts
		{
			get => Q<Conflicts? >("conflicts");
			set => Q("conflicts", value);
		}

		///<summary>The default operator for query string query (AND or OR)</summary>
		public DefaultOperator? DefaultOperator
		{
			get => Q<DefaultOperator? >("default_operator");
			set => Q("default_operator", value);
		}

		///<summary>The field to use as default where no field prefix is given in the query string</summary>
		public string Df
		{
			get => Q<string>("df");
			set => Q("df", value);
		}

		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public ExpandWildcards? ExpandWildcards
		{
			get => Q<ExpandWildcards? >("expand_wildcards");
			set => Q("expand_wildcards", value);
		}

		///<summary>Starting offset (default: 0)</summary>
		public long? From
		{
			get => Q<long? >("from");
			set => Q("from", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Specify whether format-based query failures (such as providing text to a numeric field) should be ignored</summary>
		public bool? Lenient
		{
			get => Q<bool? >("lenient");
			set => Q("lenient", value);
		}

		///<summary>Ingest pipeline to set on index requests made by this action. (default: none)</summary>
		public string Pipeline
		{
			get => Q<string>("pipeline");
			set => Q("pipeline", value);
		}

		///<summary>Specify the node or shard the operation should be performed on (default: random)</summary>
		public string Preference
		{
			get => Q<string>("preference");
			set => Q("preference", value);
		}

		///<summary>Query in the Lucene query string syntax</summary>
		public string QueryOnQueryString
		{
			get => Q<string>("q");
			set => Q("q", value);
		}

		///<summary>Should the affected indexes be refreshed?</summary>
		public bool? Refresh
		{
			get => Q<bool? >("refresh");
			set => Q("refresh", value);
		}

		///<summary>Specify if request cache should be used for this request or not, defaults to index level setting</summary>
		public bool? RequestCache
		{
			get => Q<bool? >("request_cache");
			set => Q("request_cache", value);
		}

		///<summary>The throttle to set on this request in sub-requests per second. -1 means no throttle.</summary>
		public long? RequestsPerSecond
		{
			get => Q<long? >("requests_per_second");
			set => Q("requests_per_second", value);
		}

		///<summary>
		/// A document is routed to a particular shard in an index using the following formula
		/// <para> shard_num = hash(_routing) % num_primary_shards</para>
		/// <para>OpenSearch will use the document id if not provided. </para>
		/// <para>For requests that are constructed from/for a document OSC will automatically infer the routing key
		/// if that document has a <see cref = "OpenSearch.Client.JoinField"/> or a routing mapping on for its type exists on <see cref = "OpenSearch.Client.ConnectionSettings"/>
		/// </para>
		///</summary>
		public Routing Routing
		{
			get => Q<Routing>("routing");
			set => Q("routing", value);
		}

		///<summary>Specify how long a consistent view of the index should be maintained for scrolled search</summary>
		public Time Scroll
		{
			get => Q<Time>("scroll");
			set => Q("scroll", value);
		}

		///<summary>Size on the scroll request powering the update by query</summary>
		public long? ScrollSize
		{
			get => Q<long? >("scroll_size");
			set => Q("scroll_size", value);
		}

		///<summary>Explicit timeout for each search request. Defaults to no timeout.</summary>
		public Time SearchTimeout
		{
			get => Q<Time>("search_timeout");
			set => Q("search_timeout", value);
		}

		///<summary>Search operation type</summary>
		public SearchType? SearchType
		{
			get => Q<SearchType? >("search_type");
			set => Q("search_type", value);
		}

		///<summary>The number of slices this task should be divided into. Defaults to 1, meaning the task isn't sliced into subtasks. Can be set to `auto`.</summary>
		public long? Slices
		{
			get => Q<long? >("slices");
			set => Q("slices", value);
		}

		///<summary>A comma-separated list of &lt;field&gt;:&lt;direction&gt; pairs</summary>
		public string[] Sort
		{
			get => Q<string[]>("sort");
			set => Q("sort", value);
		}

		///<summary>Whether the _source should be included in the response.</summary>
		public bool? SourceEnabled
		{
			get => Q<bool? >("_source");
			set => Q("_source", value);
		}

		///<summary>A list of fields to exclude from the returned _source field</summary>
		public Fields SourceExcludes
		{
			get => Q<Fields>("_source_excludes");
			set => Q("_source_excludes", value);
		}

		///<summary>A list of fields to extract and return from the _source field</summary>
		public Fields SourceIncludes
		{
			get => Q<Fields>("_source_includes");
			set => Q("_source_includes", value);
		}

		///<summary>Specific 'tag' of the request for logging and statistical purposes</summary>
		public string[] Stats
		{
			get => Q<string[]>("stats");
			set => Q("stats", value);
		}

		///<summary>The maximum number of documents to collect for each shard, upon reaching which the query execution will terminate early.</summary>
		public long? TerminateAfter
		{
			get => Q<long? >("terminate_after");
			set => Q("terminate_after", value);
		}

		///<summary>Time each individual bulk request should wait for shards that are unavailable.</summary>
		public Time Timeout
		{
			get => Q<Time>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Specify whether to return document version as part of a hit</summary>
		public bool? Version
		{
			get => Q<bool? >("version");
			set => Q("version", value);
		}

		///<summary>Should the document increment the version number (internal) on hit or not (reindex)</summary>
		public bool? VersionType
		{
			get => Q<bool? >("version_type");
			set => Q("version_type", value);
		}

		///<summary>
		/// Sets the number of shard copies that must be active before proceeding with the update by query operation. Defaults to 1, meaning the
		/// primary shard only. Set to `all` for all shard copies, otherwise set to any non-negative value less than or equal to the total number of
		/// copies for the shard (number of replicas + 1)
		///</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}

		///<summary>Should the request should block until the update by query operation is complete.</summary>
		public bool? WaitForCompletion
		{
			get => Q<bool? >("wait_for_completion");
			set => Q("wait_for_completion", value);
		}
	}

	public partial class UpdateByQueryRequest<TDocument> : UpdateByQueryRequest, IUpdateByQueryRequest<TDocument>
	{
		protected IUpdateByQueryRequest<TDocument> TypedSelf => this;
		///<summary>/{index}/_update_by_query</summary>
		///<param name = "index">this parameter is required</param>
		public UpdateByQueryRequest(Indices index): base(index)
		{
		}

		///<summary>/{index}/_update_by_query</summary>
		public UpdateByQueryRequest(): base(typeof(TDocument))
		{
		}
	}

	[InterfaceDataContract]
	public partial interface IUpdateByQueryRethrottleRequest : IRequest<UpdateByQueryRethrottleRequestParameters>
	{
		[IgnoreDataMember]
		TaskId TaskId
		{
			get;
		}
	}

	///<summary>Request for UpdateByQueryRethrottle <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/update-by-query/</para></summary>
	public partial class UpdateByQueryRethrottleRequest : PlainRequestBase<UpdateByQueryRethrottleRequestParameters>, IUpdateByQueryRethrottleRequest
	{
		protected IUpdateByQueryRethrottleRequest Self => this;
		internal override ApiUrls ApiUrls => ApiUrlsLookups.NoNamespaceUpdateByQueryRethrottle;
		///<summary>/_update_by_query/{task_id}/_rethrottle</summary>
		///<param name = "taskId">this parameter is required</param>
		public UpdateByQueryRethrottleRequest(TaskId taskId): base(r => r.Required("task_id", taskId))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected UpdateByQueryRethrottleRequest(): base()
		{
		}

		// values part of the url path
		[IgnoreDataMember]
		TaskId IUpdateByQueryRethrottleRequest.TaskId => Self.RouteValues.Get<TaskId>("task_id");
		// Request parameters
		///<summary>The throttle to set on this request in floating sub-requests per second. -1 means set no throttle.</summary>
		public long? RequestsPerSecond
		{
			get => Q<long? >("requests_per_second");
			set => Q("requests_per_second", value);
		}
	}
}
