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
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Ephemeral.Plugins;
using OpenSearch.OpenSearch.Xunit;
using OpenSearch.Stack.ArtifactsApi.Products;
using OpenSearch.Net;
using OpenSearch.Client;
using Tests.Configuration;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Tasks;
using Tests.Domain.Extensions;

namespace Tests.Core.ManagedOpenSearch.Clusters;

public abstract class ClientTestClusterBase(ClientTestClusterConfiguration configuration)
    : XunitClusterBase<ClientTestClusterConfiguration>(configuration),
        IOpenSearchClientTestCluster
{
    protected ClientTestClusterBase() : this(new ClientTestClusterConfiguration()) { }

    protected ClientTestClusterBase(params OpenSearchPlugin[] plugins) : this(new ClientTestClusterConfiguration(plugins)) { }

    public IOpenSearchClient Client => this.GetOrAddClient(s => ConnectionSettings(s.ApplyDomainSettings()));

    protected virtual ConnectionSettings ConnectionSettings(ConnectionSettings s) => s;

    protected sealed override void SeedCluster()
    {
        var clusterHealth = new ClusterHealthRequest
        {
            WaitForNodes = ClusterConfiguration.NumberOfNodes.ToString(),
            WaitForStatus = HealthStatus.Green,
            Level = ClusterHealthLevel.Shards
        };

        Client.Cluster.Health(clusterHealth).ShouldBeValid();

        SeedNode();

        Client.Cluster.Health(clusterHealth).ShouldBeValid();
    }

    protected virtual void SeedNode() { }
}

public class ClientTestClusterConfiguration : XunitClusterConfiguration
{
    public ClientTestClusterConfiguration(params OpenSearchPlugin[] plugins) : this(numberOfNodes: 1, plugins: plugins) { }

    public ClientTestClusterConfiguration(ClusterFeatures features = ClusterFeatures.SSL, int numberOfNodes = 1,
        params OpenSearchPlugin[] plugins
    )
        : base(TestClient.Configuration.OpenSearchVersion, features, new OpenSearchPlugins(plugins), numberOfNodes)
    {
        TestConfiguration = TestClient.Configuration;

        ShowOpenSearchOutputAfterStarted = TestConfiguration.ShowOpenSearchOutputAfterStarted;
        HttpFiddlerAware = true;

        CacheOpenSearchHomeInstallation = true;

        Add(AttributeKey("testingcluster"), "true");
        Add(AttributeKey("gateway"), "true");

        Add("cluster.routing.allocation.disk.watermark.low", "100%");
        Add("cluster.routing.allocation.disk.watermark.high", "100%");
        Add("cluster.routing.allocation.disk.watermark.flood_stage", "100%");

        Add("script.disable_max_compilations_rate", "true");
        Add("script.allowed_types", "inline,stored");

        AdditionalBeforeNodeStartedTasks.Add(new WriteAnalysisFiles());
    }

    public string AnalysisFolder => Path.Combine(FileSystem.ConfigPath, "analysis");
    public TestConfigurationBase TestConfiguration { get; }
}
