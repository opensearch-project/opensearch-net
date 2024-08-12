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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net.Extensions;

namespace OpenSearch.Net;

/// <summary>
/// Low level client that exposes all of OpenSearch API endpoints but leaves you in charge of building request and handling the response
/// </summary>
// ReSharper disable once RedundantExtendsListEntry
public partial class OpenSearchLowLevelClient : IOpenSearchLowLevelClient
{
    /// <summary>Instantiate a new low level OpenSearch client to http://localhost:9200</summary>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public OpenSearchLowLevelClient() : this(new Transport<IConnectionConfigurationValues>(new ConnectionConfiguration())) { }

    /// <summary>Instantiate a new low level OpenSearch client using the specified settings</summary>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public OpenSearchLowLevelClient(IConnectionConfigurationValues settings) : this(
        new Transport<IConnectionConfigurationValues>(settings ?? new ConnectionConfiguration()))
    { }

    /// <summary>
    /// Sets up the client to communicate to OpenSearch Cloud using <paramref name="cloudId"/>,
    /// <para><see cref="CloudConnectionPool"/> documentation for more information on how to obtain your Cloud Id</para>
    /// <para></para>If you want more control use the <see cref="OpenSearchLowLevelClient(IConnectionConfigurationValues)"/> constructor and pass an instance of
    /// <see cref="ConnectionConfiguration" /> that takes <paramref name="cloudId"/> in its constructor as well
    /// </summary>
    public OpenSearchLowLevelClient(string cloudId, BasicAuthenticationCredentials credentials) : this(new ConnectionConfiguration(cloudId, credentials)) { }

    /// <summary>
    /// Sets up the client to communicate to OpenSearch Cloud using <paramref name="cloudId"/>,
    /// <para><see cref="CloudConnectionPool"/> documentation for more information on how to obtain your Cloud Id</para>
    /// <para></para>If you want more control use the <see cref="OpenSearchLowLevelClient(IConnectionConfigurationValues)"/> constructor and pass an instance of
    /// <see cref="ConnectionConfiguration" /> that takes <paramref name="cloudId"/> in its constructor as well
    /// </summary>
    public OpenSearchLowLevelClient(string cloudId, ApiKeyAuthenticationCredentials credentials) : this(new ConnectionConfiguration(cloudId, credentials)) { }

    /// <summary>
    /// Instantiate a new low level OpenSearch client explicitly specifying a custom transport setup
    /// </summary>
    public OpenSearchLowLevelClient(ITransport<IConnectionConfigurationValues> transport)
    {
        transport.ThrowIfNull(nameof(transport));
        transport.Settings.ThrowIfNull(nameof(transport.Settings));
        transport.Settings.RequestResponseSerializer.ThrowIfNull(nameof(transport.Settings.RequestResponseSerializer));

        Transport = transport;
        UrlFormatter = Transport.Settings.UrlFormatter;
        SetupNamespaces();
        SetupGeneratedNamespaces();
    }

    partial void SetupNamespaces();

    partial void SetupGeneratedNamespaces();

    public IOpenSearchSerializer Serializer => Transport.Settings.RequestResponseSerializer;

    public IConnectionConfigurationValues Settings => Transport.Settings;

    protected ITransport<IConnectionConfigurationValues> Transport { get; set; }

    private OpenSearchUrlFormatter UrlFormatter { get; }

    public TResponse DoRequest<TResponse>(HttpMethod method, string path, PostData data = null, IRequestParameters requestParameters = null)
        where TResponse : class, IOpenSearchResponse, new() =>
        Transport.Request<TResponse>(method, path, data, requestParameters);

    public Task<TResponse> DoRequestAsync<TResponse>(HttpMethod method, string path, CancellationToken cancellationToken, PostData data = null,
        IRequestParameters requestParameters = null
    )
        where TResponse : class, IOpenSearchResponse, new() =>
        Transport.RequestAsync<TResponse>(method, path, cancellationToken, data, requestParameters);

    protected internal string Url(FormattableString formattable) => formattable.ToString(UrlFormatter);

    protected internal TRequestParams RequestParams<TRequestParams>(TRequestParams requestParams, string contentType = null, string accept = null)
        where TRequestParams : class, IRequestParameters, new()
    {
        if (contentType.IsNullOrEmpty() && accept.IsNullOrEmpty()) return requestParams;

        requestParams ??= new TRequestParams();
        requestParams.RequestConfiguration ??= new RequestConfiguration();
        if (!contentType.IsNullOrEmpty() && requestParams.RequestConfiguration.ContentType.IsNullOrEmpty())
            requestParams.RequestConfiguration.ContentType = contentType;
        if (!accept.IsNullOrEmpty() && requestParams.RequestConfiguration.Accept.IsNullOrEmpty())
            requestParams.RequestConfiguration.Accept = accept;
        return requestParams;
    }
}
