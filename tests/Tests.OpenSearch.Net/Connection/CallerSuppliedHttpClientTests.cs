/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
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
			var client = CreateClient(new HttpConnection(_ => (HttpClient)null));

			Action act = () => client.DoRequest<StringResponse>(HttpMethod.GET, "/");

			act.Should().Throw<UnexpectedOpenSearchClientException>()
				.WithInnerException<InvalidOperationException>().WithMessage("*returned null*");
		}

		[U] public async Task PipelineExceptionsFailOverToTheNextNode()
		{
			var nodes = new[] { new Uri("http://node1:9200"), new Uri("http://node2:9200") };
			var handler = new RecordingHandler(failFor: nodes[0]);
			using var httpClient = new HttpClient(handler);
			var settings = new ConnectionConfiguration(new StaticConnectionPool(nodes, randomize: false), new HttpConnection(httpClient));
			var client = new OpenSearchLowLevelClient(settings);

			var syncResponse = client.DoRequest<StringResponse>(HttpMethod.GET, "/");
			var asyncResponse = await client.DoRequestAsync<StringResponse>(HttpMethod.GET, "/", CancellationToken.None);

			foreach (var response in new[] { syncResponse, asyncResponse })
			{
				response.Success.Should().BeTrue();
				response.ApiCall.Uri.Host.Should().Be("node2");
			}
		}

		[U] public void PipelineExceptionIsReportedAsFailedCall()
		{
			using var httpClient = new HttpClient(new RecordingHandler(failFor: Node));
			var client = CreateClient(new HttpConnection(httpClient));

			var response = client.DoRequest<StringResponse>(HttpMethod.GET, "/");

			response.Success.Should().BeFalse();
			response.OriginalException.Should().BeOfType<HttpRequestException>()
				.Which.InnerException.Should().BeOfType<PipelineFailureException>();
		}

		[U] public void DefaultHttpClientHandlerIsConfiguredFromSettings()
		{
			var settings = new ConnectionConfiguration(Node)
				.EnableHttpCompression()
				.ConnectionLimit(12)
				.Proxy(new Uri("http://proxy:8080"), "user", "pass");

			var handler = HttpConnection.CreateDefaultHttpClientHandler(settings);

			handler.AutomaticDecompression.Should().Be(DecompressionMethods.GZip | DecompressionMethods.Deflate);
			handler.MaxConnectionsPerServer.Should().Be(12);
			handler.Proxy.GetProxy(new Uri("http://remote:9200")).Should().Be(new Uri("http://proxy:8080"));
			handler.Proxy.IsBypassed(new Uri("http://127.0.0.1:9200")).Should().BeTrue();
			((Action)(() => HttpConnection.CreateDefaultHttpClientHandler(null))).Should().Throw<ArgumentNullException>();
		}

		[U] public async Task SendsSyncAndAsyncRequestsThroughSuppliedHandler()
		{
			var handler = new RecordingHandler();
			var paths = new List<string>();
			var client = CreateClient(new HttpConnection(requestData =>
			{
				paths.Add(requestData.PathAndQuery);
				return handler;
			}));

			client.DoRequest<StringResponse>(HttpMethod.GET, "/sync").Success.Should().BeTrue();
			(await client.DoRequestAsync<StringResponse>(HttpMethod.GET, "/async", CancellationToken.None)).Success.Should().BeTrue();

			paths.Should().Equal("/sync", "/async");
			handler.Requests.Select(r => r.RequestUri).Should().Equal(new Uri(Node, "/sync"), new Uri(Node, "/async"));
			handler.Disposed.Should().BeFalse();
		}

		[U] public async Task SuppliedClientTimeoutCapsALongerRequestTimeout()
		{
			using var httpClient = new HttpClient(new RecordingHandler(delay: TimeSpan.FromMilliseconds(500)))
			{
				Timeout = TimeSpan.FromMilliseconds(100)
			};
			var client = CreateClient(new HttpConnection(httpClient), c => c.RequestTimeout(TimeSpan.FromSeconds(30)));

			var response = await client.DoRequestAsync<StringResponse>(HttpMethod.GET, "/", CancellationToken.None);

			response.Success.Should().BeFalse("the client's own Timeout applies on top of RequestTimeout");
		}

		[U] public async Task RequestTimeoutIsTheOnlyLimitForSuppliedHandler()
		{
			var handler = new RecordingHandler(delay: TimeSpan.FromMilliseconds(300));
			var client = CreateClient(new HttpConnection(_ => handler), c => c.RequestTimeout(TimeSpan.FromMilliseconds(100)));
			var longerTimeout = new PingRequestParameters
			{
				RequestConfiguration = new RequestConfiguration { RequestTimeout = TimeSpan.FromSeconds(30) }
			};

			var defaultTimeout = await client.DoRequestAsync<StringResponse>(HttpMethod.GET, "/", CancellationToken.None);
			var overridden = await client.DoRequestAsync<StringResponse>(HttpMethod.GET, "/", CancellationToken.None, null, longerTimeout);
			var overriddenSync = client.DoRequest<StringResponse>(HttpMethod.GET, "/", null, longerTimeout);

			defaultTimeout.Success.Should().BeFalse();
			defaultTimeout.OriginalException.Should().BeAssignableTo<OperationCanceledException>();
			overridden.Success.Should().BeTrue("a per request timeout longer than the default takes effect");
			overriddenSync.Success.Should().BeTrue();
		}

		[U] public async Task PipelineExceptionsFromSuppliedHandlerFailOverToTheNextNode()
		{
			var nodes = new[] { new Uri("http://node1:9200"), new Uri("http://node2:9200") };
			var handler = new RecordingHandler(failFor: nodes[0]);
			var settings = new ConnectionConfiguration(new StaticConnectionPool(nodes, randomize: false), new HttpConnection(_ => handler));

			var response = await new OpenSearchLowLevelClient(settings).DoRequestAsync<StringResponse>(HttpMethod.GET, "/", CancellationToken.None);

			response.Success.Should().BeTrue();
			response.ApiCall.Uri.Host.Should().Be("node2");
		}

		[U] public void DisposingConnectionDoesNotDisposeSuppliedHandler()
		{
			var handler = new RecordingHandler();
			var connection = new HttpConnection(_ => handler);
			CreateClient(connection).DoRequest<StringResponse>(HttpMethod.GET, "/");

			((IDisposable)connection).Dispose();

			handler.Disposed.Should().BeFalse();
		}

		[U] public void HandlerProviderReturningNullIsReported()
		{
			var client = CreateClient(new HttpConnection(_ => (HttpMessageHandler)null));

			Action act = () => client.DoRequest<StringResponse>(HttpMethod.GET, "/");

			act.Should().Throw<UnexpectedOpenSearchClientException>()
				.WithInnerException<InvalidOperationException>().WithMessage("*HttpMessageHandler provider*returned null*");
		}

		[U] public void NullArgumentsAreRejected()
		{
			Action nullClient = () => new HttpConnection((HttpClient)null);
			Action nullProvider = () => new HttpConnection((Func<RequestData, HttpClient>)null);
			Action nullHandlerProvider = () => new HttpConnection((Func<RequestData, HttpMessageHandler>)null);

			nullClient.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
			nullProvider.Should().Throw<ArgumentNullException>().WithParameterName("httpClientProvider");
			nullHandlerProvider.Should().Throw<ArgumentNullException>().WithParameterName("httpMessageHandlerProvider");
		}

		private static OpenSearchLowLevelClient CreateClient(IConnection connection, Func<ConnectionConfiguration, ConnectionConfiguration> configure = null)
		{
			var settings = new ConnectionConfiguration(new SingleNodeConnectionPool(Node), connection);
			return new OpenSearchLowLevelClient(configure?.Invoke(settings) ?? settings);
		}

		private sealed class RecordingHandler : HttpMessageHandler
		{
			private readonly bool _waitForCancellation;
			private readonly Uri _failFor;
			private readonly TimeSpan? _delay;

			public RecordingHandler(bool waitForCancellation = false, Uri failFor = null, TimeSpan? delay = null)
			{
				_waitForCancellation = waitForCancellation;
				_failFor = failFor;
				_delay = delay;
			}

			public List<HttpRequestMessage> Requests { get; } = new();

			public bool Disposed { get; private set; }

			protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			{
				Requests.Add(request);
				if (_failFor != null && request.RequestUri.Authority == _failFor.Authority)
					throw new PipelineFailureException();
				if (_waitForCancellation)
					await Task.Delay(Timeout.Infinite, cancellationToken);
				if (_delay.HasValue)
					await Task.Delay(_delay.Value, cancellationToken);
				return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };
			}

			protected override void Dispose(bool disposing)
			{
				Disposed = true;
				base.Dispose(disposing);
			}
		}

		/// <summary> Stands in for a handler specific failure, e.g. a resilience handler's open circuit. </summary>
		private sealed class PipelineFailureException : Exception { }
	}
}
