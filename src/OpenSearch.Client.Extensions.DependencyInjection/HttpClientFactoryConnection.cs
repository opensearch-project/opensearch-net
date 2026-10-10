/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Net.Http;
using OpenSearch.Net;

namespace OpenSearch.Client.Extensions.DependencyInjection
{
	/// <summary>
	/// An <see cref="HttpConnection" /> that sends requests through a named client from <see cref="IHttpClientFactory" />,
	/// leaving handler pooling, rotation and the message handler pipeline to the factory.
	/// <para>
	/// Handler-level <see cref="IConnectionConfigurationValues" /> such as compression, connection limit, proxy and certificates are
	/// not applied by this connection. Registering with
	/// <see cref="Microsoft.Extensions.DependencyInjection.OpenSearchServiceCollectionExtensions.AddOpenSearchClient(Microsoft.Extensions.DependencyInjection.IServiceCollection, Func{IServiceProvider, IConnection, IConnectionSettingsValues}, string)" />
	/// configures them on the named client's primary handler for you.
	/// </para>
	/// </summary>
	public class HttpClientFactoryConnection : HttpConnection
	{
		/// <param name="httpClientFactory">The factory to create clients from.</param>
		/// <param name="httpClientName">The name of the client configured with <c>AddHttpClient(name)</c>.</param>
		public HttpClientFactoryConnection(IHttpClientFactory httpClientFactory, string httpClientName)
			: base(CreateProvider(httpClientFactory, httpClientName)) { }

		private static Func<RequestData, HttpClient> CreateProvider(IHttpClientFactory httpClientFactory, string httpClientName)
		{
			if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));
			if (httpClientName == null) throw new ArgumentNullException(nameof(httpClientName));

			return _ => httpClientFactory.CreateClient(httpClientName);
		}
	}
}
