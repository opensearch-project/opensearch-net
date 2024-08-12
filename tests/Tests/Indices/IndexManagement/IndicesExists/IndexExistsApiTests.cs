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

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Indices.IndexManagement.IndicesExists;

public class IndexExistsApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, ExistsResponse, IIndexExistsRequest, IndexExistsDescriptor, IndexExistsRequest>
{
    public IndexExistsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.HEAD;

    protected override IndexExistsRequest Initializer => new IndexExistsRequest(Index<Project>());
    protected override string UrlPath => $"/project";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.Exists(Index<Project>()),
        (client, f) => client.Indices.ExistsAsync(Index<Project>()),
        (client, r) => client.Indices.Exists(r),
        (client, r) => client.Indices.ExistsAsync(r)
    );

    protected override void ExpectResponse(ExistsResponse response) => response.Exists.Should().BeTrue();
}

// DisableDirectStreaming = false so that response stream is not seekable
public class IndexNotExistsApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, ExistsResponse, IIndexExistsRequest, IndexExistsDescriptor, IndexExistsRequest>
{
    private const string NonExistentIndex = "non-existent-index";

    public IndexNotExistsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;
    protected override int ExpectStatusCode => 404;
    protected override HttpMethod HttpMethod => HttpMethod.HEAD;

    protected override IndexExistsRequest Initializer => new IndexExistsRequest(NonExistentIndex)
    {
        RequestConfiguration = new RequestConfiguration
        {
            DisableDirectStreaming = false
        }
    };
    protected override string UrlPath => $"/{NonExistentIndex}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.Exists(NonExistentIndex, r => r.RequestConfiguration(c => c.DisableDirectStreaming(false))),
        (client, f) => client.Indices.ExistsAsync(NonExistentIndex, r => r.RequestConfiguration(c => c.DisableDirectStreaming(false))),
        (client, r) => client.Indices.Exists(r),
        (client, r) => client.Indices.ExistsAsync(r)
    );

    protected override void ExpectResponse(ExistsResponse response) => response.Exists.Should().BeFalse();
}
