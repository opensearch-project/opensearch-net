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
using System.Reflection;
using FluentAssertions.Common;
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Managed;
using Tests.Configuration;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.ClusterLauncher;

public static class ClusterLaunchProgram
{
    private static ICluster<EphemeralClusterConfiguration> Instance { get; set; }

    public static int Main(string[] arguments)
    {
        var clusters = GetClusters();
        if (arguments.Length < 1)
        {
            Console.Error.WriteLine("cluster command needs atleast one argument to indicate the cluster to start");
            foreach (var c in clusters)
                Console.WriteLine(" - " + c.Name.Replace("Cluster", "").ToLowerInvariant());

            return 3;
        }

        // Force TestConfiguration to load as if started from the command line even if we are actually starting
        // from the IDE. Also force configuration mode to integration test so the seeders run
        Environment.SetEnvironmentVariable("OSC_COMMAND_LINE_BUILD", "1", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("OSC_INTEGRATION_TEST", "1", EnvironmentVariableTarget.Process);

        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OSC_YAML_FILE")))
        {
            // build always sets previous argument, assume we are running from the IDE or dotnet run
            var yamlFile = TestConfiguration.LocateTestYamlFile();
            Environment.SetEnvironmentVariable("OSC_YAML_FILE", yamlFile, EnvironmentVariableTarget.Process);
        }

        // if version is passed this will take precedence over the version in the yaml file
        // in the constructor of EnvironmentConfiguration
        var clusterName = arguments[0];
        if (arguments.Length > 1)
            Environment.SetEnvironmentVariable("OSC_INTEGRATION_VERSION", arguments[1], EnvironmentVariableTarget.Process);

        var cluster = clusters.FirstOrDefault(c => c.Name.StartsWith(clusterName, StringComparison.OrdinalIgnoreCase));
        if (cluster == null)
        {
            Console.Error.WriteLine($"No cluster found that starts with '{clusterName}");
            return 4;
        }

        //best effort, wont catch all the things
        //https://github.com/dotnet/coreclr/issues/8565
        //Don't want to make this windows only by registering a SetConsoleCtrlHandler  though P/Invoke.
        AppDomain.CurrentDomain.ProcessExit += (s, ev) => Instance?.Dispose();
        Console.CancelKeyPress += (s, ev) => Instance?.Dispose();

        try
        {
            if (TryStartClientTestClusterBaseImplementation(cluster)) return 0;

            Console.Error.WriteLine($"Could not create an instance of '{cluster.FullName}");
            return 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Instance?.Dispose();
            throw;
        }
    }

    private static bool TryStartClientTestClusterBaseImplementation(Type cluster)
    {
        if (!(Activator.CreateInstance(cluster) is ClientTestClusterBase instance)) return false;

        Instance = instance;
        using (instance)
            return Run(instance);
    }

    private static bool Run(ICluster<EphemeralClusterConfiguration> instance)
    {
        TestConfiguration.Instance.DumpConfiguration();
        using var start = instance.Start();
        if (!instance.Started)
        {
            Console.Error.WriteLine($"Failed to start cluster: '{instance.GetType().FullName}");
            return false;
        }
        Console.WriteLine("Press any key to shutdown the running cluster");
        var c = default(ConsoleKeyInfo);
        while (c.Key != ConsoleKey.Q) c = Console.ReadKey();
        return true;
    }

    private static Type[] GetClusters()
    {
        IEnumerable<Type> types;

        try
        {
            types = typeof(IOpenSearchClientTestCluster).Assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(t => t != null);
        }

        return types
            .Where(t => typeof(IEphemeralCluster).IsAssignableFrom(t))
            .Where(t => !t.IsAbstract)
            .ToArray();
    }
}
