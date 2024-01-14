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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;
using OpenSearch.Stack.ArtifactsApi.Products;
using SemanticVersioning;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.InstallationTasks
{
	public class InitialConfiguration : ClusterComposeTask
	{
		public override void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster)
		{
			var fs = cluster.FileSystem;

			var installConfigDir = Path.Combine(fs.OpenSearchHome, "config");
			var installConfigFile = Path.Combine(installConfigDir, "opensearch.yml");
			var pluginSecurity = Path.Combine(fs.OpenSearchHome, "plugins/opensearch-security");

			if (!Directory.Exists(pluginSecurity))
				return;

			var isNewDemoScript = cluster.ClusterConfiguration.Version.BaseVersion() >= new Version(2, 12, 0);

			const string securityInstallDemoConfigSubPath = "tools/install_demo_configuration.sh";
			var securityInstallDemoConfig = Path.Combine(pluginSecurity, securityInstallDemoConfigSubPath);

			cluster.Writer?.WriteDiagnostic($"{{{nameof(InitialConfiguration)}}} going to run [{securityInstallDemoConfigSubPath}]");

			if (File.Exists(installConfigFile) && File.ReadLines(installConfigFile).Any(l => l.Contains("plugins.security"))) return;

			var env = new Dictionary<string, string>();
			var args = new List<string> { securityInstallDemoConfig, "-y", "-i" };

			if (isNewDemoScript)
			{
				env.Add("OPENSEARCH_INITIAL_ADMIN_PASSWORD", "admin");
				args.Add("-t");
			}

			ExecuteBinary(
				cluster.ClusterConfiguration,
				cluster.Writer,
				"/bin/bash",
				"install security plugin demo configuration",
				env,
				args.ToArray());
		}
	}
}
