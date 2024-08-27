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
using System.Linq;
using OpenSearch.OpenSearch.Ephemeral.Plugins;
using OpenSearch.OpenSearch.Ephemeral.Tasks;
using OpenSearch.OpenSearch.Managed.Configuration;
using OpenSearch.Stack.ArtifactsApi;

namespace OpenSearch.OpenSearch.Ephemeral;

public class EphemeralClusterConfiguration : ClusterConfiguration<EphemeralFileSystem>
{
    public EphemeralClusterConfiguration(OpenSearchVersion version, OpenSearchPlugins plugins = null,
        int numberOfNodes = 1)
        : this(version, ClusterFeatures.None, plugins, numberOfNodes)
    {
    }

    public EphemeralClusterConfiguration(OpenSearchVersion version, ClusterFeatures features,
        OpenSearchPlugins plugins = null, int numberOfNodes = 1)
        : base(version, (v, s) => new EphemeralFileSystem(v, s), numberOfNodes, EphemeralClusterName)
    {
        Features = features;

        var pluginsList = plugins?.ToList() ?? [];
        Plugins = new OpenSearchPlugins(pluginsList);

        Add("plugins.security.disabled", (!EnableSsl).ToString().ToLowerInvariant());
        if (EnableSsl) Add("plugins.security.audit.type", "debug");
    }

    private static string UniqueishSuffix => Guid.NewGuid().ToString("N").Substring(0, 6);
    private static string EphemeralClusterName => $"ephemeral-cluster-{UniqueishSuffix}";

    /// <summary>
    ///     The features supported by the cluster
    /// </summary>
    public ClusterFeatures Features { get; }

    /// <summary>
    ///     The collection of plugins to install
    /// </summary>
    public OpenSearchPlugins Plugins { get; }

    /// <summary>
    ///     Validates that the plugins to install can be installed on the target OpenSearch version.
    ///     This can be useful to fail early when subsequent operations are relying on installation
    ///     succeeding.
    /// </summary>
    public bool ValidatePluginsToInstall { get; set; } = true;

    public bool EnableSsl => Features.HasFlag(ClusterFeatures.SSL);

    public IList<IClusterComposeTask> AdditionalBeforeNodeStartedTasks { get; } = new List<IClusterComposeTask>();

    public IList<IClusterComposeTask> AdditionalAfterStartedTasks { get; } = new List<IClusterComposeTask>();

    /// <summary>
    ///     Expert level setting, skips all built-in validation tasks for cases where you need to guarantee your call is the
    ///     first call into the cluster
    /// </summary>
    public bool SkipBuiltInAfterStartTasks { get; set; }

    /// <summary> Bootstrapping HTTP calls should attempt to auto route traffic through fiddler if its running </summary>
    public bool HttpFiddlerAware { get; set; }

    protected virtual string NodePrefix => "ephemeral";

    public override string CreateNodeName(int? node)
    {
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        return $"{NodePrefix}-node-{suffix}{node}";
    }
}
