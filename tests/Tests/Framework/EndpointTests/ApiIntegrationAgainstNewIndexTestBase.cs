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

using System.Linq;
using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Framework.EndpointTests;

public abstract class ApiIntegrationAgainstNewIndexTestBase<TCluster, TResponse, TInterface, TDescriptor, TInitializer>
    : ApiIntegrationTestBase<TCluster, TResponse, TInterface, TDescriptor, TInitializer>
    where TCluster : IEphemeralCluster<EphemeralClusterConfiguration>, IOpenSearchClientTestCluster, new()
    where TResponse : class, IResponse
    where TDescriptor : class, TInterface
    where TInitializer : class, TInterface
    where TInterface : class
{
    protected ApiIntegrationAgainstNewIndexTestBase(TCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var index in values.Values) client.Indices.Create(index, CreateIndexSettings).ShouldBeValid();
        var indices = Infer.Indices(values.Values.Select(i => (IndexName)i));
        client.Cluster.Health(indices, f => f.WaitForStatus(HealthStatus.Yellow))
            .ShouldBeValid();
    }

    protected virtual ICreateIndexRequest CreateIndexSettings(CreateIndexDescriptor create) => create;

    // https://youtrack.jetbrains.com/issue/RIDER-19912
    [U] protected override Task HitsTheCorrectUrl() => base.HitsTheCorrectUrl();

    [U] protected override Task UsesCorrectHttpMethod() => base.UsesCorrectHttpMethod();

    [U] protected override void SerializesInitializer() => base.SerializesInitializer();

    [U] protected override void SerializesFluent() => base.SerializesFluent();

    [I] public override Task ReturnsExpectedStatusCode() => base.ReturnsExpectedResponse();

    [I] public override Task ReturnsExpectedIsValid() => base.ReturnsExpectedIsValid();

    [I] public override Task ReturnsExpectedResponse() => base.ReturnsExpectedResponse();
}
