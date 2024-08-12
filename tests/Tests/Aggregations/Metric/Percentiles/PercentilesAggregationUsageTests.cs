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
using static OpenSearch.Client.Infer;

namespace Tests.Aggregations.Metric.Percentiles;

public class PercentilesAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public PercentilesAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        commits_outlier = new
        {
            percentiles = new
            {
                field = "numberOfCommits",
                percents = new[] { 95.0, 99.0, 99.9 },
                hdr = new
                {
                    number_of_significant_value_digits = 3
                },
                script = new
                {
                    source = "doc['numberOfCommits'].value * 1.2",
                },
                missing = 0.0
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Percentiles("commits_outlier", pr => pr
            .Field(p => p.NumberOfCommits)
            .Percents(95, 99, 99.9)
            .Method(m => m
                .HDRHistogram(hdr => hdr
                    .NumberOfSignificantValueDigits(3)
                )
            )
            .Script(ss => ss.Source("doc['numberOfCommits'].value * 1.2"))
            .Missing(0)
        );

    protected override AggregationDictionary InitializerAggs =>
        new PercentilesAggregation("commits_outlier", Field<Project>(p => p.NumberOfCommits))
        {
            Percents = new[] { 95, 99, 99.9 },
            Method = new HDRHistogramMethod
            {
                NumberOfSignificantValueDigits = 3
            },
            Script = new InlineScript("doc['numberOfCommits'].value * 1.2"),
            Missing = 0
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var commitsOutlier = response.Aggregations.Percentiles("commits_outlier");
        commitsOutlier.Should().NotBeNull();
        commitsOutlier.Items.Should().NotBeNullOrEmpty();
        foreach (var item in commitsOutlier.Items)
            item.Value.Should().BeGreaterThan(0);
    }
}

// hide
public class PercentilesAggregationNonKeyedValuesUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public PercentilesAggregationNonKeyedValuesUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        commits_outlier = new
        {
            percentiles = new
            {
                field = "numberOfCommits",
                percents = new[] { 95.0, 99.0, 99.9 },
                hdr = new
                {
                    number_of_significant_value_digits = 3
                },
                script = new
                {
                    source = "doc['numberOfCommits'].value * 1.2",
                },
                missing = 0.0,
                keyed = false
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Percentiles("commits_outlier", pr => pr
            .Field(p => p.NumberOfCommits)
            .Percents(95, 99, 99.9)
            .Method(m => m
                .HDRHistogram(hdr => hdr
                    .NumberOfSignificantValueDigits(3)
                )
            )
            .Script(ss => ss.Source("doc['numberOfCommits'].value * 1.2"))
            .Missing(0)
            .Keyed(false)
        );

    protected override AggregationDictionary InitializerAggs =>
        new PercentilesAggregation("commits_outlier", Field<Project>(p => p.NumberOfCommits))
        {
            Percents = new[] { 95, 99, 99.9 },
            Method = new HDRHistogramMethod
            {
                NumberOfSignificantValueDigits = 3
            },
            Script = new InlineScript("doc['numberOfCommits'].value * 1.2"),
            Missing = 0,
            Keyed = false
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var commitsOutlier = response.Aggregations.Percentiles("commits_outlier");
        commitsOutlier.Should().NotBeNull();
        commitsOutlier.Items.Should().NotBeNullOrEmpty();
        foreach (var item in commitsOutlier.Items)
            item.Value.Should().BeGreaterThan(0);
    }
}
