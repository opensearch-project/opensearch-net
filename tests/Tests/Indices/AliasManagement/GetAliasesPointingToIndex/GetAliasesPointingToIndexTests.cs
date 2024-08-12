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
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.DocumentationTests;

namespace Tests.Indices.AliasManagement.GetAliasesPointingToIndex;

public class GetAliasesPointingToIndexTests : IntegrationDocumentationTestBase, IClusterFixture<WritableCluster>
{
    private static readonly string Unique = RandomString();
    private static readonly string Index = "aliases-index-" + Unique;

    private readonly IOpenSearchClient _client;

    public GetAliasesPointingToIndexTests(WritableCluster cluster) : base(cluster)
    {
        _client = cluster.Client;

        if (_client.Indices.Exists(Index).Exists) return;

        lock (Unique)
        {
            if (_client.Indices.Exists(Index).Exists) return;

            var createResponse = _client.Indices.Create(Index, c => c
                .Settings(s => s
                    .NumberOfShards(1)
                    .NumberOfReplicas(0)
                )
                .Aliases(a => a
                    .Alias(Alias(1))
                    .Alias(Alias(2))
                    .Alias(Alias(3))
                )
            );

            createResponse.ShouldBeValid();
        }
    }

    private static string Alias(int alias) => "aliases-index-" + Unique + "-alias-" + alias;

    [I]
    public void ShouldGetAliasesPointingToIndex()
    {
        var aliasesPointingToIndex = _client.GetAliasesPointingToIndex(Index);
        AssertGetAliasesPointingToIndexResponse(aliasesPointingToIndex);
    }

    [I]
    public void ShouldGetIndicesPointingToAlias()
    {
        var indices = _client.GetIndicesPointingToAlias(Alias(3));
        indices.Should().NotBeEmpty().And.Contain(Index);
    }

    [I]
    public async Task ShouldGetAliasesPointingToIndexAsync()
    {
        var aliasesPointingToIndex = await _client.GetAliasesPointingToIndexAsync(Index);
        AssertGetAliasesPointingToIndexResponse(aliasesPointingToIndex);
    }

    [I]
    public async Task ShouldGetIndicesPointingToAliasAsync()
    {
        var indices = await _client.GetIndicesPointingToAliasAsync(Alias(3));
        indices.Should().NotBeEmpty().And.Contain(Index);
    }
    [I]
    public async Task NotFoundAliasReturnEmpty()
    {
        var indices = await _client.GetIndicesPointingToAliasAsync(Alias(4));
        indices.Should().BeEmpty();
    }

    private static void AssertGetAliasesPointingToIndexResponse(IReadOnlyDictionary<string, AliasDefinition> aliasesPointingToIndex)
    {
        aliasesPointingToIndex.Should()
            .NotBeEmpty()
            .And.HaveCount(3)
            .And.ContainKey(Alias(1))
            .And.ContainKey(Alias(2))
            .And.ContainKey(Alias(3));

        aliasesPointingToIndex[Alias(1)].Should().NotBeNull();
        aliasesPointingToIndex[Alias(2)].Should().NotBeNull();
        aliasesPointingToIndex[Alias(3)].Should().NotBeNull();
    }
}
