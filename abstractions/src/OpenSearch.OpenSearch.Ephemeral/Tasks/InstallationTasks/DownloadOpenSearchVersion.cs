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

using System.IO;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;
using OpenSearch.Stack.ArtifactsApi.Products;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.InstallationTasks
{
	public class DownloadOpenSearchVersion : ClusterComposeTask
	{
		public override void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster)
		{
			if (cluster.CachingAndCachedHomeExists()) return;

			var fs = cluster.FileSystem;
			var v = cluster.ClusterConfiguration.Version;
			var a = cluster.ClusterConfiguration.Artifact;
			var from = v.Artifact(Product.OpenSearch).DownloadUrl;
			var to = Path.Combine(fs.LocalFolder, a.Archive);
			if (File.Exists(to))
			{
				cluster.Writer?.WriteDiagnostic(
					$"{{{nameof(DownloadOpenSearchVersion)}}} {v} was already downloaded");
				return;
			}

			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(DownloadOpenSearchVersion)}}} downloading OpenSearch [{v}] from {{{from}}} {{{to}}}");
			DownloadFile(from, to);
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(DownloadOpenSearchVersion)}}} downloaded OpenSearch [{v}] from {{{from}}} {{{to}}}");
		}
	}
}
