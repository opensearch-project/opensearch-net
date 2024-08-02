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
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.Search
{
	public class SearchApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, ISearchResponse<Project>, ISearchRequest, SearchDescriptor<Project>, SearchRequest<Project>>
	{
		public SearchApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => new
		{
			from = 10,
			size = 20,
			query = new
			{
				match_all = new { }
			},
			aggs = new
			{
				startDates = new
				{
					terms = new
					{
						field = "startedOn"
					}
				}
			},
			post_filter = new
			{
				term = new
				{
					state = new
					{
						value = "Stable"
					}
				}
			},
            ext = new
            {
                personalize_request_parameters = new
                {
                    user_id = "<USER_ID>",
                    context = new { DEVICE = "mobile phone"}
                }
            }
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.From(10)
			.Size(20)
			.Query(q => q
				.MatchAll()
			)
			.Aggregations(a => a
				.Terms("startDates", t => t
					.Field(p => p.StartedOn)
				)
			)
			.PostFilter(f => f
				.Term(p => p.State, StateOfBeing.Stable)
			)
            .Ext(e => e
                .Add("personalize_request_parameters", new Dictionary<string, object>
                {
                    ["user_id"] = "<USER_ID>",
                    ["context"] = new Dictionary<string, string>{ ["DEVICE"] = "mobile phone" }
                }));

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override SearchRequest<Project> Initializer => new SearchRequest<Project>()
		{
			From = 10,
			Size = 20,
			Query = new QueryContainer(new MatchAllQuery()),
			Aggregations = new TermsAggregation("startDates")
			{
				Field = "startedOn"
			},
			PostFilter = new QueryContainer(new TermQuery
			{
				Field = "state",
				Value = "Stable"
			}),
            Ext = new Dictionary<string, object>
            {
                ["personalize_request_parameters"] = new Dictionary<string, object>
                {
                    ["user_id"] = "<USER_ID>",
                    ["context"] = new Dictionary<string, string>
                    {
                        ["DEVICE"] = "mobile phone"
                    }
                }
            }
		};

		protected override string UrlPath => $"/project/_search";

		protected override LazyResponses ClientUsage() => Calls(
			(c, f) => c.Search(f),
			(c, f) => c.SearchAsync(f),
			(c, r) => c.Search<Project>(r),
			(c, r) => c.SearchAsync<Project>(r)
		);

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.Total.Should().BeGreaterThan(0);
			response.Hits.Count.Should().BeGreaterThan(0);
			response.HitsMetadata.Total.Value.Should().Be(response.Total);
			response.HitsMetadata.Total.Relation.Should().Be(TotalHitsRelation.EqualTo);
			response.Hits.First().Should().NotBeNull();
			response.Hits.First().Source.Should().NotBeNull();
			response.Aggregations.Count.Should().BeGreaterThan(0);
			response.Took.Should().BeGreaterThan(0);
			var startDates = response.Aggregations.Terms("startDates");
			startDates.Should().NotBeNull();

			foreach (var document in response.Documents) document.ShouldAdhereToSourceSerializerWhenSet();
		}
	}

	public class SearchApiSequenceNumberPrimaryTermTests
		: SearchApiTests
	{
		public SearchApiSequenceNumberPrimaryTermTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new
		{
			query = new
			{
				match_all = new { }
			}
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.SequenceNumberPrimaryTerm()
			.Query(q => q
				.MatchAll()
			);

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override SearchRequest<Project> Initializer => new SearchRequest<Project>()
		{
			SequenceNumberPrimaryTerm = true,
			Query = new QueryContainer(new MatchAllQuery()),
		};

		protected override string UrlPath => $"/project/_search?seq_no_primary_term=true";

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.Total.Should().BeGreaterThan(0);
			response.Hits.Count.Should().BeGreaterThan(0);
			response.HitsMetadata.Total.Value.Should().Be(response.Total);
			response.HitsMetadata.Total.Relation.Should().Be(TotalHitsRelation.EqualTo);

			foreach (var hit in response.Hits)
			{
				hit.Should().NotBeNull();
				hit.Source.Should().NotBeNull();
				hit.SequenceNumber.Should().HaveValue();
				hit.PrimaryTerm.Should().HaveValue();
			}
		}
	}

	public class SearchApiStoredFieldsTests : SearchApiTests
	{
		public SearchApiStoredFieldsTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new
		{
			from = 10,
			size = 20,
			query = new
			{
				match_all = new { }
			},
			aggs = new
			{
				startDates = new
				{
					terms = new
					{
						field = "startedOn"
					}
				}
			},
			post_filter = new
			{
				term = new
				{
					state = new
					{
						value = "Stable"
					}
				}
			},
			stored_fields = new[] { "name", "numberOfCommits" }
		};

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.From(10)
			.Size(20)
			.Query(q => q
				.MatchAll()
			)
			.Aggregations(a => a
				.Terms("startDates", t => t
					.Field(p => p.StartedOn)
				)
			)
			.PostFilter(f => f
				.Term(p => p.State, StateOfBeing.Stable)
			)
			.StoredFields(fs => fs
				.Field(p => p.Name)
				.Field(p => p.NumberOfCommits)
			);

		protected override SearchRequest<Project> Initializer => new SearchRequest<Project>()
		{
			From = 10,
			Size = 20,
			Query = new QueryContainer(new MatchAllQuery()),
			Aggregations = new TermsAggregation("startDates")
			{
				Field = "startedOn"
			},
			PostFilter = new QueryContainer(new TermQuery
			{
				Field = "state",
				Value = "Stable"
			}),
			StoredFields = Infer.Fields<Project>(p => p.Name, p => p.NumberOfCommits)
		};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.Hits.Count.Should().BeGreaterThan(0);
			response.Hits.First().Should().NotBeNull();
			response.Hits.First().Fields.ValueOf<Project, string>(p => p.Name).Should().NotBeNullOrEmpty();
			response.Hits.First().Fields.ValueOf<Project, int?>(p => p.NumberOfCommits).Should().BeGreaterThan(0);
			response.Aggregations.Count.Should().BeGreaterThan(0);
			var startDates = response.Aggregations.Terms("startDates");
			startDates.Should().NotBeNull();
		}
	}

	public class SearchApiDocValueFieldsTests : SearchApiTests
	{
		public SearchApiDocValueFieldsTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new
		{
			from = 10,
			size = 20,
			query = new
			{
				match_all = new { }
			},
			aggs = new
			{
				startDates = new
				{
					terms = new
					{
						field = "startedOn"
					}
				}
			},
			post_filter = new
			{
				term = new
				{
					state = new
					{
						value = "Stable"
					}
				}
			},
			docvalue_fields = new object[]
			{
				"name",
				new
				{
					field = "lastActivity",
					format = DateFormat.basic_date
				},
			}
		};

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.From(10)
			.Size(20)
			.Query(q => q
				.MatchAll()
			)
			.Aggregations(a => a
				.Terms("startDates", t => t
					.Field(p => p.StartedOn)
				)
			)
			.PostFilter(f => f
				.Term(p => p.State, StateOfBeing.Stable)
			)
			.DocValueFields(fs => fs
				.Field(p => p.Name)
				.Field(p => p.LastActivity, format: DateFormat.basic_date)
			);

		protected override SearchRequest<Project> Initializer => new SearchRequest<Project>()
		{
			From = 10,
			Size = 20,
			Query = new QueryContainer(new MatchAllQuery()),
			Aggregations = new TermsAggregation("startDates")
			{
				Field = "startedOn"
			},
			PostFilter = new QueryContainer(new TermQuery
			{
				Field = "state",
				Value = "Stable"
			}),
			DocValueFields = Infer.Field<Project>(p => p.Name)
				.And<Project>(p => p.LastActivity, format: DateFormat.basic_date)
		};

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.HitsMetadata.Should().NotBeNull();
			response.Hits.Count().Should().BeGreaterThan(0);
			response.Hits.First().Should().NotBeNull();
			if (Cluster.ClusterConfiguration.Version < "2.0.0")
				response.Hits.First().Type.Should().NotBeNullOrWhiteSpace();
			response.Hits.First().Fields.ValueOf<Project, string>(p => p.Name).Should().NotBeNullOrEmpty();
			var lastActivityYear = Convert.ToInt32(response.Hits.First().Fields.Value<string>("lastActivity"));
			lastActivityYear.Should().BeGreaterThan(0);
			response.Aggregations.Count.Should().BeGreaterThan(0);
			var startDates = response.Aggregations.Terms("startDates");
			startDates.Should().NotBeNull();
		}
	}

	public class SearchApiContainingConditionlessQueryContainerTests : SearchApiTests
	{
		public SearchApiContainingConditionlessQueryContainerTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new
		{
			query = new
			{
				@bool = new
				{
					must = new object[] { new { query_string = new { query = "query" } } },
					should = new object[] { new { query_string = new { query = "query" } } },
					must_not = new object[] { new { query_string = new { query = "query" } } }
				}
			}
		};

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.Query(q => q
				.Bool(b => b
					.Must(
						m => m.QueryString(qs => qs.Query("query")),
						m => m.QueryString(qs => qs.Query(string.Empty)),
						m => m.QueryString(qs => qs.Query(null)),
						m => new QueryContainer(),
						null
					)
					.Should(
						m => m.QueryString(qs => qs.Query("query")),
						m => m.QueryString(qs => qs.Query(string.Empty)),
						m => m.QueryString(qs => qs.Query(null)),
						m => new QueryContainer(),
						null
					)
					.MustNot(
						m => m.QueryString(qs => qs.Query("query")),
						m => m.QueryString(qs => qs.Query(string.Empty)),
						m => m.QueryString(qs => qs.Query(null)),
						m => new QueryContainer(),
						null
					)
				)
			);

		protected override SearchRequest<Project> Initializer => new SearchRequest<Project>()
		{
			Query = new BoolQuery
			{
				Must = new List<QueryContainer>
				{
					new QueryStringQuery { Query = "query" },
					new QueryStringQuery { Query = string.Empty },
					new QueryStringQuery { Query = null },
					new QueryContainer(),
					null
				},
				Should = new List<QueryContainer>
				{
					new QueryStringQuery { Query = "query" },
					new QueryStringQuery { Query = string.Empty },
					new QueryStringQuery { Query = null },
					new QueryContainer(),
					null
				},
				MustNot = new List<QueryContainer>
				{
					new QueryStringQuery { Query = "query" },
					new QueryStringQuery { Query = string.Empty },
					new QueryStringQuery { Query = null },
					new QueryContainer(),
					null
				}
			}
		};

		protected override void ExpectResponse(ISearchResponse<Project> response) => response.ShouldBeValid();
	}

	public class SearchApiNullQueryContainerTests : SearchApiTests
	{
		public SearchApiNullQueryContainerTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new { };

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.Query(q => q
				.Bool(b => b
					.Must((Func<QueryContainerDescriptor<Project>, QueryContainer>)null)
					.Should((Func<QueryContainerDescriptor<Project>, QueryContainer>)null)
					.MustNot((Func<QueryContainerDescriptor<Project>, QueryContainer>)null)
				)
			);

		protected override SearchRequest<Project> Initializer => new SearchRequest<Project>()
		{
			Query = new BoolQuery
			{
				Must = null,
				Should = null,
				MustNot = null
			}
		};

		protected override void ExpectResponse(ISearchResponse<Project> response) => response.ShouldBeValid();
	}

	public class SearchApiNullQueriesInQueryContainerTests : SearchApiTests
	{
		public SearchApiNullQueriesInQueryContainerTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override object ExpectJson => new
		{
			query = new
			{
				@bool = new { }
			}
		};

		// There is no *direct equivalent* to a query container collection only with a null querycontainer
		// since the fluent methods filter them out
		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.Query(q => q
				.Bool(b =>
				{
					b.Verbatim();
					IBoolQuery bq = b;
					bq.Must = new QueryContainer[] { null };
					bq.Should = new QueryContainer[] { null };
					bq.MustNot = new QueryContainer[] { null };
					return bq;
				})
			);

		protected override SearchRequest<Project> Initializer => new SearchRequest<Project>()
		{
			Query = new BoolQuery
			{
				IsVerbatim = true,
				Must = new QueryContainer[] { null },
				Should = new QueryContainer[] { null },
				MustNot = new QueryContainer[] { null }
			}
		};

		// when we serialize we write and empty bool, when we read the fact it was verbatim is lost so while
		// we technically DO support deserialization here (and empty bool will get set) when we write it a second
		// time it will NOT write that bool because the is verbatim did not carry over.
		protected override bool SupportsDeserialization => false;

		protected override void ExpectResponse(ISearchResponse<Project> response) => response.ShouldBeValid();
	}


	public class OpaqueIdApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, ListTasksResponse, IListTasksRequest, ListTasksDescriptor, ListTasksRequest>
	{
		public OpaqueIdApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;

		protected override Func<ListTasksDescriptor, IListTasksRequest> Fluent => s => s
			.RequestConfiguration(r => r.OpaqueId(CallIsolatedValue));

		protected override HttpMethod HttpMethod => HttpMethod.GET;

		protected override ListTasksRequest Initializer => new ListTasksRequest()
		{
			RequestConfiguration = new RequestConfiguration { OpaqueId = CallIsolatedValue },
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => $"/_tasks?pretty=true&error_trace=true";

		protected override LazyResponses ClientUsage() => Calls(
			(c, f) => c.Tasks.List(f),
			(c, f) => c.Tasks.ListAsync(f),
			(c, r) => c.Tasks.List(r),
			(c, r) => c.Tasks.ListAsync(r)
		);

		protected override void OnBeforeCall(IOpenSearchClient client)
		{
			var searchResponse = client.Search<Project>(s => s
					.RequestConfiguration(r => r.OpaqueId(CallIsolatedValue))
					.Scroll("10m") // Create a scroll in order to keep the task around.
			);

			searchResponse.ShouldBeValid();
		}

		protected override void ExpectResponse(ListTasksResponse response)
		{
			response.ShouldBeValid();
			foreach (var node in response.Nodes)
			foreach (var task in node.Value.Tasks)
			{
				task.Value.Headers.Should().NotBeNull();
				if (task.Value.Headers.TryGetValue(RequestData.OpaqueIdHeader, out var opaqueIdValue))
					opaqueIdValue.Should()
						.Be(CallIsolatedValue,
							$"OpaqueId header {opaqueIdValue} did not match {CallIsolatedValue}");
				// TODO: Determine if this is a valid assertion i.e. should all tasks returned have an OpaqueId header?
//				else
//				{
//					Assert.True(false,
//						$"No OpaqueId header for task {task.Key} and OpaqueId value {this.CallIsolatedValue}");
//				}
			}
		}
	}

	public class CrossClusterSearchApiTests
		: ApiIntegrationTestBase<CrossCluster, ISearchResponse<Project>, ISearchRequest, SearchDescriptor<Project>, SearchRequest<Project>>
	{
		public CrossClusterSearchApiTests(CrossCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson => new
		{
			query = new
			{
				match_all = new { }
			}
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
			.Index(OpenSearch.Client.Indices.Index<Project>().And("cluster_two:project"))
			.Query(q => q
				.MatchAll()
			);

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override SearchRequest<Project> Initializer => new SearchRequest<Project>(OpenSearch.Client.Indices.Index<Project>().And("cluster_two:project"))
		{
			Query = new MatchAllQuery()
		};

		protected override string UrlPath => $"/project%2Ccluster_two%3Aproject/_search";

		protected override LazyResponses ClientUsage() => Calls(
			(c, f) => c.Search(f),
			(c, f) => c.SearchAsync(f),
			(c, r) => c.Search<Project>(r),
			(c, r) => c.SearchAsync<Project>(r)
		);

		protected override void ExpectResponse(ISearchResponse<Project> response)
		{
			response.Clusters.Should().NotBeNull();
			response.Clusters.Total.Should().Be(2);
			response.Clusters.Skipped.Should().Be(1);
			response.Clusters.Successful.Should().Be(1);
		}
	}
}
