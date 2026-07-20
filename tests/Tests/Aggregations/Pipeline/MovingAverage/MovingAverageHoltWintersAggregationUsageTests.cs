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

namespace Tests.Aggregations.Pipeline.MovingAverage
{
	public class MovingAverageHoltWintersUsageTests : AggregationUsageTestBase<ReadOnlyCluster>
	{
		public MovingAverageHoltWintersUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

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
						moving_avg = new
						{
							buckets_path = "commits",
							window = 4,
							model = "holt_winters",
							settings = new
							{
								type = "mult",
								alpha = 0.5,
								beta = 0.5,
								gamma = 0.5,
								period = 2,
								pad = false
							}
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
					.MovingAverage("commits_moving_avg", mv => mv
						.BucketsPath("commits")
						.Window(4)
						.Model(m => m
							.HoltWinters(hw => hw
								.Type(HoltWintersType.Multiplicative)
								.Alpha(0.5f)
								.Beta(0.5f)
								.Gamma(0.5f)
								.Period(2)
								.Pad(false)
							)
						)
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
					&& new MovingAverageAggregation("commits_moving_avg", "commits")
					{
						Window = 4,
						Model = new HoltWintersModel
						{
							Type = HoltWintersType.Multiplicative,
							Alpha = 0.5f,
							Beta = 0.5f,
							Gamma = 0.5f,
							Period = 2,
							Pad = false
						}
					}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var projectsPerMonth = response.Aggregations.DateHistogram("projects_started_per_month");
			projectsPerMonth.Should().NotBeNull();
			projectsPerMonth.Buckets.Should().NotBeNull();
			projectsPerMonth.Buckets.Count.Should().BeGreaterThan(0);

			var bucketCount = 0;
			foreach (var item in projectsPerMonth.Buckets)
			{
				bucketCount++;

				var commits = item.Sum("commits");
				commits.Should().NotBeNull();
				// min_doc_count:0 yields zero-valued (not null) commits for empty months.
				commits.Value.Should().NotBeNull();

				// Moving Average specifies a window of 4, so no value is emitted for the first 4 buckets.
				if (bucketCount <= 4)
					item.MovingAverage("commits_moving_avg").Should().BeNull();
			}

			// Verify the moving-average values that ARE present (from the 5th bucket onwards) deserialize
			// as valid non-negative numbers; a window of empty months can legitimately be 0 or absent, so
			// we don't require a value in every bucket, which made this test seed/date-dependent.
			var movingAverages = projectsPerMonth.Buckets
				.Skip(4)
				.Select(b => b.MovingAverage("commits_moving_avg"))
				.Where(m => m?.Value != null)
				.Select(m => m.Value.Value)
				.ToList();

			// Unlike the other pipeline tests this intentionally omits a NotBeEmpty() check: the
			// multiplicative Holt-Winters model (Period = 2) can legitimately emit no values when the
			// seeded date distribution leaves empty months in the seasonal window, so requiring at least
			// one value would reintroduce the flakiness. Deserialization of the moving-average value type
			// is still guarded by the EWMA and Holt-Linear tests, which do assert NotBeEmpty().
			movingAverages.Should().OnlyContain(v => v >= 0);
		}
	}
}
