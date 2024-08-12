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
using System.Threading;

namespace Tests.Configuration;

public static class TestConfiguration
{
    private static readonly Lazy<TestConfigurationBase> Lazy
        = new Lazy<TestConfigurationBase>(LoadConfiguration, LazyThreadSafetyMode.ExecutionAndPublication);

    public static TestConfigurationBase Instance => Lazy.Value;

    private static TestConfigurationBase LoadConfiguration() =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OSC_COMMAND_LINE_BUILD"))
            ? (TestConfigurationBase)LoadCommandLineConfiguration()
            : LoadYamlConfiguration();

    /// <summary>
    /// Loads configuration by reading from the yaml and overriding specific configuration settings through
    /// environment variables set by the command line build.
    /// </summary>
    private static EnvironmentConfiguration LoadCommandLineConfiguration()
    {
        var yamlFile = Environment.GetEnvironmentVariable("OSC_YAML_FILE");
        if (string.IsNullOrWhiteSpace(yamlFile))
            throw new Exception("expected OSC_YAML_FILE to be set when calling build.bat or build.sh");
        if (!File.Exists(yamlFile))
            throw new Exception($"expected {yamlFile} to exist on disk OSC_YAML_FILE seems misconfigured");

        //load the test seed from the explicitly passed yaml file when running from FAKE
        var tempYamlConfiguration = new YamlConfiguration(yamlFile);
        return new EnvironmentConfiguration(tempYamlConfiguration);
    }

    public static string LocateTestYamlFile()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        var testsConfigurationFolder = FindTestsConfigurationFolder(directory) ?? throw new Exception($"Tried to locate a parent test folder starting from  pwd:{directory.FullName}");
        var localYamlFile = Path.Combine(testsConfigurationFolder.FullName, "tests.yaml");
        var defaultYamlFile = Path.Combine(testsConfigurationFolder.FullName, "tests.default.yaml");
        if (File.Exists(localYamlFile)) return localYamlFile;

        if (File.Exists(defaultYamlFile)) return defaultYamlFile;

        throw new Exception($"Tried to load a yaml file from {testsConfigurationFolder.FullName}");

    }

    /// <summary>
    /// The test configuration loaded when you run the tests
    /// <para> - from the IDE </para>
    /// <para> - when calling dotnet test in the tests directory </para>
    /// </summary>
    private static YamlConfiguration LoadYamlConfiguration()
    {
        var yamlFile = LocateTestYamlFile();
        return new YamlConfiguration(yamlFile);
    }

    private static DirectoryInfo FindTestsConfigurationFolder(DirectoryInfo directoryInfo)
    {
        do
        {
            var yamlConfigDir = Path.Combine(directoryInfo.FullName, "Tests.Configuration");
            if (Directory.Exists(yamlConfigDir))
                return new DirectoryInfo(yamlConfigDir);

            // traverse up
            directoryInfo = directoryInfo.Parent;
        } while (directoryInfo != null);
        return null;
    }
}
