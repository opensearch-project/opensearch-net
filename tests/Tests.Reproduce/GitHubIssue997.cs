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

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce
{
	public class GitHubIssue997
	{
		[U]
		public void DeleteByQueryIsNotValidOnTransportFailure()
		{
			var client = TransportFailureClient();

			var response = client.DeleteByQuery<object>(d => d.Index("test-index").Query(q => q.MatchAll()));

			response.ApiCall.Success.Should().BeFalse();
			response.ApiCall.HttpStatusCode.Should().BeNull();
			response.IsValid.Should().BeFalse();
		}

		[U]
		public void UpdateByQueryIsNotValidOnTransportFailure()
		{
			var client = TransportFailureClient();

			var response = client.UpdateByQuery<object>(d => d.Index("test-index").Query(q => q.MatchAll()));

			response.ApiCall.Success.Should().BeFalse();
			response.ApiCall.HttpStatusCode.Should().BeNull();
			response.IsValid.Should().BeFalse();
		}

		private static OpenSearchClient TransportFailureClient()
		{
			var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
			var settings = new ConnectionSettings(pool, new TransportFailureConnection()).DisablePing();
			return new OpenSearchClient(settings);
		}

		private sealed class TransportFailureConnection : IConnection
		{
			public TResponse Request<TResponse>(RequestData requestData)
				where TResponse : class, IOpenSearchResponse, new() =>
				ResponseBuilder.ToResponse<TResponse>(requestData, new HttpRequestException("simulated transport failure"), null, null,
					Stream.Null, RequestData.MimeType);

			public Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
				where TResponse : class, IOpenSearchResponse, new() =>
				Task.FromResult(Request<TResponse>(requestData));

			public void Dispose() { }
		}
	}
}
