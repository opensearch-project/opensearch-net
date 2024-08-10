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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using OpenSearch.OpenSearch.Managed.Configuration;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;
using OpenSearch.OpenSearch.Managed.FileSystem;
using OpenSearch.Stack.ArtifactsApi;
using ProcNet.Std;

namespace OpenSearch.OpenSearch.Managed;

public interface ICluster<out TConfiguration> : IDisposable
    where TConfiguration : IClusterConfiguration<NodeFileSystem>
{
    string ClusterMoniker { get; }
    TConfiguration ClusterConfiguration { get; }
    INodeFileSystem FileSystem { get; }
    bool Started { get; }
    ReadOnlyCollection<OpenSearchNode> Nodes { get; }
    IConsoleLineHandler Writer { get; }

    IDisposable Start();

    IDisposable Start(TimeSpan waitForStarted);

    IDisposable Start(IConsoleLineHandler writer, TimeSpan waitForStarted);
}


public abstract class ClusterBase : ClusterBase<ClusterConfiguration>
{
    protected ClusterBase(ClusterConfiguration clusterConfiguration) : base(clusterConfiguration)
    {
    }
}

public abstract class ClusterBase<TConfiguration> : ICluster<TConfiguration>
    where TConfiguration : IClusterConfiguration<NodeFileSystem>
{
    private Action<NodeConfiguration, int> _defaultConfigSelector = (n, i) => { };

    protected ClusterBase(TConfiguration clusterConfiguration)
    {
        ClusterConfiguration = clusterConfiguration;
        ClusterMoniker = GetType().Name.Replace("Cluster", "");

        NodeConfiguration Modify(NodeConfiguration n, int p)
        {
            ModifyNodeConfiguration(n, p);
            return n;
        }

        var nodes =
            (from port in Enumerable.Range(ClusterConfiguration.StartingPortNumber,
                    ClusterConfiguration.NumberOfNodes)
             let config = new NodeConfiguration(clusterConfiguration, port, ClusterMoniker)
             {
                 ShowOpenSearchOutputAfterStarted =
                     clusterConfiguration.ShowOpenSearchOutputAfterStarted,
             }
             let node = new OpenSearchNode(Modify(config, port))
             {
                 AssumeStartedOnNotEnoughMasterPing = ClusterConfiguration.NumberOfNodes > 1,
             }
             select node).ToList();

        var initialMasterNodes = string.Join(",", nodes.Select(n => n.NodeConfiguration.DesiredNodeName));
        foreach (var node in nodes)
            node.NodeConfiguration.InitialMasterNodes(initialMasterNodes);

        Nodes = new ReadOnlyCollection<OpenSearchNode>(nodes);
    }

    /// <summary>
    ///     A short name to identify the cluster defaults to the <see cref="ClusterBase" /> subclass name with Cluster
    ///     removed
    /// </summary>
    public virtual string ClusterMoniker { get; }

    public TConfiguration ClusterConfiguration { get; }
    public INodeFileSystem FileSystem => ClusterConfiguration.FileSystem;

    public ReadOnlyCollection<OpenSearchNode> Nodes { get; }
    public bool Started { get; private set; }
    public IConsoleLineHandler Writer { get; private set; } = NoopConsoleLineWriter.Instance;

    public IDisposable Start() => Start(TimeSpan.FromMinutes(2));

    public IDisposable Start(TimeSpan waitForStarted)
    {
        var nodes = Nodes.Select(n => n.NodeConfiguration.DesiredNodeName).ToArray();
        var lineHighlightWriter = new LineHighlightWriter(nodes, LineOutParser.OpenSearch);
        return Start(lineHighlightWriter, waitForStarted);
    }

    public IDisposable Start(IConsoleLineHandler writer, TimeSpan waitForStarted)
    {
        Writer = writer ?? NoopConsoleLineWriter.Instance;

        OnBeforeStart();

        var subscriptions = new Subscriptions();

        foreach (var node in Nodes)
        {
            subscriptions.Add(node.SubscribeLines(writer));
            if (node.WaitForStarted(waitForStarted)) continue;

            var nodeExceptions = Nodes.Select(n => n.LastSeenException).Where(e => e != null).ToList();
            writer?.WriteError(
                $"{{{GetType().Name}.{nameof(Start)}}} cluster did not start after {waitForStarted}");
            throw new AggregateException($"Not all nodes started after waiting {waitForStarted}", nodeExceptions);
        }

        Started = Nodes.All(n => n.NodeStarted);
        if (!Started)
        {
            var nodeExceptions = Nodes.Select(n => n.LastSeenException).Where(e => e != null).ToList();
            var message = $"{{{GetType().Name}.{nameof(Start)}}} cluster did not start successfully";
            var seeLogsMessage = SeeLogsMessage(message);
            writer?.WriteError(seeLogsMessage);
            throw new AggregateException(seeLogsMessage, nodeExceptions);
        }

        try
        {
            OnAfterStarted();
            SeedCluster();
        }
        catch (Exception e)
        {
            writer?.WriteError(e.ToString());
            throw;
        }

        return subscriptions;
    }

    public void Dispose()
    {
        Started = false;
        foreach (var node in Nodes)
            node?.Dispose();

        OnDispose();
    }

    protected virtual void ModifyNodeConfiguration(NodeConfiguration nodeConfiguration, int port)
    {
    }

    protected virtual void SeedCluster()
    {
    }


    protected virtual string SeeLogsMessage(string message)
    {
        var log = Path.Combine(FileSystem.LogsPath, $"{ClusterConfiguration.ClusterName}.log");
        return $"{message} see {log} to diagnose the issue";
    }

    public void WaitForExit(TimeSpan waitForCompletion)
    {
        foreach (var node in Nodes)
            node.WaitForCompletion(waitForCompletion);
    }

    protected virtual void OnAfterStarted()
    {
    }

    protected virtual void OnBeforeStart()
    {
    }

    protected virtual void OnDispose()
    {
    }

    private class Subscriptions : IDisposable
    {
        private List<IDisposable> Disposables { get; } = new List<IDisposable>();

        public void Dispose()
        {
            foreach (var d in Disposables) d.Dispose();
        }

        internal void Add(IDisposable disposable) => Disposables.Add(disposable);
    }
}
