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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Aggregations.Bucket.Histogram;

public class HistogramAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public HistogramAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        commits = new
        {
            histogram = new
            {
                field = "numberOfCommits",
                interval = 100.0,
                min_doc_count = 1,
                order = new
                {
                    _key = "desc"
                },
                offset = 1.1

            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Histogram("commits", h => h
            .Field(p => p.NumberOfCommits)
            .Interval(100)
            .MinimumDocumentCount(1)
            .Order(HistogramOrder.KeyDescending)
            .Offset(1.1)
        );

    protected override AggregationDictionary InitializerAggs =>
        new HistogramAggregation("commits")
        {
            Field = Field<Project>(p => p.NumberOfCommits),
            Interval = 100,
            MinimumDocumentCount = 1,
            Order = HistogramOrder.KeyDescending,
            Offset = 1.1
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var commits = response.Aggregations.Histogram("commits");
        commits.Should().NotBeNull();
        commits.Buckets.Should().NotBeNull();
        commits.Buckets.Count.Should().BeGreaterThan(0);
        foreach (var item in commits.Buckets)
            item.DocCount.Should().BeGreaterThan(0);
    }
}

public class HistogramAggregationWithHardBoundsUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    private const double HardBoundsMinimum = 100;
    private const double HardBoundsMaximum = 300;

    public HistogramAggregationWithHardBoundsUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        commits = new
        {
            histogram = new
            {
                field = "numberOfCommits",
                hard_bounds = new { min = HardBoundsMinimum, max = HardBoundsMaximum },
                interval = 100.0,
                min_doc_count = 1,
                order = new
                {
                    _key = "desc"
                }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Histogram("commits", h => h
            .Field(p => p.NumberOfCommits)
            .Interval(100)
            .MinimumDocumentCount(1)
            .Order(HistogramOrder.KeyDescending)
            .HardBounds(HardBoundsMinimum, HardBoundsMaximum)
        );

    protected override AggregationDictionary InitializerAggs =>
        new HistogramAggregation("commits")
        {
            Field = Field<Project>(p => p.NumberOfCommits),
            Interval = 100,
            MinimumDocumentCount = 1,
            Order = HistogramOrder.KeyDescending,
            HardBounds = new HardBounds<double>
            {
                Minimum = HardBoundsMinimum,
                Maximum = HardBoundsMaximum
            }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var commits = response.Aggregations.Histogram("commits");
        commits.Should().NotBeNull();
        commits.Buckets.Should().NotBeNull();

        foreach (var bucket in commits.Buckets)
            bucket.Key.Should().BeGreaterOrEqualTo(HardBoundsMinimum).And.BeLessOrEqualTo(HardBoundsMaximum);
    }
}
