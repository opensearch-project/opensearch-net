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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Connection.Http;
using HttpMethod = OpenSearch.Net.HttpMethod;

namespace Tests.Extensions.DependencyInjection;

public class OpenSearchServiceCollectionExtensionsTests
{
	private static readonly Uri Node = new("http://localhost:9200");

	[U]
	public async Task SendsRequestsThroughTheNamedClientPipeline()
	{
		var sentRequests = new List<HttpRequestMessage>();
		var services = new ServiceCollection();
		services.AddOpenSearchClient(Node, httpClientName: "my-opensearch")
			.AddHttpMessageHandler(() => new HeaderAddingHandler("x-pipeline", "custom"))
			.ConfigurePrimaryHttpMessageHandler(() => new MockHttpMessageHandler(r =>
			{
				sentRequests.Add(r);
				return JsonResponse();
			}));
		using var provider = services.BuildServiceProvider();

		var client = provider.GetRequiredService<IOpenSearchClient>();
		var lowLevel = provider.GetRequiredService<IOpenSearchLowLevelClient>();

		client.LowLevel.DoRequest<StringResponse>(HttpMethod.GET, "/sync").Success.Should().BeTrue();
		(await lowLevel.DoRequestAsync<StringResponse>(HttpMethod.GET, "/async", CancellationToken.None)).Success.Should().BeTrue();

		lowLevel.Should().BeSameAs(client.LowLevel);
		provider.GetRequiredService<IOpenSearchClient>().Should().BeSameAs(client);
		sentRequests.Should().HaveCount(2);
		foreach (var request in sentRequests)
			request.Headers.GetValues("x-pipeline").Should().Equal("custom");
		sentRequests[0].RequestUri.Should().Be(new Uri(Node, "/sync"));
	}

	[U]
	public void SettingsFactoryReceivesServicesAndConnection()
	{
		var services = new ServiceCollection();
		services.AddSingleton(new StaticConnectionPool(new[] { Node }));
		services.AddOpenSearchClient((sp, connection) =>
			new ConnectionSettings(sp.GetRequiredService<StaticConnectionPool>(), connection).DefaultIndex("my-index"));
		using var provider = services.BuildServiceProvider();

		var settings = provider.GetRequiredService<IOpenSearchClient>().ConnectionSettings;

		settings.DefaultIndex.Should().Be("my-index");
		settings.ConnectionPool.Should().BeSameAs(provider.GetRequiredService<StaticConnectionPool>());
		settings.Connection.Should().BeOfType<global::OpenSearch.Client.Extensions.DependencyInjection.HttpClientFactoryConnection>();
	}

	[U]
	public void ResolvingHttpClientFactoryBeforeTheClientDoesNotDeadlock()
	{
		var services = new ServiceCollection();
		services.AddOpenSearchClient(Node).ConfigurePrimaryHttpMessageHandler(() => new MockHttpMessageHandler(_ => JsonResponse()));
		using var provider = services.BuildServiceProvider();

		var resolve = Task.Run(() =>
		{
			provider.GetRequiredService<IHttpClientFactory>().CreateClient(OpenSearchServiceCollectionExtensions.DefaultHttpClientName);
			return provider.GetRequiredService<IOpenSearchLowLevelClient>().DoRequest<StringResponse>(HttpMethod.GET, "/");
		});

		resolve.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue("resolving the factory first must not deadlock the container");
		resolve.Result.Success.Should().BeTrue();
	}

	[U]
	public void SettingsFactoryReturningNullIsReported()
	{
		var services = new ServiceCollection();
		services.AddOpenSearchClient((_, _) => null);
		using var provider = services.BuildServiceProvider();

		Action act = () => provider.GetRequiredService<IOpenSearchClient>();

		act.Should().Throw<InvalidOperationException>().WithMessage("*returned null*");
	}

	[U]
	public void PrimaryHandlerIsConfiguredFromConnectionSettings()
	{
		Func<object, System.Security.Cryptography.X509Certificates.X509Certificate, System.Security.Cryptography.X509Certificates.X509Chain,
			System.Net.Security.SslPolicyErrors, bool> callback = (_, _, _, _) => true;

		var handler = PrimaryHandler((_, s) => s
			.EnableHttpCompression()
			.ConnectionLimit(12)
			.Proxy(new Uri("http://proxy:8080"), "user", "pass")
			.ServerCertificateValidationCallback(callback));

		handler.AutomaticDecompression.Should().Be(DecompressionMethods.GZip | DecompressionMethods.Deflate);
		handler.MaxConnectionsPerServer.Should().Be(12);
		handler.Proxy.GetProxy(new Uri("http://remote:9200")).Should().Be(new Uri("http://proxy:8080"));
		handler.Proxy.Should().BeOfType<WebProxy>();
		handler.Proxy.Credentials.Should().BeOfType<NetworkCredential>().Which.Password.Should().Be("pass");
		handler.ServerCertificateCustomValidationCallback.Should().NotBeNull();
	}

	[U]
	public void PrimaryHandlerDefaultsMatchHttpConnection()
	{
		var handler = PrimaryHandler();

		handler.AutomaticDecompression.Should().Be(DecompressionMethods.None);
		handler.MaxConnectionsPerServer.Should().Be(ConnectionConfiguration.DefaultConnectionLimit);
		handler.UseProxy.Should().BeTrue();
	}

	[U]
	public void PrimaryHandlerHonoursDisabledProxyDetection() =>
		PrimaryHandler((_, s) => s.DisableAutomaticProxyDetection()).UseProxy.Should().BeFalse();

	[U]
	public void HandlerLifetimeDefaultsToDnsRefreshTimeout()
	{
		var services = new ServiceCollection();
		services.AddOpenSearchClient(Node, (_, s) => s.DnsRefreshTimeout(TimeSpan.FromMinutes(7)));
		using var provider = services.BuildServiceProvider();

		HandlerLifetime(provider).Should().Be(TimeSpan.FromMinutes(7));
	}

	[U]
	public void HandlerLifetimeCanBeOverridden()
	{
		var services = new ServiceCollection();
		services.AddOpenSearchClient(Node, (_, s) => s.DnsRefreshTimeout(TimeSpan.FromMinutes(7)))
			.SetHandlerLifetime(TimeSpan.FromMinutes(1));
		using var provider = services.BuildServiceProvider();

		HandlerLifetime(provider).Should().Be(TimeSpan.FromMinutes(1));
	}

	[U]
	public void HttpClientTimeoutIsLeftToRequestTimeout()
	{
		var services = new ServiceCollection();
		services.AddOpenSearchClient(Node);
		using var provider = services.BuildServiceProvider();

		provider.GetRequiredService<IHttpClientFactory>()
			.CreateClient(OpenSearchServiceCollectionExtensions.DefaultHttpClientName)
			.Timeout.Should().Be(Timeout.InfiniteTimeSpan);
	}

	[U]
	public void NullArgumentsAreRejected()
	{
		var services = new ServiceCollection();

		Action nullNode = () => services.AddOpenSearchClient((Uri)null);
		Action nullFactory = () => services.AddOpenSearchClient((Func<IServiceProvider, IConnection, IConnectionSettingsValues>)null);
		Action nullName = () => services.AddOpenSearchClient(Node, httpClientName: null);

		nullNode.Should().Throw<ArgumentNullException>().WithParameterName("node");
		nullFactory.Should().Throw<ArgumentNullException>().WithParameterName("settingsFactory");
		nullName.Should().Throw<ArgumentNullException>().WithParameterName("httpClientName");
	}

	private static HttpClientHandler PrimaryHandler(Action<IServiceProvider, ConnectionSettings> configure = null)
	{
		var services = new ServiceCollection();
		services.AddOpenSearchClient(Node, configure);
		using var provider = services.BuildServiceProvider();

		var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>()
			.CreateHandler(OpenSearchServiceCollectionExtensions.DefaultHttpClientName);
		while (handler is DelegatingHandler delegating)
			handler = delegating.InnerHandler;

		return handler.Should().BeOfType<HttpClientHandler>().Subject;
	}

	private static TimeSpan HandlerLifetime(IServiceProvider provider) =>
		provider.GetRequiredService<IOptionsMonitor<HttpClientFactoryOptions>>()
			.Get(OpenSearchServiceCollectionExtensions.DefaultHttpClientName)
			.HandlerLifetime;

	private static HttpResponseMessage JsonResponse() =>
		new(HttpStatusCode.OK) { Content = new StringContent("{}", Encoding.UTF8, "application/json") };

	private sealed class HeaderAddingHandler : DelegatingHandler
	{
		private readonly string _name;
		private readonly string _value;

		public HeaderAddingHandler(string name, string value)
		{
			_name = name;
			_value = value;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.Headers.Add(_name, _value);
			return base.SendAsync(request, cancellationToken);
		}
	}
}
