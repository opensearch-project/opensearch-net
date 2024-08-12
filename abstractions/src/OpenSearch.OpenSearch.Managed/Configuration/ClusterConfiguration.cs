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
using System.IO;
using OpenSearch.OpenSearch.Managed.FileSystem;
using OpenSearch.Stack.ArtifactsApi;
using OpenSearch.Stack.ArtifactsApi.Products;

namespace OpenSearch.OpenSearch.Managed.Configuration;

public interface IClusterConfiguration<out TFileSystem> where TFileSystem : INodeFileSystem
{
    TFileSystem FileSystem { get; }

    string ClusterName { get; }
    NodeSettings DefaultNodeSettings { get; }
    OpenSearchVersion Version { get; }
    int NumberOfNodes { get; }
    int StartingPortNumber { get; set; }
    bool NoCleanupAfterNodeStopped { get; set; }

    bool ShowOpenSearchOutputAfterStarted { get; set; }
    bool CacheOpenSearchHomeInstallation { get; set; }

    string CreateNodeName(int? node);
}

public class ClusterConfiguration : ClusterConfiguration<NodeFileSystem>
{
    public ClusterConfiguration(OpenSearchVersion version, string openSearchHome, int numberOfNodes = 1)
        : base(version, (v, s) => new NodeFileSystem(v, openSearchHome), numberOfNodes, null) { }

    public ClusterConfiguration(OpenSearchVersion version, Func<OpenSearchVersion, string, NodeFileSystem> fileSystem = null,
        int numberOfNodes = 1,
        string clusterName = null
    )
        : base(version, fileSystem ?? ((v, s) => new NodeFileSystem(v, s)), numberOfNodes, clusterName) { }
}

public class ClusterConfiguration<TFileSystem> : IClusterConfiguration<TFileSystem>
    where TFileSystem : INodeFileSystem
{
    /// <summary>
    ///     Creates a new instance of a configuration for an OpenSearch cluster.
    /// </summary>
    /// <param name="version">The version of OpenSearch</param>
    /// <param name="fileSystem">
    ///     A delegate to create the instance of <typeparamref name="TFileSystem" />.
    ///     Passed the OpenSearch version and the Cluster name
    /// </param>
    /// <param name="numberOfNodes">The number of nodes in the cluster</param>
    /// <param name="clusterName">The name of the cluster</param>
    public ClusterConfiguration(OpenSearchVersion version, Func<OpenSearchVersion, string, TFileSystem> fileSystem,
        int numberOfNodes = 1, string clusterName = null
    )
    {
        if (fileSystem == null) throw new ArgumentException(nameof(fileSystem));

        ClusterName = clusterName;
        Version = version;
        Artifact = version.Artifact(Product.From("opensearch"));
        FileSystem = fileSystem(Version, ClusterName);
        NumberOfNodes = numberOfNodes;

        var fs = FileSystem;
        Add("node.max_local_storage_nodes", numberOfNodes.ToString(CultureInfo.InvariantCulture), "1.0.0");

        Add("cluster.name", clusterName);
        Add("path.repo", fs.RepositoryPath);
        Add("path.data", fs.DataPath);
        var logsPathDefault = Path.Combine(fs.OpenSearchHome, "logs");
        if (logsPathDefault != fs.LogsPath) Add("path.logs", fs.LogsPath);
    }

    public Artifact Artifact { get; }

    public string JavaHomeEnvironmentVariable => "JAVA_HOME";

    /// <summary> Will print the contents of all the yaml files when starting the cluster up, great for debugging purposes</summary>
    public bool PrintYamlFilesInConfigFolder { get; set; }

    public string ClusterName { get; }
    public OpenSearchVersion Version { get; }
    public TFileSystem FileSystem { get; }
    public int NumberOfNodes { get; }
    public int StartingPortNumber { get; set; } = 9200;
    public bool NoCleanupAfterNodeStopped { get; set; }

    /// <summary>
    ///     Whether <see cref="OpenSearchNode" /> should continue to write output to console after it has started.
    ///     <para>Defaults to <c>true</c></para>
    /// </summary>
    public bool ShowOpenSearchOutputAfterStarted { get; set; } = true;

    public bool CacheOpenSearchHomeInstallation { get; set; }

    /// <summary>The node settings to apply to each started node</summary>
    public NodeSettings DefaultNodeSettings { get; } = new NodeSettings();

    /// <summary>
    ///     Creates a node name
    /// </summary>
    public virtual string CreateNodeName(int? node) =>
        node.HasValue ? $"managed-opensearch-{node}" : " managed-opensearch";

    /// <summary>
    ///     Calculates the quorum given the number of instances
    /// </summary>
    private static int Quorum(int instanceCount) => Math.Max(1, (int)Math.Floor((double)instanceCount / 2) + 1);

    /// <summary>
    ///     Creates a node attribute for the version of OpenSearch
    /// </summary>
    public string AttributeKey(string attribute)
    {
        var attr = "attr.";
        return $"node.{attr}{attribute}";
    }

    /// <summary>
    ///     Adds a node setting to the default node settings
    /// </summary>
    protected void Add(string key, string value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;

        DefaultNodeSettings.Add(key, value);
    }

    /// <summary>
    ///     Adds a node setting to the default node settings only if the OpenSearch
    ///     version is in the range.
    /// </summary>
    protected void Add(string key, string value, string range)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) return;

        if (string.IsNullOrWhiteSpace(range) || Version.InRange(range))
            DefaultNodeSettings.Add(key, value, range);
    }
}
