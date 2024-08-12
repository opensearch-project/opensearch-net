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
using Tests.Core.Xunit;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Aggregations.Metric.GeoCentroid;

/**
	 * A metric aggregation that computes the weighted centroid from all coordinate values
	 * for a Geo-point datatype field.
	 *
	 * Be sure to read the OpenSearch documentation on {ref_current}/search-aggregations-metrics-geocentroid-aggregation.html[Geo Centroid Aggregation]
	 */
public class GeoCentroidAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public GeoCentroidAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        centroid = new
        {
            geo_centroid = new
            {
                field = "locationPoint"
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .GeoCentroid("centroid", gb => gb
            .Field(p => p.LocationPoint)
        );

    protected override AggregationDictionary InitializerAggs =>
        new GeoCentroidAggregation("centroid", Infer.Field<Project>(p => p.LocationPoint));

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var centroid = response.Aggregations.GeoCentroid("centroid");
        centroid.Should().NotBeNull();
        centroid.Count.Should().BeGreaterThan(0);
        centroid.Location.Should().NotBeNull();

        centroid.Location.Latitude.Should().NotBe(0);
        centroid.Location.Longitude.Should().NotBe(0);
    }
}

/**
	 *[float]
	 *[[geo-centroid-sub-aggregation]]
	 *=== Geo Centroid Sub Aggregation
	 *
	 * The `geo_centroid` aggregation is more interesting when combined as a sub-aggregation to other bucket aggregations
	 */
public class NestedGeoCentroidAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public NestedGeoCentroidAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        projects = new
        {
            terms = new
            {
                field = "name"
            },
            aggs = new
            {
                centroid = new
                {
                    geo_centroid = new
                    {
                        field = "locationPoint"
                    }
                }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Terms("projects", t => t
            .Field(p => p.Name)
            .Aggregations(sa => sa
                .GeoCentroid("centroid", gb => gb
                    .Field(p => p.LocationPoint)
                )
            )
        );

    protected override AggregationDictionary InitializerAggs =>
        new TermsAggregation("projects")
        {
            Field = Infer.Field<Project>(p => p.Name),
            Aggregations = new GeoCentroidAggregation("centroid", Infer.Field<Project>(p => p.LocationPoint))
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();

        var projects = response.Aggregations.Terms("projects");

        foreach (var bucket in projects.Buckets)
        {
            var centroid = bucket.GeoCentroid("centroid");
            centroid.Should().NotBeNull();
            centroid.Count.Should().BeGreaterThan(0);
            centroid.Location.Should().NotBeNull();

            centroid.Location.Latitude.Should().NotBe(0);
            centroid.Location.Longitude.Should().NotBe(0);
        }
    }
}

[NeedsTypedKeys]
public class GeoCentroidNoResultsAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public GeoCentroidNoResultsAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        centroid = new
        {
            geo_centroid = new { field = "locationPoint" }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .GeoCentroid("centroid", gb => gb
            .Field(p => p.LocationPoint)
        );

    protected override AggregationDictionary InitializerAggs =>
        new GeoCentroidAggregation("centroid", Infer.Field<Project>(p => p.LocationPoint));

    protected override QueryContainer QueryScope => new TermQuery { Field = Infer.Field<Project>(p => p.Name), Value = "noresult" };
    protected override object QueryScopeJson { get; } = new { term = new { name = new { value = "noresult" } } };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var centroid = response.Aggregations.GeoCentroid("centroid");
        centroid.Should().NotBeNull();
        centroid.Count.Should().Be(0);
    }
}
