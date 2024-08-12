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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.Reproduce;

public class GithubIssue4197 : IClusterFixture<WritableCluster>
{
    private readonly IOpenSearchClient _client;

    public GithubIssue4197(WritableCluster cluster) => _client = cluster.Client;

    [I]
    public void CanDeserializeAnonymousFiltersAggregation()
    {
        const string index = "github-issue-4197";

        _client.Indices.Create(index);

        _client.Index(new Doc { ModificationDate = DateTime.Now },
            i => i.Index(index).Refresh(Refresh.WaitFor));

        var searchResponse = _client.Search<Doc>(s => s
            .Index(index)
            .Aggregations(a => a
                .Filters("Modification date", f => f
                    .AnonymousFilters(q => q
                        .DateRange(dr => dr
                            .Field(d => d.ModificationDate)
                            .GreaterThan(DateMath.Now.Subtract(TimeSpan.FromDays(120)))
                        )
                    )
                )
            )
        );

        var filtersAggregate = searchResponse.Aggregations.Filters("Modification date");
        filtersAggregate.AnonymousBuckets().Count.Should().Be(1, "there should be exactly one anonymous bucket");
        filtersAggregate.AnonymousBuckets()[0].DocCount.Should().Be(1, "there should be exactly one document");
    }

    private class Doc
    {
        public DateTime ModificationDate { get; set; }
    }
}
