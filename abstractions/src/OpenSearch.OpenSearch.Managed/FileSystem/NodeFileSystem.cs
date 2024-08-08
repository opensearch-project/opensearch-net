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
using System.IO;
using System.Runtime.InteropServices;
using OpenSearch.Stack.ArtifactsApi;
using OpenSearch.Stack.ArtifactsApi.Products;

namespace OpenSearch.OpenSearch.Managed.FileSystem
{
    /// <inheritdoc />
    public class NodeFileSystem : INodeFileSystem
    {
        protected const string SubFolder = "OpenSearchManaged";

        public NodeFileSystem(OpenSearchVersion version, string openSearchHome = null)
        {
            Version = version;
            Artifact = version.Artifact(Product.OpenSearch);
            LocalFolder = AppDataFolder(version);
            OpenSearchHome = openSearchHome ??
                                GetOpenSearchHomeVariable() ?? throw new ArgumentNullException(nameof(openSearchHome));

            ConfigEnvironmentVariableName = "OPENSEARCH_PATH_CONF";
        }

        protected OpenSearchVersion Version { get; }
        protected Artifact Artifact { get; }

        private static bool IsMono { get; } = Type.GetType("Mono.Runtime") != null;

        protected static string BinarySuffix => IsMono || Path.DirectorySeparatorChar == '/' ? "" : ".bat";

        /// <inheritdoc />
        public string Binary => Path.Combine(OpenSearchHome, "bin", "opensearch") + BinarySuffix;

        /// <inheritdoc />
        public string PluginBinary => Path.Combine(OpenSearchHome, "bin", "opensearch-plugin") + BinarySuffix;

        /// <inheritdoc />
        public string OpenSearchHome { get; }

        /// <inheritdoc />
        public string LocalFolder { get; }

        /// <inheritdoc />
        public virtual string ConfigPath => null;

        /// <inheritdoc />
        public virtual string DataPath => null;

        /// <inheritdoc />
        public virtual string LogsPath => null;

        /// <inheritdoc />
        public virtual string RepositoryPath => null;

        public string ConfigEnvironmentVariableName { get; }

        protected static string AppDataFolder(OpenSearchVersion version)
        {
            var appData = GetApplicationDataDirectory();
            return Path.Combine(appData, SubFolder, version.Artifact(Product.OpenSearch).LocalFolderName);
        }

        protected static string GetOpenSearchHomeVariable() => Environment.GetEnvironmentVariable("OPENSEARCH_HOME");

        protected static string GetApplicationDataDirectory() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("LocalAppData")
                : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.Create);
    }
}
