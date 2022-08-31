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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Configuration;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Aggregations.Bucket.Composite
{
	/**
	 * A multi-bucket aggregation that creates composite buckets from different sources.
     *
     * Unlike the other multi-bucket aggregation the composite aggregation can be
	 * used to paginate all buckets from a multi-level aggregation efficiently.
	 * This aggregation provides a way to stream all buckets of a specific aggregation
	 * similarly to what scroll does for documents.
     *
     * The composite buckets are built from the combinations of the values extracted/created
	 * for each document and each combination is considered as a composite bucket.
	 *
	 * Be sure to read the OpenSearch documentation on {ref_current}/search-aggregations-bucket-composite-aggregation.html[Composite Aggregation].
	*/
	public class CompositeAggregationUsageTests : ProjectsOnlyAggregationUsageTestBase
	{
		public CompositeAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object AggregationJson => new
		{
			my_buckets = new
			{
				composite = new
				{
					sources = new object[]
					{
						new
						{
							branches = new
							{
								terms = new
								{
									field = "branches.keyword"
								}
							}
						},
						new
						{
							started = new
							{
								date_histogram = new
								{
									field = "startedOn",
									calendar_interval = "month"
								}
							}
						},
						new
						{
							branch_count = new
							{
								histogram = new
								{
									field = "requiredBranches",
									interval = 1d
								}
							}
						},
						new
						{
							geo = new
							{
								geotile_grid = new
								{
									field = "locationPoint",
									precision = 12
								}
							}
						},
					}
				},
				aggs = new
				{
					project_tags = new
					{
						nested = new
						{
							path = "tags"
						},
						aggs = new
						{
							tags = new
							{
								terms = new { field = "tags.name" }
							}
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.Composite("my_buckets", date => date
				.Sources(s => s
					.Terms("branches", t => t
						.Field(f => f.Branches.Suffix("keyword"))
					)
					.DateHistogram("started", d => d
						.Field(f => f.StartedOn)
						.CalendarInterval(DateInterval.Month)
					)
					.Histogram("branch_count", h => h
						.Field(f => f.RequiredBranches)
						.Interval(1)
					)
					.GeoTileGrid("geo", h => h
						.Field(f => f.LocationPoint)
						.Precision(GeoTilePrecision.Precision12)
					)
				)
				.Aggregations(childAggs => childAggs
					.Nested("project_tags", n => n
						.Path(p => p.Tags)
						.Aggregations(nestedAggs => nestedAggs
							.Terms("tags", avg => avg.Field(p => p.Tags.First().Name))
						)
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new CompositeAggregation("my_buckets")
			{
				Sources = new List<ICompositeAggregationSource>
				{
					new TermsCompositeAggregationSource("branches")
					{
						Field = Field<Project>(f => f.Branches.Suffix("keyword"))
					},
					new DateHistogramCompositeAggregationSource("started")
					{
						Field = Field<Project>(f => f.StartedOn),
						CalendarInterval = DateInterval.Month
					},
					new HistogramCompositeAggregationSource("branch_count")
					{
						Field = Field<Project>(f => f.RequiredBranches),
						Interval = 1
					},
					new GeoTileGridCompositeAggregationSource("geo")
					{
						Field = Field<Project>(f => f.LocationPoint),
						Precision = GeoTilePrecision.Precision12
					}
				},
				Aggregations = new NestedAggregation("project_tags")
				{
					Path = Field<Project>(p => p.Tags),
					Aggregations = new TermsAggregation("tags")
					{
						Field = Field<Project>(p => p.Tags.First().Name)
					}
				}
			};

		/**==== Handling Responses
		 * Each Composite aggregation bucket key is a `CompositeKey` type, a specialized
		 * `IReadOnlyDictionary<string, object>` type with methods to convert values to supported types
		 */
		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var composite = response.Aggregations.Composite("my_buckets");
			composite.Should().NotBeNull();
			composite.Buckets.Should().NotBeNullOrEmpty();
			composite.AfterKey.Should().NotBeNull();
			composite.AfterKey.Should()
				.HaveCount(4)
				.And.ContainKeys("branches", "started", "branch_count", "geo");
			foreach (var item in composite.Buckets)
			{
				var key = item.Key;
				key.Should().NotBeNull();

				key.TryGetValue("branches", out string branches).Should().BeTrue();
				branches.Should().NotBeNullOrEmpty();

				key.TryGetValue("started", out DateTime started).Should().BeTrue();
				started.Should().BeAfter(default(DateTime));

				key.TryGetValue("branch_count", out int branchCount).Should().BeTrue();
				branchCount.Should().BeGreaterThan(0);

				item.DocCount.Should().BeGreaterThan(0);

				var nested = item.Nested("project_tags");
				nested.Should().NotBeNull();

				if (nested.DocCount > 0)
				{
					var nestedTerms = nested.Terms("tags");
					nestedTerms.Buckets.Count.Should().BeGreaterThan(0);
				}
			}
		}
	}

	/**[float]
	* === Missing buckets
	* By default documents without a value for a given source are ignored.
	* It is possible to include them in the response by setting missing_bucket to `true` (defaults to `false`):
	*/
	public class CompositeAggregationMissingBucketUsageTests : ProjectsOnlyAggregationUsageTestBase
	{
		public CompositeAggregationMissingBucketUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object AggregationJson => new
		{
			my_buckets = new
			{
				composite = new
				{
					sources = new object[]
					{
						new
						{
							branches = new
							{
								terms = new
								{
									field = "branches.keyword",
									order = "asc",
									missing_bucket = true
								}
							}
						},
					}
				},
				aggs = new
				{
					project_tags = new
					{
						nested = new { path = "tags" },
						aggs = new
						{
							tags = new { terms = new { field = "tags.name" } }
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.Composite("my_buckets", date => date
				.Sources(s => s
					.Terms("branches", t => t
						.Field(f => f.Branches.Suffix("keyword"))
						.MissingBucket()
						.Order(SortOrder.Ascending)
					)
				)
				.Aggregations(childAggs => childAggs
					.Nested("project_tags", n => n
						.Path(p => p.Tags)
						.Aggregations(nestedAggs => nestedAggs
							.Terms("tags", avg => avg.Field(p => p.Tags.First().Name))
						)
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new CompositeAggregation("my_buckets")
			{
				Sources = new List<ICompositeAggregationSource>
				{
					new TermsCompositeAggregationSource("branches")
					{
						Field = Field<Project>(f => f.Branches.Suffix("keyword")),
						MissingBucket = true,
						Order = SortOrder.Ascending
					}
				},
				Aggregations = new NestedAggregation("project_tags")
				{
					Path = Field<Project>(p => p.Tags),
					Aggregations = new TermsAggregation("tags")
					{
						Field = Field<Project>(p => p.Tags.First().Name)
					}
				}
			};

		/**==== Handling Responses
		 * Each Composite aggregation bucket key is an `CompositeKey`, a specialized
		 * `IReadOnlyDictionary<string, object>` type with methods to convert values to supported types
		 */
		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var composite = response.Aggregations.Composite("my_buckets");
			composite.Should().NotBeNull();
			composite.Buckets.Should().NotBeNullOrEmpty();
			composite.AfterKey.Should().NotBeNull();
			composite.AfterKey.Should().HaveCount(1).And.ContainKeys("branches");

			var i = 0;
			foreach (var item in composite.Buckets)
			{
				var key = item.Key;
				key.Should().NotBeNull();

				key.TryGetValue("branches", out string branches).Should().BeTrue("expected to find 'branches' in composite bucket");
				if (i == 0) branches.Should().BeNull("First key should be null as we expect to have some projects with no branches");
				else branches.Should().NotBeNullOrEmpty();

				var nested = item.Nested("project_tags");
				nested.Should().NotBeNull();

				var nestedTerms = nested.Terms("tags");
				nestedTerms.Buckets.Count.Should().BeGreaterThan(0);
				i++;
			}
		}
	}

	public class DateFormatCompositeAggregationUsageTests : ProjectsOnlyAggregationUsageTestBase
	{
		public DateFormatCompositeAggregationUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

		protected override object AggregationJson => new
		{
			my_buckets = new
			{
				composite = new
				{
					sources = new object[]
					{
						new
						{
							started = new
							{
								date_histogram = new
								{
									field = "startedOn",
									fixed_interval = "30d",
									format = "yyyy-MM-dd"
								}
							}
						},
					}
				},
				aggs = new
				{
					project_tags = new
					{
						nested = new
						{
							path = "tags"
						},
						aggs = new
						{
							tags = new
							{
								terms = new { field = "tags.name" }
							}
						}
					}
				}
			}
		};

		protected override Func<AggregationContainerDescriptor<Project>, IAggregationContainer> FluentAggs => a => a
			.Composite("my_buckets", date => date
				.Sources(s => s
					.DateHistogram("started", d => d
						.Field(f => f.StartedOn)
						.FixedInterval("30d")
						.Format("yyyy-MM-dd")
					)
				)
				.Aggregations(childAggs => childAggs
					.Nested("project_tags", n => n
						.Path(p => p.Tags)
						.Aggregations(nestedAggs => nestedAggs
							.Terms("tags", avg => avg.Field(p => p.Tags.First().Name))
						)
					)
				)
			);

		protected override AggregationDictionary InitializerAggs =>
			new CompositeAggregation("my_buckets")
			{
				Sources = new List<ICompositeAggregationSource>
				{
					new DateHistogramCompositeAggregationSource("started")
					{
						Field = Field<Project>(f => f.StartedOn),
						FixedInterval = new Time(30, TimeUnit.Day),
						Format = "yyyy-MM-dd"
					},
				},
				Aggregations = new NestedAggregation("project_tags")
				{
					Path = Field<Project>(p => p.Tags),
					Aggregations = new TermsAggregation("tags")
					{
						Field = Field<Project>(p => p.Tags.First().Name)
					}
				}
			};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.ShouldBeValid();

			var composite = response.Aggregations.Composite("my_buckets");
			composite.Should().NotBeNull();
			composite.Buckets.Should().NotBeNullOrEmpty();
			composite.AfterKey.Should().NotBeNull();
			composite.AfterKey.Should().HaveCount(1).And.ContainKeys("started");
			foreach (var item in composite.Buckets)
			{
				var key = item.Key;
				key.Should().NotBeNull();

				key.TryGetValue("started", out string startedString).Should().BeTrue();
				startedString.Should().NotBeNullOrWhiteSpace();
			}
		}
	}
}
