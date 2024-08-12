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

namespace Tests.Indices.AliasManagement.Alias;

public class AliasApiTests
    : ApiIntegrationAgainstNewIndexTestBase<WritableCluster, BulkAliasResponse, IBulkAliasRequest, BulkAliasDescriptor, BulkAliasRequest>
{
    public AliasApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        actions = new object[]
        {
            new Dictionary<string, object>
                { { "add", new { alias = "alias", index = CallIsolatedValue, index_routing = "x", search_routing = "y" } } },
            new Dictionary<string, object>
                { { "add", new { aliases = new [] { "alias1", "alias2" }, indices = new [] { CallIsolatedValue } } } },
            new Dictionary<string, object> { { "remove", new { alias = "alias", index = CallIsolatedValue } } },
            new Dictionary<string, object> { { "remove", new { aliases = new [] { "alias1", "alias2" }, indices = new[] { CallIsolatedValue } } } },
        }
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<BulkAliasDescriptor, IBulkAliasRequest> Fluent => d => d
        .Add(a => a.Alias("alias").Index(CallIsolatedValue).IndexRouting("x").SearchRouting("y"))
        .Add(a => a.Aliases("alias1", "alias2").Indices(CallIsolatedValue))
        .Remove(a => a.Alias("alias").Index(CallIsolatedValue))
        .Remove(a => a.Aliases("alias1", "alias2").Indices(CallIsolatedValue));

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override BulkAliasRequest Initializer => new BulkAliasRequest
    {
        Actions = new List<IAliasAction>
        {
            new AliasAddAction
                { Add = new AliasAddOperation { Alias = "alias", Index = CallIsolatedValue, IndexRouting = "x", SearchRouting = "y" } },
            new AliasAddAction
                { Add = new AliasAddOperation { Aliases = new[] { "alias1", "alias2" }, Indices = CallIsolatedValue } },
            new AliasRemoveAction { Remove = new AliasRemoveOperation { Alias = "alias", Index = CallIsolatedValue } },
            new AliasRemoveAction { Remove = new AliasRemoveOperation { Aliases = new[] { "alias1", "alias2" }, Indices = CallIsolatedValue } },
        }
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/_aliases";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.BulkAlias(f),
        (client, f) => client.Indices.BulkAliasAsync(f),
        (client, r) => client.Indices.BulkAlias(r),
        (client, r) => client.Indices.BulkAliasAsync(r)
    );
}

public class AliasIsWriteIndexApiTests
    : ApiIntegrationAgainstNewIndexTestBase<WritableCluster, BulkAliasResponse, IBulkAliasRequest, BulkAliasDescriptor, BulkAliasRequest>
{
    public AliasIsWriteIndexApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        actions = new object[]
        {
            new Dictionary<string, object>
            {
                {
                    "add",
                    new { alias = Alias(1), index = Index, index_routing = "x", search_routing = "y", is_write_index = false }
                }
            },
            new Dictionary<string, object>
            {
                {
                    "add",
                    new { alias = Alias(2), index = Index, index_routing = "x", search_routing = "y", is_write_index = true }
                }
            },
        }
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<BulkAliasDescriptor, IBulkAliasRequest> Fluent => d => d
        .Add(a => a.Alias(Alias(1)).Index(Index).IndexRouting("x").SearchRouting("y").IsWriteIndex(false))
        .Add(a => a.Alias(Alias(2)).Index(Index).IndexRouting("x").SearchRouting("y").IsWriteIndex());

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override BulkAliasRequest Initializer => new BulkAliasRequest
    {
        Actions = new List<IAliasAction>
        {
            new AliasAddAction
            {
                Add =
                    new AliasAddOperation { Alias = Alias(1), Index = Index, IndexRouting = "x", SearchRouting = "y", IsWriteIndex = false }
            },
            new AliasAddAction
            {
                Add =
                    new AliasAddOperation { Alias = Alias(2), Index = Index, IndexRouting = "x", SearchRouting = "y", IsWriteIndex = true }
            },
        }
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/_aliases";
    private string Index => CallIsolatedValue;

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.BulkAlias(f),
        (client, f) => client.Indices.BulkAliasAsync(f),
        (client, r) => client.Indices.BulkAlias(r),
        (client, r) => client.Indices.BulkAliasAsync(r)
    );

    private string Alias(int i) => $"alias-{CallIsolatedValue}-{i}";

    protected override void OnAfterCall(IOpenSearchClient client)
    {
        var secondAlias = Alias(2);
        var aliasResponse = Client.Indices.GetAlias(secondAlias);
        aliasResponse.ShouldBeValid();
        aliasResponse.Indices.Should().NotBeEmpty().And.ContainKey(Index);
        var indexAliases = aliasResponse.Indices[Index].Aliases;

        indexAliases.Should().NotBeEmpty().And.ContainKey(secondAlias);
        var alias = indexAliases[secondAlias];
        alias.IsWriteIndex.Should()
            .HaveValue()
            .And
            .BeTrue($"{secondAlias} was stored is is_write_index, so we need to be able to read it too");
    }
}
