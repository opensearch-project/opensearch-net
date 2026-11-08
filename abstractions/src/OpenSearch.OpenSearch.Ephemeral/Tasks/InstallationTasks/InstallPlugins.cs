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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using OpenSearch.OpenSearch.Managed;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;
using OpenSearch.OpenSearch.Managed.FileSystem;
using OpenSearch.Stack.ArtifactsApi;
using OpenSearch.Stack.ArtifactsApi.Products;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.InstallationTasks
{
	public class InstallPlugins : ClusterComposeTask
	{
		private static readonly HttpClient HttpClient = new();

		public override void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster)
		{
			if (cluster.CachingAndCachedHomeExists()) return;

			var v = cluster.ClusterConfiguration.Version;

			var fs = cluster.FileSystem;
			var requiredPlugins = cluster.ClusterConfiguration.Plugins;

			if (cluster.ClusterConfiguration.ValidatePluginsToInstall)
			{
				var invalidPlugins = requiredPlugins
					.Where(p => !p.IsValid(v))
					.Select(p => p.SubProductName).ToList();
				if (invalidPlugins.Any())
                {
                    throw new OpenSearchCleanExitException(
                        $"Can not install the following plugins for version {v}: {string.Join(", ", invalidPlugins)} ");
                }
            }

			foreach (var plugin in requiredPlugins)
			{
                if (plugin.IsIncludedOutOfTheBox(v))
				{
					cluster.Writer?.WriteDiagnostic(
						$"{{{nameof(InstallPlugins)}}} SKIP plugin [{plugin.SubProductName}] shipped OOTB as of: {{{plugin.ShippedByDefaultAsOf}}}");
					continue;
				}

                if (!plugin.IsValid(v))
				{
					cluster.Writer?.WriteDiagnostic(
						$"{{{nameof(InstallPlugins)}}} SKIP plugin [{plugin.SubProductName}] not valid for version: {{{v}}}");
					continue;
				}

                if (AlreadyInstalled(fs, plugin.SubProductName))
				{
					cluster.Writer?.WriteDiagnostic(
						$"{{{nameof(InstallPlugins)}}} SKIP plugin [{plugin.SubProductName}] already installed");
					continue;
				}

				cluster.Writer?.WriteDiagnostic(
					$"{{{nameof(InstallPlugins)}}} attempting install [{plugin.SubProductName}] as it's not OOTB: {{{plugin.ShippedByDefaultAsOf}}} and valid for {v}");

				var homeConfigPath = Path.Combine(fs.OpenSearchHome, "config");

				if (!Directory.Exists(homeConfigPath)) Directory.CreateDirectory(homeConfigPath);

				var env = new Dictionary<string, string>
				{
					{ fs.ConfigEnvironmentVariableName, homeConfigPath }
				};

				ExecuteBinary(
					cluster.ClusterConfiguration,
					cluster.Writer,
					fs.PluginBinary,
					$"install opensearch plugin: {plugin.SubProductName}",
					env,
					"install", "--batch", GetPluginLocation(plugin, v));

				CopyConfigDirectoryToHomeCacheConfigDirectory(cluster, plugin);
			}

			cluster.Writer?.WriteDiagnostic($"{{{nameof(InstallPlugins)}}} all plugins installed");
		}

		private static string GetPluginLocation(OpenSearchPlugin plugin, OpenSearchVersion v)
		{
			var pluginName = plugin.SubProductName;
			var versionVariants = new[]
			{
				v.ToString(),
				$"{v.BaseVersion()}.0{(v.IsPreRelease ? $"-{v.PreRelease}" : string.Empty)}",
			};

			if (Environment.GetEnvironmentVariable("OPENSEARCH_PLUGINS_DIRECTORY") is { } pluginsDirectory)
			{
				foreach (var versionVariant in versionVariants)
				{
					var pluginFile = Path.Combine(pluginsDirectory, $"{pluginName}-{versionVariant}.zip");
					if (File.Exists(pluginFile))
					{
						return new UriBuilder("file",string.Empty)
							{
								Path = pluginFile
									.Replace("%",$"%{(int)'%':X2}")
									.Replace("[",$"%{(int)'[':X2}")
									.Replace("]",$"%{(int)']':X2}"),
							}
							.Uri
							.AbsoluteUri;
					}
				}
			}

			if (v.IsSnapshot)
				return DeterminePluginSnapshotUrl(pluginName, versionVariants);

			return pluginName;
		}

		private static string DeterminePluginSnapshotUrl(string pluginName, string[] versionVariants)
		{
			try
			{
				var baseUrl = $"https://aws.oss.sonatype.org/content/repositories/snapshots/org/opensearch/plugin/{pluginName}";

				var versionConditions = string.Join(" or ", versionVariants.Select(v => $".='{v}'"));
				var version = SelectNodeWithinRemoteXml(
						$"{baseUrl}/maven-metadata.xml",
						$"metadata/versioning/versions/version[{versionConditions}]")
					.InnerText;

				var versionUrl = $"{baseUrl}/{version}";

				var snapshotVersion = SelectNodeWithinRemoteXml(
						$"{versionUrl}/maven-metadata.xml",
						"metadata/versioning/snapshotVersions/snapshotVersion[extension='zip']/value")
					.InnerText;

				return $"{versionUrl}/{pluginName}-{snapshotVersion}.zip";
			}
			catch (Exception e)
			{
				throw new Exception($"Could not determine snapshot url for plugin `{pluginName}` at versions `{string.Join(", ", versionVariants)}`", e);
			}
		}

		private static XmlNode SelectNodeWithinRemoteXml(string url, [LanguageInjection("XPath")] string xPath)
		{
			var task = Task.Run(async () =>
			{
				var msg = await HttpClient.GetAsync(url);
				msg.EnsureSuccessStatusCode();
				var xml = await msg.Content.ReadAsStringAsync();
				var doc = new XmlDocument();
				doc.LoadXml(xml);
				return doc.SelectSingleNode(xPath) ?? throw new Exception($"Could not find node matching XPath: `{xPath}` within `{xml}`");
			});
			task.Wait();
			return task.Result;
		}

		private static void CopyConfigDirectoryToHomeCacheConfigDirectory(
			IEphemeralCluster<EphemeralClusterConfiguration> cluster, OpenSearchPlugin plugin)
		{
			if (!cluster.ClusterConfiguration.CacheOpenSearchHomeInstallation) return;
			var fs = cluster.FileSystem;
			var cachedOpenSearchHomeFolder = Path.Combine(fs.LocalFolder, cluster.GetCacheFolderName());
			var configTarget = Path.Combine(cachedOpenSearchHomeFolder, "config");

			var configPluginPath = Path.Combine(fs.ConfigPath, plugin.SubProductName);
			var configPluginPathCached = Path.Combine(configTarget, plugin.SubProductName);
			if (!Directory.Exists(configPluginPath) || Directory.Exists(configPluginPathCached)) return;

			Directory.CreateDirectory(configPluginPathCached);
			CopyFolder(configPluginPath, configPluginPathCached);
		}

		private static bool AlreadyInstalled(INodeFileSystem fileSystem, string folderName)
		{
			var pluginFolder = Path.Combine(fileSystem.OpenSearchHome, "plugins", folderName);
			return Directory.Exists(pluginFolder);
		}
	}
}
