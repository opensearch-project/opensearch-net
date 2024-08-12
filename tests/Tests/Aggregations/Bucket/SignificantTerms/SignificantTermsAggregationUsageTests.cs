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

namespace Tests.Aggregations.Bucket.SignificantTerms;

/**
	 * An aggregation that returns interesting or unusual occurrences of terms in a set.
	 *
	 * [WARNING]
	 * --
	 * The significant_terms aggregation can be very heavy when run on large indices. Work is in progress
	 * to provide more lightweight sampling techniques.
	 * As a result, the API for this feature may change in non-backwards compatible ways
	 * --
	 *
	 * See the OpenSearch documentation on {ref_current}/search-aggregations-bucket-significantterms-aggregation.html[significant terms aggregation] for more detail.
	 */
public class SignificantTermsAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public SignificantTermsAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        significant_names = new
        {
            significant_terms = new
            {
                field = "name",
                min_doc_count = 10,
                mutual_information = new
                {
                    background_is_superset = true,
                    include_negatives = true
                }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .SignificantTerms("significant_names", st => st
            .Field(p => p.Name)
            .MinimumDocumentCount(10)
            .MutualInformation(mi => mi
                .BackgroundIsSuperSet()
                .IncludeNegatives()
            )
        );

    protected override AggregationDictionary InitializerAggs =>
        new SignificantTermsAggregation("significant_names")
        {
            Field = Field<Project>(p => p.Name),
            MinimumDocumentCount = 10,
            MutualInformation = new MutualInformationHeuristic
            {
                BackgroundIsSuperSet = true,
                IncludeNegatives = true
            }
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var sigNames = response.Aggregations.SignificantTerms("significant_names");
        sigNames.Should().NotBeNull();
        sigNames.DocCount.Should().BeGreaterThan(0);
    }
}

/**
	 * [float]
	 * [[significant-terms-pattern-filter]]
	 * == Filtering with a regular expression pattern
	 *
	 * Using significant terms aggregation with filtering to include values using a regular expression pattern
	 */
public class SignificantTermsIncludePatternAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public SignificantTermsIncludePatternAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        significant_names = new
        {
            significant_terms = new
            {
                field = "name",
                min_doc_count = 10,
                mutual_information = new
                {
                    background_is_superset = true,
                    include_negatives = true
                },
                include = "pi*"
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .SignificantTerms("significant_names", st => st
            .Field(p => p.Name)
            .MinimumDocumentCount(10)
            .MutualInformation(mi => mi
                .BackgroundIsSuperSet()
                .IncludeNegatives()
            )
            .Include("pi*")
        );

    protected override AggregationDictionary InitializerAggs =>
        new SignificantTermsAggregation("significant_names")
        {
            Field = Field<Project>(p => p.Name),
            MinimumDocumentCount = 10,
            MutualInformation = new MutualInformationHeuristic
            {
                BackgroundIsSuperSet = true,
                IncludeNegatives = true
            },
            Include = new IncludeExclude("pi*")
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var sigNames = response.Aggregations.SignificantTerms("significant_names");
        sigNames.Should().NotBeNull();
        sigNames.DocCount.Should().BeGreaterThan(0);
    }
}

/**
	 * [float]
	 * [[significant-terms-exact-value-filter]]
	 * == Filtering with exact values
	 *
	 * Using significant terms aggregation with filtering to exclude specific values
	 */
public class SignificantTermsExcludeExactValuesAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public SignificantTermsExcludeExactValuesAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        significant_names = new
        {
            significant_terms = new
            {
                field = "name",
                min_doc_count = 10,
                mutual_information = new
                {
                    background_is_superset = true,
                    include_negatives = true
                },
                exclude = new[] { "pierce" }
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .SignificantTerms("significant_names", st => st
            .Field(p => p.Name)
            .MinimumDocumentCount(10)
            .MutualInformation(mi => mi
                .BackgroundIsSuperSet()
                .IncludeNegatives()
            )
            .Exclude(new[] { "pierce" })
        );

    protected override AggregationDictionary InitializerAggs =>
        new SignificantTermsAggregation("significant_names")
        {
            Field = Field<Project>(p => p.Name),
            MinimumDocumentCount = 10,
            MutualInformation = new MutualInformationHeuristic
            {
                BackgroundIsSuperSet = true,
                IncludeNegatives = true
            },
            Exclude = new IncludeExclude(new[] { "pierce" })
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var sigNames = response.Aggregations.SignificantTerms("significant_names");
        sigNames.Should().NotBeNull();
        sigNames.DocCount.Should().BeGreaterThan(0);
    }
}

/**
	 * [float]
	 * [[significant-terms-numeric-field]]
	 * == Numeric fields
	 *
	 * A significant terms aggregation on a numeric field
	 */
public class NumericSignificantTermsAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
{
    public NumericSignificantTermsAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override object AggregationJson => new
    {
        commits = new
        {
            significant_terms = new
            {
                field = "numberOfContributors"
            }
        }
    };

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .SignificantTerms("commits", st => st
            .Field(p => p.NumberOfContributors)
        );

    protected override AggregationDictionary InitializerAggs =>
        new SignificantTermsAggregation("commits")
        {
            Field = Field<Project, int>(p => p.NumberOfContributors)
        };

    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();
        var commits = response.Aggregations.SignificantTerms<int>("commits");
        commits.Should().NotBeNull();
        commits.Buckets.Should().NotBeNull();
        commits.Buckets.Count.Should().BeGreaterThan(0);
        foreach (var item in commits.Buckets)
        {
            item.Key.Should().BeGreaterThan(0);
            item.DocCount.Should().BeGreaterOrEqualTo(1);
        }
    }
}
