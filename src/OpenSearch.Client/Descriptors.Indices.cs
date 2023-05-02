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
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Specification.IndicesApi;

// ReSharper disable RedundantBaseConstructorCall
// ReSharper disable UnusedTypeParameter
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable RedundantNameQualifier
namespace OpenSearch.Client.Specification.IndicesApi
{
	///<summary>Descriptor for AddBlock <para></para></summary>
	public partial class AddIndexBlockDescriptor : RequestDescriptorBase<AddIndexBlockDescriptor, AddIndexBlockRequestParameters, IAddIndexBlockRequest>, IAddIndexBlockRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesAddBlock;
		///<summary>/{index}/_block/{block}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "block">this parameter is required</param>
		public AddIndexBlockDescriptor(Indices index, IndexBlock block): base(r => r.Required("index", index).Required("block", block))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected AddIndexBlockDescriptor(): base()
		{
		}

		// values part of the url path
		Indices IAddIndexBlockRequest.Index => Self.RouteValues.Get<Indices>("index");
		IndexBlock IAddIndexBlockRequest.Block => Self.RouteValues.Get<IndexBlock>("block");
		///<summary>A comma separated list of indices to add a block to</summary>
		public AddIndexBlockDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public AddIndexBlockDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public AddIndexBlockDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public AddIndexBlockDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public AddIndexBlockDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public AddIndexBlockDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public AddIndexBlockDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public AddIndexBlockDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public AddIndexBlockDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
	}

	///<summary>Descriptor for Analyze <para></para></summary>
	public partial class AnalyzeDescriptor : RequestDescriptorBase<AnalyzeDescriptor, AnalyzeRequestParameters, IAnalyzeRequest>, IAnalyzeRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesAnalyze;
		///<summary>/_analyze</summary>
		public AnalyzeDescriptor(): base()
		{
		}

		///<summary>/{index}/_analyze</summary>
		///<param name = "index">Optional, accepts null</param>
		public AnalyzeDescriptor(IndexName index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		IndexName IAnalyzeRequest.Index => Self.RouteValues.Get<IndexName>("index");
		///<summary>The name of the index to scope the operation</summary>
		public AnalyzeDescriptor Index(IndexName index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public AnalyzeDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (IndexName)v));
	// Request parameters
	}

	///<summary>Descriptor for ClearCache <para></para></summary>
	public partial class ClearCacheDescriptor : RequestDescriptorBase<ClearCacheDescriptor, ClearCacheRequestParameters, IClearCacheRequest>, IClearCacheRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesClearCache;
		///<summary>/_cache/clear</summary>
		public ClearCacheDescriptor(): base()
		{
		}

		///<summary>/{index}/_cache/clear</summary>
		///<param name = "index">Optional, accepts null</param>
		public ClearCacheDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		Indices IClearCacheRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index name to limit the operation</summary>
		public ClearCacheDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public ClearCacheDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public ClearCacheDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public ClearCacheDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public ClearCacheDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Clear field data</summary>
		public ClearCacheDescriptor Fielddata(bool? fielddata = true) => Qs("fielddata", fielddata);
		///<summary>A comma-separated list of fields to clear when using the `fielddata` parameter (default: all)</summary>
		public ClearCacheDescriptor Fields(Fields fields) => Qs("fields", fields);
		///<summary>A comma-separated list of fields to clear when using the `fielddata` parameter (default: all)</summary>
		public ClearCacheDescriptor Fields<T>(params Expression<Func<T, object>>[] fields)
			where T : class => Qs("fields", fields?.Select(e => (Field)e));
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public ClearCacheDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Clear query caches</summary>
		public ClearCacheDescriptor Query(bool? query = true) => Qs("query", query);
		///<summary>Clear request cache</summary>
		public ClearCacheDescriptor Request(bool? request = true) => Qs("request", request);
	}

	///<summary>Descriptor for Clone <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/</para></summary>
	public partial class CloneIndexDescriptor : RequestDescriptorBase<CloneIndexDescriptor, CloneIndexRequestParameters, ICloneIndexRequest>, ICloneIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesClone;
		///<summary>/{index}/_clone/{target}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "target">this parameter is required</param>
		public CloneIndexDescriptor(IndexName index, IndexName target): base(r => r.Required("index", index).Required("target", target))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected CloneIndexDescriptor(): base()
		{
		}

		// values part of the url path
		IndexName ICloneIndexRequest.Index => Self.RouteValues.Get<IndexName>("index");
		IndexName ICloneIndexRequest.Target => Self.RouteValues.Get<IndexName>("target");
		///<summary>The name of the source index to clone</summary>
		public CloneIndexDescriptor Index(IndexName index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public CloneIndexDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (IndexName)v));
		// Request parameters
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public CloneIndexDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public CloneIndexDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public CloneIndexDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
		///<summary>Set the number of active shards to wait for on the cloned index before the operation returns.</summary>
		public CloneIndexDescriptor WaitForActiveShards(string waitforactiveshards) => Qs("wait_for_active_shards", waitforactiveshards);
	}

	///<summary>Descriptor for Close <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</para></summary>
	public partial class CloseIndexDescriptor : RequestDescriptorBase<CloseIndexDescriptor, CloseIndexRequestParameters, ICloseIndexRequest>, ICloseIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesClose;
		///<summary>/{index}/_close</summary>
		///<param name = "index">this parameter is required</param>
		public CloseIndexDescriptor(Indices index): base(r => r.Required("index", index))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected CloseIndexDescriptor(): base()
		{
		}

		// values part of the url path
		Indices ICloseIndexRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma separated list of indices to close</summary>
		public CloseIndexDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public CloseIndexDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public CloseIndexDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public CloseIndexDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public CloseIndexDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public CloseIndexDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public CloseIndexDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public CloseIndexDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public CloseIndexDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
		///<summary>Sets the number of active shards to wait for before the operation returns.</summary>
		public CloseIndexDescriptor WaitForActiveShards(string waitforactiveshards) => Qs("wait_for_active_shards", waitforactiveshards);
	}

	///<summary>Descriptor for Create <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/</para></summary>
	public partial class CreateIndexDescriptor : RequestDescriptorBase<CreateIndexDescriptor, CreateIndexRequestParameters, ICreateIndexRequest>, ICreateIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesCreate;
		///<summary>/{index}</summary>
		///<param name = "index">this parameter is required</param>
		public CreateIndexDescriptor(IndexName index): base(r => r.Required("index", index))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected CreateIndexDescriptor(): base()
		{
		}

		// values part of the url path
		IndexName ICreateIndexRequest.Index => Self.RouteValues.Get<IndexName>("index");
		///<summary>The name of the index</summary>
		public CreateIndexDescriptor Index(IndexName index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public CreateIndexDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (IndexName)v));
		// Request parameters
		///<summary>Whether a type should be expected in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public CreateIndexDescriptor IncludeTypeName(bool? includetypename = true) => Qs("include_type_name", includetypename);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public CreateIndexDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public CreateIndexDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public CreateIndexDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
		///<summary>Set the number of active shards to wait for before the operation returns.</summary>
		public CreateIndexDescriptor WaitForActiveShards(string waitforactiveshards) => Qs("wait_for_active_shards", waitforactiveshards);
	}

	///<summary>Descriptor for Delete <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/</para></summary>
	public partial class DeleteIndexDescriptor : RequestDescriptorBase<DeleteIndexDescriptor, DeleteIndexRequestParameters, IDeleteIndexRequest>, IDeleteIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesDelete;
		///<summary>/{index}</summary>
		///<param name = "index">this parameter is required</param>
		public DeleteIndexDescriptor(Indices index): base(r => r.Required("index", index))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DeleteIndexDescriptor(): base()
		{
		}

		// values part of the url path
		Indices IDeleteIndexRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of indices to delete; use `_all` or `*` string to delete all indices</summary>
		public DeleteIndexDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public DeleteIndexDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public DeleteIndexDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Ignore if a wildcard expression resolves to no concrete indices (default: false)</summary>
		public DeleteIndexDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether wildcard expressions should get expanded to open or closed indices (default: open)</summary>
		public DeleteIndexDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Ignore unavailable indexes (default: false)</summary>
		public DeleteIndexDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public DeleteIndexDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public DeleteIndexDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public DeleteIndexDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
	}

	///<summary>Descriptor for DeleteAlias <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public partial class DeleteAliasDescriptor : RequestDescriptorBase<DeleteAliasDescriptor, DeleteAliasRequestParameters, IDeleteAliasRequest>, IDeleteAliasRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesDeleteAlias;
		///<summary>/{index}/_alias/{name}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "name">this parameter is required</param>
		public DeleteAliasDescriptor(Indices index, Names name): base(r => r.Required("index", index).Required("name", name))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DeleteAliasDescriptor(): base()
		{
		}

		// values part of the url path
		Indices IDeleteAliasRequest.Index => Self.RouteValues.Get<Indices>("index");
		Names IDeleteAliasRequest.Name => Self.RouteValues.Get<Names>("name");
		///<summary>A comma-separated list of index names (supports wildcards); use `_all` for all indices</summary>
		public DeleteAliasDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public DeleteAliasDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public DeleteAliasDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public DeleteAliasDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public DeleteAliasDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit timestamp for the document</summary>
		public DeleteAliasDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
	}

	///<summary>Descriptor for DeleteTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public partial class DeleteIndexTemplateDescriptor : RequestDescriptorBase<DeleteIndexTemplateDescriptor, DeleteIndexTemplateRequestParameters, IDeleteIndexTemplateRequest>, IDeleteIndexTemplateRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesDeleteTemplate;
		///<summary>/_template/{name}</summary>
		///<param name = "name">this parameter is required</param>
		public DeleteIndexTemplateDescriptor(Name name): base(r => r.Required("name", name))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected DeleteIndexTemplateDescriptor(): base()
		{
		}

		// values part of the url path
		Name IDeleteIndexTemplateRequest.Name => Self.RouteValues.Get<Name>("name");
		// Request parameters
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public DeleteIndexTemplateDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public DeleteIndexTemplateDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public DeleteIndexTemplateDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
	}

	///<summary>Descriptor for Exists <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</para></summary>
	public partial class IndexExistsDescriptor : RequestDescriptorBase<IndexExistsDescriptor, IndexExistsRequestParameters, IIndexExistsRequest>, IIndexExistsRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesExists;
		///<summary>/{index}</summary>
		///<param name = "index">this parameter is required</param>
		public IndexExistsDescriptor(Indices index): base(r => r.Required("index", index))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected IndexExistsDescriptor(): base()
		{
		}

		// values part of the url path
		Indices IIndexExistsRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names</summary>
		public IndexExistsDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public IndexExistsDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public IndexExistsDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Ignore if a wildcard expression resolves to no concrete indices (default: false)</summary>
		public IndexExistsDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether wildcard expressions should get expanded to open or closed indices (default: open)</summary>
		public IndexExistsDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Return settings in flat format (default: false)</summary>
		public IndexExistsDescriptor FlatSettings(bool? flatsettings = true) => Qs("flat_settings", flatsettings);
		///<summary>Ignore unavailable indexes (default: false)</summary>
		public IndexExistsDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Whether to return all default setting for each of the indices.</summary>
		public IndexExistsDescriptor IncludeDefaults(bool? includedefaults = true) => Qs("include_defaults", includedefaults);
		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public IndexExistsDescriptor Local(bool? local = true) => Qs("local", local);
	}

	///<summary>Descriptor for AliasExists <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public partial class AliasExistsDescriptor : RequestDescriptorBase<AliasExistsDescriptor, AliasExistsRequestParameters, IAliasExistsRequest>, IAliasExistsRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesAliasExists;
		///<summary>/_alias/{name}</summary>
		///<param name = "name">this parameter is required</param>
		public AliasExistsDescriptor(Names name): base(r => r.Required("name", name))
		{
		}

		///<summary>/{index}/_alias/{name}</summary>
		///<param name = "index">Optional, accepts null</param>
		///<param name = "name">this parameter is required</param>
		public AliasExistsDescriptor(Indices index, Names name): base(r => r.Optional("index", index).Required("name", name))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected AliasExistsDescriptor(): base()
		{
		}

		// values part of the url path
		Names IAliasExistsRequest.Name => Self.RouteValues.Get<Names>("name");
		Indices IAliasExistsRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names to filter aliases</summary>
		public AliasExistsDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public AliasExistsDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public AliasExistsDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public AliasExistsDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public AliasExistsDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public AliasExistsDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public AliasExistsDescriptor Local(bool? local = true) => Qs("local", local);
	}

	///<summary>Descriptor for TemplateExists <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public partial class IndexTemplateExistsDescriptor : RequestDescriptorBase<IndexTemplateExistsDescriptor, IndexTemplateExistsRequestParameters, IIndexTemplateExistsRequest>, IIndexTemplateExistsRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesTemplateExists;
		///<summary>/_template/{name}</summary>
		///<param name = "name">this parameter is required</param>
		public IndexTemplateExistsDescriptor(Names name): base(r => r.Required("name", name))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected IndexTemplateExistsDescriptor(): base()
		{
		}

		// values part of the url path
		Names IIndexTemplateExistsRequest.Name => Self.RouteValues.Get<Names>("name");
		// Request parameters
		///<summary>Return settings in flat format (default: false)</summary>
		public IndexTemplateExistsDescriptor FlatSettings(bool? flatsettings = true) => Qs("flat_settings", flatsettings);
		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public IndexTemplateExistsDescriptor Local(bool? local = true) => Qs("local", local);
		///<summary>Specify timeout for connection to master node</summary>
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public IndexTemplateExistsDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public IndexTemplateExistsDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
	}

	///<summary>Descriptor for TypeExists <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</para></summary>
	///<remarks>Deprecated as of OpenSearch 2.0</remarks>
	public partial class TypeExistsDescriptor : RequestDescriptorBase<TypeExistsDescriptor, TypeExistsRequestParameters, ITypeExistsRequest>, ITypeExistsRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesTypeExists;
		///<summary>/{index}/_mapping/{type}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "type">this parameter is required</param>
		public TypeExistsDescriptor(Indices index, Names type): base(r => r.Required("index", index).Required("type", type))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected TypeExistsDescriptor(): base()
		{
		}

		// values part of the url path
		Indices ITypeExistsRequest.Index => Self.RouteValues.Get<Indices>("index");
		Names ITypeExistsRequest.Type => Self.RouteValues.Get<Names>("type");
		///<summary>A comma-separated list of index names; use `_all` to check the types across all indices</summary>
		public TypeExistsDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public TypeExistsDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public TypeExistsDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public TypeExistsDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public TypeExistsDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public TypeExistsDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public TypeExistsDescriptor Local(bool? local = true) => Qs("local", local);
	}

	///<summary>Descriptor for Flush <para></para></summary>
	public partial class FlushDescriptor : RequestDescriptorBase<FlushDescriptor, FlushRequestParameters, IFlushRequest>, IFlushRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesFlush;
		///<summary>/_flush</summary>
		public FlushDescriptor(): base()
		{
		}

		///<summary>/{index}/_flush</summary>
		///<param name = "index">Optional, accepts null</param>
		public FlushDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		Indices IFlushRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names; use the special string `_all` or Indices.All for all indices</summary>
		public FlushDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public FlushDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public FlushDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public FlushDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public FlushDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether a flush should be forced even if it is not necessarily needed ie. if no changes will be committed to the index. This is useful if transaction log IDs should be incremented even if no uncommitted changes are present. (This setting can be considered as internal)</summary>
		public FlushDescriptor Force(bool? force = true) => Qs("force", force);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public FlushDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>If set to true the flush operation will block until the flush can be executed if another flush operation is already executing. The default is true. If set to false the flush will be skipped iff if another flush operation is already running.</summary>
		public FlushDescriptor WaitIfOngoing(bool? waitifongoing = true) => Qs("wait_if_ongoing", waitifongoing);
	}

	///<summary>Descriptor for ForceMerge <para></para></summary>
	public partial class ForceMergeDescriptor : RequestDescriptorBase<ForceMergeDescriptor, ForceMergeRequestParameters, IForceMergeRequest>, IForceMergeRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesForceMerge;
		///<summary>/_forcemerge</summary>
		public ForceMergeDescriptor(): base()
		{
		}

		///<summary>/{index}/_forcemerge</summary>
		///<param name = "index">Optional, accepts null</param>
		public ForceMergeDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		Indices IForceMergeRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices</summary>
		public ForceMergeDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public ForceMergeDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public ForceMergeDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public ForceMergeDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public ForceMergeDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Specify whether the index should be flushed after performing the operation (default: true)</summary>
		public ForceMergeDescriptor Flush(bool? flush = true) => Qs("flush", flush);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public ForceMergeDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>The number of segments the index should be merged into (default: dynamic)</summary>
		public ForceMergeDescriptor MaxNumSegments(long? maxnumsegments) => Qs("max_num_segments", maxnumsegments);
		///<summary>Specify whether the operation should only expunge deleted documents</summary>
		public ForceMergeDescriptor OnlyExpungeDeletes(bool? onlyexpungedeletes = true) => Qs("only_expunge_deletes", onlyexpungedeletes);
	}

	///<summary>Descriptor for Get <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/</para></summary>
	public partial class GetIndexDescriptor : RequestDescriptorBase<GetIndexDescriptor, GetIndexRequestParameters, IGetIndexRequest>, IGetIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesGet;
		///<summary>/{index}</summary>
		///<param name = "index">this parameter is required</param>
		public GetIndexDescriptor(Indices index): base(r => r.Required("index", index))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected GetIndexDescriptor(): base()
		{
		}

		// values part of the url path
		Indices IGetIndexRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names</summary>
		public GetIndexDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public GetIndexDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public GetIndexDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Ignore if a wildcard expression resolves to no concrete indices (default: false)</summary>
		public GetIndexDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether wildcard expressions should get expanded to open or closed indices (default: open)</summary>
		public GetIndexDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Return settings in flat format (default: false)</summary>
		public GetIndexDescriptor FlatSettings(bool? flatsettings = true) => Qs("flat_settings", flatsettings);
		///<summary>Ignore unavailable indexes (default: false)</summary>
		public GetIndexDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Whether to return all default setting for each of the indices.</summary>
		public GetIndexDescriptor IncludeDefaults(bool? includedefaults = true) => Qs("include_defaults", includedefaults);
		///<summary>Whether to add the type name to the response (default: false)</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public GetIndexDescriptor IncludeTypeName(bool? includetypename = true) => Qs("include_type_name", includetypename);
		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public GetIndexDescriptor Local(bool? local = true) => Qs("local", local);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public GetIndexDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public GetIndexDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
	}

	///<summary>Descriptor for GetAlias <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public partial class GetAliasDescriptor : RequestDescriptorBase<GetAliasDescriptor, GetAliasRequestParameters, IGetAliasRequest>, IGetAliasRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesGetAlias;
		///<summary>/_alias</summary>
		public GetAliasDescriptor(): base()
		{
		}

		///<summary>/_alias/{name}</summary>
		///<param name = "name">Optional, accepts null</param>
		public GetAliasDescriptor(Names name): base(r => r.Optional("name", name))
		{
		}

		///<summary>/{index}/_alias/{name}</summary>
		///<param name = "index">Optional, accepts null</param>
		///<param name = "name">Optional, accepts null</param>
		public GetAliasDescriptor(Indices index, Names name): base(r => r.Optional("index", index).Optional("name", name))
		{
		}

		///<summary>/{index}/_alias</summary>
		///<param name = "index">Optional, accepts null</param>
		public GetAliasDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		Names IGetAliasRequest.Name => Self.RouteValues.Get<Names>("name");
		Indices IGetAliasRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of alias names to return</summary>
		public GetAliasDescriptor Name(Names name) => Assign(name, (a, v) => a.RouteValues.Optional("name", v));
		///<summary>A comma-separated list of index names to filter aliases</summary>
		public GetAliasDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public GetAliasDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public GetAliasDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public GetAliasDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public GetAliasDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public GetAliasDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public GetAliasDescriptor Local(bool? local = true) => Qs("local", local);
	}

	///<summary>Descriptor for GetFieldMapping <para>https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</para></summary>
	public partial class GetFieldMappingDescriptor<TDocument> : RequestDescriptorBase<GetFieldMappingDescriptor<TDocument>, GetFieldMappingRequestParameters, IGetFieldMappingRequest>, IGetFieldMappingRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesGetFieldMapping;
		///<summary>/{index}/_mapping/field/{fields}</summary>
		///<param name = "fields">this parameter is required</param>
		public GetFieldMappingDescriptor(Fields fields): this(typeof(TDocument), fields)
		{
		}

		///<summary>/{index}/_mapping/field/{fields}</summary>
		///<param name = "index">Optional, accepts null</param>
		///<param name = "fields">this parameter is required</param>
		public GetFieldMappingDescriptor(Indices index, Fields fields): base(r => r.Optional("index", index).Required("fields", fields))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected GetFieldMappingDescriptor(): base()
		{
		}

		// values part of the url path
		Fields IGetFieldMappingRequest.Fields => Self.RouteValues.Get<Fields>("fields");
		Indices IGetFieldMappingRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names</summary>
		public GetFieldMappingDescriptor<TDocument> Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public GetFieldMappingDescriptor<TDocument> Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public GetFieldMappingDescriptor<TDocument> AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public GetFieldMappingDescriptor<TDocument> AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public GetFieldMappingDescriptor<TDocument> ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public GetFieldMappingDescriptor<TDocument> IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Whether the default mapping values should be returned as well</summary>
		public GetFieldMappingDescriptor<TDocument> IncludeDefaults(bool? includedefaults = true) => Qs("include_defaults", includedefaults);
		///<summary>Whether a type should be returned in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public GetFieldMappingDescriptor<TDocument> IncludeTypeName(bool? includetypename = true) => Qs("include_type_name", includetypename);
		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public GetFieldMappingDescriptor<TDocument> Local(bool? local = true) => Qs("local", local);
	}

	///<summary>Descriptor for GetMapping <para>https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</para></summary>
	public partial class GetMappingDescriptor<TDocument> : RequestDescriptorBase<GetMappingDescriptor<TDocument>, GetMappingRequestParameters, IGetMappingRequest>, IGetMappingRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesGetMapping;
		///<summary>/{index}/_mapping</summary>
		public GetMappingDescriptor(): this(typeof(TDocument))
		{
		}

		///<summary>/{index}/_mapping</summary>
		///<param name = "index">Optional, accepts null</param>
		public GetMappingDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		Indices IGetMappingRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names</summary>
		public GetMappingDescriptor<TDocument> Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public GetMappingDescriptor<TDocument> Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public GetMappingDescriptor<TDocument> AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public GetMappingDescriptor<TDocument> AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public GetMappingDescriptor<TDocument> ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public GetMappingDescriptor<TDocument> IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Whether to add the type name to the response (default: false)</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public GetMappingDescriptor<TDocument> IncludeTypeName(bool? includetypename = true) => Qs("include_type_name", includetypename);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public GetMappingDescriptor<TDocument> MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public GetMappingDescriptor<TDocument> ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
	}

	///<summary>Descriptor for GetSettings <para></para></summary>
	public partial class GetIndexSettingsDescriptor : RequestDescriptorBase<GetIndexSettingsDescriptor, GetIndexSettingsRequestParameters, IGetIndexSettingsRequest>, IGetIndexSettingsRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesGetSettings;
		///<summary>/_settings</summary>
		public GetIndexSettingsDescriptor(): base()
		{
		}

		///<summary>/{index}/_settings</summary>
		///<param name = "index">Optional, accepts null</param>
		public GetIndexSettingsDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		///<summary>/{index}/_settings/{name}</summary>
		///<param name = "index">Optional, accepts null</param>
		///<param name = "name">Optional, accepts null</param>
		public GetIndexSettingsDescriptor(Indices index, Names name): base(r => r.Optional("index", index).Optional("name", name))
		{
		}

		///<summary>/_settings/{name}</summary>
		///<param name = "name">Optional, accepts null</param>
		public GetIndexSettingsDescriptor(Names name): base(r => r.Optional("name", name))
		{
		}

		// values part of the url path
		Indices IGetIndexSettingsRequest.Index => Self.RouteValues.Get<Indices>("index");
		Names IGetIndexSettingsRequest.Name => Self.RouteValues.Get<Names>("name");
		///<summary>A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices</summary>
		public GetIndexSettingsDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public GetIndexSettingsDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public GetIndexSettingsDescriptor AllIndices() => Index(Indices.All);
		///<summary>The name of the settings that should be included</summary>
		public GetIndexSettingsDescriptor Name(Names name) => Assign(name, (a, v) => a.RouteValues.Optional("name", v));
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public GetIndexSettingsDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public GetIndexSettingsDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Return settings in flat format (default: false)</summary>
		public GetIndexSettingsDescriptor FlatSettings(bool? flatsettings = true) => Qs("flat_settings", flatsettings);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public GetIndexSettingsDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Whether to return all default setting for each of the indices.</summary>
		public GetIndexSettingsDescriptor IncludeDefaults(bool? includedefaults = true) => Qs("include_defaults", includedefaults);
		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public GetIndexSettingsDescriptor Local(bool? local = true) => Qs("local", local);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public GetIndexSettingsDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public GetIndexSettingsDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
	}

	///<summary>Descriptor for GetTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public partial class GetIndexTemplateDescriptor : RequestDescriptorBase<GetIndexTemplateDescriptor, GetIndexTemplateRequestParameters, IGetIndexTemplateRequest>, IGetIndexTemplateRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesGetTemplate;
		///<summary>/_template</summary>
		public GetIndexTemplateDescriptor(): base()
		{
		}

		///<summary>/_template/{name}</summary>
		///<param name = "name">Optional, accepts null</param>
		public GetIndexTemplateDescriptor(Names name): base(r => r.Optional("name", name))
		{
		}

		// values part of the url path
		Names IGetIndexTemplateRequest.Name => Self.RouteValues.Get<Names>("name");
		///<summary>The comma separated names of the index templates</summary>
		public GetIndexTemplateDescriptor Name(Names name) => Assign(name, (a, v) => a.RouteValues.Optional("name", v));
		// Request parameters
		///<summary>Return settings in flat format (default: false)</summary>
		public GetIndexTemplateDescriptor FlatSettings(bool? flatsettings = true) => Qs("flat_settings", flatsettings);
		///<summary>Whether a type should be returned in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public GetIndexTemplateDescriptor IncludeTypeName(bool? includetypename = true) => Qs("include_type_name", includetypename);
		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public GetIndexTemplateDescriptor Local(bool? local = true) => Qs("local", local);
		///<summary>Specify timeout for connection to master node</summary>
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public GetIndexTemplateDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public GetIndexTemplateDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
	}

	///<summary>Descriptor for Open <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</para></summary>
	public partial class OpenIndexDescriptor : RequestDescriptorBase<OpenIndexDescriptor, OpenIndexRequestParameters, IOpenIndexRequest>, IOpenIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesOpen;
		///<summary>/{index}/_open</summary>
		///<param name = "index">this parameter is required</param>
		public OpenIndexDescriptor(Indices index): base(r => r.Required("index", index))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected OpenIndexDescriptor(): base()
		{
		}

		// values part of the url path
		Indices IOpenIndexRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma separated list of indices to open</summary>
		public OpenIndexDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public OpenIndexDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public OpenIndexDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public OpenIndexDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public OpenIndexDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public OpenIndexDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public OpenIndexDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public OpenIndexDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public OpenIndexDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
		///<summary>Sets the number of active shards to wait for before the operation returns.</summary>
		public OpenIndexDescriptor WaitForActiveShards(string waitforactiveshards) => Qs("wait_for_active_shards", waitforactiveshards);
	}

	///<summary>Descriptor for PutAlias <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public partial class PutAliasDescriptor : RequestDescriptorBase<PutAliasDescriptor, PutAliasRequestParameters, IPutAliasRequest>, IPutAliasRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesPutAlias;
		///<summary>/{index}/_alias/{name}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "name">this parameter is required</param>
		public PutAliasDescriptor(Indices index, Name name): base(r => r.Required("index", index).Required("name", name))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected PutAliasDescriptor(): base()
		{
		}

		// values part of the url path
		Indices IPutAliasRequest.Index => Self.RouteValues.Get<Indices>("index");
		Name IPutAliasRequest.Name => Self.RouteValues.Get<Name>("name");
		///<summary>A comma-separated list of index names the alias should point to (supports wildcards); use `_all` to perform the operation on all indices.</summary>
		public PutAliasDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public PutAliasDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public PutAliasDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public PutAliasDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public PutAliasDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit timestamp for the document</summary>
		public PutAliasDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
	}

	///<summary>Descriptor for PutMapping <para>https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</para></summary>
	public partial class PutMappingDescriptor<TDocument> : RequestDescriptorBase<PutMappingDescriptor<TDocument>, PutMappingRequestParameters, IPutMappingRequest<TDocument>>, IPutMappingRequest<TDocument>
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesPutMapping;
		///<summary>/{index}/_mapping</summary>
		///<param name = "index">this parameter is required</param>
		public PutMappingDescriptor(Indices index): base(r => r.Required("index", index))
		{
		}

		///<summary>/{index}/_mapping</summary>
		public PutMappingDescriptor(): this(typeof(TDocument))
		{
		}

		// values part of the url path
		Indices IPutMappingRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names the mapping should be added to (supports wildcards); use `_all` or omit to add the mapping on all indices.</summary>
		public PutMappingDescriptor<TDocument> Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public PutMappingDescriptor<TDocument> Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public PutMappingDescriptor<TDocument> AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public PutMappingDescriptor<TDocument> AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public PutMappingDescriptor<TDocument> ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public PutMappingDescriptor<TDocument> IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Whether a type should be expected in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public PutMappingDescriptor<TDocument> IncludeTypeName(bool? includetypename = true) => Qs("include_type_name", includetypename);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public PutMappingDescriptor<TDocument> MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public PutMappingDescriptor<TDocument> ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public PutMappingDescriptor<TDocument> Timeout(Time timeout) => Qs("timeout", timeout);
		///<summary>When true, applies mappings only to the write index of an alias</summary>
		public PutMappingDescriptor<TDocument> WriteIndexOnly(bool? writeindexonly = true) => Qs("write_index_only", writeindexonly);
	}

	///<summary>Descriptor for UpdateSettings <para></para></summary>
	public partial class UpdateIndexSettingsDescriptor : RequestDescriptorBase<UpdateIndexSettingsDescriptor, UpdateIndexSettingsRequestParameters, IUpdateIndexSettingsRequest>, IUpdateIndexSettingsRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesUpdateSettings;
		///<summary>/_settings</summary>
		public UpdateIndexSettingsDescriptor(): base()
		{
		}

		///<summary>/{index}/_settings</summary>
		///<param name = "index">Optional, accepts null</param>
		public UpdateIndexSettingsDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		Indices IUpdateIndexSettingsRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices</summary>
		public UpdateIndexSettingsDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public UpdateIndexSettingsDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public UpdateIndexSettingsDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public UpdateIndexSettingsDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public UpdateIndexSettingsDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Return settings in flat format (default: false)</summary>
		public UpdateIndexSettingsDescriptor FlatSettings(bool? flatsettings = true) => Qs("flat_settings", flatsettings);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public UpdateIndexSettingsDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public UpdateIndexSettingsDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public UpdateIndexSettingsDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Whether to update existing settings. If set to `true` existing settings on an index remain unchanged, the default is `false`</summary>
		public UpdateIndexSettingsDescriptor PreserveExisting(bool? preserveexisting = true) => Qs("preserve_existing", preserveexisting);
		///<summary>Explicit operation timeout</summary>
		public UpdateIndexSettingsDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
	}

	///<summary>Descriptor for PutTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public partial class PutIndexTemplateDescriptor : RequestDescriptorBase<PutIndexTemplateDescriptor, PutIndexTemplateRequestParameters, IPutIndexTemplateRequest>, IPutIndexTemplateRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesPutTemplate;
		///<summary>/_template/{name}</summary>
		///<param name = "name">this parameter is required</param>
		public PutIndexTemplateDescriptor(Name name): base(r => r.Required("name", name))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected PutIndexTemplateDescriptor(): base()
		{
		}

		// values part of the url path
		Name IPutIndexTemplateRequest.Name => Self.RouteValues.Get<Name>("name");
		// Request parameters
		///<summary>Whether the index template should only be added if new or can also replace an existing one</summary>
		public PutIndexTemplateDescriptor Create(bool? create = true) => Qs("create", create);
		///<summary>Whether a type should be returned in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public PutIndexTemplateDescriptor IncludeTypeName(bool? includetypename = true) => Qs("include_type_name", includetypename);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public PutIndexTemplateDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public PutIndexTemplateDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
	}

	///<summary>Descriptor for Refresh <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
	public partial class RefreshDescriptor : RequestDescriptorBase<RefreshDescriptor, RefreshRequestParameters, IRefreshRequest>, IRefreshRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesRefresh;
		///<summary>/_refresh</summary>
		public RefreshDescriptor(): base()
		{
		}

		///<summary>/{index}/_refresh</summary>
		///<param name = "index">Optional, accepts null</param>
		public RefreshDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		Indices IRefreshRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices</summary>
		public RefreshDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public RefreshDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public RefreshDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public RefreshDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public RefreshDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public RefreshDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
	}

	///<summary>Descriptor for Resolve <para></para></summary>
	public partial class ResolveIndexDescriptor : RequestDescriptorBase<ResolveIndexDescriptor, ResolveIndexRequestParameters, IResolveIndexRequest>, IResolveIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesResolve;
		///<summary>/_resolve/index/{name}</summary>
		///<param name = "name">this parameter is required</param>
		public ResolveIndexDescriptor(Names name): base(r => r.Required("name", name))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected ResolveIndexDescriptor(): base()
		{
		}

		// values part of the url path
		Names IResolveIndexRequest.Name => Self.RouteValues.Get<Names>("name");
		// Request parameters
		///<summary>Whether wildcard expressions should get expanded to open or closed indices (default: open)</summary>
		public ResolveIndexDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
	}

	///<summary>Descriptor for Rollover <para>https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream</para></summary>
	public partial class RolloverIndexDescriptor : RequestDescriptorBase<RolloverIndexDescriptor, RolloverIndexRequestParameters, IRolloverIndexRequest>, IRolloverIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesRollover;
		///<summary>/{alias}/_rollover</summary>
		///<param name = "alias">this parameter is required</param>
		public RolloverIndexDescriptor(Name alias): base(r => r.Required("alias", alias))
		{
		}

		///<summary>/{alias}/_rollover/{new_index}</summary>
		///<param name = "alias">this parameter is required</param>
		///<param name = "newIndex">Optional, accepts null</param>
		public RolloverIndexDescriptor(Name alias, IndexName newIndex): base(r => r.Required("alias", alias).Optional("new_index", newIndex))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected RolloverIndexDescriptor(): base()
		{
		}

		// values part of the url path
		Name IRolloverIndexRequest.Alias => Self.RouteValues.Get<Name>("alias");
		IndexName IRolloverIndexRequest.NewIndex => Self.RouteValues.Get<IndexName>("new_index");
		///<summary>The name of the rollover index</summary>
		public RolloverIndexDescriptor NewIndex(IndexName newIndex) => Assign(newIndex, (a, v) => a.RouteValues.Optional("new_index", v));
		// Request parameters
		///<summary>If set to true the rollover action will only be validated but not actually performed even if a condition matches. The default is false</summary>
		public RolloverIndexDescriptor DryRun(bool? dryrun = true) => Qs("dry_run", dryrun);
		///<summary>Whether a type should be included in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public RolloverIndexDescriptor IncludeTypeName(bool? includetypename = true) => Qs("include_type_name", includetypename);
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public RolloverIndexDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public RolloverIndexDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public RolloverIndexDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
		///<summary>Set the number of active shards to wait for on the newly created rollover index before the operation returns.</summary>
		public RolloverIndexDescriptor WaitForActiveShards(string waitforactiveshards) => Qs("wait_for_active_shards", waitforactiveshards);
	}

	///<summary>Descriptor for ShardStores <para></para></summary>
	public partial class IndicesShardStoresDescriptor : RequestDescriptorBase<IndicesShardStoresDescriptor, IndicesShardStoresRequestParameters, IIndicesShardStoresRequest>, IIndicesShardStoresRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesShardStores;
		///<summary>/_shard_stores</summary>
		public IndicesShardStoresDescriptor(): base()
		{
		}

		///<summary>/{index}/_shard_stores</summary>
		///<param name = "index">Optional, accepts null</param>
		public IndicesShardStoresDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		Indices IIndicesShardStoresRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names; use the special string `_all` or Indices.All to perform the operation on all indices</summary>
		public IndicesShardStoresDescriptor Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public IndicesShardStoresDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public IndicesShardStoresDescriptor AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public IndicesShardStoresDescriptor AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public IndicesShardStoresDescriptor ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public IndicesShardStoresDescriptor IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>A comma-separated list of statuses used to filter on shards to get store information for</summary>
		public IndicesShardStoresDescriptor Status(params string[] status) => Qs("status", status);
	}

	///<summary>Descriptor for Shrink <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/shrink-index/</para></summary>
	public partial class ShrinkIndexDescriptor : RequestDescriptorBase<ShrinkIndexDescriptor, ShrinkIndexRequestParameters, IShrinkIndexRequest>, IShrinkIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesShrink;
		///<summary>/{index}/_shrink/{target}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "target">this parameter is required</param>
		public ShrinkIndexDescriptor(IndexName index, IndexName target): base(r => r.Required("index", index).Required("target", target))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected ShrinkIndexDescriptor(): base()
		{
		}

		// values part of the url path
		IndexName IShrinkIndexRequest.Index => Self.RouteValues.Get<IndexName>("index");
		IndexName IShrinkIndexRequest.Target => Self.RouteValues.Get<IndexName>("target");
		///<summary>The name of the source index to shrink</summary>
		public ShrinkIndexDescriptor Index(IndexName index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public ShrinkIndexDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (IndexName)v));
		// Request parameters
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public ShrinkIndexDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public ShrinkIndexDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public ShrinkIndexDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
		///<summary>Set the number of active shards to wait for on the shrunken index before the operation returns.</summary>
		public ShrinkIndexDescriptor WaitForActiveShards(string waitforactiveshards) => Qs("wait_for_active_shards", waitforactiveshards);
	}

	///<summary>Descriptor for Split <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/</para></summary>
	public partial class SplitIndexDescriptor : RequestDescriptorBase<SplitIndexDescriptor, SplitIndexRequestParameters, ISplitIndexRequest>, ISplitIndexRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesSplit;
		///<summary>/{index}/_split/{target}</summary>
		///<param name = "index">this parameter is required</param>
		///<param name = "target">this parameter is required</param>
		public SplitIndexDescriptor(IndexName index, IndexName target): base(r => r.Required("index", index).Required("target", target))
		{
		}

		///<summary>Used for serialization purposes, making sure we have a parameterless constructor</summary>
		[SerializationConstructor]
		protected SplitIndexDescriptor(): base()
		{
		}

		// values part of the url path
		IndexName ISplitIndexRequest.Index => Self.RouteValues.Get<IndexName>("index");
		IndexName ISplitIndexRequest.Target => Self.RouteValues.Get<IndexName>("target");
		///<summary>The name of the source index to split</summary>
		public SplitIndexDescriptor Index(IndexName index) => Assign(index, (a, v) => a.RouteValues.Required("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public SplitIndexDescriptor Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Required("index", (IndexName)v));
		// Request parameters
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public SplitIndexDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public SplitIndexDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Explicit operation timeout</summary>
		public SplitIndexDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
		///<summary>Set the number of active shards to wait for on the shrunken index before the operation returns.</summary>
		public SplitIndexDescriptor WaitForActiveShards(string waitforactiveshards) => Qs("wait_for_active_shards", waitforactiveshards);
	}

	///<summary>Descriptor for BulkAlias <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public partial class BulkAliasDescriptor : RequestDescriptorBase<BulkAliasDescriptor, BulkAliasRequestParameters, IBulkAliasRequest>, IBulkAliasRequest
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesBulkAlias;
		// values part of the url path
		// Request parameters
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeout"/> instead</remarks>
		public BulkAliasDescriptor MasterTimeout(Time mastertimeout) => Qs("master_timeout", mastertimeout);
		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeout"/></remarks>
		public BulkAliasDescriptor ClusterManagerTimeout(Time timeout) => Qs("cluster_manager_timeout", timeout);
		///<summary>Request timeout</summary>
		public BulkAliasDescriptor Timeout(Time timeout) => Qs("timeout", timeout);
	}

	///<summary>Descriptor for ValidateQuery <para></para></summary>
	public partial class ValidateQueryDescriptor<TDocument> : RequestDescriptorBase<ValidateQueryDescriptor<TDocument>, ValidateQueryRequestParameters, IValidateQueryRequest<TDocument>>, IValidateQueryRequest<TDocument>
	{
		internal override ApiUrls ApiUrls => ApiUrlsLookups.IndicesValidateQuery;
		///<summary>/{index}/_validate/query</summary>
		public ValidateQueryDescriptor(): this(typeof(TDocument))
		{
		}

		///<summary>/{index}/_validate/query</summary>
		///<param name = "index">Optional, accepts null</param>
		public ValidateQueryDescriptor(Indices index): base(r => r.Optional("index", index))
		{
		}

		// values part of the url path
		Indices IValidateQueryRequest.Index => Self.RouteValues.Get<Indices>("index");
		///<summary>A comma-separated list of index names to restrict the operation; use the special string `_all` or Indices.All to perform the operation on all indices</summary>
		public ValidateQueryDescriptor<TDocument> Index(Indices index) => Assign(index, (a, v) => a.RouteValues.Optional("index", v));
		///<summary>a shortcut into calling Index(typeof(TOther))</summary>
		public ValidateQueryDescriptor<TDocument> Index<TOther>()
			where TOther : class => Assign(typeof(TOther), (a, v) => a.RouteValues.Optional("index", (Indices)v));
		///<summary>A shortcut into calling Index(Indices.All)</summary>
		public ValidateQueryDescriptor<TDocument> AllIndices() => Index(Indices.All);
		// Request parameters
		///<summary>Execute validation on all shards instead of one random shard per index</summary>
		public ValidateQueryDescriptor<TDocument> AllShards(bool? allshards = true) => Qs("all_shards", allshards);
		///<summary>Whether to ignore if a wildcard indices expression resolves into no concrete indices. (This includes `_all` string or when no indices have been specified)</summary>
		public ValidateQueryDescriptor<TDocument> AllowNoIndices(bool? allownoindices = true) => Qs("allow_no_indices", allownoindices);
		///<summary>Specify whether wildcard and prefix queries should be analyzed (default: false)</summary>
		public ValidateQueryDescriptor<TDocument> AnalyzeWildcard(bool? analyzewildcard = true) => Qs("analyze_wildcard", analyzewildcard);
		///<summary>The analyzer to use for the query string</summary>
		public ValidateQueryDescriptor<TDocument> Analyzer(string analyzer) => Qs("analyzer", analyzer);
		///<summary>The default operator for query string query (AND or OR)</summary>
		public ValidateQueryDescriptor<TDocument> DefaultOperator(DefaultOperator? defaultoperator) => Qs("default_operator", defaultoperator);
		///<summary>The field to use as default where no field prefix is given in the query string</summary>
		public ValidateQueryDescriptor<TDocument> Df(string df) => Qs("df", df);
		///<summary>Whether to expand wildcard expression to concrete indices that are open, closed or both.</summary>
		public ValidateQueryDescriptor<TDocument> ExpandWildcards(ExpandWildcards? expandwildcards) => Qs("expand_wildcards", expandwildcards);
		///<summary>Return detailed information about the error</summary>
		public ValidateQueryDescriptor<TDocument> Explain(bool? explain = true) => Qs("explain", explain);
		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public ValidateQueryDescriptor<TDocument> IgnoreUnavailable(bool? ignoreunavailable = true) => Qs("ignore_unavailable", ignoreunavailable);
		///<summary>Specify whether format-based query failures (such as providing text to a numeric field) should be ignored</summary>
		public ValidateQueryDescriptor<TDocument> Lenient(bool? lenient = true) => Qs("lenient", lenient);
		///<summary>Query in the Lucene query string syntax</summary>
		public ValidateQueryDescriptor<TDocument> QueryOnQueryString(string queryonquerystring) => Qs("q", queryonquerystring);
		///<summary>Provide a more detailed explanation showing the actual Lucene query that will be executed.</summary>
		public ValidateQueryDescriptor<TDocument> Rewrite(bool? rewrite = true) => Qs("rewrite", rewrite);
	}
}
