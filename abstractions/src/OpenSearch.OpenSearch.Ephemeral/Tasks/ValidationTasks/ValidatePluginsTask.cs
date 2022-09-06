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
using OpenSearch.OpenSearch.Ephemeral.Tasks.InstallationTasks;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;
using OpenSearch.Stack.ArtifactsApi;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.ValidationTasks
{
	public class ValidatePluginsTask : ClusterComposeTask
	{
		public override void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster)
		{
			var v = cluster.ClusterConfiguration.Version;
			var requestPlugins = cluster.ClusterConfiguration.Plugins
				.Where(p => p.IsValid(v))
				.Where(p => !p.IsIncludedOutOfTheBox(v))
				.Select(p => p.GetExistsMoniker(v))
				.ToList();
			if (!requestPlugins.Any()) return;

			cluster.Writer.WriteDiagnostic(
				$"{{{nameof(ValidatePluginsTask)}}} validating the cluster is running the requested plugins");
			var catPlugins = Get(cluster, "_cat/plugins", "h=component");
			if (catPlugins == null || !catPlugins.IsSuccessStatusCode)
				throw new Exception(
					$"Calling _cat/plugins did not result in an OK response {GetResponseException(catPlugins)}");

			var installedPlugins = GetResponseString(catPlugins)
				.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();

			var missingPlugins = requestPlugins.Except(installedPlugins).ToList();
			if (!missingPlugins.Any()) return;

			var missingString = string.Join(", ", missingPlugins);
			var pluginsString = string.Join(", ", installedPlugins);
			throw new Exception(
				$"Already running opensearch missed the following plugin(s): {missingString} currently installed: {pluginsString}.");
		}
	}
}
