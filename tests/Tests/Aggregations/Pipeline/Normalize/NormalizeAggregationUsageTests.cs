/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
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
 using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
 using FluentAssertions;
using Osc;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Aggregations.Pipeline.Normalize
{
	/**
	 * A parent pipeline aggregation which calculates the specific normalized/rescaled value for a specific bucket value.
	 * Values that cannot be normalized, will be skipped using the skip gap policy.
	 */
	public class NormalizeAggregationUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
	{
		public NormalizeAggregationUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object AggregationJson => new
		{
			projects_started_per_month = new
			{
				date_histogram = new
				{
					field = "startedOn",
					calendar_interval = "month",
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
					percent_of_commits = new
					{
						normalize = new
						{
							buckets_path = "commits",
							method = "percent_of_sum",
							format = "00.00%"
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.DateHistogram("projects_started_per_month", dh => dh
				.Field(p => p.StartedOn)
				.CalendarInterval(DateInterval.Month)
				.Aggregations(aa => aa
					.Sum("commits", sm => sm
						.Field(p => p.NumberOfCommits)
					)
					.Normalize("percent_of_commits", aaa => aaa
						.BucketsPath("commits")
						.Method(NormalizeMethod.PercentOfSum)
						.Format("00.00%")
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new DateHistogramAggregation("projects_started_per_month")
			{
				Field = "startedOn",
				CalendarInterval = DateInterval.Month,
				Aggregations = new SumAggregation("commits", "numberOfCommits") &&
					new NormalizeAggregation("percent_of_commits", "commits")
					{
						Method = NormalizeMethod.PercentOfSum,
						Format = "00.00%"
					}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var projectsPerMonth = response.Aggregations.DateHistogram("projects_started_per_month");
			projectsPerMonth.Should().NotBeNull();
			projectsPerMonth.Buckets.Should().NotBeNull();
			projectsPerMonth.Buckets.Count.Should().BeGreaterThan(0);

			foreach (var bucket in projectsPerMonth.Buckets)
			{
				var normalize = bucket.Normalize("percent_of_commits");
				normalize.Value.Should().BeGreaterOrEqualTo(0);
				normalize.ValueAsString.Should().NotBeNullOrEmpty();
			}
		}
	}
}
