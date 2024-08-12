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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;

namespace Tests.Reproduce;

public class GithubIssue2306 : IClusterFixture<ReadOnlyCluster>
{
    private readonly ReadOnlyCluster _cluster;

    public GithubIssue2306(ReadOnlyCluster cluster) => _cluster = cluster;

    [I]
    public void DeleteNonExistentDocumentReturnsNotFound()
    {
        var client = _cluster.Client;
        var response = client.Delete<Project>("non-existent-id", d => d.Routing("routing"));

        response.ShouldNotBeValid();
        response.Result.Should().Be(Result.NotFound);
        response.Index.Should().Be("project");
        if (_cluster.ClusterConfiguration.Version < "2.0.0")
            response.Type.Should().Be("_doc");
        response.Id.Should().Be("non-existent-id");
    }
}
