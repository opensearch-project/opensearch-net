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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.Diagnostics;
using OpenSearch.Net.Extensions;
using static System.Net.DecompressionMethods;

namespace OpenSearch.Net
{
	internal class WebProxy : IWebProxy
	{
		private readonly Uri _uri;

		public WebProxy(Uri uri) => _uri = uri;

		public ICredentials Credentials { get; set; }

		public Uri GetProxy(Uri destination) => _uri;

		public bool IsBypassed(Uri host) => host.IsLoopback;
	}

	/// <summary> The default IConnection implementation. Uses <see cref="HttpClient" />.</summary>
	public class HttpConnection : IConnection
	{
		private static DiagnosticSource DiagnosticSource { get; } = new DiagnosticListener(DiagnosticSources.HttpConnection.SourceName);

		private static readonly string MissingConnectionLimitMethodError =
			$"Your target platform does not support {nameof(ConnectionConfiguration.ConnectionLimit)}"
			+ $" please set {nameof(ConnectionConfiguration.ConnectionLimit)} to -1 on your connection configuration/settings."
			+ $" this will cause the {nameof(HttpClientHandler.MaxConnectionsPerServer)} not to be set on {nameof(HttpClientHandler)}";

		private static readonly System.Net.Http.HttpMethod Patch = new System.Net.Http.HttpMethod("PATCH");

		/// <summary> Null when the <see cref="HttpClient" /> is supplied by the caller. </summary>
		private RequestDataHttpClientFactory HttpClientFactory { get; }

		/// <summary> Null when the <see cref="HttpClient" /> is created and owned by this connection. </summary>
		private readonly Func<RequestData, HttpClient> _httpClientProvider;

		public int InUseHandlers => HttpClientFactory?.InUseHandlers ?? 0;
		public int RemovedHandlers => HttpClientFactory?.RemovedHandlers ?? 0;

		public HttpConnection() => HttpClientFactory = new RequestDataHttpClientFactory(r => CreateHttpClientHandler(r));

		/// <summary>
		/// Creates a connection that sends every request through the supplied <paramref name="httpClient" />.
		/// <para>See <see cref="HttpConnection(Func{RequestData, HttpClient})" /> for the responsibilities this places on the caller.</para>
		/// </summary>
		/// <param name="httpClient">The client to send requests with. It is not disposed by this connection.</param>
		public HttpConnection(HttpClient httpClient) : this(CreateProvider(httpClient)) { }

		/// <summary>
		/// Creates a connection that asks <paramref name="httpClientProvider" /> for the <see cref="HttpClient" /> to send each request with.
		/// <para>
		/// The caller owns the returned clients and their handlers, so the handler-level settings this connection would otherwise
		/// apply in <see cref="CreateHttpClientHandler" /> must be configured on the caller's handler instead:
		/// <see cref="IConnectionConfigurationValues.EnableHttpCompression" /> (response decompression),
		/// <see cref="IConnectionConfigurationValues.ConnectionLimit" />, proxy settings, client certificates,
		/// <see cref="IConnectionConfigurationValues.ServerCertificateValidationCallback" /> and
		/// <see cref="IConnectionConfigurationValues.DnsRefreshTimeout" /> (handler lifetime). <see cref="CreateDefaultHttpMessageHandler" />
		/// creates a handler configured from the settings. Since this connection creates no handlers, per request
		/// <see cref="IRequestConfiguration.ClientCertificates" /> are not applied, and neither is an override of
		/// <see cref="CreateHttpClientHandler" /> in a subclass.
		/// </para>
		/// <para>
		/// Exceptions thrown by the client's handler pipeline, such as a resilience handler's timeout or open circuit, are reported as a
		/// failed call to the node, so the request fails over to the next node like any other transport failure.
		/// </para>
		/// <para>
		/// <see cref="IConnectionConfigurationValues.RequestTimeout" /> is still honoured per request through cancellation, but the
		/// client's own <see cref="HttpClient.Timeout" /> applies as well and caps it: a longer request timeout never takes effect. Set
		/// <see cref="HttpClient.Timeout" /> to <see cref="Timeout.InfiniteTimeSpan" />, or use
		/// <see cref="HttpConnection(Func{RequestData, HttpMessageHandler})" /> to make the request timeout the only limit.
		/// </para>
		/// </summary>
		/// <param name="httpClientProvider">Returns the client to send a request with. Returned clients are not disposed by this connection.</param>
		public HttpConnection(Func<RequestData, HttpClient> httpClientProvider) =>
			_httpClientProvider = httpClientProvider ?? throw new ArgumentNullException(nameof(httpClientProvider));

		/// <summary>
		/// Creates a connection that asks <paramref name="httpMessageHandlerProvider" /> for the <see cref="HttpMessageHandler" /> to send
		/// each request through, e.g. <c>_ => httpMessageHandlerFactory.CreateHandler("opensearch")</c> to send through a named
		/// <c>IHttpClientFactory</c> client's handler pipeline.
		/// <para>
		/// Unlike with a supplied <see cref="HttpClient" />, no client level timeout applies, so
		/// <see cref="IConnectionConfigurationValues.RequestTimeout" /> and per request overrides of it are the only limit on a request.
		/// Client level configuration such as <see cref="HttpClient.DefaultRequestHeaders" /> is not applied either. Otherwise the
		/// responsibilities described on <see cref="HttpConnection(Func{RequestData, HttpClient})" /> apply.
		/// </para>
		/// </summary>
		/// <param name="httpMessageHandlerProvider">Returns the handler to send a request through. Returned handlers are not disposed by this connection.</param>
		public HttpConnection(Func<RequestData, HttpMessageHandler> httpMessageHandlerProvider)
			: this(CreateProvider(httpMessageHandlerProvider)) { }

		private static Func<RequestData, HttpClient> CreateProvider(HttpClient httpClient)
		{
			if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

			return _ => httpClient;
		}

		private static Func<RequestData, HttpClient> CreateProvider(Func<RequestData, HttpMessageHandler> httpMessageHandlerProvider)
		{
			if (httpMessageHandlerProvider == null) throw new ArgumentNullException(nameof(httpMessageHandlerProvider));

			// Clients are cheap to create around a handler, the same as IHttpClientFactory does. With no client timeout, the request
			// timeout enforced through cancellation is the only one.
			return requestData =>
			{
				var handler = httpMessageHandlerProvider(requestData)
					?? throw new InvalidOperationException(
						$"The {nameof(HttpMessageHandler)} provider passed to {nameof(HttpConnection)} returned null.");
				return new HttpClient(handler, disposeHandler: false) { Timeout = Timeout.InfiniteTimeSpan };
			};
		}

		public virtual TResponse Request<TResponse>(RequestData requestData)
			where TResponse : class, IOpenSearchResponse, new()
		{
			var client = GetClient(requestData);
			HttpResponseMessage responseMessage;
			int? statusCode = null;
			IEnumerable<string> warnings = null;
			Stream responseStream = null;
			Exception ex = null;
			string mimeType = null;
			IDisposable receive = DiagnosticSources.SingletonDisposable;
			ReadOnlyDictionary<TcpState, int> tcpStats = null;
			ReadOnlyDictionary<string, ThreadPoolStatistics> threadPoolStats = null;

			try
			{
				var requestMessage = CreateHttpRequestMessage(requestData);

				if (requestData.PostData != null)
					SetContent(requestMessage, requestData);

				using(requestMessage?.Content ?? (IDisposable)Stream.Null)
				using (var d = DiagnosticSource.Diagnose<RequestData, int?>(DiagnosticSources.HttpConnection.SendAndReceiveHeaders, requestData))
				{
					if (requestData.TcpStats)
						tcpStats = TcpStats.GetStates();

					if (requestData.ThreadPoolStats)
						threadPoolStats = ThreadPoolStats.GetStats();

					responseMessage = SendAsync(client, requestMessage, requestData, CancellationToken.None).GetAwaiter().GetResult();
					statusCode = (int)responseMessage.StatusCode;
					d.EndState = statusCode;
				}

				requestData.MadeItToResponse = true;
				responseMessage.Headers.TryGetValues("Warning", out warnings);
				mimeType = responseMessage.Content.Headers.ContentType?.MediaType;

				if (responseMessage.Content != null)
				{
					receive = DiagnosticSource.Diagnose(DiagnosticSources.HttpConnection.ReceiveBody, requestData, statusCode);
					responseStream = responseMessage.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
				}
			}
			catch (TaskCanceledException e)
			{
				ex = e;
			}
			catch (HttpRequestException e)
			{
				ex = e;
			}
			using(receive)
			using (responseStream ??= Stream.Null)
			{
				var response = ResponseBuilder.ToResponse<TResponse>(requestData, ex, statusCode, warnings, responseStream, mimeType);

				// set TCP and threadpool stats on the response here so that in the event the request fails after the point of
				// gathering stats, they are still exposed on the call details. Ideally these would be set inside ResponseBuilder.ToResponse,
				// but doing so would be a breaking change
				response.ApiCall.TcpStats = tcpStats;
				response.ApiCall.ThreadPoolStats = threadPoolStats;
				return response;
			}
		}

		public virtual async Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
			where TResponse : class, IOpenSearchResponse, new()
		{
			var client = GetClient(requestData);
			HttpResponseMessage responseMessage;
			int? statusCode = null;
			IEnumerable<string> warnings = null;
			Stream responseStream = null;
			Exception ex = null;
			string mimeType = null;
			IDisposable receive = DiagnosticSources.SingletonDisposable;
			ReadOnlyDictionary<TcpState, int> tcpStats = null;
			ReadOnlyDictionary<string, ThreadPoolStatistics> threadPoolStats = null;
			requestData.IsAsync = true;

			try
			{
				var requestMessage = CreateHttpRequestMessage(requestData);

				if (requestData.PostData != null)
					await SetContentAsync(requestMessage, requestData, cancellationToken).ConfigureAwait(false);

				using(requestMessage?.Content ?? (IDisposable)Stream.Null)
				using (var d = DiagnosticSource.Diagnose<RequestData, int?>(DiagnosticSources.HttpConnection.SendAndReceiveHeaders, requestData))
				{
					if (requestData.TcpStats)
						tcpStats = TcpStats.GetStates();

					if (requestData.ThreadPoolStats)
						threadPoolStats = ThreadPoolStats.GetStats();

					responseMessage = await SendAsync(client, requestMessage, requestData, cancellationToken).ConfigureAwait(false);
					statusCode = (int)responseMessage.StatusCode;
					d.EndState = statusCode;
				}

				requestData.MadeItToResponse = true;
				mimeType = responseMessage.Content.Headers.ContentType?.MediaType;
				responseMessage.Headers.TryGetValues("Warning", out warnings);

				if (responseMessage.Content != null)
				{
					receive = DiagnosticSource.Diagnose(DiagnosticSources.HttpConnection.ReceiveBody, requestData, statusCode);
					responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
				}
			}
			catch (TaskCanceledException e)
			{
				ex = e;
			}
			catch (HttpRequestException e)
			{
				ex = e;
			}
			using (receive)
			using (responseStream = responseStream ?? Stream.Null)
			{
				var response = await ResponseBuilder.ToResponseAsync<TResponse>
						(requestData, ex, statusCode, warnings, responseStream, mimeType, cancellationToken)
					.ConfigureAwait(false);

				// set TCP and threadpool stats on the response here so that in the event the request fails after the point of
				// gathering stats, they are still exposed on the call details. Ideally these would be set inside ResponseBuilder.ToResponse,
				// but doing so would be a breaking change
				response.ApiCall.TcpStats = tcpStats;
				response.ApiCall.ThreadPoolStats = threadPoolStats;
				return response;
			}
		}

		void IDisposable.Dispose() => DisposeManagedResources();

		private HttpClient GetClient(RequestData requestData)
		{
			if (_httpClientProvider == null)
				return HttpClientFactory.CreateClient(requestData);

			return _httpClientProvider(requestData)
				?? throw new InvalidOperationException($"The {nameof(HttpClient)} provider passed to {nameof(HttpConnection)} returned null.");
		}

		private async Task<HttpResponseMessage> SendAsync(HttpClient client, HttpRequestMessage requestMessage, RequestData requestData,
			CancellationToken cancellationToken
		)
		{
			using (var timeout = CreateRequestTimeout(requestData, cancellationToken))
			{
				try
				{
					return await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, timeout?.Token ?? cancellationToken)
						.ConfigureAwait(false);
				}
				// A caller supplied pipeline can fail with any exception, e.g. a resilience handler's timeout or open circuit. Surface it
				// like a transport failure so the node is marked dead and the request fails over, rather than escaping the pipeline.
				catch (Exception e) when (_httpClientProvider != null && !(e is HttpRequestException || e is TaskCanceledException)
					&& !cancellationToken.IsCancellationRequested)
				{
					throw new HttpRequestException(e.Message, e);
				}
			}
		}

		/// <summary>
		/// Enforces <see cref="RequestData.RequestTimeout" /> on a caller supplied <see cref="HttpClient" />, whose
		/// <see cref="HttpClient.Timeout" /> can't be changed per request. Owned clients get it via <see cref="HttpClient.Timeout" />
		/// already, so this returns null for them, as it does when there is no timeout.
		/// </summary>
		private CancellationTokenSource CreateRequestTimeout(RequestData requestData, CancellationToken cancellationToken)
		{
			if (_httpClientProvider == null || requestData.RequestTimeout <= TimeSpan.Zero) return null;

			var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			timeout.CancelAfter(requestData.RequestTimeout);
			return timeout;
		}

		protected virtual HttpMessageHandler CreateHttpClientHandler(RequestData requestData) =>
			BuildHttpMessageHandler(requestData.ConnectionSettings, requestData.ClientCertificates);

		/// <summary>
		/// Creates the handler this connection would create for <paramref name="settings" />, applying response decompression,
		/// <see cref="IConnectionConfigurationValues.ConnectionLimit" />, proxy settings, server certificate validation and client certificates.
		/// Useful as the primary handler of an <see cref="HttpClient" /> passed to <see cref="HttpConnection(Func{RequestData, HttpClient})" />.
		/// </summary>
		public static HttpMessageHandler CreateDefaultHttpMessageHandler(IConnectionConfigurationValues settings)
		{
			if (settings == null)
				throw new ArgumentNullException(nameof(settings));

			return BuildHttpMessageHandler(settings, settings.ClientCertificates);
		}

		private static HttpMessageHandler BuildHttpMessageHandler(IConnectionConfigurationValues settings, X509CertificateCollection clientCertificates)
		{
#if NETCOREAPP2_1_OR_GREATER
			var handler = new SocketsHttpHandler { AutomaticDecompression = settings.EnableHttpCompression ? GZip | Deflate : None, };
#else
			var handler = new HttpClientHandler { AutomaticDecompression = settings.EnableHttpCompression ? GZip | Deflate : None, };
#endif
			// same limit as desktop clr
			if (settings.ConnectionLimit > 0)
			{
				try
				{
					handler.MaxConnectionsPerServer = settings.ConnectionLimit;
				}
				catch (MissingMethodException e)
				{
					throw new Exception(MissingConnectionLimitMethodError, e);
				}
				catch (PlatformNotSupportedException e)
				{
					throw new Exception(MissingConnectionLimitMethodError, e);
				}
			}

			if (!settings.ProxyAddress.IsNullOrEmpty())
			{
				var uri = new Uri(settings.ProxyAddress);
				var proxy = new WebProxy(uri);
				if (!string.IsNullOrEmpty(settings.ProxyUsername))
				{
					var credentials = new NetworkCredential(settings.ProxyUsername, settings.ProxyPassword);
					proxy.Credentials = credentials;
				}
				handler.Proxy = proxy;
			}
			else if (settings.DisableAutomaticProxyDetection) handler.UseProxy = false;

			var callback = settings.ServerCertificateValidationCallback;

#if NETCOREAPP2_1_OR_GREATER
			if (callback != null && handler.SslOptions.RemoteCertificateValidationCallback == null)
				handler.SslOptions.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors)
					=> callback(sender, certificate, chain, sslPolicyErrors);

			if (clientCertificates != null)
			{
				handler.SslOptions.ClientCertificates?.AddRange(clientCertificates);
			}
#else
			if (callback != null && handler.ServerCertificateCustomValidationCallback == null)
				handler.ServerCertificateCustomValidationCallback = callback;

			if (clientCertificates != null)
			{
				handler.ClientCertificateOptions = ClientCertificateOption.Manual;
				handler.ClientCertificates.AddRange(clientCertificates);
			}
#endif

			return handler;
		}

		protected virtual HttpRequestMessage CreateHttpRequestMessage(RequestData requestData)
		{
			var request = CreateRequestMessage(requestData);
			SetAuthenticationIfNeeded(request, requestData);
			return request;
		}

		protected virtual void SetAuthenticationIfNeeded(HttpRequestMessage requestMessage, RequestData requestData)
		{
			//If user manually specifies an Authorization Header give it preference
			if (requestData.Headers.HasKeys() && requestData.Headers.AllKeys.Contains("Authorization"))
			{
				var header = AuthenticationHeaderValue.Parse(requestData.Headers["Authorization"]);
				requestMessage.Headers.Authorization = header;
				return;
			}

			// Api Key authentication takes precedence
			var apiKeySet = SetApiKeyAuthenticationIfNeeded(requestMessage, requestData);

			if (!apiKeySet)
				SetBasicAuthenticationIfNeeded(requestMessage, requestData);
		}

		protected virtual bool SetApiKeyAuthenticationIfNeeded(HttpRequestMessage requestMessage, RequestData requestData)
		{
			// ApiKey auth credentials take the following precedence (highest -> lowest):
			// 1 - Specified on the request (highest precedence)
			// 2 - Specified at the global IConnectionSettings level


			string apiKey = null;
			if (requestData.ApiKeyAuthenticationCredentials != null)
				apiKey = requestData.ApiKeyAuthenticationCredentials.Base64EncodedApiKey.CreateString();

			if (string.IsNullOrWhiteSpace(apiKey))
				return false;

			requestMessage.Headers.Authorization = new AuthenticationHeaderValue("ApiKey", apiKey);
			return true;

		}

		protected virtual void SetBasicAuthenticationIfNeeded(HttpRequestMessage requestMessage, RequestData requestData)
		{
			// Basic auth credentials take the following precedence (highest -> lowest):
			// 1 - Specified on the request (highest precedence)
			// 2 - Specified at the global IConnectionSettings level
			// 3 - Specified with the URI (lowest precedence)

			string userInfo = null;
			if (!requestData.Uri.UserInfo.IsNullOrEmpty())
				userInfo = Uri.UnescapeDataString(requestData.Uri.UserInfo);
			else if (requestData.BasicAuthorizationCredentials != null)
				userInfo =
					$"{requestData.BasicAuthorizationCredentials.Username}:{requestData.BasicAuthorizationCredentials.Password.CreateString()}";
			if (!userInfo.IsNullOrEmpty())
			{
				var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(userInfo));
				requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
			}
		}

		protected virtual HttpRequestMessage CreateRequestMessage(RequestData requestData)
		{
			var method = ConvertHttpMethod(requestData.Method);
			var requestMessage = new HttpRequestMessage(method, requestData.Uri);

			if (requestData.Headers != null)
				foreach (string key in requestData.Headers)
					requestMessage.Headers.TryAddWithoutValidation(key, requestData.Headers.GetValues(key));

			requestMessage.Headers.Connection.Clear();
			requestMessage.Headers.ConnectionClose = false;
			requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(requestData.Accept));

			if (!string.IsNullOrWhiteSpace(requestData.UserAgent))
			{
				requestMessage.Headers.UserAgent.Clear();
				requestMessage.Headers.UserAgent.TryParseAdd(requestData.UserAgent);
			}

			if (!requestData.RunAs.IsNullOrEmpty())
				requestMessage.Headers.Add(RequestData.RunAsSecurityHeader, requestData.RunAs);

			if (requestData.MetaHeaderProvider is object)
			{
				var value = requestData.MetaHeaderProvider.ProduceHeaderValue(requestData);

				if (!string.IsNullOrEmpty(value))
					requestMessage.Headers.TryAddWithoutValidation(requestData.MetaHeaderProvider.HeaderName, value);
			}

			return requestMessage;
		}

		private static void SetContent(HttpRequestMessage message, RequestData requestData)
		{
			if (requestData.TransferEncodingChunked)
				message.Content = new RequestDataContent(requestData);
			else
			{
				var stream = requestData.MemoryStreamFactory.Create();
				if (requestData.HttpCompression)
					using (var zipStream = new GZipStream(stream, CompressionMode.Compress, true))
						requestData.PostData.Write(zipStream, requestData.ConnectionSettings);
				else
					requestData.PostData.Write(stream, requestData.ConnectionSettings);

				// the written bytes are uncompressed, so can only be used when http compression isn't used
				if (requestData.PostData.DisableDirectStreaming.GetValueOrDefault(false) && !requestData.HttpCompression)
				{
					message.Content = new ByteArrayContent(requestData.PostData.WrittenBytes);
					stream.Dispose();
				}
				else
				{
					stream.Position = 0;
					message.Content = new StreamContent(stream);
				}

				if (requestData.HttpCompression)
					message.Content.Headers.ContentEncoding.Add("gzip");

				message.Content.Headers.ContentType = new MediaTypeHeaderValue(requestData.RequestMimeType);
			}
		}

		private static async Task SetContentAsync(HttpRequestMessage message, RequestData requestData, CancellationToken cancellationToken)
		{
			if (requestData.TransferEncodingChunked)
				message.Content = new RequestDataContent(requestData, cancellationToken);
			else
			{
				var stream = requestData.MemoryStreamFactory.Create();
				if (requestData.HttpCompression)
					using (var zipStream = new GZipStream(stream, CompressionMode.Compress, true))
						await requestData.PostData.WriteAsync(zipStream, requestData.ConnectionSettings, cancellationToken).ConfigureAwait(false);
				else
					await requestData.PostData.WriteAsync(stream, requestData.ConnectionSettings, cancellationToken).ConfigureAwait(false);

				// the written bytes are uncompressed, so can only be used when http compression isn't used
				if (requestData.PostData.DisableDirectStreaming.GetValueOrDefault(false) && !requestData.HttpCompression)
				{
					message.Content = new ByteArrayContent(requestData.PostData.WrittenBytes);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
						await stream.DisposeAsync().ConfigureAwait(false);

#else
						stream.Dispose();
#endif
				}
				else
				{
					stream.Position = 0;
					message.Content = new StreamContent(stream);
				}

				if (requestData.HttpCompression)
					message.Content.Headers.ContentEncoding.Add("gzip");

				message.Content.Headers.ContentType = new MediaTypeHeaderValue(requestData.RequestMimeType);
			}
		}

		private static System.Net.Http.HttpMethod ConvertHttpMethod(HttpMethod httpMethod) =>
			httpMethod switch
			{
				HttpMethod.GET => System.Net.Http.HttpMethod.Get,
				HttpMethod.POST => System.Net.Http.HttpMethod.Post,
				HttpMethod.PUT => System.Net.Http.HttpMethod.Put,
				HttpMethod.DELETE => System.Net.Http.HttpMethod.Delete,
				HttpMethod.HEAD => System.Net.Http.HttpMethod.Head,
				HttpMethod.PATCH => Patch,
				_ => throw new ArgumentException("Invalid value for HttpMethod", nameof(httpMethod))
			};

		internal static int GetClientKey(RequestData requestData)
		{
			unchecked
			{
				var hashCode = requestData.RequestTimeout.GetHashCode();
				hashCode = (hashCode * 397) ^ requestData.HttpCompression.GetHashCode();
				hashCode = (hashCode * 397) ^ (requestData.ProxyAddress?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (requestData.ProxyUsername?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (requestData.ProxyPassword?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ requestData.DisableAutomaticProxyDetection.GetHashCode();
				return hashCode;
			}
		}

		protected virtual void DisposeManagedResources() => HttpClientFactory?.Dispose();
	}
}
