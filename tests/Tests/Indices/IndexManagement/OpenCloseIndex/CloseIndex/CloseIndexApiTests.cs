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

using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Client.Specification.IndicesApi;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexManagement.OpenCloseIndex.CloseIndex
{
	public class CloseIndexApiTests
		: ApiIntegrationAgainstNewIndexTestBase<WritableCluster, CloseIndexResponse, ICloseIndexRequest, CloseIndexDescriptor, CloseIndexRequest>
	{
		public CloseIndexApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override CloseIndexRequest Initializer => new CloseIndexRequest(CallIsolatedValue);
		protected override string UrlPath => $"/{CallIsolatedValue}/_close";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Indices.Close(CallIsolatedValue),
			(client, f) => client.Indices.CloseAsync(CallIsolatedValue),
			(client, r) => client.Indices.Close(r),
			(client, r) => client.Indices.CloseAsync(r)
		);
	}

	public class CloseIndexWithShardsAcknowledgedApiTests
		: ApiIntegrationAgainstNewIndexTestBase<WritableCluster, CloseIndexResponse, ICloseIndexRequest, CloseIndexDescriptor, CloseIndexRequest>
	{
		public CloseIndexWithShardsAcknowledgedApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override CloseIndexRequest Initializer => new CloseIndexRequest(CallIsolatedValue);
		protected override string UrlPath => $"/{CallIsolatedValue}/_close";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Indices.Close(CallIsolatedValue),
			(client, f) => client.Indices.CloseAsync(CallIsolatedValue),
			(client, r) => client.Indices.Close(r),
			(client, r) => client.Indices.CloseAsync(r)
		);

		protected override void ExpectResponse(CloseIndexResponse response)
		{
			response.ShouldBeValid();
			response.ShardsAcknowledged.Should().BeTrue();
			response.Indices.Should().NotBeNull().And.ContainKey(CallIsolatedValue);

			var closeIndexResult = response.Indices[CallIsolatedValue];
			closeIndexResult.Closed.Should().BeTrue();
			closeIndexResult.Shards.Should().NotBeNull();
		}
	}
}
