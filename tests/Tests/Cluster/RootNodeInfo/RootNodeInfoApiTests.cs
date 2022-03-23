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

using OpenSearch.Net;
using FluentAssertions;
using Osc;
using Tests.Configuration;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.RootNodeInfo
{
	public class RootNodeInfoApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, RootNodeInfoResponse, IRootNodeInfoRequest, RootNodeInfoDescriptor, RootNodeInfoRequest>
	{
		public RootNodeInfoApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override string UrlPath => "/";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.RootNodeInfo(),
			(client, f) => client.RootNodeInfoAsync(),
			(client, r) => client.RootNodeInfo(r),
			(client, r) => client.RootNodeInfoAsync(r)
		);

		protected override void ExpectResponse(RootNodeInfoResponse response)
		{
			response.Tagline.Should().NotBeNullOrWhiteSpace();
			response.Name.Should().NotBeNullOrWhiteSpace();
			response.ClusterName.Should().NotBeNullOrWhiteSpace();
			response.ClusterUUID.Should().NotBeNullOrWhiteSpace();
			response.Version.Should().NotBeNull();
			response.Version.Number.Should().NotBeNullOrWhiteSpace();
			response.Version.BuildDate.Should().BeAfter(default);
			if (TestConfiguration.Instance.ServerType == OpenSearch.Stack.ArtifactsApi.ServerType.OpenSearch)
				response.Version.Distribution.Should().NotBeNullOrWhiteSpace();
			else
				response.Version.BuildFlavor.Should().NotBeNullOrWhiteSpace();
			response.Version.BuildHash.Should().NotBeNullOrWhiteSpace();
			response.Version.BuildSnapshot.Should().Be(TestConfiguration.Instance.OpenSearchVersionIsSnapshot);
			response.Version.BuildType.Should().NotBeNullOrWhiteSpace();
			response.Version.MinimumIndexCompatibilityVersion.Should().NotBeNullOrWhiteSpace();
			response.Version.MinimumWireCompatibilityVersion.Should().NotBeNullOrWhiteSpace();
		}
	}
}
