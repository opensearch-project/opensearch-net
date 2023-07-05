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
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.ClientConcepts.Connection;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Core.Extensions;

namespace Tests.ClientConcepts.ConnectionPooling.Pinging
{
	public class PingTests : IClusterFixture<ReadOnlyCluster>
	{
		private readonly ReadOnlyCluster _cluster;

		public PingTests(ReadOnlyCluster cluster) => _cluster = cluster;

		[I]
		public void UsesRelativePathForPing()
		{
			var uris = _cluster.NodesUris().Select(u => new Uri(u.AbsoluteUri.Trim('/') + "/opensearch/"));
			var pool = new StaticConnectionPool(uris);
			var settings = new ConnectionSettings(pool,
				new HttpConnectionTests.TestableHttpConnection(response =>
				{
					response.RequestMessage.RequestUri.AbsolutePath.Should().StartWith("/opensearch/");
				}));
			settings = (ConnectionSettings)_cluster.UpdateSettings(settings);

			var client = new OpenSearchClient(settings);
			var healthResponse = client.Ping();
			healthResponse.ApiCall.AuditTrail[0].Event.Should().Be(AuditEvent.PingSuccess);
			healthResponse.ApiCall.AuditTrail[1].Event.Should().Be(AuditEvent.HealthyResponse);
		}
	}
}

