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

using System.Collections.Generic;
using OpenSearch.Stack.ArtifactsApi.Products;
using Tests.Core.Client;
using Tests.Core.ManagedOpenSearch.NodeSeeders;
using static OpenSearch.Stack.ArtifactsApi.Products.OpenSearchPlugin;

namespace Tests.Core.ManagedOpenSearch.Clusters
{
	/// <summary>
	/// Use this cluster for APIs that do writes. If they are however intrusive or long running consider IntrusiveOperationCluster
	/// instead.
	/// </summary>
	public class WritableCluster : ClientTestClusterBase
	{
		public WritableCluster() : base(CreateConfiguration()) { }

		private static ClientTestClusterConfiguration CreateConfiguration()
		{
			var plugins = new List<OpenSearchPlugin>
			{
				IngestGeoIp,
				IngestAttachment,
				AnalysisKuromoji,
				AnalysisIcu,
				AnalysisPhonetic,
				MapperMurmur3,
			};

			plugins.Add(new OpenSearchPlugin("analysis-nori"));

			return new ClientTestClusterConfiguration(plugins.ToArray())
			{
				MaxConcurrency = 4
			};
		}

		protected override void SeedNode()
		{
			var seeder = new DefaultSeeder(Client);
			seeder.SeedNode();
		}
	}
}
