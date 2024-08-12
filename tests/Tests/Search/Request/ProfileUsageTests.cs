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
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.Request;

/**
	* WARNING: This functionality is experimental and may be changed or removed completely in a future release.
	*
	* The Profile API provides detailed timing information about the execution of individual components in a query.
	* It gives the user insight into how queries are executed at a low level so that the user can understand
	* why certain queries are slow, and take steps to improve their slow queries.
	*
	* The output from the Profile API is very verbose, especially for complicated queries executed across many shards.
	* Pretty-printing the response is recommended to help understand the output
	*
	* See the OpenSearch documentation on {ref_current}/search-profile.html[Profile API] for more detail.
	*/
public class ProfileUsageTests : SearchUsageTestBase
{
    public ProfileUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => new
    {
        profile = true,
        query = new
        {
            term = new { name = new { value = Project.First.Name } }
        }
    };

    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .Profile()
        .Query(q => q
            .Term(p => p.Name, Project.First.Name)
        );

    protected override SearchRequest<Project> Initializer =>
        new SearchRequest<Project>
        {
            Profile = true,
            Query = new TermQuery
            {
                Field = "name",
                Value = Project.First.Name
            }
        };

    [I]
    public async Task ProfileResults() => await AssertOnAllResponses((r) =>
    {
        r.HitsMetadata.Total.Value.Should().BeGreaterThan(0);

        r.Profile.Should().NotBeNull();
        r.Profile.Shards.Should().NotBeNullOrEmpty();

        var shard = r.Profile.Shards.First();
        shard.Id.Should().NotBeNullOrWhiteSpace();
        shard.Searches.Should().NotBeNullOrEmpty();

        var firstSearch = shard.Searches.First();
        firstSearch.RewriteTime.Should().BeGreaterThan(0);
        firstSearch.Query.Should().NotBeNullOrEmpty();
        firstSearch.Collector.Should().NotBeNullOrEmpty();

        var firstQuery = firstSearch.Query.First();
        firstQuery.Breakdown.Should().NotBeNull();
        firstQuery.Breakdown.CreateWeight.Should().BeGreaterThan(0);
        firstQuery.Breakdown.BuildScorer.Should().BeGreaterThan(0);

        firstQuery.Description.Should().NotBeNullOrWhiteSpace();
        firstQuery.Type.Should().NotBeNullOrWhiteSpace();
        firstQuery.TimeInNanoseconds.Should().BeGreaterThan(0);

        var firstCollector = firstSearch.Collector.First();
        firstCollector.Name.Should().NotBeNullOrEmpty();
        firstCollector.Reason.Should().NotBeNullOrEmpty();
        firstCollector.TimeInNanoseconds.Should().BeGreaterThan(0);
    });
}
