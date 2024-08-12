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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;

namespace Tests.Reproduce;

public class GithubIssue2871 : IClusterFixture<WritableCluster>
{
    private readonly WritableCluster _cluster;

    public GithubIssue2871(WritableCluster cluster) => _cluster = cluster;

    [I]
    public void IsValidFalseAndDeserializedErrorsWhenMultiGetDocHasErrors()
    {
        var index1 = "index1";
        var index2 = "index2";
        var alias = "my_alias";
        var client = _cluster.Client;

        client.Indices.Create(index1, c => c
            .Map<Project>(mm => mm
                .AutoMap()
            )
        );

        client.Indices.Create(index2, c => c
            .Map<Project>(mm => mm
                .AutoMap()
            )
        );

        var projects = new[]
        {
            new Project { Name = "project1" },
            new Project { Name = "project2" },
        };

        client.Bulk(b => b
            .IndexMany(projects, (bi, p) => bi.Index(index1).Document(p))
            .IndexMany(projects, (bi, p) => bi.Index(index2).Document(p))
            .Refresh(Refresh.WaitFor)
        );

        client.Indices.BulkAlias(a => a
            .Add(add => add
                .Alias(alias)
                .Index(index1)
            )
            .Add(add => add
                .Alias(alias)
                .Index(index2)
            )
        );

        var multiGetRequest = new MultiGetRequest
        {
            Documents = new[]
            {
                new MultiGetOperation<Project>("project1") { Index = alias },
                new MultiGetOperation<Project>("project2") { Index = alias }
            }
        };

        var response = client.MultiGet(multiGetRequest);
        response.ShouldNotBeValid();

        var firstMultiGetHit = response.Hits.First();
        firstMultiGetHit.Error.Should().NotBeNull();
        firstMultiGetHit.Error.Should().NotBeNull();
        if (_cluster.ClusterConfiguration.Version < "2.0.0")
            firstMultiGetHit.Error.Type.Should().NotBeNullOrEmpty();
        firstMultiGetHit.Error.Reason.Should().NotBeNullOrEmpty();
        firstMultiGetHit.Error.RootCause.Should().NotBeNull().And.HaveCount(1);

        var lastMultiGetHit = response.Hits.Last();
        lastMultiGetHit.Error.Should().NotBeNull();
        lastMultiGetHit.Error.Should().NotBeNull();
        if (_cluster.ClusterConfiguration.Version < "2.0.0")
            lastMultiGetHit.Error.Type.Should().NotBeNullOrEmpty();
        lastMultiGetHit.Error.Reason.Should().NotBeNullOrEmpty();
        lastMultiGetHit.Error.RootCause.Should().NotBeNull().And.HaveCount(1);
    }
}
