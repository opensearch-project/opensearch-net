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
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Aggregations.Pipeline.PercentilesBucket;

public class PercentilesBucketAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public PercentilesBucketAggregationUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

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
                }
            }
        },
        commits_outlier = new
        {
            percentiles_bucket = new
            {
                buckets_path = "projects_started_per_month>commits",
                percents = new[] { 95.0, 99.0, 99.9 }
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
            )
        )
        .PercentilesBucket("commits_outlier", aaa => aaa
            .BucketsPath("projects_started_per_month>commits")
            .Percents(95, 99, 99.9)
        );

    protected override AggregationDictionary InitializerAggs =>
        new DateHistogramAggregation("projects_started_per_month")
        {
            Field = "startedOn",
            CalendarInterval = DateInterval.Month,
            Aggregations = new SumAggregation("commits", "numberOfCommits")
        }
        && new PercentilesBucketAggregation("commits_outlier", "projects_started_per_month>commits")
        {
            Percents = new[] { 95, 99, 99.9 }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();

        var projectsPerMonth = response.Aggregations.DateHistogram("projects_started_per_month");
        projectsPerMonth.Should().NotBeNull();
        projectsPerMonth.Buckets.Should().NotBeNull();
        projectsPerMonth.Buckets.Count.Should().BeGreaterThan(0);

        var commitsOutlier = response.Aggregations.PercentilesBucket("commits_outlier");
        commitsOutlier.Should().NotBeNull();
        commitsOutlier.Items.Should().NotBeNullOrEmpty();
        foreach (var item in commitsOutlier.Items)
            item.Value.Should().BeGreaterThan(0);
    }
}
