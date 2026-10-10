/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using HttpMethod = OpenSearch.Net.HttpMethod;

namespace Tests.OpenSearch.Net.Connection
{
	/// <summary> <see cref="HttpConnection" /> sending requests through an <see cref="HttpClient" /> owned by the caller. </summary>
	public class CallerSuppliedHttpClientTests
	{
		private static readonly Uri Node = new("http://localhost:9200");

		[U] public async Task SendsSyncAndAsyncRequestsThroughSuppliedClient()
		{
			var handler = new RecordingHandler();
			using var httpClient = new HttpClient(handler);
			var connection = new HttpConnection(httpClient);
			var client = CreateClient(connection);

			client.DoRequest<StringResponse>(HttpMethod.GET, "/sync").Success.Should().BeTrue();
			(await client.DoRequestAsync<StringResponse>(HttpMethod.GET, "/async", CancellationToken.None)).Success.Should().BeTrue();

			handler.Requests.Should().HaveCount(2);
			handler.Requests[0].RequestUri.Should().Be(new Uri(Node, "/sync"));
			handler.Requests[1].RequestUri.Should().Be(new Uri(Node, "/async"));
			connection.InUseHandlers.Should().Be(0);
			connection.RemovedHandlers.Should().Be(0);
		}

		[U] public void ProviderIsCalledPerRequestWithRequestData()
		{
			var handler = new RecordingHandler();
			var paths = new List<string>();
			var connection = new HttpConnection(requestData =>
			{
				paths.Add(requestData.PathAndQuery);
				return new HttpClient(handler, disposeHandler: false);
			});
			var client = CreateClient(connection);

			client.DoRequest<StringResponse>(HttpMethod.GET, "/one");
			client.DoRequest<StringResponse>(HttpMethod.GET, "/two");

			paths.Should().Equal("/one", "/two");
			handler.Requests.Should().HaveCount(2);
		}

		[U] public async Task RequestTimeoutIsEnforcedThroughCancellation()
		{
			using var httpClient = new HttpClient(new RecordingHandler(waitForCancellation: true)) { Timeout = Timeout.InfiniteTimeSpan };
			var client = CreateClient(new HttpConnection(httpClient), c => c.RequestTimeout(TimeSpan.FromMilliseconds(100)));

			var syncResponse = client.DoRequest<StringResponse>(HttpMethod.GET, "/");
			var asyncResponse = await client.DoRequestAsync<StringResponse>(HttpMethod.GET, "/", CancellationToken.None);

			foreach (var response in new[] { syncResponse, asyncResponse })
			{
				response.Success.Should().BeFalse();
				response.ApiCall.HttpStatusCode.Should().BeNull();
				response.OriginalException.Should().NotBeNull();
			}
		}

		[U] public async Task CallerCancellationIsHonoured()
		{
			using var httpClient = new HttpClient(new RecordingHandler(waitForCancellation: true)) { Timeout = Timeout.InfiniteTimeSpan };
			var client = CreateClient(new HttpConnection(httpClient), c => c.RequestTimeout(TimeSpan.FromMinutes(5)));
			using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

			Func<Task> act = () => client.DoRequestAsync<StringResponse>(HttpMethod.GET, "/", cancellation.Token);

			// Same as for a library-owned client: the pipeline surfaces caller cancellation as an unexpected exception.
			(await act.Should().ThrowAsync<UnexpectedOpenSearchClientException>())
				.WithInnerException<OperationCanceledException>();
		}

		[U] public async Task DisposingConnectionDoesNotDisposeSuppliedClient()
		{
			var handler = new RecordingHandler();
			using var httpClient = new HttpClient(handler);
			var connection = new HttpConnection(httpClient);

			((IDisposable)connection).Dispose();

			Func<Task> act = () => httpClient.GetAsync(Node);
			await act.Should().NotThrowAsync();
		}

		[U] public void ProviderReturningNullIsReported()
		{
			var client = CreateClient(new HttpConnection(_ => null));

			Action act = () => client.DoRequest<StringResponse>(HttpMethod.GET, "/");

			act.Should().Throw<UnexpectedOpenSearchClientException>()
				.WithInnerException<InvalidOperationException>().WithMessage("*returned null*");
		}

		[U] public void NullArgumentsAreRejected()
		{
			Action nullClient = () => new HttpConnection((HttpClient)null);
			Action nullProvider = () => new HttpConnection((Func<RequestData, HttpClient>)null);

			nullClient.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
			nullProvider.Should().Throw<ArgumentNullException>().WithParameterName("httpClientProvider");
		}

		private static OpenSearchLowLevelClient CreateClient(IConnection connection, Func<ConnectionConfiguration, ConnectionConfiguration> configure = null)
		{
			var settings = new ConnectionConfiguration(new SingleNodeConnectionPool(Node), connection);
			return new OpenSearchLowLevelClient(configure?.Invoke(settings) ?? settings);
		}

		private sealed class RecordingHandler : HttpMessageHandler
		{
			private readonly bool _waitForCancellation;

			public RecordingHandler(bool waitForCancellation = false) => _waitForCancellation = waitForCancellation;

			public List<HttpRequestMessage> Requests { get; } = new();

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				Requests.Add(request);
				if (_waitForCancellation)
					await Task.Delay(Timeout.Infinite, cancellationToken);
				return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
			}
		}
	}
}
