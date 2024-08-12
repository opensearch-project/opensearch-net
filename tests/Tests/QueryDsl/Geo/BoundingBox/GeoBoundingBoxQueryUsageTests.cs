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

using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.Geo.BoundingBox;

public class GeoBoundingBoxQueryUsageTests : QueryDslUsageTestsBase
{
    public GeoBoundingBoxQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IGeoBoundingBoxQuery>(a => a.GeoBoundingBox)
    {
        q => q.BoundingBox = null,
        q => q.BoundingBox = new OpenSearch.Client.BoundingBox(),
        q => q.Field = null
    };

    protected override QueryContainer QueryInitializer => new GeoBoundingBoxQuery
    {
        Boost = 1.1,
        Name = "named_query",
        Field = Infer.Field<Project>(p => p.LocationPoint),
        BoundingBox = new OpenSearch.Client.BoundingBox
        {
            TopLeft = new GeoLocation(34, -34),
            BottomRight = new GeoLocation(-34, 34),
        },
        Type = GeoExecution.Indexed,
        ValidationMethod = GeoValidationMethod.Strict
    };

    protected override object QueryJson => new
    {
        geo_bounding_box = new
        {
            type = "indexed",
            validation_method = "strict",
            _name = "named_query",
            boost = 1.1,
            locationPoint = new
            {
                top_left = new
                {
                    lat = 34.0,
                    lon = -34.0
                },
                bottom_right = new
                {
                    lat = -34.0,
                    lon = 34.0
                }
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .GeoBoundingBox(g => g
            .Boost(1.1)
            .Name("named_query")
            .Field(p => p.LocationPoint)
            .BoundingBox(b => b
                .TopLeft(34, -34)
                .BottomRight(-34, 34)
            )
            .ValidationMethod(GeoValidationMethod.Strict)
            .Type(GeoExecution.Indexed)
        );
}

public class GeoBoundingBoxWKTQueryUsageTests : QueryDslUsageTestsBase
{
    public GeoBoundingBoxWKTQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IGeoBoundingBoxQuery>(a => a.GeoBoundingBox)
    {
        q => q.BoundingBox = null,
        q => q.BoundingBox = new OpenSearch.Client.BoundingBox(),
        q => q.Field = null
    };

    protected override QueryContainer QueryInitializer => new GeoBoundingBoxQuery
    {
        Boost = 1.1,
        Name = "named_query",
        Field = Infer.Field<Project>(p => p.LocationPoint),
        BoundingBox = new OpenSearch.Client.BoundingBox
        {
            WellKnownText = "BBOX (-34, 34, 34, -34)"
        },
        Type = GeoExecution.Indexed,
        ValidationMethod = GeoValidationMethod.Strict
    };

    protected override object QueryJson => new
    {
        geo_bounding_box = new
        {
            type = "indexed",
            validation_method = "strict",
            _name = "named_query",
            boost = 1.1,
            locationPoint = new
            {
                wkt = "BBOX (-34, 34, 34, -34)"
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .GeoBoundingBox(g => g
            .Boost(1.1)
            .Name("named_query")
            .Field(p => p.LocationPoint)
            .BoundingBox(b => b
                .WellKnownText("BBOX (-34, 34, 34, -34)")
            )
            .ValidationMethod(GeoValidationMethod.Strict)
            .Type(GeoExecution.Indexed)
        );
}
