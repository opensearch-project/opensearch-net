/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Net;
using System.Net.Http;
using OpenSearch.Net;
using static System.Net.DecompressionMethods;

namespace OpenSearch.Client.Extensions.DependencyInjection
{
	/// <summary>
	/// Builds the primary handler for a factory-created client from connection settings, mirroring what
	/// <see cref="HttpConnection" /> applies to the handlers it creates itself.
	/// </summary>
	internal static class ConnectionSettingsHttpMessageHandler
	{
		public static HttpMessageHandler Create(IConnectionConfigurationValues settings)
		{
			var handler = new HttpClientHandler { AutomaticDecompression = settings.EnableHttpCompression ? GZip | Deflate : None };

			if (settings.ConnectionLimit > 0)
				handler.MaxConnectionsPerServer = settings.ConnectionLimit;

			if (!string.IsNullOrEmpty(settings.ProxyAddress))
			{
				var proxy = new WebProxy(new Uri(settings.ProxyAddress));
				if (!string.IsNullOrEmpty(settings.ProxyUsername))
					proxy.Credentials = new NetworkCredential(settings.ProxyUsername, settings.ProxyPassword);
				handler.Proxy = proxy;
			}
			else if (settings.DisableAutomaticProxyDetection) handler.UseProxy = false;

			if (settings.ServerCertificateValidationCallback != null)
				handler.ServerCertificateCustomValidationCallback = settings.ServerCertificateValidationCallback;

			if (settings.ClientCertificates != null)
			{
				handler.ClientCertificateOptions = ClientCertificateOption.Manual;
				handler.ClientCertificates.AddRange(settings.ClientCertificates);
			}

			return handler;
		}
	}
}
