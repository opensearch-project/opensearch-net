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
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.AliasManagement.PutAlias;

public class PutAliasApiTests
    : ApiIntegrationAgainstNewIndexTestBase<WritableCluster, PutAliasResponse, IPutAliasRequest, PutAliasDescriptor, PutAliasRequest>
{
    public PutAliasApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;

    protected override Func<PutAliasDescriptor, IPutAliasRequest> Fluent => null;
    protected override HttpMethod HttpMethod => HttpMethod.PUT;
    protected override PutAliasRequest Initializer => new PutAliasRequest(CallIsolatedValue, CallIsolatedValue + "-alias");

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/{CallIsolatedValue}/_alias/{CallIsolatedValue + "-alias"}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.PutAlias(CallIsolatedValue, CallIsolatedValue + "-alias"),
        (client, f) => client.Indices.PutAliasAsync(CallIsolatedValue, CallIsolatedValue + "-alias"),
        (client, r) => client.Indices.PutAlias(r),
        (client, r) => client.Indices.PutAliasAsync(r)
    );
}

public class PutAliasIsWriteIndexApiTests
    : ApiIntegrationAgainstNewIndexTestBase<WritableCluster, PutAliasResponse, IPutAliasRequest, PutAliasDescriptor, PutAliasRequest>
{
    public PutAliasIsWriteIndexApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson => new { is_write_index = true };
    protected override int ExpectStatusCode => 200;

    protected override Func<PutAliasDescriptor, IPutAliasRequest> Fluent => f => f.IsWriteIndex();
    protected override HttpMethod HttpMethod => HttpMethod.PUT;

    protected override PutAliasRequest Initializer => new PutAliasRequest(CallIsolatedValue, CallIsolatedValue + "-alias")
    {
        IsWriteIndex = true
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/{CallIsolatedValue}/_alias/{CallIsolatedValue + "-alias"}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.PutAlias(CallIsolatedValue, CallIsolatedValue + "-alias"),
        (client, f) => client.Indices.PutAliasAsync(CallIsolatedValue, CallIsolatedValue + "-alias"),
        (client, r) => client.Indices.PutAlias(r),
        (client, r) => client.Indices.PutAliasAsync(r)
    );

    protected override PutAliasDescriptor NewDescriptor() => new PutAliasDescriptor(CallIsolatedValue, CallIsolatedValue + "-alias");
}
