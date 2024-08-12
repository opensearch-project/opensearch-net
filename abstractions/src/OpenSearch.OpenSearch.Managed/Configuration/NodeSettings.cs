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
using OpenSearch.Stack.ArtifactsApi;

namespace OpenSearch.OpenSearch.Managed.Configuration;

public class NodeSettings : List<NodeSetting>
{
    public NodeSettings()
    {
    }

    public NodeSettings(NodeSettings settings) : base(settings)
    {
    }

    public void Add(string setting)
    {
        var s = setting.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
        if (s.Length != 2)
            throw new ArgumentException($"Can only add node settings in key=value from but received: {setting}");
        Add(new NodeSetting(s[0], s[1], null));
    }

    public void Add(string key, string value) => Add(new NodeSetting(key, value, null));

    public void Add(string key, string value, string versionRange) =>
        Add(new NodeSetting(key, value, versionRange));

    public string[] ToCommandLineArguments(OpenSearchVersion version) =>
        this
            //if a node setting is only applicable for a certain version make sure its filtered out
            .Where(s => string.IsNullOrEmpty(s.VersionRange) || version.InRange(s.VersionRange))
            //allow additional settings to take precedence over already DefaultNodeSettings
            //without relying on opensearch to dedup
            .GroupBy(setting => setting.Key)
            .Select(g => g.Last())
            .SelectMany(s => new[] { "-E", s.ToString() })
            .ToArray();
}
