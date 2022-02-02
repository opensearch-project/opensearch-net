/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
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

using System;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.VirtualizedCluster.Providers;

namespace OpenSearch.Net.VirtualizedCluster
{
	public class VirtualizedCluster
	{
		private readonly FixedPipelineFactory _fixedRequestPipeline;
		private readonly TestableDateTimeProvider _dateTimeProvider;
		private readonly ConnectionConfiguration _settings;
		private Func<IOpenSearchLowLevelClient, Func<RequestConfigurationDescriptor, IRequestConfiguration>, Task<IOpenSearchResponse>> _asyncCall;
		private Func<IOpenSearchLowLevelClient, Func<RequestConfigurationDescriptor, IRequestConfiguration>, IOpenSearchResponse> _syncCall;

		private class VirtualResponse : OpenSearchResponseBase { }

		public VirtualizedCluster(TestableDateTimeProvider dateTimeProvider, ConnectionConfiguration settings)
		{
			_dateTimeProvider = dateTimeProvider;
			_settings = settings;
			_fixedRequestPipeline = new FixedPipelineFactory(settings, _dateTimeProvider);

			_syncCall = (c, r) => c.Search<VirtualResponse>(PostData.Serializable(new {}), new SearchRequestParameters
			{
					RequestConfiguration = r?.Invoke(new RequestConfigurationDescriptor(null))
			});
			_asyncCall = async (c, r) =>
			{
				var res = await c.SearchAsync<VirtualResponse>
				(
					PostData.Serializable(new { }),
					new SearchRequestParameters { RequestConfiguration = r?.Invoke(new RequestConfigurationDescriptor(null)) },
					CancellationToken.None
				).ConfigureAwait(false);
				return (IOpenSearchResponse)res;
			};
		}

		public VirtualClusterConnection Connection => Client.Settings.Connection as VirtualClusterConnection;
		public IConnectionPool ConnectionPool => Client.Settings.ConnectionPool;
		public OpenSearchLowLevelClient Client => _fixedRequestPipeline?.Client;

		public VirtualizedCluster ClientProxiesTo(
			Func<IOpenSearchLowLevelClient, Func<RequestConfigurationDescriptor, IRequestConfiguration>, IOpenSearchResponse> sync,
			Func<IOpenSearchLowLevelClient, Func<RequestConfigurationDescriptor, IRequestConfiguration>, Task<IOpenSearchResponse>> async
		)
		{
			_syncCall = sync;
			_asyncCall = async;
			return this;
		}

		public IOpenSearchResponse ClientCall(Func<RequestConfigurationDescriptor, IRequestConfiguration> requestOverrides = null) =>
			_syncCall(Client, requestOverrides);

		public async Task<IOpenSearchResponse> ClientCallAsync(Func<RequestConfigurationDescriptor, IRequestConfiguration> requestOverrides = null) =>
			await _asyncCall(Client, requestOverrides).ConfigureAwait(false);

		public void ChangeTime(Func<DateTime, DateTime> change) => _dateTimeProvider.ChangeTime(change);

		public void ClientThrows(bool throws) => _settings.ThrowExceptions(throws);
	}
}
