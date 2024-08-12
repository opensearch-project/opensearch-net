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

namespace Tests.Aggregations.Bucket.GeoHashGrid;

public class GeoHashGridAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public GeoHashGridAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        my_geohash_grid = new
        {
            geohash_grid = new
            {
                field = "locationPoint",
                precision = 3,
                size = 1000,
                shard_size = 100
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .GeoHash("my_geohash_grid", g => g
            .Field(p => p.LocationPoint)
            .GeoHashPrecision(GeoHashPrecision.Precision3)
            .Size(1000)
            .ShardSize(100)
        );

    protected override AggregationDictionary InitializerAggs =>
        new GeoHashGridAggregation("my_geohash_grid")
        {
            Field = Field<Project>(p => p.LocationPoint),
            Precision = GeoHashPrecision.Precision3,
            Size = 1000,
            ShardSize = 100
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var myGeoHashGrid = response.Aggregations.GeoHash("my_geohash_grid");
        myGeoHashGrid.Should().NotBeNull();
    }
}

// hide
public class GeoHashGridAggregationWithBoundsUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public GeoHashGridAggregationWithBoundsUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        my_geohash_grid = new
        {
            geohash_grid = new
            {
                field = "locationPoint",
                bounds = new
                {
                    top_left = new
                    {
                        lat = 90.0,
                        lon = -180.0
                    },
                    bottom_right = new
                    {
                        lat = -90.0,
                        lon = 180.0
                    }
                }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .GeoHash("my_geohash_grid", g => g
            .Field(p => p.LocationPoint)
            .Bounds(b => b
                .TopLeft(90, -180)
                .BottomRight(-90, 180)
            )
        );

    protected override AggregationDictionary InitializerAggs =>
        new GeoHashGridAggregation("my_geohash_grid")
        {
            Field = Field<Project>(p => p.LocationPoint),
            Bounds = new BoundingBox
            {
                TopLeft = new GeoLocation(90, -180),
                BottomRight = new GeoLocation(-90, 180)
            }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var myGeoHashGrid = response.Aggregations.GeoHash("my_geohash_grid");
        myGeoHashGrid.Should().NotBeNull();
    }
}
