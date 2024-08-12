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

using System.Collections.Generic;
using static OpenSearch.Net.OpenSearchUrlFormatter;

// ReSharper disable once CheckNamespace
namespace OpenSearch.Net;

/// <summary>
/// Used by the raw client to compose querystring parameters in a matter that still exposes some xmldocs
/// You can always pass a simple NameValueCollection if you want.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class RequestParameters<T> : IRequestParameters where T : RequestParameters<T>
{
    public abstract HttpMethod DefaultHttpMethod { get; }
    public abstract bool SupportsBody { get; }

    public CustomResponseBuilderBase CustomResponseBuilder { get; set; }
    public Dictionary<string, object> QueryString { get; set; } = new Dictionary<string, object>();
    public IRequestConfiguration RequestConfiguration { get; set; }
    private IRequestParameters Self => this;

    /// <inheritdoc />
    public bool ContainsQueryString(string name) => Self.QueryString != null && Self.QueryString.ContainsKey(name);

    /// <inheritdoc />
    public TOut GetQueryStringValue<TOut>(string name)
    {
        if (!ContainsQueryString(name))
            return default;

        var value = Self.QueryString[name];
        if (value == null)
            return default;

        return (TOut)value;
    }

    /// <inheritdoc />
    public string GetResolvedQueryStringValue(string n, IConnectionConfigurationValues s) =>
        CreateString(GetQueryStringValue<object>(n), s);

    /// <inheritdoc />
    public void SetQueryString(string name, object value)
    {
        if (value == null) RemoveQueryString(name);
        else Self.QueryString[name] = value;
    }

    //These exists solely so the generated code can call these shortened methods
    protected TOut Q<TOut>(string name) => GetQueryStringValue<TOut>(name);

    protected void Q(string name, object value) => SetQueryString(name, value);

    private void RemoveQueryString(string name)
    {
        if (!Self.QueryString.ContainsKey(name)) return;

        Self.QueryString.Remove(name);
    }

    protected void SetAcceptHeader(string format)
    {
        if (RequestConfiguration == null)
            RequestConfiguration = new RequestConfiguration();

        RequestConfiguration.Accept = AcceptHeaderFromFormat(format);
    }

    /// <inheritdoc />
    public string AcceptHeaderFromFormat(string format)
    {
        if (format == null)
            return null;

        var lowerFormat = format.ToLowerInvariant();

        switch (lowerFormat)
        {
            case "smile":
            case "yaml":
            case "cbor":
            case "json":
                return $"application/{lowerFormat}";
            default:
                return null;
        }
    }
}
