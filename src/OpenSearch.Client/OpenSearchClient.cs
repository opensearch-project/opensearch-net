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
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace OpenSearch.Client
{

    public class NamespacedClientProxy
    {
        private readonly OpenSearchClient _client;

        protected NamespacedClientProxy(OpenSearchClient client) => _client = client;

        internal TResponse DoRequest<TRequest, TResponse>(
            TRequest request,
            IRequestParameters parameters,
            Action<IRequestConfiguration> forceConfiguration = null
        )
            where TRequest : class, IRequest
            where TResponse : class, IOpenSearchResponse, new() =>
            _client.DoRequest<TRequest, TResponse>(request, parameters, forceConfiguration);

        internal Task<TResponse> DoRequestAsync<TRequest, TResponse>(
            TRequest request,
            IRequestParameters parameters,
            CancellationToken ct,
            Action<IRequestConfiguration> forceConfiguration = null
        )
            where TRequest : class, IRequest
            where TResponse : class, IOpenSearchResponse, new() =>
            _client.DoRequestAsync<TRequest, TResponse>(request, parameters, ct, forceConfiguration);

        internal TResponse DoRequest<TRequest, TResponse>(
            TRequest request,
            IRequestParameters parameters,
            Func<TRequest, PostData> bodySelector,
            Action<IRequestConfiguration> forceConfiguration = null
        )
            where TRequest : class, IRequest
            where TResponse : class, IOpenSearchResponse, new() =>
            _client.DoRequest<TRequest, TResponse>(request, parameters, bodySelector, forceConfiguration);

        internal Task<TResponse> DoRequestAsync<TRequest, TResponse>(
            TRequest request,
            IRequestParameters parameters,
            Func<TRequest, PostData> bodySelector,
            CancellationToken ct,
            Action<IRequestConfiguration> forceConfiguration = null
        )
            where TRequest : class, IRequest
            where TResponse : class, IOpenSearchResponse, new() =>
            _client.DoRequestAsync<TRequest, TResponse>(request, parameters, bodySelector, ct, forceConfiguration);

        protected CatResponse<TCatRecord> DoCat<TRequest, TParams, TCatRecord>(TRequest request)
            where TCatRecord : ICatRecord
            where TParams : RequestParameters<TParams>, new()
            where TRequest : class, IRequest<TParams>
        {
            if (typeof(TCatRecord) == typeof(CatHelpRecord))
            {
                request.RequestParameters.CustomResponseBuilder = CatHelpResponseBuilder.Instance;
                return DoRequest<TRequest, CatResponse<TCatRecord>>(request, request.RequestParameters, r => OpenSearchClient.ForceTextPlain(r));
            }
            request.RequestParameters.CustomResponseBuilder = CatResponseBuilder<TCatRecord>.Instance;
            return DoRequest<TRequest, CatResponse<TCatRecord>>(request, request.RequestParameters, r => OpenSearchClient.ForceJson(r));
        }

        protected Task<CatResponse<TCatRecord>> DoCatAsync<TRequest, TParams, TCatRecord>(TRequest request, CancellationToken ct)
            where TCatRecord : ICatRecord
            where TParams : RequestParameters<TParams>, new()
            where TRequest : class, IRequest<TParams>
        {
            if (typeof(TCatRecord) == typeof(CatHelpRecord))
            {
                request.RequestParameters.CustomResponseBuilder = CatHelpResponseBuilder.Instance;
                return DoRequestAsync<TRequest, CatResponse<TCatRecord>>(request, request.RequestParameters, ct, r => OpenSearchClient.ForceTextPlain(r));
            }
            request.RequestParameters.CustomResponseBuilder = CatResponseBuilder<TCatRecord>.Instance;
            return DoRequestAsync<TRequest, CatResponse<TCatRecord>>(request, request.RequestParameters, ct, r => OpenSearchClient.ForceJson(r));
        }
    }
    /// <summary>
    /// OpenSearchClient is a strongly typed client which exposes fully mapped OpenSearch endpoints
    /// </summary>
    public partial class OpenSearchClient : IOpenSearchClient
    {
        public OpenSearchClient() : this(new ConnectionSettings(new Uri("http://localhost:9200"))) { }

        public OpenSearchClient(Uri uri) : this(new ConnectionSettings(uri)) { }

        /// <summary>
        /// Sets up the client to communicate to OpenSearch Cloud using <paramref name="cloudId"/>,
        /// <para><see cref="CloudConnectionPool"/> documentation for more information on how to obtain your Cloud Id</para>
        /// <para></para>If you want more control use the <see cref="OpenSearchClient(IConnectionSettingsValues)"/> constructor and pass an instance of
        /// <see cref="ConnectionSettings" /> that takes <paramref name="cloudId"/> in its constructor as well
        /// </summary>
        public OpenSearchClient(string cloudId, BasicAuthenticationCredentials credentials) : this(new ConnectionSettings(cloudId, credentials)) { }

        /// <summary>
        /// Sets up the client to communicate to OpenSearch Cloud using <paramref name="cloudId"/>,
        /// <para><see cref="CloudConnectionPool"/> documentation for more information on how to obtain your Cloud Id</para>
        /// <para></para>If you want more control use the <see cref="OpenSearchClient(IConnectionSettingsValues)"/> constructor and pass an instance of
        /// <see cref="ConnectionSettings" /> that takes <paramref name="cloudId"/> in its constructor as well
        /// </summary>
        public OpenSearchClient(string cloudId, ApiKeyAuthenticationCredentials credentials) : this(new ConnectionSettings(cloudId, credentials)) { }

        public OpenSearchClient(IConnectionSettingsValues connectionSettings)
            : this(new Transport<IConnectionSettingsValues>(connectionSettings ?? new ConnectionSettings())) { }

        public OpenSearchClient(ITransport<IConnectionSettingsValues> transport)
        {
            transport.ThrowIfNull(nameof(transport));
            transport.Settings.ThrowIfNull(nameof(transport.Settings));
            transport.Settings.RequestResponseSerializer.ThrowIfNull(nameof(transport.Settings.RequestResponseSerializer));
            transport.Settings.Inferrer.ThrowIfNull(nameof(transport.Settings.Inferrer));

            Transport = transport;
            LowLevel = new OpenSearchLowLevelClient(Transport);
            SetupNamespaces();
            SetupGeneratedNamespaces();
        }

        partial void SetupNamespaces();
        partial void SetupGeneratedNamespaces();

        public IConnectionSettingsValues ConnectionSettings => Transport.Settings;
        public Inferrer Infer => Transport.Settings.Inferrer;

        public IOpenSearchLowLevelClient LowLevel { get; }
        public IOpenSearchSerializer RequestResponseSerializer => Transport.Settings.RequestResponseSerializer;
        public IOpenSearchSerializer SourceSerializer => Transport.Settings.SourceSerializer;

        private ITransport<IConnectionSettingsValues> Transport { get; }

        internal TResponse DoRequest<TRequest, TResponse>(
            TRequest request,
            IRequestParameters parameters,
            Action<IRequestConfiguration> forceConfiguration = null
        )
            where TRequest : class, IRequest
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequest<TRequest, TResponse>(request, parameters, PostData.Serializable, forceConfiguration);

        internal Task<TResponse> DoRequestAsync<TRequest, TResponse>(
            TRequest request,
            IRequestParameters parameters,
            CancellationToken ct,
            Action<IRequestConfiguration> forceConfiguration = null
        )
            where TRequest : class, IRequest
            where TResponse : class, IOpenSearchResponse, new() =>
            DoRequestAsync<TRequest, TResponse>(request, parameters, PostData.Serializable, ct, forceConfiguration);

        internal TResponse DoRequest<TRequest, TResponse>(
            TRequest request,
            IRequestParameters parameters,
            Func<TRequest, PostData> bodySelector,
            Action<IRequestConfiguration> forceConfiguration = null
        )
            where TRequest : class, IRequest
            where TResponse : class, IOpenSearchResponse, new()
        {
            var (method, url, body) = PrepareRequest(request, parameters, bodySelector, forceConfiguration);
            return LowLevel.DoRequest<TResponse>(method, url, body, parameters);
        }

        internal Task<TResponse> DoRequestAsync<TRequest, TResponse>(
            TRequest request,
            IRequestParameters parameters,
            Func<TRequest, PostData> bodySelector,
            CancellationToken ct,
            Action<IRequestConfiguration> forceConfiguration = null
        )
            where TRequest : class, IRequest
            where TResponse : class, IOpenSearchResponse, new()
        {
            var (method, url, body) = PrepareRequest(request, parameters, bodySelector, forceConfiguration);
            return LowLevel.DoRequestAsync<TResponse>(method, url, ct, body, parameters);
        }

        private (HttpMethod method, string url, PostData body) PrepareRequest<TRequest>(
            TRequest request,
            IRequestParameters parameters,
            Func<TRequest, PostData> bodySelector,
            Action<IRequestConfiguration> forceConfiguration
        )
            where TRequest : class, IRequest
        {
            if (forceConfiguration != null) ForceConfiguration(request, forceConfiguration);
            if (request.ContentType != null) ForceContentType(request, request.ContentType);

            var url = request.GetUrl(ConnectionSettings);
            var method = request.HttpMethod;
            var body = !parameters.SupportsBody ? null : bodySelector(request);

            return (method, url, body);
        }

        private static void ForceConfiguration(IRequest request, Action<IRequestConfiguration> forceConfiguration)
        {
            if (forceConfiguration == null) return;

            var configuration = request.RequestParameters.RequestConfiguration ?? new RequestConfiguration();
            forceConfiguration(configuration);
            request.RequestParameters.RequestConfiguration = configuration;
        }
        private void ForceContentType<TRequest>(TRequest request, string contentType) where TRequest : class, IRequest
        {
            var configuration = request.RequestParameters.RequestConfiguration ?? new RequestConfiguration();
            configuration.Accept = contentType;
            configuration.ContentType = contentType;
            request.RequestParameters.RequestConfiguration = configuration;
        }

        internal static void ForceJson(IRequestConfiguration requestConfiguration)
        {
            requestConfiguration.Accept = RequestData.MimeType;
            requestConfiguration.ContentType = RequestData.MimeType;
        }
        internal static void ForceTextPlain(IRequestConfiguration requestConfiguration)
        {
            requestConfiguration.Accept = RequestData.MimeTypeTextPlain;
            requestConfiguration.ContentType = RequestData.MimeTypeTextPlain;
        }

        internal IRequestParameters ResponseBuilder(SourceRequestParameters parameters, CustomResponseBuilderBase builder)
        {
            parameters.CustomResponseBuilder = builder;
            return parameters;
        }
    }
}
