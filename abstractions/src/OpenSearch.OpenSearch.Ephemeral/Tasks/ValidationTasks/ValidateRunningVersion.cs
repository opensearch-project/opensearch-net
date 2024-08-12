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
using System.Net.Http;
using System.Threading;
using OpenSearch.OpenSearch.Managed.ConsoleWriters;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.ValidationTasks;

public class ValidateRunningVersion : ClusterComposeTask
{
    public override void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster)
    {
        void WriteDiagnostic(string message) =>
            cluster.Writer?.WriteDiagnostic($"{{{nameof(ValidateRunningVersion)}}} {message}");
        var requestedVersion = cluster.ClusterConfiguration.Version;

        WriteDiagnostic($"validating the cluster is running the requested version: {requestedVersion}");

        HttpResponseMessage catNodes = null;
        var retryCount = 4;
        var initialRetryWait = 5;
        foreach (var retry in Enumerable.Range(1, retryCount))
        {
            catNodes = Get(cluster, "_cat/nodes", "h=version");
            if (catNodes.IsSuccessStatusCode) break;
            var retryWait = TimeSpan.FromSeconds(initialRetryWait * retry);
            WriteDiagnostic($"{catNodes.StatusCode} response for GET _cat/nodes. Waiting to retry #{retry}");
            Thread.Sleep(retryWait);
        }
        if (catNodes is not { IsSuccessStatusCode: true })
        {
            throw new Exception(
                $"Calling _cat/nodes for version checking did not result in an OK response {GetResponseException(catNodes)}");
        }

        var nodeVersions = GetResponseString(catNodes).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        var anchorVersion = $"{requestedVersion.Major}.{requestedVersion.Minor}.{requestedVersion.Patch}";
        var allOnRequestedVersion = nodeVersions.All(v => v.Trim() == anchorVersion);
        if (!allOnRequestedVersion)
        {
            throw new Exception(
                $"Not all the running nodes in the cluster are on requested version: {anchorVersion} received: {string.Join(", ", nodeVersions)}");
        }
    }
}
