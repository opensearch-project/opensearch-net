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
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.ClusterSettings.ClusterPutSettings;

public class ClusterPutSettingsApiTests
    : ApiIntegrationTestBase<IntrusiveOperationCluster, ClusterPutSettingsResponse, IClusterPutSettingsRequest, ClusterPutSettingsDescriptor,
        ClusterPutSettingsRequest>
{
    public ClusterPutSettingsApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override int ExpectStatusCode => 200;

    protected override Func<ClusterPutSettingsDescriptor, IClusterPutSettingsRequest> Fluent => c => c
        .Transient(s => s
            .Add("indices.recovery.max_bytes_per_sec", "41mb")
        );

    protected override HttpMethod HttpMethod => HttpMethod.PUT;

    protected override ClusterPutSettingsRequest Initializer => new ClusterPutSettingsRequest
    {
        Transient = new Dictionary<string, object>
        {
            { "indices.recovery.max_bytes_per_sec", "41mb" }
        }
    };

    protected override string UrlPath => "/_cluster/settings";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cluster.PutSettings(f),
        (client, f) => client.Cluster.PutSettingsAsync(f),
        (client, r) => client.Cluster.PutSettings(r),
        (client, r) => client.Cluster.PutSettingsAsync(r)
    );

    protected override void ExpectResponse(ClusterPutSettingsResponse response)
    {
        response.ShouldBeValid();
        response.Acknowledged.Should().BeTrue();
        response.Transient.Should().HaveCount(1);
    }
}

public class ClusterPutSettingsNoopApiTests
    : ApiIntegrationTestBase<IntrusiveOperationCluster, ClusterPutSettingsResponse, IClusterPutSettingsRequest, ClusterPutSettingsDescriptor,
        ClusterPutSettingsRequest>
{
    public ClusterPutSettingsNoopApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;

    protected override int ExpectStatusCode => 400;

    protected override HttpMethod HttpMethod => HttpMethod.PUT;

    protected override ClusterPutSettingsRequest Initializer => new ClusterPutSettingsRequest();

    protected override string UrlPath => "/_cluster/settings";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cluster.PutSettings(f),
        (client, f) => client.Cluster.PutSettingsAsync(f),
        (client, r) => client.Cluster.PutSettings(r),
        (client, r) => client.Cluster.PutSettingsAsync(r)
    );

    protected override void ExpectResponse(ClusterPutSettingsResponse response)
    {
        response.ShouldNotBeValid();
        response.ServerError.Should().NotBeNull();
        response.ServerError.Status.Should().Be(400);
        response.ServerError.Error.Should().NotBeNull();
        response.ServerError.Error.Reason.Should().Contain("no settings to update");
        response.ServerError.Error.Type.Should().Contain("action_request_validation_exception");
    }
}
