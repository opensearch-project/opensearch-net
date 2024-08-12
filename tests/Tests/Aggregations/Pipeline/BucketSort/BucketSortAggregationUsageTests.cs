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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Aggregations.Pipeline.BucketSort;

public class BucketSortAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public BucketSortAggregationUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object AggregationJson => new
    {
        projects_started_per_month = new
        {
            date_histogram = new
            {
                field = "startedOn",
                calendar_interval = "month",
            },
            aggs = new
            {
                commits = new
                {
                    sum = new
                    {
                        field = "numberOfCommits"
                    }
                },
                commits_bucket_sort = new
                {
                    bucket_sort = new
                    {
                        sort = new[]
                        {
                            new { commits = new { order = "desc" } }
                        },
                        from = 0,
                        size = 3,
                        gap_policy = "insert_zeros"
                    }
                }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .DateHistogram("projects_started_per_month", dh => dh
            .Field(p => p.StartedOn)
            .CalendarInterval(DateInterval.Month)
            .Aggregations(aa => aa
                .Sum("commits", sm => sm
                    .Field(p => p.NumberOfCommits)
                )
                .BucketSort("commits_bucket_sort", bs => bs
                    .Sort(s => s
                        .Descending("commits")
                    )
                    .From(0)
                    .Size(3)
                    .GapPolicy(GapPolicy.InsertZeros)
                )
            )
        );

    protected override AggregationDictionary InitializerAggs =>
        new DateHistogramAggregation("projects_started_per_month")
        {
            Field = "startedOn",
            CalendarInterval = DateInterval.Month,
            Aggregations =
                new SumAggregation("commits", "numberOfCommits") &&
                new BucketSortAggregation("commits_bucket_sort")
                {
                    Sort = new List<ISort>
                    {
                        new FieldSort { Field = "commits", Order = SortOrder.Descending }
                    },
                    From = 0,
                    Size = 3,
                    GapPolicy = GapPolicy.InsertZeros
                }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();

        var projectsPerMonth = response.Aggregations.DateHistogram("projects_started_per_month");
        projectsPerMonth.Should().NotBeNull();
        projectsPerMonth.Buckets.Should().NotBeNull();
        projectsPerMonth.Buckets.Count.Should().Be(3);

        double previousCommits = -1;

        // sum of commits should descend over buckets
        foreach (var item in projectsPerMonth.Buckets)
        {
            var value = item.Sum("commits").Value;
            if (value == null) continue;

            var numberOfCommits = value.Value;
            if (Math.Abs(previousCommits - (-1)) > double.Epsilon)
                numberOfCommits.Should().BeLessOrEqualTo(previousCommits);

            previousCommits = numberOfCommits;
        }
    }
}
