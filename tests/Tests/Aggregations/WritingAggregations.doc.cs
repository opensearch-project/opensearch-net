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
using Tests.Aggregations.Bucket.Children;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.EndpointTests.TestState;
using Tests.IndexModules;
using static OpenSearch.Client.Infer;

namespace Tests.Aggregations;

/**=== Writing aggregations
	* OSC allows you to write your aggregations using
	*
	* - a strict fluent DSL
	* - a verbatim object initializer syntax that maps verbatim to the OpenSearch API
	* - a more terse object initializer aggregation DSL
	*
	* Three different ways, yikes that's a lot to take in! Let's go over them one at a time and explain when you might
	* want to use each.
	*/
public class Usage : UsageTestBase<ISearchRequest, SearchDescriptor<Project>, SearchRequest<Project>>
{
    /**
		* This is the json output for each example
		**/
    protected override object ExpectJson => new
    {
        aggs = new
        {
            name_of_child_agg = new
            {
                children = new
                {
                    type = "commits"
                },
                aggs = new
                {
                    average_per_child = new
                    {
                        avg = new
                        {
                            field = "confidenceFactor"
                        }
                    },
                    max_per_child = new
                    {
                        max = new
                        {
                            field = "confidenceFactor"
                        }
                    },
                    min_per_child = new
                    {
                        min = new
                        {
                            field = "confidenceFactor"
                        }
                    }
                }
            }
        }
    };

    /**[float]
		* === Fluent DSL
		* The fluent lambda syntax is the most terse way to write aggregations.
		* It benefits from types that are carried over to sub aggregations
		*/
    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .Aggregations(aggs => aggs
            .Children<CommitActivity>("name_of_child_agg", child => child
                .Aggregations(childAggs => childAggs
                    .Average("average_per_child", avg => avg.Field(p => p.ConfidenceFactor))
                    .Max("max_per_child", avg => avg.Field(p => p.ConfidenceFactor))
                    .Min("min_per_child", avg => avg.Field(p => p.ConfidenceFactor))
                )
            )
        );

    /**[float]
		* === Object Initializer syntax
		* The object initializer syntax (OIS) is a one-to-one mapping with how aggregations
		* have to be represented in the OpenSearch API. While it has the benefit of being a one-to-one
		* mapping, being dictionary based in C# means it can grow verbose rather quickly.
		*
		* Here are the same aggregations as expressed in the Fluent API above, with the dictionary-based
		* object initializer syntax
		*/
    protected override SearchRequest<Project> Initializer =>
        new SearchRequest<Project>
        {
            Aggregations = new AggregationDictionary
            {
                {
                    "name_of_child_agg", new ChildrenAggregation("name_of_child_agg", typeof(CommitActivity))
                    {
                        Aggregations = new AggregationDictionary
                        {
                            {"average_per_child", new AverageAggregation("average_per_child", "confidenceFactor")},
                            {"max_per_child", new MaxAggregation("max_per_child", "confidenceFactor")},
                            {"min_per_child", new MinAggregation("min_per_child", "confidenceFactor")},
                        }
                    }
                }
            }
        };
    /**
		 * As you can see, the key in the dictionary is repeated as the name passed to the aggregation constructor.
		 * This starts to get hard to read and a little error prone, wouldn't you agree?
		 *
		 * There is a better way however...
		 */
}

public class AggregationDslUsage : Usage
{
    /**[float]
		* === Terse Object Initializer syntax
		* The Object Initializer syntax can be shortened dramatically by using `*Aggregation` types directly,
		* allowing you to forego the need to introduce intermediary dictionaries to represent the aggregation DSL.
		* In using these, it is also possible to combine multiple aggregations using the bitwise `&&` operator.
		*
		* Compare the following example with the previous vanilla Object Initializer syntax
		*/
    protected override SearchRequest<Project> Initializer =>
        new SearchRequest<Project>
        {
            Aggregations = new ChildrenAggregation("name_of_child_agg", typeof(CommitActivity))
            {
                Aggregations =
                    new AverageAggregation("average_per_child", Field<CommitActivity>(p => p.ConfidenceFactor))
                    && new MaxAggregation("max_per_child", Field<CommitActivity>(p => p.ConfidenceFactor))
                    && new MinAggregation("min_per_child", Field<CommitActivity>(p => p.ConfidenceFactor))
            }
        };

    /** Now that's much cleaner! Assigning an `*Aggregation` type directly to the `Aggregation` property
		 *  on a search request works because there are implicit conversions within OSC to handle this for you.
		 */
}

public class AggregationDslMixedUsage : Usage
{
    /**[float]
		* === Mixed usage of object initializer and fluent
		*
		* Sometimes its useful to mix and match fluent and object initializer, the fluent Aggregations method therefore
		* also accepts `AggregationDictionary` directly.
		*/
    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .Aggregations(new ChildrenAggregation("name_of_child_agg", typeof(CommitActivity))
        {
            Aggregations =
                new AverageAggregation("average_per_child", Field<CommitActivity>(p => p.ConfidenceFactor))
                && new MaxAggregation("max_per_child", Field<CommitActivity>(p => p.ConfidenceFactor))
                && new MinAggregation("min_per_child", Field<CommitActivity>(p => p.ConfidenceFactor))
        });
}

public class ReusingTheSameDescriptorWithBitwiseOperationsCombinatoryUsage : Usage
{
    /**[float]
		* === Binary operators off the same descriptor
		*
		* For dynamic aggregation building using the fluent syntax,
		* it can be useful to abstract the construction to methods as much as possible.
		* You can use the binary operator `&&` on the same aggregation descriptor to compose the graph.
		* Each side of the binary operation can return null dynamically.
		*/
    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .Aggregations(aggs => aggs
            .Children<CommitActivity>("name_of_child_agg", child => child
                .Aggregations(Combine)
            )
        );

    protected IAggregationContainer Combine(AggregationContainerDescriptor<CommitActivity> aggs) => aggs
        .Average("average_per_child", avg => avg.Field(p => p.ConfidenceFactor))
        && MaxPerChild(aggs)
        && MinPerChild(aggs)
        && null;

    private static AggregationContainerDescriptor<CommitActivity> MinPerChild(AggregationContainerDescriptor<CommitActivity> aggs) =>
        aggs.Min("min_per_child", avg => avg.Field(p => p.ConfidenceFactor));

    private static AggregationContainerDescriptor<CommitActivity> MaxPerChild(AggregationContainerDescriptor<CommitActivity> aggs) =>
        aggs.Max("max_per_child", avg => avg.Field(p => p.ConfidenceFactor));
}

public class OisInsideLambdaCombinatoryUsage : Usage
{
    /**[float]
		* === Returning a different AggregationContainer in fluent syntax
		*
		* All the fluent selector expects is an `IAggregationContainer` to be returned. You could abstract this to a
		* method returning `AggregationContainer` which is free to use the object initializer syntax
		* to compose that `AggregationContainer`.
		*/
    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .Aggregations(aggs => aggs
            .Children<CommitActivity>("name_of_child_agg", child => child
                .Aggregations(childAggs => Combine())
            )
        );

    protected AggregationContainer Combine() =>
        new AverageAggregation("average_per_child", Field<CommitActivity>(p => p.ConfidenceFactor))
        && new MaxAggregation("max_per_child", Field<CommitActivity>(p => p.ConfidenceFactor))
        && new MinAggregation("min_per_child", Field<CommitActivity>(p => p.ConfidenceFactor))
        && null;

}

public class AdvancedAggregationDslUsage : Usage
{
    /**[float]
		* === Aggregating over a collection of aggregations
		* An advanced scenario may involve an existing collection of aggregation functions that should be set as aggregations
		* on the request. Using LINQ's `.Aggregate()` method, each function can be applied to the aggregation descriptor
		* (`childAggs` below) in turn, returning the descriptor after each function application.
		*
		*/
    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent
    {
        get
        {
            var aggregations =
                new List<Func<AggregationContainerDescriptor<CommitActivity>, IAggregationContainer>> //<1> a list of aggregation functions to apply
					{
                    a => a.Average("average_per_child", avg => avg.Field(p => p.ConfidenceFactor)),
                    a => a.Max("max_per_child", avg => avg.Field(p => p.ConfidenceFactor)),
                    a => a.Min("min_per_child", avg => avg.Field(p => p.ConfidenceFactor))
                };

            return s => s
                .Aggregations(aggs => aggs
                    .Children<CommitActivity>("name_of_child_agg", child => child
                        .Aggregations(childAggs =>
                                aggregations.Aggregate(childAggs, (acc, agg) =>
                                {
                                    agg(acc);
                                    return acc;
                                }) // <2> Using LINQ's `Aggregate()` function to accumulate/apply all of the aggregation functions
                        )
                    )
                );
        }
    }
}


/**[float]
	* [[handling-aggregate-response]]
	*=== Handling responses
	*
	* The `SearchResponse` exposes an `AggregateDictionary` which is specialized dictionary over `<string, IAggregate>` that also
	* exposes handy helper methods that automatically cast `IAggregate` to the expected aggregate response.
	*
	* Let's see this in action:
	*/
public class ChildrenAggregationFluentAggsUsageTests : ChildrenAggregationUsageTests
{
    public ChildrenAggregationFluentAggsUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
        .Children<CommitActivity>("name_of_child_agg", child => child
            .Aggregations(childAggs => childAggs
                .Average("average_per_child", avg => avg.Field(p => p.ConfidenceFactor))
                .Max("max_per_child", avg => avg.Field(p => p.ConfidenceFactor))
                .Min("min_per_child", avg => avg.Field(p => p.ConfidenceFactor))
            )
        );

    /**
		* Now, using `.Aggregations`, we can easily get the `Children` aggregation response out and from that,
		* the `Average` and `Max` sub aggregations.
		*/
    protected override void ExpectResponse(ISearchResponse<Project> response)
    {
        response.ShouldBeValid();

        var childAggregation = response.Aggregations.Children("name_of_child_agg");

        var averagePerChild = childAggregation.Average("average_per_child");

        averagePerChild.Should().NotBeNull(); //<1> Do something with the average per child. Here we just assert it's not null

        var maxPerChild = childAggregation.Max("max_per_child");

        maxPerChild.Should().NotBeNull(); //<2> Do something with the max per child. Here we just assert it's not null
    }
}
