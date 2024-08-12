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
using System.IO;
using System.Linq;

namespace Tests.Configuration;

public class YamlConfiguration : TestConfigurationBase
{
    private readonly Dictionary<string, string> _config;

    public YamlConfiguration(string configurationFile)
    {
        if (!File.Exists(configurationFile)) return;

        _config = File.ReadAllLines(configurationFile)
            .Where(l => !l.Trim().StartsWith("#") && !string.IsNullOrWhiteSpace(l))
            .ToDictionary(ConfigName, ConfigValue);

        Mode = GetTestMode(_config["mode"]);
        var version = _config["opensearch_version"];
        OpenSearchVersion = version;
        if (string.IsNullOrWhiteSpace(version))
            throw new Exception("No default version was set in test.yaml or test.default.yaml");
        ForceReseed = BoolConfig("force_reseed", false);
        TestOnlyOne = BoolConfig("test_only_one", false);
        TestAgainstAlreadyRunningOpenSearch = BoolConfig("test_against_already_running_opensearch", true);
        ClusterFilter = _config.ContainsKey("cluster_filter") ? _config["cluster_filter"] : null;
        TestFilter = _config.ContainsKey("test_filter") ? _config["test_filter"] : null;

        var externalSeed = _config.TryGetValue("seed", out var seed) ? int.Parse(seed) : (int?)null;
        SetExternalSeed(externalSeed, out var randomizer);
        Random = new RandomConfiguration
        {
            SourceSerializer = RandomBool("source_serializer", randomizer),
            TypedKeys = RandomBool("typed_keys", randomizer),
            HttpCompression = RandomBool("http_compression", randomizer),
        };
    }

    private bool BoolConfig(string key, bool @default) => _config.TryGetValue(key, out var v) ? bool.Parse(v) : @default;

    private bool RandomBool(string key, Random random) =>
        _config.TryGetValue($"random_{key}", out var v) ? bool.Parse(v) : random.NextDouble() >= 0.5;

    private static string ConfigName(string configLine) => Parse(configLine, 0);

    private static string ConfigValue(string configLine) => Parse(configLine, 1);

    private static string Parse(string configLine, int index) => configLine.Split(':')[index].Trim(' ');

    private static TestMode GetTestMode(string mode)
    {
        switch (mode)
        {
            case "unit":
            case "u":
                return TestMode.Unit;
            case "integration":
            case "i":
                return TestMode.Integration;
            case "mixed":
            case "m":
                return TestMode.Mixed;
            default:
                throw new ArgumentException($"Unknown test mode: {mode}");
        }
    }
}
