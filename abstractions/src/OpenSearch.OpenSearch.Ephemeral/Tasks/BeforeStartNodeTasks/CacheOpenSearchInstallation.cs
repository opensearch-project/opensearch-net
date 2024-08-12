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

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.BeforeStartNodeTasks;

public class CacheOpenSearchInstallation : ClusterComposeTask
{
    public override void Run(IEphemeralCluster<EphemeralClusterConfiguration> cluster)
    {
        if (!cluster.ClusterConfiguration.CacheOpenSearchHomeInstallation) return;

        var fs = cluster.FileSystem;
        var cachedOpenSearchHomeFolder = Path.Combine(fs.LocalFolder, cluster.GetCacheFolderName());
        var cachedOpenSearchConfig = Path.Combine(cachedOpenSearchHomeFolder, "config");
        if (File.Exists(cachedOpenSearchConfig))
        {
            cluster.Writer?.WriteDiagnostic(
                $"{{{nameof(CacheOpenSearchInstallation)}}} cached home already exists [{cachedOpenSearchHomeFolder}]");
            return;
        }

        var source = fs.OpenSearchHome;
        var target = cachedOpenSearchHomeFolder;
        cluster.Writer?.WriteDiagnostic(
            $"{{{nameof(CacheOpenSearchInstallation)}}} caching {{{source}}} to [{target}]");
        CopyFolder(source, target, false);
    }
}
