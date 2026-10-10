/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Http;
using OpenSearch.Client;
using OpenSearch.Client.Extensions.DependencyInjection;
using OpenSearch.Net;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary> Registers OpenSearch clients that send their requests through <see cref="IHttpClientFactory" />. </summary>
	public static class OpenSearchServiceCollectionExtensions
	{
		/// <summary> The name of the <see cref="HttpClient" /> registered when none is specified. </summary>
		public const string DefaultHttpClientName = "OpenSearch.Client";

		/// <summary>
		/// Registers a singleton <see cref="IOpenSearchClient" /> (and its <see cref="IOpenSearchLowLevelClient" />) talking to
		/// <paramref name="node" /> through the named <see cref="HttpClient" /> <paramref name="httpClientName" />.
		/// </summary>
		/// <param name="services">The service collection.</param>
		/// <param name="node">The node to connect to.</param>
		/// <param name="configure">Optionally configures the connection settings.</param>
		/// <param name="httpClientName">The name of the <see cref="HttpClient" /> to register and send requests with.</param>
		/// <returns>The builder for the named <see cref="HttpClient" />, to add message handlers, resilience etc.</returns>
		/// <seealso cref="AddOpenSearchClient(IServiceCollection, Func{IServiceProvider, IConnection, IConnectionSettingsValues}, string)" />
		public static IHttpClientBuilder AddOpenSearchClient(this IServiceCollection services, Uri node,
			Action<IServiceProvider, ConnectionSettings> configure = null, string httpClientName = DefaultHttpClientName
		)
		{
			if (node == null) throw new ArgumentNullException(nameof(node));

			return services.AddOpenSearchClient((serviceProvider, connection) =>
			{
				var settings = new ConnectionSettings(node, connection);
				configure?.Invoke(serviceProvider, settings);
				return settings;
			}, httpClientName);
		}

		/// <summary>
		/// Registers a singleton <see cref="IOpenSearchClient" /> (and its <see cref="IOpenSearchLowLevelClient" />) using the settings
		/// from <paramref name="settingsFactory" />, which must pass the given <see cref="IConnection" /> to its
		/// <see cref="ConnectionSettings" /> for requests to go through the named <see cref="HttpClient" /> <paramref name="httpClientName" />.
		/// <para>
		/// The named client's primary handler is created from the connection settings (compression, connection limit, proxy,
		/// certificates), its handler lifetime defaults to <see cref="IConnectionConfigurationValues.DnsRefreshTimeout" />, and its
		/// <see cref="HttpClient.Timeout" /> is disabled in favour of <see cref="IConnectionConfigurationValues.RequestTimeout" />.
		/// Each of these can be overridden on the returned builder.
		/// </para>
		/// <para>
		/// The client already retries and fails over across nodes, see <see cref="IConnectionConfigurationValues.MaxRetries" />.
		/// Resilience handlers added to the builder retry within a single attempt on top of that.
		/// </para>
		/// </summary>
		/// <param name="services">The service collection.</param>
		/// <param name="settingsFactory">Creates the connection settings, given the service provider and the connection to use.</param>
		/// <param name="httpClientName">The name of the <see cref="HttpClient" /> to register and send requests with.</param>
		/// <returns>The builder for the named <see cref="HttpClient" />, to add message handlers, resilience etc.</returns>
		/// <example>
		/// <code>
		/// services.AddOpenSearchClient((sp, connection) =>
		///         new ConnectionSettings(new StaticConnectionPool(nodes), connection).DefaultIndex("my-index"))
		///     .AddHttpMessageHandler&lt;MyTracingHandler&gt;();
		/// </code>
		/// </example>
		public static IHttpClientBuilder AddOpenSearchClient(this IServiceCollection services,
			Func<IServiceProvider, IConnection, IConnectionSettingsValues> settingsFactory, string httpClientName = DefaultHttpClientName
		)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			if (settingsFactory == null) throw new ArgumentNullException(nameof(settingsFactory));
			if (httpClientName == null) throw new ArgumentNullException(nameof(httpClientName));

			services.AddSingleton(serviceProvider =>
			{
				var connection = new HttpClientFactoryConnection(serviceProvider.GetRequiredService<IHttpClientFactory>(), httpClientName);
				return settingsFactory(serviceProvider, connection)
					?? throw new InvalidOperationException($"The {nameof(settingsFactory)} passed to {nameof(AddOpenSearchClient)} returned null.");
			});
			services.AddSingleton<IOpenSearchClient>(serviceProvider => new OpenSearchClient(serviceProvider.GetRequiredService<IConnectionSettingsValues>()));
			services.AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IOpenSearchClient>().LowLevel);

			// Registered before AddHttpClient so that SetHandlerLifetime on the returned builder takes precedence.
			// The settings are resolved when the options are evaluated rather than as a Configure<TDep> dependency: building them needs
			// IHttpClientFactory, which itself depends on these options, so an eager dependency deadlocks the container.
			services.AddOptions<HttpClientFactoryOptions>(httpClientName)
				.Configure<IServiceProvider>((options, serviceProvider) =>
					options.HandlerLifetime = serviceProvider.GetRequiredService<IConnectionSettingsValues>().DnsRefreshTimeout);

			return services.AddHttpClient(httpClientName)
				.ConfigureHttpClient(client => client.Timeout = Timeout.InfiniteTimeSpan)
				.ConfigurePrimaryHttpMessageHandler(serviceProvider =>
					ConnectionSettingsHttpMessageHandler.Create(serviceProvider.GetRequiredService<IConnectionSettingsValues>()));
		}
	}
}
