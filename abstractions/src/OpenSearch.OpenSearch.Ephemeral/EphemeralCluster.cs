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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OpenSearch.OpenSearch.Managed;
using OpenSearch.OpenSearch.Managed.Configuration;
using OpenSearch.Stack.ArtifactsApi;

namespace OpenSearch.OpenSearch.Ephemeral;

public class EphemeralCluster : EphemeralCluster<EphemeralClusterConfiguration>
{
    public EphemeralCluster(OpenSearchVersion version, int numberOfNodes = 1)
        : base(new EphemeralClusterConfiguration(version, ClusterFeatures.None, numberOfNodes: numberOfNodes))
    {
    }

    public EphemeralCluster(EphemeralClusterConfiguration clusterConfiguration) : base(clusterConfiguration)
    {
    }
}

public abstract class EphemeralCluster<TConfiguration> : ClusterBase<TConfiguration>,
    IEphemeralCluster<TConfiguration>
    where TConfiguration : EphemeralClusterConfiguration
{
    protected EphemeralCluster(TConfiguration clusterConfiguration) : base(clusterConfiguration) =>
        Composer = new EphemeralClusterComposer<TConfiguration>(this);

    protected EphemeralClusterComposer<TConfiguration> Composer { get; }

    protected override void ModifyNodeConfiguration(NodeConfiguration nodeConfiguration, int port)
    {
        base.ModifyNodeConfiguration(nodeConfiguration, port);

        if (!ClusterConfiguration.EnableSsl) nodeConfiguration.Add("plugins.security.disabled", "true");
    }

    public virtual ICollection<Uri> NodesUris(string hostName = null)
    {
        hostName = hostName ?? (ClusterConfiguration.HttpFiddlerAware && Process.GetProcessesByName("fiddler").Any()
            ? "ipv4.fiddler"
            : "localhost");
        var ssl = ClusterConfiguration.EnableSsl ? "s" : "";
        return Nodes
            .Select(n => $"http{ssl}://{hostName}:{n.Port ?? 9200}")
            .Distinct()
            .Select(n => new Uri(n))
            .ToList();
    }

    public bool CachingAndCachedHomeExists()
    {
        if (!ClusterConfiguration.CacheOpenSearchHomeInstallation) return false;
        var cachedOpenSearchHomeFolder = Path.Combine(FileSystem.LocalFolder, GetCacheFolderName());
        return Directory.Exists(cachedOpenSearchHomeFolder);
    }

    public virtual string GetCacheFolderName()
    {
        var config = ClusterConfiguration;

        var sb = new StringBuilder();
        sb.Append(EphemeralClusterComposerBase.InstallationTasks.Count());
        sb.Append("-");
        if (config.EnableSsl) sb.Append("ssl");
        if (config.Plugins != null && config.Plugins.Count > 0)
        {
            sb.Append("-");
            foreach (var p in config.Plugins.OrderBy(p => p.SubProductName))
                sb.Append(p.SubProductName.ToLowerInvariant());
        }

        var name = sb.ToString();

        return CalculateSha1(name, Encoding.UTF8);
    }

    protected override void OnBeforeStart()
    {
        Composer.Install();
        Composer.OnBeforeStart();
    }

    protected override void OnDispose() => Composer.OnStop();

    protected override void OnAfterStarted() => Composer.OnAfterStart();

    protected override string SeeLogsMessage(string message)
    {
        var log = Path.Combine(FileSystem.LogsPath, $"{ClusterConfiguration.ClusterName}.log");
        if (!File.Exists(log) || ClusterConfiguration.ShowOpenSearchOutputAfterStarted) return message;
        if (!Started) return message;
        using (var fileStream = new FileStream(log, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var textReader = new StreamReader(fileStream))
        {
            var logContents = textReader.ReadToEnd();
            return message + $" contents of {log}:{Environment.NewLine}" + logContents;
        }
    }

    public static string CalculateSha1(string text, Encoding enc)
    {
        var buffer = enc.GetBytes(text);
        var cryptoTransformSha1 = new SHA1CryptoServiceProvider();
        return BitConverter.ToString(cryptoTransformSha1.ComputeHash(buffer))
            .Replace("-", "").ToLowerInvariant().Substring(0, 12);
    }
}
