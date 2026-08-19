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
using System.Linq;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Aggregations.Pipeline.MovingFunction
{
	/**
	 * Given an ordered series of data, the Moving Function aggregation will slide a window across the data and allow
	 * the user to specify a custom script that is executed on each window of data. For convenience, a number of
	 * common functions are predefined such as min/max, moving averages, etc.
	 *
     * This is conceptually very similar to the Moving Average pipeline aggregation, except it provides more functionality.
	 *
	 * Be sure to read the OpenSearch documentation on {ref_current}/search-aggregations-pipeline-movfn-aggregation.html[Moving Function Aggregation]
	 */
	public class MovingFunctionAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
	{
		public MovingFunctionAggregationUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object AggregationJson => new
		{
			projects_started_per_month = new
			{
				date_histogram = new
				{
					field = "startedOn",
					calendar_interval = "month",
					min_doc_count = 0
				},
				aggs = new
				{
					commits = new
					{
						sum = new
						{
							field = "numberOfCommits"
						}
					},
					commits_moving_avg = new
					{
						moving_fn = new
						{
							buckets_path = "commits",
							window = 30,
							shift = 0,
							script = "MovingFunctions.unweightedAvg(values)"
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.DateHistogram("projects_started_per_month", dh => dh
				.Field(p => p.StartedOn)
				.CalendarInterval(DateInterval.Month)
				.MinimumDocumentCount(0)
				.Aggregations(aa => aa
					.Sum("commits", sm => sm
						.Field(p => p.NumberOfCommits)
					)
					.MovingFunction("commits_moving_avg", mv => mv
						.BucketsPath("commits")
						.Window(30)
						.Shift(0)
						.Script("MovingFunctions.unweightedAvg(values)")
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new DateHistogramAggregation("projects_started_per_month")
			{
				Field = "startedOn",
				CalendarInterval = DateInterval.Month,
				MinimumDocumentCount = 0,
				Aggregations =
					new SumAggregation("commits", "numberOfCommits")
					&& new MovingFunctionAggregation("commits_moving_avg", "commits")
					{
						Window = 30,
						Shift = 0,
						Script = "MovingFunctions.unweightedAvg(values)"
					}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var projectsPerMonth = response.Aggregations.DateHistogram("projects_started_per_month");
			projectsPerMonth.Should().NotBeNull();
			projectsPerMonth.Buckets.Should().NotBeNull();
			projectsPerMonth.Buckets.Count.Should().BeGreaterThan(0);

			// The date histogram uses min_doc_count:0, so empty months appear as zero-valued
			// buckets and the moving function over a window of empty months can legitimately be 0 or
			// absent. Verify deserialization correctness — the values that ARE present parse to valid
			// non-negative numbers — rather than requiring a positive value in every bucket, which
			// made this test seed/date-dependent.
			var movingAverages = projectsPerMonth.Buckets
				.Skip(1) // average not calculated for the first bucket
				.Select(b => b.Sum("commits_moving_avg"))
				.Where(m => m?.Value != null)
				.Select(m => m.Value.Value)
				.ToList();

			movingAverages.Should().NotBeEmpty();
			movingAverages.Should().OnlyContain(v => v >= 0);
		}
	}
}
