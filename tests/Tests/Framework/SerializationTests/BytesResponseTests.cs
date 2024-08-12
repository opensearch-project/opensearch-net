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
using System.Linq;
using System.Text;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.Framework.SerializationTests;

public class BytesResponseTests : IClusterFixture<ReadOnlyCluster>
{
    private readonly ReadOnlyCluster _cluster;

    public BytesResponseTests(ReadOnlyCluster cluster) => _cluster = cluster;

    [I]
    public void NonNullBytesResponse()
    {
        var client = _cluster.Client;

        var bytesResponse = client.LowLevel.Search<BytesResponse>("project", PostData.Serializable(new { }));
        bytesResponse.Success.Should().BeTrue();
        bytesResponse.Body.Should().NotBeNull();
        bytesResponse.Body.Should().BeEquivalentTo(bytesResponse.ResponseBodyInBytes);
    }

    [I]
    public void NonNullBytesLowLevelResponse()
    {
        var settings = new ConnectionConfiguration(_cluster.NodesUris().First());
        settings = (ConnectionConfiguration)_cluster.UpdateSettings(settings);
        var lowLevelClient = new OpenSearchLowLevelClient(settings);

        var bytesResponse = lowLevelClient.Search<BytesResponse>("project", PostData.Serializable(new { }));

        bytesResponse.Success.Should().BeTrue();
        bytesResponse.Body.Should().NotBeNull();
        bytesResponse.Body.Should().BeEquivalentTo(bytesResponse.ResponseBodyInBytes);
    }

    [U]
    public void TryGetServerErrorDoesNotThrowException()
    {
        var responseBytes = Encoding.UTF8.GetBytes(StubResponse.NginxHtml401Response);

        var client = FixedResponseClient.Create(responseBytes, 401,
            modifySettings: s => s.DisableDirectStreaming(),
            contentType: "text/html",
            exception: new Exception("problem with the request as a result of 401")
        );

        var bytesResponse = client.LowLevel.Search<BytesResponse>("project", PostData.Serializable(new { }));

        bytesResponse.Body.Should().NotBeNull();
        bytesResponse.Body.Should().BeEquivalentTo(responseBytes);
        bytesResponse.TryGetServerError(out var serverError).Should().BeFalse();
        serverError.Should().BeNull();
    }

    [U]
    public void SkipDeserializationForStatusCodesSetsBody()
    {
        var responseBytes = Encoding.UTF8.GetBytes(StubResponse.NginxHtml401Response);

        var client = FixedResponseClient.Create(responseBytes, 401,
            modifySettings: s => s.DisableDirectStreaming().SkipDeserializationForStatusCodes(401),
            contentType: "text/html",
            exception: new Exception("problem with the request as a result of 401")
        );

        var bytesResponse = client.LowLevel.Search<BytesResponse>("project", PostData.Serializable(new { }));

        bytesResponse.Body.Should().NotBeNull();
        bytesResponse.Body.Should().BeEquivalentTo(responseBytes);
    }
}
