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
using System.Text;

namespace OpenSearch.Net;

/// <summary>
/// An <see cref="IConnectionPool"/> implementation that can be seeded with a cloud id
/// and will signal the right defaults for the client to use for OpenSearch Cloud to <see cref="IConnectionConfigurationValues"/>.
///
/// <para>Read more about OpenSearch Cloud Id:</para>
/// <para></para>
/// </summary>
public class CloudConnectionPool : SingleNodeConnectionPool
{
    /// <summary>
    /// An <see cref="IConnectionPool"/> implementation that can be seeded with a cloud id
    /// and will signal the right defaults for the client to use for OpenSearch Cloud to <see cref="IConnectionConfigurationValues"/>.
    ///
    /// <para>Read more about OpenSearch Cloud Id here</para>
    /// <para></para>
    /// </summary>
    /// <param name="cloudId">
    /// The Cloud Id, this is available on your cluster's dashboard and is a string in the form of <c>cluster_name:base_64_encoded_string</c>
    /// <para>Base64 encoded string contains the following tokens in order separated by $:</para>
    /// <para>* Host Name (mandatory)</para>
    /// <para>* OpenSearch UUID (mandatory)</para>
    /// <para>* OpenSearchDashboards UUID</para>
    /// <para>* APM UUID</para>
    /// <para></para>
    /// <para> We then use these tokens to create the URI to your OpenSearch Cloud cluster!</para>
    /// <para></para>
    /// <para> Read more here: </para>
    /// </param>
    public CloudConnectionPool(string cloudId, BasicAuthenticationCredentials credentials, IDateTimeProvider dateTimeProvider = null) : this(ParseCloudId(cloudId), dateTimeProvider) =>
        BasicCredentials = credentials;

    /// <summary>
    /// An <see cref="IConnectionPool"/> implementation that can be seeded with a cloud id
    /// and will signal the right defaults for the client to use for OpenSearch Cloud to <see cref="IConnectionConfigurationValues"/>.
    ///
    /// <para>Read more about OpenSearch Cloud Id here</para>
    /// <para></para>
    /// </summary>
    /// <param name="cloudId">
    /// The Cloud Id, this is available on your cluster's dashboard and is a string in the form of <c>cluster_name:base_64_encoded_string</c>
    /// <para>Base64 encoded string contains the following tokens in order separated by $:</para>
    /// <para>* Host Name (mandatory)</para>
    /// <para>* OpenSearch UUID (mandatory)</para>
    /// <para>* OpenSearchDashboards UUID</para>
    /// <para>* APM UUID</para>
    /// <para></para>
    /// <para> We then use these tokens to create the URI to your OpenSearch Cloud cluster!</para>
    /// <para></para>
    /// <para> Read more here: </para>
    /// </param>
    public CloudConnectionPool(string cloudId, ApiKeyAuthenticationCredentials credentials, IDateTimeProvider dateTimeProvider = null) : this(ParseCloudId(cloudId), dateTimeProvider) =>
        ApiKeyCredentials = credentials;

    private CloudConnectionPool(ParsedCloudId parsedCloudId, IDateTimeProvider dateTimeProvider = null) : base(parsedCloudId.Uri, dateTimeProvider) =>
        ClusterName = parsedCloudId.Name;

    //TODO implement debugger display for IConnectionPool implementations and display it there and its ToString()
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private string ClusterName { get; }

    public BasicAuthenticationCredentials BasicCredentials { get; }

    public ApiKeyAuthenticationCredentials ApiKeyCredentials { get; }

    private struct ParsedCloudId
    {
        public ParsedCloudId(string clusterName, Uri uri)
        {
            Name = clusterName;
            Uri = uri;
        }

        public string Name { get; }
        public Uri Uri { get; }
    }

    private static ParsedCloudId ParseCloudId(string cloudId)
    {
        const string exceptionSuffix = "should be a string in the form of cluster_name:base_64_data";
        if (string.IsNullOrWhiteSpace(cloudId))
            throw new ArgumentException($"Parameter {nameof(cloudId)} was null or empty but {exceptionSuffix}", nameof(cloudId));

        var tokens = cloudId.Split(new[] { ':' }, 2);
        if (tokens.Length != 2)
            throw new ArgumentException($"Parameter {nameof(cloudId)} not in expected format, {exceptionSuffix}", nameof(cloudId));

        var clusterName = tokens[0];
        var encoded = tokens[1];
        if (string.IsNullOrWhiteSpace(encoded))
            throw new ArgumentException($"Parameter {nameof(cloudId)} base_64_data is empty, {exceptionSuffix}", nameof(cloudId));

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        var parts = decoded.Split(new[] { '$' });
        if (parts.Length < 2)
            throw new ArgumentException($"Parameter {nameof(cloudId)} decoded base_64_data contains less then 2 tokens, {exceptionSuffix}", nameof(cloudId));

        var domainName = parts[0].Trim();
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException($"Parameter {nameof(cloudId)} decoded base_64_data contains no domain name, {exceptionSuffix}", nameof(cloudId));

        var opensearchUuid = parts[1].Trim();
        if (string.IsNullOrWhiteSpace(opensearchUuid))
            throw new ArgumentException($"Parameter {nameof(cloudId)} decoded base_64_data contains no opensearch UUID, {exceptionSuffix}", nameof(cloudId));

        return new ParsedCloudId(clusterName, new Uri($"https://{opensearchUuid}.{domainName}"));
    }

    protected override void DisposeManagedResources()
    {
        ApiKeyCredentials?.Dispose();
        BasicCredentials?.Dispose();
    }
}
