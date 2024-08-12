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
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.AliasManagement.Alias;

public class AliasApiRemoveIndexTests
    : ApiIntegrationAgainstNewIndexTestBase<WritableCluster, BulkAliasResponse, IBulkAliasRequest, BulkAliasDescriptor, BulkAliasRequest>
{
    public AliasApiRemoveIndexTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new
    {
        actions = new object[]
        {
            new Dictionary<string, object> { { "remove_index", new { index = CallIsolatedValue + "-1" } } },
            new Dictionary<string, object> { { "add", new { alias = CallIsolatedValue + "-1", index = CallIsolatedValue + "-2" } } },
        }
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<BulkAliasDescriptor, IBulkAliasRequest> Fluent => d => d
        .RemoveIndex(a => a.Index(CallIsolatedValue + "-1"))
        .Add(a => a.Alias(CallIsolatedValue + "-1").Index(CallIsolatedValue + "-2"));

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override BulkAliasRequest Initializer => new BulkAliasRequest
    {
        Actions = new List<IAliasAction>
        {
            new AliasRemoveIndexAction { RemoveIndex = new AliasRemoveIndexOperation { Index = Infer.Index(CallIsolatedValue + "-1") } },
            new AliasAddAction { Add = new AliasAddOperation { Alias = CallIsolatedValue + "-1", Index = CallIsolatedValue + "-2" } },
        }
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/_aliases";

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var value in values.Values)
        {
            var createIndexResponse = client.Indices.Create(value + "-1", c => c);
            if (!createIndexResponse.IsValid)
                throw new Exception(createIndexResponse.DebugInformation);

            createIndexResponse = client.Indices.Create(value + "-2", c => c);
            if (!createIndexResponse.IsValid)
                throw new Exception(createIndexResponse.DebugInformation);
        }
    }

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.BulkAlias(f),
        (client, f) => client.Indices.BulkAliasAsync(f),
        (client, r) => client.Indices.BulkAlias(r),
        (client, r) => client.Indices.BulkAliasAsync(r)
    );
}
