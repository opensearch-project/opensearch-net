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
using System.Collections.Specialized;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace OpenSearch.Net;

public interface IConnectionConfigurationValues : IDisposable
{
    /// <summary>
    /// Basic access authorization credentials to specify with all requests.
    /// </summary>
    /// <remarks>
    /// Cannot be used in conjuction with <see cref="ApiKeyAuthenticationCredentials"/>
    /// </remarks>
    BasicAuthenticationCredentials BasicAuthenticationCredentials { get; }

    /// <summary>
    /// Api Key authorization credentials to specify with all requests.
    /// </summary>
    /// <remarks>
    /// Cannot be used in conjuction with <see cref="BasicAuthenticationCredentials"/>
    /// </remarks>
    ApiKeyAuthenticationCredentials ApiKeyAuthenticationCredentials { get; }

    /// <summary> Provides a semaphoreslim to transport implementations that need to limit access to a resource</summary>
    SemaphoreSlim BootstrapLock { get; }

    /// <summary>
    /// Use the following certificates to authenticate all HTTP requests. You can also set them on individual
    /// request using <see cref="RequestConfiguration.ClientCertificates" />
    /// </summary>
    X509CertificateCollection ClientCertificates { get; }

    /// <summary> The connection implementation to use when talking with OpenSearch </summary>
    IConnection Connection { get; }

    /// <summary>
    /// Limits the number of concurrent connections that can be opened to an endpoint. Defaults to 80 (see
    /// <see cref="ConnectionConfiguration.DefaultConnectionLimit" />).
    /// <para>
    /// For Desktop CLR, this setting applies to the DefaultConnectionLimit property on the  ServicePointManager object when creating
    /// ServicePoint objects, affecting the default <see cref="IConnection" /> implementation.
    /// </para>
    /// <para>
    /// For Core CLR, this setting applies to the MaxConnectionsPerServer property on the HttpClientHandler instances used by the HttpClient
    /// inside the default <see cref="IConnection" /> implementation
    /// </para>
    /// </summary>
    int ConnectionLimit { get; }

    /// <summary> The connection pool to use when talking with OpenSearch </summary>
    IConnectionPool ConnectionPool { get; }

    /// <summary>
    /// The time to put dead nodes out of rotation (this will be multiplied by the number of times they've been dead)
    /// </summary>
    TimeSpan? DeadTimeout { get; }

    /// <summary>
    /// Disabled proxy detection on the webrequest, in some cases this may speed up the first connection
    /// your appdomain makes, in other cases it will actually increase the time for the first connection.
    /// No silver bullet! use with care!
    /// </summary>
    bool DisableAutomaticProxyDetection { get; }

    /// <summary>
    /// When set to true will disable (de)serializing directly to the request and response stream and return a byte[]
    /// copy of the raw request and response on OpenSearch calls. Defaults to  false
    /// </summary>
    bool DisableDirectStreaming { get; }

    bool DisableMetaHeader { get; }

    /// <summary>
    /// This signals that we do not want to send initial pings to unknown/previously dead nodes
    /// and just send the call straightaway
    /// </summary>
    bool DisablePings { get; }

    /// <summary>
    /// Enable gzip compressed requests and responses, do note that you need to configure OpenSearch to set this
    /// <para></para>
    /// </summary>
    bool EnableHttpCompression { get; }

    /// <summary>
    /// Try to send these headers for every request
    /// </summary>
    NameValueCollection Headers { get; }

    /// <summary>
    /// Whether HTTP pipelining is enabled. The default is <c>true</c>
    /// </summary>
    bool HttpPipeliningEnabled { get; }

    /// <summary>
    /// KeepAliveInterval - specifies the interval, in milliseconds, between
    /// when successive keep-alive packets are sent if no acknowledgement is
    /// received.
    /// </summary>
    TimeSpan? KeepAliveInterval { get; }

    /// <summary>
    /// KeepAliveTime - specifies the timeout, in milliseconds, with no
    /// activity until the first keep-alive packet is sent.
    /// </summary>
    TimeSpan? KeepAliveTime { get; }

    /// <summary>
    /// The maximum amount of time a node is allowed to marked dead
    /// </summary>
    TimeSpan? MaxDeadTimeout { get; }

    /// <summary>
    /// When a retryable exception occurs or status code is returned this controls the maximum
    /// amount of times we should retry the call to OpenSearch
    /// </summary>
    int? MaxRetries { get; }

    /// <summary>
    /// Limits the total runtime including retries separately from <see cref="RequestTimeout" />
    /// <pre>
    /// When not specified defaults to <see cref="RequestTimeout" /> which itself defaults to 60 seconds
    /// </pre>
    /// </summary>
    TimeSpan? MaxRetryTimeout { get; }

    /// <summary> Provides a memory stream factory</summary>
    IMemoryStreamFactory MemoryStreamFactory { get; }

    /// <summary>
    /// Register a predicate to select which nodes that you want to execute API calls on. Note that sniffing requests omit this predicate and
    /// always execute on all nodes.
    /// When using an <see cref="IConnectionPool" /> implementation that supports reseeding of nodes, this will default to omitting cluster_manager only
    /// node from regular API calls.
    /// When using static or single node connection pooling it is assumed the list of node you instantiate the client with should be taken
    /// verbatim.
    /// </summary>
    Func<Node, bool> NodePredicate { get; }

    /// <summary>
    /// Allows you to register a callback every time a an API call is returned
    /// </summary>
    Action<IApiCallDetails> OnRequestCompleted { get; }

    /// <summary>
    /// An action to run when the <see cref="RequestData" /> for a request has been
    /// created.
    /// </summary>
    Action<RequestData> OnRequestDataCreated { get; }

    /// <summary>
    /// The timeout in milliseconds to use for ping requests, which are issued to determine whether a node is alive
    /// </summary>
    TimeSpan? PingTimeout { get; }

    /// <summary>
    /// Forces all requests to have ?pretty=true, causing OpenSearch to return formatted json.
    /// Also forces the client to send out formatted json. Defaults to false
    /// </summary>
    bool PrettyJson { get; }

    /// <summary>
    /// When set will force all connections through this proxy
    /// </summary>
    string ProxyAddress { get; }

    /// <summary>
    /// The password for the proxy, when configured
    /// </summary>
    SecureString ProxyPassword { get; }

    /// <summary>
    /// The username for the proxy, when configured
    /// </summary>
    string ProxyUsername { get; }

    /// <summary>
    /// Append these query string parameters automatically to every request
    /// </summary>
    NameValueCollection QueryStringParameters { get; }

    /// <summary>The serializer to use to serialize requests and deserialize responses</summary>
    IOpenSearchSerializer RequestResponseSerializer { get; }

    /// <summary>
    /// The timeout in milliseconds for each request to OpenSearch
    /// </summary>
    TimeSpan RequestTimeout { get; }

    /// <summary>
    /// Register a ServerCertificateValidationCallback per request
    /// </summary>
    Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> ServerCertificateValidationCallback { get; }

    /// <summary>
    /// Configure the client to skip deserialization of certain status codes e.g: you run OpenSearch behind a proxy that returns an unexpected
    /// json format
    /// </summary>
    IReadOnlyCollection<int> SkipDeserializationForStatusCodes { get; }

    /// <summary>
    /// Force a new sniff for the cluster when the cluster state information is older than
    /// the specified timespan
    /// </summary>
    TimeSpan? SniffInformationLifeSpan { get; }

    /// <summary>
    /// Force a new sniff for the cluster state every time a connection dies
    /// </summary>
    bool SniffsOnConnectionFault { get; }

    /// <summary>
    /// Sniff the cluster state immediately on startup
    /// </summary>
    bool SniffsOnStartup { get; }

    /// <summary>
    /// Instead of following a c/go like error checking on response.IsValid do throw an exception (except when <see cref="IApiCallDetails.SuccessOrKnownError"/> is false)
    /// on the client when a call resulted in an exception on either the client or the OpenSearch server.
    /// <para>Reasons for such exceptions could be search parser errors, index missing exceptions, etc...</para>
    /// </summary>
    bool ThrowExceptions { get; }

    OpenSearchUrlFormatter UrlFormatter { get; }

    /// <summary>
    /// The user agent string to send with requests. Useful for debugging purposes to understand client and framework
    /// versions that initiate requests to OpenSearch
    /// </summary>
    string UserAgent { get; }

    /// <summary>
    /// Allow you to override the status code inspection that sets <see cref="OpenSearchResponseBase.Success"/>
    /// <para>
    /// Defaults to validating the statusCode is greater or equal to 200 and less then 300
    /// </para>
    /// <para>
    /// When the request is using <see cref="HttpMethod.HEAD"/> 404 is valid out of the box as well
    /// </para>
    /// <para></para>
    /// <para>NOTE: if a request specifies <see cref="IRequestConfiguration.AllowedStatusCodes"/> this takes precedence</para>
    /// </summary>
    Func<HttpMethod, int, bool> StatusCodeToResponseSuccess { get; }

    /// <summary>
    /// Whether the request should be sent with chunked Transfer-Encoding.
    /// </summary>
    bool TransferEncodingChunked { get; }

    /// <summary>
    /// DnsRefreshTimeout for the connections. Defaults to 5 minutes.
    /// <para>Will create new instances of <see cref="System.Net.Http.HttpClient"/> after this timeout to force DNS updates</para>
    /// </summary>
    TimeSpan DnsRefreshTimeout { get; }

    /// <summary>
    /// Enable statistics about TCP connections to be collected when making a request
    /// </summary>
    bool EnableTcpStats { get; }

    /// <summary>
    /// Enable statistics about thread pools to be collected when making a request
    /// </summary>
    bool EnableThreadPoolStats { get; }

    /// <summary>
    /// Produces the client meta header for a request.
    /// </summary>
    MetaHeaderProvider MetaHeaderProvider { get; }
}
