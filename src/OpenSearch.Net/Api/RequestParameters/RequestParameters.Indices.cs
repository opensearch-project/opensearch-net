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

// ReSharper disable once CheckNamespace
namespace OpenSearch.Net.Specification.IndicesApi
{
	///<summary>Request options for AddBlock <para></para></summary>
	public class AddIndexBlockRequestParameters : RequestParameters<AddIndexBlockRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => false;
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

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}
	}

	///<summary>Request options for Analyze <para></para></summary>
	public class AnalyzeRequestParameters : RequestParameters<AnalyzeRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => true;
		///<summary>The name of the index to scope the operation</summary>
		public string IndexQueryString
		{
			get => Q<string>("index");
			set => Q("index", value);
		}
	}

	///<summary>Request options for ClearCache <para></para></summary>
	public class ClearCacheRequestParameters : RequestParameters<ClearCacheRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => false;
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

		///<summary>Clear field data</summary>
		public bool? Fielddata
		{
			get => Q<bool? >("fielddata");
			set => Q("fielddata", value);
		}

		///<summary>A comma-separated list of fields to clear when using the `fielddata` parameter (default: all)</summary>
		public string[] Fields
		{
			get => Q<string[]>("fields");
			set => Q("fields", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>A comma-separated list of index name to limit the operation</summary>
		public string[] IndexQueryString
		{
			get => Q<string[]>("index");
			set => Q("index", value);
		}

		///<summary>Clear query caches</summary>
		public bool? Query
		{
			get => Q<bool? >("query");
			set => Q("query", value);
		}

		///<summary>Clear request cache</summary>
		public bool? Request
		{
			get => Q<bool? >("request");
			set => Q("request", value);
		}
	}

	///<summary>Request options for Clone <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/clone/</para></summary>
	public class CloneIndexRequestParameters : RequestParameters<CloneIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Set the number of active shards to wait for on the cloned index before the operation returns.</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	///<summary>Request options for Close <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</para></summary>
	public class CloseIndexRequestParameters : RequestParameters<CloseIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => false;
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

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}

		///<summary>
		/// Sets the number of active shards to wait for before the operation returns. Set to `index-setting` to wait according to the index setting
		/// `index.write.wait_for_active_shards`, or `all` to wait for all shards, or an integer. Defaults to `0`.
		///</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	///<summary>Request options for Create <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/create-index/</para></summary>
	public class CreateIndexRequestParameters : RequestParameters<CreateIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
		///<summary>Whether a type should be expected in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public bool? IncludeTypeName
		{
			get => Q<bool? >("include_type_name");
			set => Q("include_type_name", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Set the number of active shards to wait for before the operation returns.</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	///<summary>Request options for Delete <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/delete-index/</para></summary>
	public class DeleteIndexRequestParameters : RequestParameters<DeleteIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
		public override bool SupportsBody => false;
		///<summary>Ignore if a wildcard expression resolves to no concrete indices (default: false)</summary>
		public bool? AllowNoIndices
		{
			get => Q<bool? >("allow_no_indices");
			set => Q("allow_no_indices", value);
		}

		///<summary>Whether wildcard expressions should get expanded to open or closed indices (default: open)</summary>
		public ExpandWildcards? ExpandWildcards
		{
			get => Q<ExpandWildcards? >("expand_wildcards");
			set => Q("expand_wildcards", value);
		}

		///<summary>Ignore unavailable indexes (default: false)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}
	}

	///<summary>Request options for DeleteAlias <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public class DeleteAliasRequestParameters : RequestParameters<DeleteAliasRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
		public override bool SupportsBody => false;
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit timestamp for the document</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}
	}

	///<summary>Request options for DeleteTemplateV2 <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	///<seealso cref="DeleteComposableIndexTemplateRequestParameters"/>
	[Obsolete($"Replaced by {nameof(DeleteComposableIndexTemplateRequestParameters)}")]
	public class DeleteIndexTemplateV2RequestParameters : RequestParameters<DeleteIndexTemplateV2RequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
		public override bool SupportsBody => false;
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}
	}

	///<summary>Request options for DeleteTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public class DeleteIndexTemplateRequestParameters : RequestParameters<DeleteIndexTemplateRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
		public override bool SupportsBody => false;
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}
	}

	///<summary>Request options for Exists <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</para></summary>
	public class IndexExistsRequestParameters : RequestParameters<IndexExistsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.HEAD;
		public override bool SupportsBody => false;
		///<summary>Ignore if a wildcard expression resolves to no concrete indices (default: false)</summary>
		public bool? AllowNoIndices
		{
			get => Q<bool? >("allow_no_indices");
			set => Q("allow_no_indices", value);
		}

		///<summary>Whether wildcard expressions should get expanded to open or closed indices (default: open)</summary>
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

		///<summary>Ignore unavailable indexes (default: false)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Whether to return all default setting for each of the indices.</summary>
		public bool? IncludeDefaults
		{
			get => Q<bool? >("include_defaults");
			set => Q("include_defaults", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}
	}

	///<summary>Request options for AliasExists <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public class AliasExistsRequestParameters : RequestParameters<AliasExistsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.HEAD;
		public override bool SupportsBody => false;
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
	}

	///<summary>Request options for ExistsTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	///<seealso cref="ComposableIndexTemplateExistsRequestParameters"/>
	[Obsolete($"Replaced by {nameof(ComposableIndexTemplateExistsRequestParameters)}")]
	public class ExistsIndexTemplateRequestParameters : RequestParameters<ExistsIndexTemplateRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.HEAD;
		public override bool SupportsBody => false;
		///<summary>Return settings in flat format (default: false)</summary>
		public bool? FlatSettings
		{
			get => Q<bool? >("flat_settings");
			set => Q("flat_settings", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Explicit operation timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for TemplateExists <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public class IndexTemplateExistsRequestParameters : RequestParameters<IndexTemplateExistsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.HEAD;
		public override bool SupportsBody => false;
		///<summary>Return settings in flat format (default: false)</summary>
		public bool? FlatSettings
		{
			get => Q<bool? >("flat_settings");
			set => Q("flat_settings", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Explicit operation timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for TypeExists <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/exists/</para></summary>
	///<remarks>Deprecated as of OpenSearch 2.0</remarks>
	public class TypeExistsRequestParameters : RequestParameters<TypeExistsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.HEAD;
		public override bool SupportsBody => false;
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
	}

	///<summary>Request options for Flush <para></para></summary>
	public class FlushRequestParameters : RequestParameters<FlushRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => false;
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

		///<summary>
		/// Whether a flush should be forced even if it is not necessarily needed ie. if no changes will be committed to the index. This is useful if
		/// transaction log IDs should be incremented even if no uncommitted changes are present. (This setting can be considered as internal)
		///</summary>
		public bool? Force
		{
			get => Q<bool? >("force");
			set => Q("force", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>
		/// If set to true the flush operation will block until the flush can be executed if another flush operation is already executing. The default
		/// is true. If set to false the flush will be skipped iff if another flush operation is already running.
		///</summary>
		public bool? WaitIfOngoing
		{
			get => Q<bool? >("wait_if_ongoing");
			set => Q("wait_if_ongoing", value);
		}
	}

	///<summary>Request options for SyncedFlush <para></para></summary>
	public class SyncedFlushRequestParameters : RequestParameters<SyncedFlushRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => false;
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
	}

	///<summary>Request options for ForceMerge <para></para></summary>
	public class ForceMergeRequestParameters : RequestParameters<ForceMergeRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => false;
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

		///<summary>Specify whether the index should be flushed after performing the operation (default: true)</summary>
		public bool? Flush
		{
			get => Q<bool? >("flush");
			set => Q("flush", value);
		}

		///<summary>Whether specified concrete indices should be ignored when unavailable (missing or closed)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>The number of segments the index should be merged into (default: dynamic)</summary>
		public long? MaxNumSegments
		{
			get => Q<long? >("max_num_segments");
			set => Q("max_num_segments", value);
		}

		///<summary>Specify whether the operation should only expunge deleted documents</summary>
		public bool? OnlyExpungeDeletes
		{
			get => Q<bool? >("only_expunge_deletes");
			set => Q("only_expunge_deletes", value);
		}
	}

	///<summary>Request options for Get <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/get-index/</para></summary>
	public class GetIndexRequestParameters : RequestParameters<GetIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
		///<summary>Ignore if a wildcard expression resolves to no concrete indices (default: false)</summary>
		public bool? AllowNoIndices
		{
			get => Q<bool? >("allow_no_indices");
			set => Q("allow_no_indices", value);
		}

		///<summary>Whether wildcard expressions should get expanded to open or closed indices (default: open)</summary>
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

		///<summary>Ignore unavailable indexes (default: false)</summary>
		public bool? IgnoreUnavailable
		{
			get => Q<bool? >("ignore_unavailable");
			set => Q("ignore_unavailable", value);
		}

		///<summary>Whether to return all default setting for each of the indices.</summary>
		public bool? IncludeDefaults
		{
			get => Q<bool? >("include_defaults");
			set => Q("include_defaults", value);
		}

		///<summary>Whether to add the type name to the response (default: false)</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public bool? IncludeTypeName
		{
			get => Q<bool? >("include_type_name");
			set => Q("include_type_name", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for GetAlias <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public class GetAliasRequestParameters : RequestParameters<GetAliasRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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
	}

	///<summary>Request options for GetFieldMapping <para>https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</para></summary>
	public class GetFieldMappingRequestParameters : RequestParameters<GetFieldMappingRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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

		///<summary>Whether the default mapping values should be returned as well</summary>
		public bool? IncludeDefaults
		{
			get => Q<bool? >("include_defaults");
			set => Q("include_defaults", value);
		}

		///<summary>Whether a type should be returned in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public bool? IncludeTypeName
		{
			get => Q<bool? >("include_type_name");
			set => Q("include_type_name", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}
	}

	///<summary>Request options for GetTemplateV2 <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	///<seealso cref="GetComposableIndexTemplateRequestParameters"/>
	[Obsolete($"Replaced by {nameof(GetComposableIndexTemplateRequestParameters)}")]
	public class GetIndexTemplateV2RequestParameters : RequestParameters<GetIndexTemplateV2RequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
		///<summary>Return settings in flat format (default: false)</summary>
		public bool? FlatSettings
		{
			get => Q<bool? >("flat_settings");
			set => Q("flat_settings", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Explicit operation timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for GetMapping <para>https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</para></summary>
	public class GetMappingRequestParameters : RequestParameters<GetMappingRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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

		///<summary>Whether to add the type name to the response (default: false)</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public bool? IncludeTypeName
		{
			get => Q<bool? >("include_type_name");
			set => Q("include_type_name", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for GetSettings <para></para></summary>
	public class GetIndexSettingsRequestParameters : RequestParameters<GetIndexSettingsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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

		///<summary>Whether to return all default setting for each of the indices.</summary>
		public bool? IncludeDefaults
		{
			get => Q<bool? >("include_defaults");
			set => Q("include_defaults", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for GetTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public class GetIndexTemplateRequestParameters : RequestParameters<GetIndexTemplateRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
		///<summary>Return settings in flat format (default: false)</summary>
		public bool? FlatSettings
		{
			get => Q<bool? >("flat_settings");
			set => Q("flat_settings", value);
		}

		///<summary>Whether a type should be returned in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public bool? IncludeTypeName
		{
			get => Q<bool? >("include_type_name");
			set => Q("include_type_name", value);
		}

		///<summary>Return local information, do not retrieve the state from cluster_manager node (default: false)</summary>
		public bool? Local
		{
			get => Q<bool? >("local");
			set => Q("local", value);
		}

		///<summary>Explicit operation timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Explicit operation timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for Open <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/close-index/</para></summary>
	public class OpenIndexRequestParameters : RequestParameters<OpenIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => false;
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

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Sets the number of active shards to wait for before the operation returns.</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	///<summary>Request options for PutAlias <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public class PutAliasRequestParameters : RequestParameters<PutAliasRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit timestamp for the document</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}
	}

	///<summary>Request options for PutTemplateV2 <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	///<seealso cref="PutComposableIndexTemplateRequestParameters"/>
	[Obsolete($"Replaced by {nameof(PutComposableIndexTemplateRequestParameters)}")]
	public class PutIndexTemplateV2RequestParameters : RequestParameters<PutIndexTemplateV2RequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
		///<summary>User defined reason for creating/updating the index template</summary>
		public string Cause
		{
			get => Q<string>("cause");
			set => Q("cause", value);
		}

		///<summary>Whether the index template should only be added if new or can also replace an existing one</summary>
		public bool? Create
		{
			get => Q<bool? >("create");
			set => Q("create", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for PutMapping <para>https://opensearch.org/docs/latest/opensearch/rest-api/update-mapping/</para></summary>
	public class PutMappingRequestParameters : RequestParameters<PutMappingRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
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

		///<summary>Whether a type should be expected in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public bool? IncludeTypeName
		{
			get => Q<bool? >("include_type_name");
			set => Q("include_type_name", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}

		///<summary>When true, applies mappings only to the write index of an alias</summary>
		public bool? WriteIndexOnly
		{
			get => Q<bool? >("write_index_only");
			set => Q("write_index_only", value);
		}
	}

	///<summary>Request options for UpdateSettings <para></para></summary>
	public class UpdateIndexSettingsRequestParameters : RequestParameters<UpdateIndexSettingsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
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

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Whether to update existing settings. If set to `true` existing settings on an index remain unchanged, the default is `false`</summary>
		public bool? PreserveExisting
		{
			get => Q<bool? >("preserve_existing");
			set => Q("preserve_existing", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}
	}

	///<summary>Request options for PutTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public class PutIndexTemplateRequestParameters : RequestParameters<PutIndexTemplateRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
		///<summary>Whether the index template should only be added if new or can also replace an existing one</summary>
		public bool? Create
		{
			get => Q<bool? >("create");
			set => Q("create", value);
		}

		///<summary>Whether a type should be returned in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public bool? IncludeTypeName
		{
			get => Q<bool? >("include_type_name");
			set => Q("include_type_name", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for RecoveryStatus <para></para></summary>
	public class RecoveryStatusRequestParameters : RequestParameters<RecoveryStatusRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
		///<summary>Display only those recoveries that are currently on-going</summary>
		public bool? ActiveOnly
		{
			get => Q<bool? >("active_only");
			set => Q("active_only", value);
		}

		///<summary>Whether to display detailed information about shard recovery</summary>
		public bool? Detailed
		{
			get => Q<bool? >("detailed");
			set => Q("detailed", value);
		}
	}

	///<summary>Request options for Refresh <para>https://opensearch.org/docs/latest/opensearch/rest-api/document-apis/get-documents/</para></summary>
	public class RefreshRequestParameters : RequestParameters<RefreshRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => false;
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
	}

	///<summary>Request options for Resolve <para></para></summary>
	public class ResolveIndexRequestParameters : RequestParameters<ResolveIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
		///<summary>Whether wildcard expressions should get expanded to open or closed indices (default: open)</summary>
		public ExpandWildcards? ExpandWildcards
		{
			get => Q<ExpandWildcards? >("expand_wildcards");
			set => Q("expand_wildcards", value);
		}
	}

	///<summary>Request options for Rollover <para>https://opensearch.org/docs/latest/opensearch/data-streams/#step-5-rollover-a-data-stream</para></summary>
	public class RolloverIndexRequestParameters : RequestParameters<RolloverIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => true;
		///<summary>If set to true the rollover action will only be validated but not actually performed even if a condition matches. The default is false</summary>
		public bool? DryRun
		{
			get => Q<bool? >("dry_run");
			set => Q("dry_run", value);
		}

		///<summary>Whether a type should be included in the body of the mappings.</summary>
		///<remarks>Deprecated as of OpenSearch 2.0</remarks>
		public bool? IncludeTypeName
		{
			get => Q<bool? >("include_type_name");
			set => Q("include_type_name", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Set the number of active shards to wait for on the newly created rollover index before the operation returns.</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	///<summary>Request options for Segments <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-segments/</para></summary>
	public class SegmentsRequestParameters : RequestParameters<SegmentsRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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

		///<summary>Includes detailed memory usage by Lucene.</summary>
		public bool? Verbose
		{
			get => Q<bool? >("verbose");
			set => Q("verbose", value);
		}
	}

	///<summary>Request options for ShardStores <para></para></summary>
	public class IndicesShardStoresRequestParameters : RequestParameters<IndicesShardStoresRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
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

		///<summary>A comma-separated list of statuses used to filter on shards to get store information for</summary>
		public string[] Status
		{
			get => Q<string[]>("status");
			set => Q("status", value);
		}
	}

	///<summary>Request options for Shrink <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/shrink-index/</para></summary>
	public class ShrinkIndexRequestParameters : RequestParameters<ShrinkIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Set the number of active shards to wait for on the shrunken index before the operation returns.</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	///<summary>Request options for SimulateIndexTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public class SimulateIndexTemplateRequestParameters : RequestParameters<SimulateIndexTemplateRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => true;
		///<summary>User defined reason for dry-run creating the new template for simulation purposes</summary>
		public string Cause
		{
			get => Q<string>("cause");
			set => Q("cause", value);
		}

		///<summary>Whether the index template we optionally defined in the body should only be dry-run added if new or can also replace an existing one</summary>
		public bool? Create
		{
			get => Q<bool? >("create");
			set => Q("create", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for SimulateTemplate <para>https://opensearch.org/docs/latest/opensearch/rest-api/cat/cat-templates/</para></summary>
	public class SimulateTemplateRequestParameters : RequestParameters<SimulateTemplateRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => true;
		///<summary>User defined reason for dry-run creating the new template for simulation purposes</summary>
		public string Cause
		{
			get => Q<string>("cause");
			set => Q("cause", value);
		}

		///<summary>Whether the index template we optionally defined in the body should only be dry-run added if new or can also replace an existing one</summary>
		public bool? Create
		{
			get => Q<bool? >("create");
			set => Q("create", value);
		}

		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}
	}

	///<summary>Request options for Split <para>https://opensearch.org/docs/latest/opensearch/rest-api/index-apis/split/</para></summary>
	public class SplitIndexRequestParameters : RequestParameters<SplitIndexRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Explicit operation timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}

		///<summary>Set the number of active shards to wait for on the shrunken index before the operation returns.</summary>
		public string WaitForActiveShards
		{
			get => Q<string>("wait_for_active_shards");
			set => Q("wait_for_active_shards", value);
		}
	}

	///<summary>Request options for BulkAlias <para>https://opensearch.org/docs/latest/opensearch/rest-api/alias/</para></summary>
	public class BulkAliasRequestParameters : RequestParameters<BulkAliasRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => true;
		///<summary>Specify timeout for connection to master node</summary>
		///<remarks>Deprecated as of OpenSearch 2.0, use <see cref="ClusterManagerTimeSpanout"/> instead</remarks>
		public TimeSpan MasterTimeSpanout
		{
			get => Q<TimeSpan>("master_timeout");
			set => Q("master_timeout", value);
		}

		///<summary>Specify timeout for connection to cluster_manager node</summary>
		///<remarks>Introduced in OpenSearch 2.0 instead of <see cref="MasterTimeSpanout"/></remarks>
		public TimeSpan ClusterManagerTimeSpanout
		{
			get => Q<TimeSpan>("cluster_manager_timeout");
			set => Q("cluster_manager_timeout", value);
		}

		///<summary>Request timeout</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}
	}

	///<summary>Request options for ValidateQuery <para></para></summary>
	public class ValidateQueryRequestParameters : RequestParameters<ValidateQueryRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => true;
		///<summary>Execute validation on all shards instead of one random shard per index</summary>
		public bool? AllShards
		{
			get => Q<bool? >("all_shards");
			set => Q("all_shards", value);
		}

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

		///<summary>Return detailed information about the error</summary>
		public bool? Explain
		{
			get => Q<bool? >("explain");
			set => Q("explain", value);
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

		///<summary>Query in the Lucene query string syntax</summary>
		public string QueryOnQueryString
		{
			get => Q<string>("q");
			set => Q("q", value);
		}

		///<summary>Provide a more detailed explanation showing the actual Lucene query that will be executed.</summary>
		public bool? Rewrite
		{
			get => Q<bool? >("rewrite");
			set => Q("rewrite", value);
		}
	}
}
