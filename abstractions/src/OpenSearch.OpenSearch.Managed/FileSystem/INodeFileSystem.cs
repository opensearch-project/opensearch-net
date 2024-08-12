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

namespace OpenSearch.OpenSearch.Managed.FileSystem;

/// <summary>
///     The file system for an OpenSearch node
/// </summary>
public interface INodeFileSystem
{
    /// <summary>
    ///     The path to the script to start OpenSearch
    /// </summary>
    string Binary { get; }

    /// <summary>
    ///     The path to the script to manage plugins
    /// </summary>
    string PluginBinary { get; }

    /// <summary>
    ///     The path to the home directory
    /// </summary>
    string OpenSearchHome { get; }

    /// <summary>
    ///     The path to the config directory
    /// </summary>
    string ConfigPath { get; }

    /// <summary>
    ///     The path to the data directory
    /// </summary>
    string DataPath { get; }

    /// <summary>
    ///     The path to the logs directory
    /// </summary>
    string LogsPath { get; }

    /// <summary>
    ///     The path to the repository directory
    /// </summary>
    string RepositoryPath { get; }

    /// <summary>
    ///     The path to the directory in which this node resides
    /// </summary>
    string LocalFolder { get; }

    /// <summary> The config environment variable to use for this version</summary>
    string ConfigEnvironmentVariableName { get; }
}
