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

using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Net;
using OpenSearch.Net.Specification.CatApi;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.Reproduce;

public class GithubIssue4243 : IClusterFixture<ReadOnlyCluster>
{
    private readonly ReadOnlyCluster _cluster;

    public GithubIssue4243(ReadOnlyCluster cluster) => _cluster = cluster;

    [I]
    public async Task UsingFormatJsonIsSuccessfulResponse()
    {
        var lowLevelClient = _cluster.Client.LowLevel;

        var response = _cluster.ClusterConfiguration.Version < "2.0.0"
#pragma warning disable CS0618 // Type or member is obsolete
            ? await lowLevelClient.Cat.MasterAsync<StringResponse>(new CatMasterRequestParameters { Format = "JSON" })
#pragma warning restore CS0618 // Type or member is obsolete
            : await lowLevelClient.Cat.ClusterManagerAsync<StringResponse>(new CatClusterManagerRequestParameters { Format = "JSON" });

        response.Success.Should().BeTrue();
        response.ApiCall.HttpStatusCode.Should().Be(200);
        response.OriginalException.Should().BeNull();
    }
}
