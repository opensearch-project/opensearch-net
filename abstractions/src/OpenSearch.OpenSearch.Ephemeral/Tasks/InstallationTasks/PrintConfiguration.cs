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
using OpenSearch.OpenSearch.Managed.ConsoleWriters;
using static OpenSearch.OpenSearch.Ephemeral.ClusterFeatures;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.InstallationTasks
{
	public class PrintConfiguration : ClusterComposeTask
	{
		public override void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster)
		{
			var c = cluster.ClusterConfiguration;
			var version = c.Version;

			string F(ClusterFeatures feature)
			{
				return c.Features.HasFlag(feature) ? Enum.GetName(typeof(ClusterFeatures), feature) : string.Empty;
			}

			var features = string.Join("|",
				new[] { F(SSL)}.Where(v => !string.IsNullOrWhiteSpace(v)));
			features = string.IsNullOrWhiteSpace(features) ? "None" : features;
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} starting {{{version}}} with features [{features}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.NumberOfNodes)}}} [{c.NumberOfNodes}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.ClusterName)}}} [{c.ClusterName}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.EnableSsl)}}} [{c.EnableSsl}]");
			cluster.Writer?.WriteDiagnostic($"{{{nameof(PrintConfiguration)}}} {{{nameof(c.Plugins)}}} [{c.Plugins}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.CacheOpenSearchHomeInstallation)}}} [{c.CacheOpenSearchHomeInstallation}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.ShowOpenSearchOutputAfterStarted)}}} [{c.ShowOpenSearchOutputAfterStarted}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.ValidatePluginsToInstall)}}} [{c.ValidatePluginsToInstall}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.PrintYamlFilesInConfigFolder)}}} [{c.PrintYamlFilesInConfigFolder}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.SkipBuiltInAfterStartTasks)}}} [{c.SkipBuiltInAfterStartTasks}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.HttpFiddlerAware)}}} [{c.HttpFiddlerAware}]");
			cluster.Writer?.WriteDiagnostic(
				$"{{{nameof(PrintConfiguration)}}} {{{nameof(c.NoCleanupAfterNodeStopped)}}} [{c.NoCleanupAfterNodeStopped}]");
		}
	}
}
