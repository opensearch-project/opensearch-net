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
using ProcNet.Std;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.AfterNodeStoppedTasks
{
	public class CleanUpDirectoriesAfterNodeStopped : IClusterTeardownTask
	{
		public void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster, bool nodeStarted)
		{
			var fs = cluster.FileSystem;
			var w = cluster.Writer;
			var a = cluster.ClusterConfiguration.Artifact;
			if (cluster.ClusterConfiguration.NoCleanupAfterNodeStopped)
			{
				w.WriteDiagnostic(
					$"{{{nameof(CleanUpDirectoriesAfterNodeStopped)}}} skipping cleanup as requested on cluster configuration");
				return;
			}

			DeleteDirectory(w, "cluster data", fs.DataPath);
			DeleteDirectory(w, "cluster config", fs.ConfigPath);
			DeleteDirectory(w, "cluster logs", fs.LogsPath);
			DeleteDirectory(w, "repositories", fs.RepositoryPath);
			var efs = fs as EphemeralFileSystem;
			if (!string.IsNullOrWhiteSpace(efs?.TempFolder))
				DeleteDirectory(w, "cluster temp folder", efs.TempFolder);

			if (efs != null)
			{
				var extractedFolder = Path.Combine(fs.LocalFolder, a.FolderInZip);
				if (extractedFolder != fs.OpenSearchHome)
					DeleteDirectory(w, "ephemeral OPENSEARCH_HOME", fs.OpenSearchHome);
				//if the node was not started delete the cached extractedFolder
				if (!nodeStarted)
					DeleteDirectory(w, "cached extracted folder - node failed to start", extractedFolder);
			}

			//if the node did not start make sure we delete the cached folder as we can not assume its in a good state
			var cachedOpenSearchHomeFolder = Path.Combine(fs.LocalFolder, cluster.GetCacheFolderName());
			if (cluster.ClusterConfiguration.CacheOpenSearchHomeInstallation && !nodeStarted)
				DeleteDirectory(w, "cached installation - node failed to start", cachedOpenSearchHomeFolder);
			else
				w.WriteDiagnostic(
					$"{{{nameof(CleanUpDirectoriesAfterNodeStopped)}}} Leaving [cached folder] on disk: {{{cachedOpenSearchHomeFolder}}}");
		}

		private static void DeleteDirectory(IConsoleLineHandler w, string description, string path)
		{
			if (!Directory.Exists(path)) return;
			w.WriteDiagnostic(
				$"{{{nameof(CleanUpDirectoriesAfterNodeStopped)}}} attempting to delete [{description}]: {{{path}}}");
			Directory.Delete(path, true);
		}
	}
}
