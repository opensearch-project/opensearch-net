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

using System.Text.RegularExpressions;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Core.Connection.MetaData;

public class MetaHeaderProviderTests
{
    private readonly Regex _validHeaderRegex = new Regex(@"^[a-z]{1,}=[a-z0-9\.\-]{1,}(?:,[a-z]{1,}=[a-z0-9\.\-]+)*$");
    private readonly Regex _validVersionRegex = new Regex(@"^[0-9]{1,2}\.[0-9]{1,2}(?:\.[0-9]{1,3})?p?$");
    private readonly Regex _validHttpClientPart = new Regex(@"^[a-z]{2,3}=[0-9]{1,2}\.[0-9]{1,2}(?:\.[0-9]{1,3})?p?$");

    [U]
    public void HeaderName_ReturnsExpectedValue()
    {
        var sut = new MetaHeaderProvider();
        sut.HeaderName.Should().Be("opensearch-client-meta");
    }

    [U]
    public void HeaderName_ReturnsNullWhenDisabled()
    {
        var sut = new MetaHeaderProvider();

        var connectionSettings = new ConnectionSettings()
            .DisableMetaHeader(true);

        var requestData = new RequestData(HttpMethod.POST, "/_search", "{}", connectionSettings,
            new SearchRequestParameters(),
            new RecyclableMemoryStreamFactory());

        sut.ProduceHeaderValue(requestData).Should().BeNull();
    }

    [U]
    public void HeaderName_ReturnsExpectedValue_ForSyncRequest_WhenNotDisabled()
    {
        var sut = new MetaHeaderProvider();

        var connectionSettings = new ConnectionSettings();

        var requestData = new RequestData(HttpMethod.POST, "/_search", "{}", connectionSettings,
            new SearchRequestParameters(),
            new RecyclableMemoryStreamFactory())
        {
            IsAsync = false
        };

        var result = sut.ProduceHeaderValue(requestData);

        _validHeaderRegex.Match(result).Success.Should().BeTrue();

        var parts = result.Split(',');
        parts.Length.Should().Be(4);

        parts[0].Should().StartWith("opensearch=");
        var clientVersion = parts[0].Substring(11);
        _validVersionRegex.Match(clientVersion).Success.Should().BeTrue();

        parts[1].Should().Be("a=0");

        parts[2].Should().StartWith("net=");
        var runtimeVersion = parts[2].Substring(4);
        _validVersionRegex.Match(runtimeVersion).Success.Should().BeTrue();

        _validHttpClientPart.Match(parts[3]).Success.Should().BeTrue();
    }

    [U]
    public void HeaderName_ReturnsExpectedValue_ForAsyncRequest_WhenNotDisabled()
    {
        var sut = new MetaHeaderProvider();

        var connectionSettings = new ConnectionSettings();

        var requestData = new RequestData(HttpMethod.POST, "/_search", "{}", connectionSettings,
            new SearchRequestParameters(),
            new RecyclableMemoryStreamFactory())
        {
            IsAsync = true
        };

        var result = sut.ProduceHeaderValue(requestData);

        _validHeaderRegex.Match(result).Success.Should().BeTrue();

        var parts = result.Split(',');
        parts.Length.Should().Be(4);

        parts[0].Should().StartWith("opensearch=");
        var clientVersion = parts[0].Substring(11);
        _validVersionRegex.Match(clientVersion).Success.Should().BeTrue();

        parts[1].Should().Be("a=1");

        parts[2].Should().StartWith("net=");
        var runtimeVersion = parts[2].Substring(4);
        _validVersionRegex.Match(runtimeVersion).Success.Should().BeTrue();

        _validHttpClientPart.Match(parts[3]).Success.Should().BeTrue();
    }
}
