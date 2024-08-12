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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexManagement.CloneIndex;

using OpenSearch.Client;

public class CloneIndexApiTests
    : ApiIntegrationTestBase<WritableCluster, CloneIndexResponse, ICloneIndexRequest, CloneIndexDescriptor, CloneIndexRequest>
{
    private const string CloneSuffix = "_clone";

    public CloneIndexApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var value in values)
        {
            var createIndexResponse = client.Indices.Create(value.Value, c => c
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0)
                )
            );

            if (!createIndexResponse.IsValid)
                throw new Exception($"exception whilst setting up integration test: {createIndexResponse.DebugInformation}");

            var updateSettings = client.Indices.UpdateSettings(value.Value, s => s
                .IndexSettings(i => i
                    .BlocksWrite()
                )
            );

            if (!updateSettings.IsValid)
                throw new Exception($"exception whilst setting up integration test: {updateSettings.DebugInformation}");
        }
    }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        settings = new Dictionary<string, object>
        {
            { "index.number_of_replicas", 0 },
            { "index.number_of_shards", 1 },
            { "index.queries.cache.enabled", true }
        },
        aliases = new Dictionary<string, object>
        {
            { CallIsolatedValue + "-alias", new { is_write_index = true } }
        }
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<CloneIndexDescriptor, ICloneIndexRequest> Fluent => d => d
        .Settings(s => s
            .NumberOfReplicas(0)
            .NumberOfShards(1)
            .Queries(q => q
                .Cache(c => c
                    .Enabled()
                )
            )
        )
        .Aliases(a => a
            .Alias(CallIsolatedValue + "-alias", aa => aa
                .IsWriteIndex()
            )
        );

    protected override HttpMethod HttpMethod => HttpMethod.PUT;

    protected override CloneIndexRequest Initializer => new(CallIsolatedValue, CallIsolatedValue + CloneSuffix)
    {
        Settings = new IndexSettings
        {
            NumberOfReplicas = 0,
            NumberOfShards = 1,
            Queries = new QueriesSettings
            {
                Cache = new QueriesCacheSettings
                {
                    Enabled = true
                }
            }
        },
        Aliases = new Aliases
        {
            { CallIsolatedValue + "-alias", new Alias { IsWriteIndex = true } }
        }
    };

    protected override string UrlPath => $"/{CallIsolatedValue}/_clone/{CallIsolatedValue + CloneSuffix}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.Clone(CallIsolatedValue, CallIsolatedValue + CloneSuffix, f),
        (client, f) => client.Indices.CloneAsync(CallIsolatedValue, CallIsolatedValue + CloneSuffix, f),
        (client, r) => client.Indices.Clone(r),
        (client, r) => client.Indices.CloneAsync(r)
    );

    protected override CloneIndexDescriptor NewDescriptor() => new(CallIsolatedValue, CallIsolatedValue + CloneSuffix);

    protected override void ExpectResponse(CloneIndexResponse response)
    {
        response.ShouldBeValid();
        response.Acknowledged.Should().BeTrue();
        response.ShardsAcknowledged.Should().BeTrue();
        response.Index.Should().Be(CallIsolatedValue + CloneSuffix);
    }
}
