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

using System.Collections;
using System.Collections.Generic;
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Stack.ArtifactsApi;
using Xunit;

namespace OpenSearch.OpenSearch.EphemeralTests
{
	public class EphemeralClusterTests
	{
		[TU]
		[ClassData(typeof(SampleClusters))]
		public void TestEphemeralCluster(OpenSearchVersion version, ServerType serverType, ClusterFeatures features)
		{
			const int numberOfNodes = 1;
			var clusterConfiguration =
				new EphemeralClusterConfiguration(version, serverType, features, null, numberOfNodes)
				{
					ShowOpenSearchOutputAfterStarted = true
				};
			using var cluster = new EphemeralCluster(clusterConfiguration);
			var timeout = new System.TimeSpan(0, 5, 30);
			using (cluster.Start(timeout))
			{
				Assert.True(cluster.Started, "OpenSearch cluster started");
				foreach (var n in cluster.Nodes)
				{
					n.SendControlC();
					Assert.True(n.WaitForCompletion(timeout), $"Failed to stop node {n.ProcessId}");
				}
			}
		}

		private class SampleClusters : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				yield return new object[] {OpenSearchVersion.From("1.2.0"), ServerType.OpenSearch, ClusterFeatures.None};
				yield return new object[]
				{
					OpenSearchVersion.From("opendistro-latest"), ServerType.OpenDistro, ClusterFeatures.None
				};
				yield return new object[] {OpenSearchVersion.From("1.2.0"), ServerType.OpenSearch, ClusterFeatures.SSL};
				yield return new object[]
				{
					OpenSearchVersion.From("opendistro-latest"), ServerType.OpenDistro, ClusterFeatures.SSL
				};
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}
