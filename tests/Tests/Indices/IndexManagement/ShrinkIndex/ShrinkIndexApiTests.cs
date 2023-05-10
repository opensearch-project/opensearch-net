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
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexManagement.ShrinkIndex
{
	using OpenSearch.Client.Specification.IndicesApi;

	public class ShrinkIndexApiTests
		: ApiIntegrationTestBase<WritableCluster, ShrinkIndexResponse, IShrinkIndexRequest, ShrinkIndexDescriptor, ShrinkIndexRequest>
	{
		public ShrinkIndexApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;

		protected override object ExpectJson { get; } = new
		{
			settings = new Dictionary<string, object>
			{
				{ "index.number_of_shards", 4 }
			}
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<ShrinkIndexDescriptor, IShrinkIndexRequest> Fluent => d => d
			.Settings(s => s
				.NumberOfShards(4)
			);

		protected override HttpMethod HttpMethod => HttpMethod.PUT;

		protected override ShrinkIndexRequest Initializer => new(CallIsolatedValue, CallIsolatedValue + "-target")
		{
			Settings = new IndexSettings
			{
				NumberOfShards = 4
			}
		};

		protected override string UrlPath => $"/{CallIsolatedValue}/_shrink/{CallIsolatedValue}-target";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Indices.Shrink(CallIsolatedValue, CallIsolatedValue + "-target", f),
			(client, f) => client.Indices.ShrinkAsync(CallIsolatedValue, CallIsolatedValue + "-target", f),
			(client, r) => client.Indices.Shrink(r),
			(client, r) => client.Indices.ShrinkAsync(r)
		);

		protected override void OnBeforeCall(IOpenSearchClient client)
		{
			var create = client.Indices.Create(CallIsolatedValue, c => c
				.Settings(s => s
					.NumberOfShards(8)
					.NumberOfReplicas(0)
				)
			);
			create.ShouldBeValid();
			var update = client.Indices.UpdateSettings(CallIsolatedValue, u => u
				.IndexSettings(s => s
					.BlocksWrite()
				)
			);
			update.ShouldBeValid();
		}

		protected override ShrinkIndexDescriptor NewDescriptor() => new(CallIsolatedValue, CallIsolatedValue + "-target");

		protected override void ExpectResponse(ShrinkIndexResponse response)
		{
			response.ShouldBeValid();
			response.Acknowledged.Should().BeTrue();
			response.ShardsAcknowledged.Should().BeTrue();
		}
	}
}
