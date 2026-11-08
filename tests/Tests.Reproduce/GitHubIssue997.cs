/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
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
