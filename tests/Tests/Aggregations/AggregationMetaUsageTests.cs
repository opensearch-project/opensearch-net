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
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Aggregations;

/**
	*=== Aggregation Metadata
	* Metadata can be provided per aggregation, and will be returned in the aggregation response
	*/
public class AggregationMetaUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public AggregationMetaUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        min_last_activity = new
        {
            min = new
            {
                field = "lastActivity"
            },
            meta = new
            {
                meta_1 = "value_1",
                meta_2 = 2,
                meta_3 = new
                {
                    meta_3 = "value_3"
                }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Min("min_last_activity", m => m
            .Field(p => p.LastActivity)
            .Meta(d => d
                .Add("meta_1", "value_1")
                .Add("meta_2", 2)
                .Add("meta_3", new { meta_3 = "value_3" })
            )
        );

    protected override AggregationDictionary InitializerAggs =>
        new MinAggregation("min_last_activity", Infer.Field<Project>(p => p.LastActivity))
        {
            Meta = new Dictionary<string, object>
            {
                { "meta_1", "value_1" },
                { "meta_2", 2 },
                { "meta_3", new { meta_3 = "value_3" } }
            }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var min = response.Aggregations.Min("min_last_activity");
        min.Meta.Should().NotBeNull().And.ContainKeys("meta_1", "meta_2", "meta_3");
    }
}
