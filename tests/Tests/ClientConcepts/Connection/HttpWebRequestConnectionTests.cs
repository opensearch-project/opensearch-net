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

#if !DOTNETCORE
using System.Linq;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch;
using Tests.Core.ManagedOpenSearch.Clusters;
using HttpMethod = OpenSearch.Net.HttpMethod;

namespace Tests.ClientConcepts.Connection
{
	public class HttpWebRequestConnectionTests : ClusterTestClassBase<ReadOnlyCluster>
	{
		public HttpWebRequestConnectionTests(ReadOnlyCluster cluster) : base(cluster) { }

		private RequestData CreateRequestData(
			TimeSpan requestTimeout = default,
			Uri proxyAddress = null,
			bool disableAutomaticProxyDetection = false,
			bool httpCompression = false,
			bool transferEncodingChunked = false
		)
		{
			if (requestTimeout == default) requestTimeout = TimeSpan.FromSeconds(10);

			var node = Client.ConnectionSettings.ConnectionPool.Nodes.First();
			var connectionSettings = new ConnectionSettings(node.Uri)
				.RequestTimeout(requestTimeout)
				.DisableAutomaticProxyDetection(disableAutomaticProxyDetection)
				.TransferEncodingChunked(transferEncodingChunked)
				.EnableHttpCompression(httpCompression);

			if (proxyAddress != null)
				connectionSettings.Proxy(proxyAddress, null, (string)null);

			var requestData = new RequestData(HttpMethod.POST, "/_search", "{ \"query\": { \"match_all\" : { } } }", connectionSettings,
				new SearchRequestParameters(),
				new MemoryStreamFactory()) { Node = node };

			return requestData;
		}

		[I] public async Task HttpWebRequestUseTransferEncodingChunkedWhenTransferEncodingChunkedTrue()
		{
			var connection = new TestableHttpWebRequestConnection();
			var requestData = CreateRequestData(transferEncodingChunked: true);

			connection.Request<StringResponse>(requestData);
			connection.LastRequest.SendChunked.Should().BeTrue();
			connection.LastRequest.ContentLength.Should().Be(-1);

			await connection.RequestAsync<StringResponse>(requestData, CancellationToken.None).ConfigureAwait(false);
			connection.LastRequest.SendChunked.Should().BeTrue();
			connection.LastRequest.ContentLength.Should().Be(-1);
		}

		[I] public async Task HttpWebRequestSetsContentLengthWhenTransferEncodingChunkedFalse()
		{
			var connection = new TestableHttpWebRequestConnection();
			var requestData = CreateRequestData(transferEncodingChunked: false);

			connection.Request<StringResponse>(requestData);
			connection.LastRequest.SendChunked.Should().BeFalse();
			connection.LastRequest.ContentLength.Should().BeGreaterThan(0);

			await connection.RequestAsync<StringResponse>(requestData, CancellationToken.None).ConfigureAwait(false);
			connection.LastRequest.SendChunked.Should().BeFalse();
			connection.LastRequest.ContentLength.Should().BeGreaterThan(0);
		}

		public class TestableHttpWebRequestConnection : HttpWebRequestConnection
		{
			public int CallCount { get; private set; }

			public HttpWebRequest LastRequest { get; private set; }

			public HttpWebResponse LastResponse => LastRequest != null && LastRequest.HaveResponse
				? (HttpWebResponse)LastRequest?.GetResponse()
				: null;

			public override TResponse Request<TResponse>(RequestData requestData)
			{
				CallCount++;
				return base.Request<TResponse>(requestData);
			}

			public override Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
			{
				CallCount++;
				return base.RequestAsync<TResponse>(requestData, cancellationToken);
			}

			protected override HttpWebRequest CreateHttpWebRequest(RequestData requestData)
			{
				LastRequest = base.CreateHttpWebRequest(requestData);
				return LastRequest;
			}
		}
	}
}
#endif
