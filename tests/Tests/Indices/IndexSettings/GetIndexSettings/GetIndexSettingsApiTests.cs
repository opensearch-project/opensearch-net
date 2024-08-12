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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using Xunit;
using static OpenSearch.Client.Infer;

namespace Tests.Indices.IndexSettings.GetIndexSettings;

public class GetIndexSettingsApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, GetIndexSettingsResponse, IGetIndexSettingsRequest, GetIndexSettingsDescriptor,
        GetIndexSettingsRequest>
{
    private static readonly IndexName PercolationIndex = Index<ProjectPercolation>();

    public GetIndexSettingsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override Func<GetIndexSettingsDescriptor, IGetIndexSettingsRequest> Fluent => d => d
        .Name("index.*")
        .Local();


    protected override GetIndexSettingsRequest Initializer => new GetIndexSettingsRequest(PercolationIndex, "index.*")
    {
        Local = true
    };

    protected override string UrlPath => $"/queries/_settings/index.%2A?local=true";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.GetSettings(Index<ProjectPercolation>(), f),
        (client, f) => client.Indices.GetSettingsAsync(Index<ProjectPercolation>(), f),
        (client, r) => client.Indices.GetSettings(r),
        (client, r) => client.Indices.GetSettingsAsync(r)
    );

    protected override void ExpectResponse(GetIndexSettingsResponse response)
    {
        response.Indices.Should().NotBeEmpty();
        var index = response.Indices[PercolationIndex];
        index.Should().NotBeNull();
        index.Settings.NumberOfShards.Should().HaveValue().And.BeGreaterThan(0);
        index.Settings.NumberOfReplicas.Should().HaveValue();
        index.Settings.AutoExpandReplicas.Should().NotBeNull();
        index.Settings.AutoExpandReplicas.MinReplicas.Should().Be(0);
        index.Settings.AutoExpandReplicas.MaxReplicas.Match(
            i => { Assert.Fail("expecting a string"); },
            s => s.Should().Be("all"));
        index.Settings.AutoExpandReplicas.ToString().Should().Be("0-all");
    }
}
