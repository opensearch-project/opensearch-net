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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Ephemeral.Tasks.ValidationTasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenSearch.OpenSearch.Xunit.Sdk
{
    internal class TestAssemblyRunner : XunitTestAssemblyRunner
    {
        private readonly Dictionary<Type, IEphemeralCluster<XunitClusterConfiguration>> _assemblyFixtureMappings =
            new Dictionary<Type, IEphemeralCluster<XunitClusterConfiguration>>();

        private readonly List<IGrouping<IEphemeralCluster<XunitClusterConfiguration>, GroupedByCluster>> _grouped;

        public TestAssemblyRunner(ITestAssembly testAssembly,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
        {
            var tests = OrderTestCollections();
            RunIntegrationTests = executionOptions.GetValue<bool>(nameof(OpenSearchXunitRunOptions.RunIntegrationTests));
            IntegrationTestsMayUseAlreadyRunningNode =
                executionOptions.GetValue<bool>(nameof(OpenSearchXunitRunOptions
                    .IntegrationTestsMayUseAlreadyRunningNode));
            RunUnitTests = executionOptions.GetValue<bool>(nameof(OpenSearchXunitRunOptions.RunUnitTests));
            TestFilter = executionOptions.GetValue<string>(nameof(OpenSearchXunitRunOptions.TestFilter));
            ClusterFilter = executionOptions.GetValue<string>(nameof(OpenSearchXunitRunOptions.ClusterFilter));

            //bit side effecty, sets up _assemblyFixtureMappings before possibly letting xunit do its regular concurrency thing
            _grouped = (from c in tests
                        let cluster = ClusterFixture(c.Item2.First().TestMethod.TestClass)
                        let testcase = new GroupedByCluster { Collection = c.Item1, TestCases = c.Item2, Cluster = cluster }
                        group testcase by testcase.Cluster
                into g
                        orderby g.Count() descending
                        select g).ToList();
        }

        public ConcurrentBag<RunSummary> Summaries { get; } = new ConcurrentBag<RunSummary>();

        public ConcurrentBag<Tuple<string, string>> FailedCollections { get; } =
            new ConcurrentBag<Tuple<string, string>>();

        public Dictionary<string, Stopwatch> ClusterTotals { get; } = new Dictionary<string, Stopwatch>();

        private bool RunIntegrationTests { get; }
        private bool IntegrationTestsMayUseAlreadyRunningNode { get; }
        private bool RunUnitTests { get; }
        private string TestFilter { get; }
        private string ClusterFilter { get; }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus b, ITestCollection c,
            IEnumerable<IXunitTestCase> t, CancellationTokenSource s)
        {
            var aggregator = new ExceptionAggregator(Aggregator);
            var fixtureObjects = new Dictionary<Type, object>();
            foreach (var kv in _assemblyFixtureMappings) fixtureObjects.Add(kv.Key, kv.Value);
            return new TestCollectionRunner(fixtureObjects, c, t, DiagnosticMessageSink, b, TestCaseOrderer, aggregator,
                    s)
                .RunAsync();
        }

        protected override async Task<RunSummary> RunTestCollectionsAsync(IMessageBus messageBus,
            CancellationTokenSource cancellationTokenSource)
        {
            //threading guess
            var defaultMaxConcurrency = Environment.ProcessorCount * 4;

            if (RunUnitTests && !RunIntegrationTests)
                return await UnitTestPipeline(defaultMaxConcurrency, messageBus, cancellationTokenSource)
                    .ConfigureAwait(false);

            return await IntegrationPipeline(defaultMaxConcurrency, messageBus, cancellationTokenSource)
                .ConfigureAwait(false);
        }


        private async Task<RunSummary> UnitTestPipeline(int defaultMaxConcurrency, IMessageBus messageBus,
            CancellationTokenSource ctx)
        {
            //make sure all clusters go in started state (won't actually start clusters in unit test mode)
            //foreach (var g in this._grouped) g.Key?.Start();

            var testFilters = CreateTestFilters(TestFilter);
            await _grouped.SelectMany(g => g)
                .ForEachAsync(defaultMaxConcurrency,
                    async g => { await RunTestCollections(messageBus, ctx, g, testFilters).ConfigureAwait(false); })
                .ConfigureAwait(false);
            //foreach (var g in this._grouped) g.Key?.Dispose();

            return new RunSummary
            {
                Total = Summaries.Sum(s => s.Total),
                Failed = Summaries.Sum(s => s.Failed),
                Skipped = Summaries.Sum(s => s.Skipped)
            };
        }

        private async Task<RunSummary> IntegrationPipeline(int defaultMaxConcurrency, IMessageBus messageBus,
            CancellationTokenSource ctx)
        {
            var testFilters = CreateTestFilters(TestFilter);
            foreach (var group in _grouped)
            {
                OpenSearchXunitRunner.CurrentCluster = @group.Key;
                if (@group.Key == null)
                {
                    var testCount = @group.SelectMany(q => q.TestCases).Count();
                    Console.WriteLine($" -> Several tests skipped because they have no cluster associated");
                    Summaries.Add(new RunSummary { Total = testCount, Skipped = testCount });
                    continue;
                }

                var type = @group.Key.GetType();
                var clusterName = type.Name.Replace("Cluster", string.Empty) ?? "UNKNOWN";
                if (!MatchesClusterFilter(clusterName)) continue;

                var dop = @group.Key.ClusterConfiguration?.MaxConcurrency ?? defaultMaxConcurrency;
                dop = dop <= 0 ? defaultMaxConcurrency : dop;

                var timeout = @group.Key.ClusterConfiguration?.Timeout ?? TimeSpan.FromMinutes(2);

                var skipReasons = @group.SelectMany(g => g.TestCases.Select(t => t.SkipReason)).ToList();
                var allSkipped = skipReasons.All(r => !string.IsNullOrWhiteSpace(r));
                if (allSkipped)
                {
                    Console.WriteLine($" -> All tests from {clusterName} are skipped under the current configuration");
                    Summaries.Add(new RunSummary { Total = skipReasons.Count, Skipped = skipReasons.Count });
                    continue;
                }

                ClusterTotals.Add(clusterName, Stopwatch.StartNew());

                bool ValidateRunningVersion()
                {
                    try
                    {
                        var t = new ValidateRunningVersion();
                        t.Run(@group.Key);
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                using (@group.Key)
                {
                    if (!IntegrationTestsMayUseAlreadyRunningNode || !ValidateRunningVersion())
                        @group.Key?.Start(timeout);

                    await @group.ForEachAsync(dop,
                            async g =>
                            {
                                await RunTestCollections(messageBus, ctx, g, testFilters).ConfigureAwait(false);
                            })
                        .ConfigureAwait(false);
                }

                ClusterTotals[clusterName].Stop();
            }

            return new RunSummary
            {
                Total = Summaries.Sum(s => s.Total),
                Failed = Summaries.Sum(s => s.Failed),
                Skipped = Summaries.Sum(s => s.Skipped)
            };
        }

        private async Task RunTestCollections(IMessageBus messageBus, CancellationTokenSource ctx, GroupedByCluster g,
            string[] testFilters)
        {
            var test = g.Collection.DisplayName.Replace("Test collection for", string.Empty).Trim();
            if (!MatchesATestFilter(test, testFilters)) return;
            if (testFilters.Length > 0) Console.WriteLine(" -> " + test);

            try
            {
                var summary = await RunTestCollectionAsync(messageBus, g.Collection, g.TestCases, ctx)
                    .ConfigureAwait(false);
                var type = g.Cluster?.GetType();
                var clusterName = type?.Name.Replace("Cluster", "") ?? "UNKNOWN";
                if (summary.Failed > 0)
                    FailedCollections.Add(Tuple.Create(clusterName, test));
                Summaries.Add(summary);
            }
            catch (TaskCanceledException)
            {
                // TODO: What should happen here?
            }
        }

        private static string[] CreateTestFilters(string testFilters) =>
            testFilters?.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray()
            ?? new string[] { };

        private static bool MatchesATestFilter(string test, IReadOnlyCollection<string> testFilters)
        {
            if (testFilters.Count == 0 || string.IsNullOrWhiteSpace(test)) return true;
            return testFilters
                .Any(filter => test.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool MatchesClusterFilter(string cluster)
        {
            if (string.IsNullOrWhiteSpace(cluster) || string.IsNullOrWhiteSpace(ClusterFilter)) return true;
            return ClusterFilter
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Any(c => cluster.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private IEphemeralCluster<XunitClusterConfiguration> ClusterFixture(ITestClass testMethodTestClass)
        {
            var clusterType = GetClusterForClass(testMethodTestClass.Class);
            if (clusterType == null) return null;

            if (_assemblyFixtureMappings.TryGetValue(clusterType, out var cluster)) return cluster;
            Aggregator.Run(() =>
            {
                var o = Activator.CreateInstance(clusterType);
                cluster = o as IEphemeralCluster<XunitClusterConfiguration>;
            });
            _assemblyFixtureMappings.Add(clusterType, cluster);
            return cluster;
        }

        public static bool IsAnIntegrationTestClusterType(Type type) =>
            typeof(XunitClusterBase).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())
            || IsSubclassOfRawGeneric(typeof(XunitClusterBase<>), type);

        public static Type GetClusterForClass(ITypeInfo testClass) =>
            GetClusterFromClassClusterFixture(testClass) ?? GetClusterFromIntegrationAttribute(testClass);

        private static Type GetClusterFromClassClusterFixture(ITypeInfo testClass) => (
            from i in testClass.Interfaces
            where i.IsGenericType
            from a in i.GetGenericArguments()
            select a.ToRuntimeType()
        ).FirstOrDefault(IsAnIntegrationTestClusterType);

        private static Type GetClusterFromIntegrationAttribute(ITypeInfo testClass) =>
            testClass.GetCustomAttributes(typeof(IntegrationTestClusterAttribute))
                .FirstOrDefault()?.GetNamedArgument<Type>(nameof(IntegrationTestClusterAttribute.ClusterType));

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) return true;

                toCheck = toCheck.GetTypeInfo().BaseType;
            }

            return false;
        }

        private class GroupedByCluster
        {
            public IEphemeralCluster<XunitClusterConfiguration> Cluster { get; set; }
            public ITestCollection Collection { get; set; }
            public List<IXunitTestCase> TestCases { get; set; }
        }
    }
}
