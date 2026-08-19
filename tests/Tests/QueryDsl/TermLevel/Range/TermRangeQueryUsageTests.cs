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

using OpenSearch.Client;
using Tests.Configuration;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.TermLevel.Range
{
	public class TermRangeQueryUsageTests : QueryDslUsageTestsBase
	{
		public TermRangeQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		/* 
		   OpenSearch 3.6.0 rejects a range query that specifies both a strict and a non-strict bound
		   on the same side (e.g. gt + gte) with "invalid lower bound for [range] query". Earlier
		   versions (1.x, 2.x, 3.0–3.5) accept it, so we still exercise that combination there.
		*/
		private static bool SupportsRedundantBounds => TestConfiguration.Instance.OpenSearchVersion < "3.6.0";

		protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<ITermRangeQuery>(q => q.Range as ITermRangeQuery)
		{
			q => q.Field = null,
			q =>
			{
				q.GreaterThan = null;
				q.GreaterThanOrEqualTo = null;
				q.LessThan = null;
				q.LessThanOrEqualTo = null;
			},
			q =>
			{
				q.GreaterThan = string.Empty;
				q.GreaterThanOrEqualTo = string.Empty;
				q.LessThan = string.Empty;
				q.LessThanOrEqualTo = string.Empty;
			},
		};

		protected override QueryContainer QueryInitializer
		{
			get
			{
				var query = new TermRangeQuery
				{
					Name = "named_query",
					Boost = 1.1,
					Field = "description",
					GreaterThanOrEqualTo = "foof",
					LessThanOrEqualTo = "barb"
				};
				if (SupportsRedundantBounds)
				{
					query.GreaterThan = "foo";
					query.LessThan = "bar";
				}
				return query;
			}
		}

		protected override object QueryJson => new
		{
			range = new
			{
				description = SupportsRedundantBounds
					? (object)new
					{
						_name = "named_query",
						boost = 1.1,
						gt = "foo",
						gte = "foof",
						lt = "bar",
						lte = "barb"
					}
					: new
					{
						_name = "named_query",
						boost = 1.1,
						gte = "foof",
						lte = "barb"
					}
			}
		};

		protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
			.TermRange(c =>
			{
				c
					.Name("named_query")
					.Boost(1.1)
					.Field(p => p.Description)
					.GreaterThanOrEquals("foof")
					.LessThanOrEquals("barb");
				if (SupportsRedundantBounds)
					c.GreaterThan("foo").LessThan("bar");
				return c;
			});
	}
}
