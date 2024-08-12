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

namespace Tests.Aggregations.Bucket.Nested;

public class NestedAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public NestedAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        tags = new
        {
            nested = new
            {
                path = "tags",
            },
            aggs = new
            {
                tag_names = new
                {
                    terms = new
                    {
                        field = "tags.name"
                    }
                }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Nested("tags", n => n
            .Path(p => p.Tags)
            .Aggregations(aa => aa
                .Terms("tag_names", t => t
                    .Field(p => p.Tags.Suffix("name"))
                )
            )
        );

    protected override AggregationDictionary InitializerAggs =>
        new NestedAggregation("tags")
        {
            Path = "tags",
            Aggregations = new TermsAggregation("tag_names")
            {
                Field = "tags.name"
            }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var tags = response.Aggregations.Nested("tags");
        tags.Should().NotBeNull();
        var tagNames = tags.Terms("tag_names");
        tagNames.Should().NotBeNull();
        foreach (var item in tagNames.Buckets)
        {
            item.Key.Should().NotBeNullOrEmpty();
            item.DocCount.Should().BeGreaterThan(0);
        }
    }
}
