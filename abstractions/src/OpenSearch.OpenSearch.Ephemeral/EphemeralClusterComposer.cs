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
using OpenSearch.OpenSearch.Ephemeral.Tasks;
using OpenSearch.OpenSearch.Ephemeral.Tasks.AfterNodeStoppedTasks;
using OpenSearch.OpenSearch.Ephemeral.Tasks.BeforeStartNodeTasks;
using OpenSearch.OpenSearch.Ephemeral.Tasks.InstallationTasks;
using OpenSearch.OpenSearch.Ephemeral.Tasks.ValidationTasks;
using OpenSearch.OpenSearch.Managed.FileSystem;

namespace OpenSearch.OpenSearch.Ephemeral
{
	public class EphemeralClusterComposerBase
	{
		protected EphemeralClusterComposerBase()
		{
		}

		internal static IEnumerable<IClusterComposeTask> InstallationTasks { get; } = new List<IClusterComposeTask>
		{
			new PrintConfiguration(),
			new CreateLocalApplicationDirectory(),
			new CopyCachedOpenSearchInstallation(),
			new EnsureJavaHomeEnvironmentVariableIsSet(),
			new DownloadOpenSearchVersion(),
			new UnzipOpenSearch(),
			new SetOpenSearchBundledJdkJavaHome(),
			new InstallPlugins(),
			new InitialConfiguration()
		};

		protected static IEnumerable<IClusterComposeTask> BeforeStart { get; } = new List<IClusterComposeTask>
		{
			new CreateEphemeralDirectory(),
			new CacheOpenSearchInstallation()
		};

		protected static IEnumerable<IClusterTeardownTask> NodeStoppedTasks { get; } = new List<IClusterTeardownTask>
		{
			new CleanUpDirectoriesAfterNodeStopped()
		};

		protected static IEnumerable<IClusterComposeTask> AfterStartedTasks { get; } = new List<IClusterComposeTask>
		{
			new ValidateRunningVersion(),
			new ValidateClusterStateTask(),
			new ValidatePluginsTask(),
		};
	}


	public class EphemeralClusterComposer<TConfiguration> : EphemeralClusterComposerBase
		where TConfiguration : EphemeralClusterConfiguration
	{
		private readonly object _lock = new object();
		public EphemeralClusterComposer(IEphemeralCluster<TConfiguration> cluster) => Cluster = cluster;

		private IEphemeralCluster<TConfiguration> Cluster { get; }

		private bool NodeStarted { get; set; }

		public void OnStop() => Itterate(NodeStoppedTasks, (t, c, fs) => t.Run(c, NodeStarted), false);

		public void Install() => Itterate(InstallationTasks, (t, c, fs) => t.Run(c));

		public void OnBeforeStart()
		{
			var tasks = new List<IClusterComposeTask>(BeforeStart);
			if (Cluster.ClusterConfiguration.AdditionalBeforeNodeStartedTasks != null)
				tasks.AddRange(Cluster.ClusterConfiguration.AdditionalBeforeNodeStartedTasks);

			if (Cluster.ClusterConfiguration.PrintYamlFilesInConfigFolder)
				tasks.Add(new PrintYamlContents());

			Itterate(tasks, (t, c, fs) => t.Run(c));

			NodeStarted = true;
		}

		public void OnAfterStart()
		{
			if (Cluster.ClusterConfiguration.SkipBuiltInAfterStartTasks) return;
			var tasks = new List<IClusterComposeTask>(AfterStartedTasks);
			if (Cluster.ClusterConfiguration.AdditionalAfterStartedTasks != null)
				tasks.AddRange(Cluster.ClusterConfiguration.AdditionalAfterStartedTasks);
			Itterate(tasks, (t, c, fs) => t.Run(c), false);
		}

		private void Itterate<T>(IEnumerable<T> collection,
			Action<T, IEphemeralCluster<TConfiguration>, INodeFileSystem> act, bool callOnStop = true)
		{
			lock (_lock)
			{
				var cluster = Cluster;
				foreach (var task in collection)
					try
					{
						act(task, cluster, cluster.FileSystem);
					}
					catch (Exception)
					{
						if (callOnStop) OnStop();
						throw;
					}
			}
		}
	}
}
