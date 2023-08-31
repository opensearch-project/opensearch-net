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
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.Specification.NodesApi;

// ReSharper disable once CheckNamespace
// ReSharper disable RedundantTypeArgumentsOfMethod
namespace OpenSearch.Client.Specification.NodesApi
{
	///<summary>
	/// Nodes APIs.
	/// <para>Not intended to be instantiated directly. Use the <see cref = "IOpenSearchClient.Nodes"/> property
	/// on <see cref = "IOpenSearchClient"/>.
	///</para>
	///</summary>
	public partial class NodesNamespace : NamespacedClientProxy
	{
		/// <summary>
		/// <c>POST</c> request to the <c>nodes.reload_secure_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public ReloadSecureSettingsResponse ReloadSecureSettings(Func<ReloadSecureSettingsDescriptor, IReloadSecureSettingsRequest> selector = null) => ReloadSecureSettings(selector.InvokeOrDefault(new ReloadSecureSettingsDescriptor()));
		/// <summary>
		/// <c>POST</c> request to the <c>nodes.reload_secure_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<ReloadSecureSettingsResponse> ReloadSecureSettingsAsync(Func<ReloadSecureSettingsDescriptor, IReloadSecureSettingsRequest> selector = null, CancellationToken ct = default) => ReloadSecureSettingsAsync(selector.InvokeOrDefault(new ReloadSecureSettingsDescriptor()), ct);
		/// <summary>
		/// <c>POST</c> request to the <c>nodes.reload_secure_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public ReloadSecureSettingsResponse ReloadSecureSettings(IReloadSecureSettingsRequest request) => DoRequest<IReloadSecureSettingsRequest, ReloadSecureSettingsResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>POST</c> request to the <c>nodes.reload_secure_settings</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<ReloadSecureSettingsResponse> ReloadSecureSettingsAsync(IReloadSecureSettingsRequest request, CancellationToken ct = default) => DoRequestAsync<IReloadSecureSettingsRequest, ReloadSecureSettingsResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>nodes.stats</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/stats-api/">https://opensearch.org/docs/latest/opensearch/stats-api/</a>
		/// </summary>
		public NodesStatsResponse Stats(Func<NodesStatsDescriptor, INodesStatsRequest> selector = null) => Stats(selector.InvokeOrDefault(new NodesStatsDescriptor()));
		/// <summary>
		/// <c>GET</c> request to the <c>nodes.stats</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/stats-api/">https://opensearch.org/docs/latest/opensearch/stats-api/</a>
		/// </summary>
		public Task<NodesStatsResponse> StatsAsync(Func<NodesStatsDescriptor, INodesStatsRequest> selector = null, CancellationToken ct = default) => StatsAsync(selector.InvokeOrDefault(new NodesStatsDescriptor()), ct);
		/// <summary>
		/// <c>GET</c> request to the <c>nodes.stats</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/stats-api/">https://opensearch.org/docs/latest/opensearch/stats-api/</a>
		/// </summary>
		public NodesStatsResponse Stats(INodesStatsRequest request) => DoRequest<INodesStatsRequest, NodesStatsResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>GET</c> request to the <c>nodes.stats</c> API, read more about this API online:
		/// <para></para>
		/// <a href = "https://opensearch.org/docs/latest/opensearch/stats-api/">https://opensearch.org/docs/latest/opensearch/stats-api/</a>
		/// </summary>
		public Task<NodesStatsResponse> StatsAsync(INodesStatsRequest request, CancellationToken ct = default) => DoRequestAsync<INodesStatsRequest, NodesStatsResponse>(request, request.RequestParameters, ct);
		/// <summary>
		/// <c>GET</c> request to the <c>nodes.usage</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public NodesUsageResponse Usage(Func<NodesUsageDescriptor, INodesUsageRequest> selector = null) => Usage(selector.InvokeOrDefault(new NodesUsageDescriptor()));
		/// <summary>
		/// <c>GET</c> request to the <c>nodes.usage</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<NodesUsageResponse> UsageAsync(Func<NodesUsageDescriptor, INodesUsageRequest> selector = null, CancellationToken ct = default) => UsageAsync(selector.InvokeOrDefault(new NodesUsageDescriptor()), ct);
		/// <summary>
		/// <c>GET</c> request to the <c>nodes.usage</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public NodesUsageResponse Usage(INodesUsageRequest request) => DoRequest<INodesUsageRequest, NodesUsageResponse>(request, request.RequestParameters);
		/// <summary>
		/// <c>GET</c> request to the <c>nodes.usage</c> API, read more about this API online:
		/// <para></para>
		/// <a href = ""></a>
		/// </summary>
		public Task<NodesUsageResponse> UsageAsync(INodesUsageRequest request, CancellationToken ct = default) => DoRequestAsync<INodesUsageRequest, NodesUsageResponse>(request, request.RequestParameters, ct);
	}
}
