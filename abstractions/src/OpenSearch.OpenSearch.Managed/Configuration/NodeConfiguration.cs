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
using System.Globalization;
using OpenSearch.OpenSearch.Managed.FileSystem;
using OpenSearch.Stack.ArtifactsApi;
using ProcNet;

namespace OpenSearch.OpenSearch.Managed.Configuration
{
	public class NodeConfiguration
	{
		private Action<StartArguments> _defaultStartArgs = s => { };

		public NodeConfiguration(OpenSearchVersion version, int? port = null) : this(new ClusterConfiguration(version, ServerType.DEFAULT),
			port)
		{
		}

		public NodeConfiguration(IClusterConfiguration<NodeFileSystem> clusterConfiguration, int? port = null,
			string nodePrefix = null)
		{
			ClusterConfiguration = clusterConfiguration;
			DesiredPort = port;
			DesiredNodeName = CreateNodeName(port, nodePrefix) ?? clusterConfiguration.CreateNodeName(port);
			Settings = new NodeSettings(clusterConfiguration.DefaultNodeSettings);

			if (!string.IsNullOrWhiteSpace(DesiredNodeName)) Settings.Add("node.name", DesiredNodeName);
			if (DesiredPort.HasValue)
				Settings.Add("http.port", DesiredPort.Value.ToString(CultureInfo.InvariantCulture));
		}

		private IClusterConfiguration<NodeFileSystem> ClusterConfiguration { get; }

		public int? DesiredPort { get; }
		public string DesiredNodeName { get; }

		public Action<StartArguments> ModifyStartArguments
		{
			get => _defaultStartArgs;
			set => _defaultStartArgs = value ?? (s => { });
		}

		/// <summary>
		///     Wheter <see cref="OpenSearchNode" /> should continue to write output to console after it has started.
		///     <para>Defaults to true but useful to turn of if it proofs to be too noisy </para>
		/// </summary>
		public bool ShowOpenSearchOutputAfterStarted { get; set; } = true;

		/// <summary>
		///     The expected duration of the shut down sequence for OpenSearch. After this wait duration a hard kill will occur.
		/// </summary>
		public TimeSpan WaitForShutdown { get; set; } = TimeSpan.FromSeconds(10);

		/// <summary>
		///     Copy of <see cref="IClusterConfiguration{TFileSystem}.DefaultNodeSettings" />. Add individual node settings here.
		/// </summary>
		public NodeSettings Settings { get; }

		public INodeFileSystem FileSystem => ClusterConfiguration.FileSystem;
		public OpenSearchVersion Version => ClusterConfiguration.Version;
		public ServerType ServerType => ClusterConfiguration.ServerType;
		public string[] CommandLineArguments => Settings.ToCommandLineArguments(Version);

		public void InitialMasterNodes(string initialMasterNodes) =>
			Settings.Add("cluster.initial_master_nodes", initialMasterNodes, ">=1.0.0");

		public string AttributeKey(string attribute)
		{
			var attr = "attr.";
			return $"node.{attr}{attribute}";
		}

		public void Add(string key, string value) => Settings.Add(key, value);

		private string CreateNodeName(int? node, string prefix = null)
		{
			if (prefix == null) return null;
			var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
			return $"{prefix.Replace("Cluster", "").ToLowerInvariant()}-node-{suffix}{node}";
		}
	}
}
