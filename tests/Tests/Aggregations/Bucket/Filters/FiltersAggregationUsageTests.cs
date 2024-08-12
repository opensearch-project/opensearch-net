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
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Aggregations.Bucket.Filters;

/**
	 * Defines a multi bucket aggregations where each bucket is associated with a filter.
	 * Each bucket will collect all documents that match its associated filter. For documents
	 * that do not match any filter, these will be collected in the _other bucket_.
	 *
	 * Be sure to read the OpenSearch documentation {ref_current}/search-aggregations-bucket-filters-aggregation.html[Filters Aggregation]
	*/

/**[float]
	* === Named filters
	*/
public class FiltersAggregationUsageTests : ProjectsOnlyAggregationUsageTestBase
{
    public FiltersAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        projects_by_state = new
        {
            filters = new
            {
                other_bucket = true,
                other_bucket_key = "other_states_of_being",
                filters = new
                {
                    belly_up = new { term = new { state = new { value = "BellyUp" } } },
                    stable = new { term = new { state = new { value = "Stable" } } },
                    very_active = new { term = new { state = new { value = "VeryActive" } } },
                }
            },
            aggs = new
            {
                project_tags = new { terms = new { field = "curatedTags.name.keyword" } }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Filters("projects_by_state", agg => agg
            .OtherBucket()
            .OtherBucketKey("other_states_of_being")
            .NamedFilters(filters => filters
                .Filter("belly_up", f => f.Term(p => p.State, StateOfBeing.BellyUp))
                .Filter("stable", f => f.Term(p => p.State, StateOfBeing.Stable))
                .Filter("very_active", f => f.Term(p => p.State, StateOfBeing.VeryActive))
            )
            .Aggregations(childAggs => childAggs
                .Terms("project_tags", avg => avg.Field(p => p.CuratedTags.First().Name.Suffix("keyword")))
            )
        );

    protected override AggregationDictionary InitializerAggs =>
        new FiltersAggregation("projects_by_state")
        {
            OtherBucket = true,
            OtherBucketKey = "other_states_of_being",
            Filters = new NamedFiltersContainer
            {
                { "belly_up", Query<Project>.Term(p => p.State, StateOfBeing.BellyUp) },
                { "stable", Query<Project>.Term(p => p.State, StateOfBeing.Stable) },
                { "very_active", Query<Project>.Term(p => p.State, StateOfBeing.VeryActive) }
            },
            Aggregations =
                new TermsAggregation("project_tags") { Field = Field<Project>(p => p.CuratedTags.First().Name.Suffix("keyword")) }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        /** ==== Handling Responses
			* The `AggregateDictionary found on `.Aggregations` on `SearchResponse<T>` has several helper methods
			* so we can fetch our aggregation results easily in the correct type.
			 * <<handling-aggregate-response, Be sure to read more about these helper methods>>
			*/
        response.ShouldBeValid();

        var filterAgg = response.Aggregations.Filters("projects_by_state");
        filterAgg.Should().NotBeNull();

        var namedResult = filterAgg.NamedBucket("belly_up");
        namedResult.Should().NotBeNull();
        namedResult.DocCount.Should().BeGreaterThan(0);

        namedResult = filterAgg.NamedBucket("stable");
        namedResult.Should().NotBeNull();
        namedResult.DocCount.Should().BeGreaterThan(0);

        namedResult = filterAgg.NamedBucket("very_active");
        namedResult.Should().NotBeNull();
        namedResult.DocCount.Should().BeGreaterThan(0);

        namedResult = filterAgg.NamedBucket("other_states_of_being");
        namedResult.Should().NotBeNull();
        namedResult.DocCount.Should().Be(0);
    }
}

/**[float]
	*=== Anonymous filters
	*/
public class AnonymousFiltersUsage : ProjectsOnlyAggregationUsageTestBase
{
    public AnonymousFiltersUsage(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        projects_by_state = new
        {
            filters = new
            {
                other_bucket = true,
                filters = new[]
                {
                    new { term = new { state = new { value = "BellyUp" } } },
                    new { term = new { state = new { value = "Stable" } } },
                    new { term = new { state = new { value = "VeryActive" } } },
                }
            },
            aggs = new
            {
                project_tags = new { terms = new { field = "curatedTags.name.keyword" } }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Filters("projects_by_state", agg => agg
            .OtherBucket()
            .AnonymousFilters(
                f => f.Term(p => p.State, StateOfBeing.BellyUp),
                f => f.Term(p => p.State, StateOfBeing.Stable),
                f => f.Term(p => p.State, StateOfBeing.VeryActive)
            )
            .Aggregations(childAggs => childAggs
                .Terms("project_tags", avg => avg.Field(p => p.CuratedTags.First().Name.Suffix("keyword")))
            )
        );

    protected override AggregationDictionary InitializerAggs =>
        new FiltersAggregation("projects_by_state")
        {
            OtherBucket = true,
            Filters = new List<QueryContainer>
            {
                Query<Project>.Term(p => p.State, StateOfBeing.BellyUp),
                Query<Project>.Term(p => p.State, StateOfBeing.Stable),
                Query<Project>.Term(p => p.State, StateOfBeing.VeryActive)
            },
            Aggregations =
                new TermsAggregation("project_tags") { Field = Field<Project>(p => p.CuratedTags.First().Name.Suffix("keyword")) }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        /** ==== Handling Responses
			* The `AggregateDictionary found on `.Aggregations` on `SearchResponse<T>` has several helper methods
			* so we can fetch our aggregation results easily in the correct type.
			 * <<handling-aggregate-response, Be sure to read more about these helper methods>>
			*/
        response.ShouldBeValid();

        var filterAgg = response.Aggregations.Filters("projects_by_state");
        filterAgg.Should().NotBeNull();
        var results = filterAgg.AnonymousBuckets();
        results.Count.Should().Be(4);

        foreach (var singleBucket in results.Take(3)) singleBucket.DocCount.Should().BeGreaterThan(0);

        results.Last().DocCount.Should().Be(0); // <1> The last bucket is the _other bucket_
    }
}
