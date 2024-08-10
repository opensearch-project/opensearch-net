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
using OpenSearch.OpenSearch.Managed;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenSearch.OpenSearch.Xunit.Sdk;

internal class TestFrameworkExecutor : XunitTestFrameworkExecutor
{
    public TestFrameworkExecutor(AssemblyName a, ISourceInformationProvider sip, IMessageSink d) : base(a, sip, d)
    {
    }

    public OpenSearchXunitRunOptions Options { get; set; }

    public override void RunAll(IMessageSink executionMessageSink, ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestFrameworkExecutionOptions executionOptions)
    {
        discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.Version), Options.Version);
        discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.RunIntegrationTests), Options.RunIntegrationTests);
        discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.IntegrationTestsMayUseAlreadyRunningNode),
            Options.IntegrationTestsMayUseAlreadyRunningNode);
        discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.RunUnitTests), Options.RunUnitTests);
        discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.TestFilter), Options.TestFilter);
        discoveryOptions.SetValue(nameof(OpenSearchXunitRunOptions.ClusterFilter), Options.ClusterFilter);

        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.Version), Options.Version);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.RunIntegrationTests), Options.RunIntegrationTests);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.IntegrationTestsMayUseAlreadyRunningNode),
            Options.IntegrationTestsMayUseAlreadyRunningNode);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.RunUnitTests), Options.RunUnitTests);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.TestFilter), Options.TestFilter);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.ClusterFilter), Options.ClusterFilter);

        base.RunAll(executionMessageSink, discoveryOptions, executionOptions);
    }


    public override void RunTests(IEnumerable<ITestCase> testCases, IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.Version), Options.Version);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.RunIntegrationTests), Options.RunIntegrationTests);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.IntegrationTestsMayUseAlreadyRunningNode),
            Options.IntegrationTestsMayUseAlreadyRunningNode);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.RunUnitTests), Options.RunUnitTests);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.TestFilter), Options.TestFilter);
        executionOptions.SetValue(nameof(OpenSearchXunitRunOptions.ClusterFilter), Options.ClusterFilter);
        base.RunTests(testCases, executionMessageSink, executionOptions);
    }

    protected override async void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink sink,
        ITestFrameworkExecutionOptions options)
    {
        options.SetValue(nameof(OpenSearchXunitRunOptions.Version), Options.Version);
        options.SetValue(nameof(OpenSearchXunitRunOptions.RunIntegrationTests), Options.RunIntegrationTests);
        options.SetValue(nameof(OpenSearchXunitRunOptions.IntegrationTestsMayUseAlreadyRunningNode),
            Options.IntegrationTestsMayUseAlreadyRunningNode);
        options.SetValue(nameof(OpenSearchXunitRunOptions.RunUnitTests), Options.RunUnitTests);
        options.SetValue(nameof(OpenSearchXunitRunOptions.TestFilter), Options.TestFilter);
        options.SetValue(nameof(OpenSearchXunitRunOptions.ClusterFilter), Options.ClusterFilter);
        try
        {
            using (var runner =
                new TestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, sink, options))
            {
                Options.OnBeforeTestsRun();
                await runner.RunAsync().ConfigureAwait(false);
                Options.OnTestsFinished(runner.ClusterTotals, runner.FailedCollections);
            }
        }
        catch (Exception e)
        {
            if (e is OpenSearchCleanExitException || e is AggregateException ae &&
                ae.Flatten().InnerException is OpenSearchCleanExitException)
                sink.OnMessage(new TestAssemblyCleanupFailure(Enumerable.Empty<ITestCase>(), TestAssembly,
                    new OpenSearchCleanExitException("Node failed to start", e)));
            else
                sink.OnMessage(new TestAssemblyCleanupFailure(Enumerable.Empty<ITestCase>(), TestAssembly, e));
            throw;
        }
    }
}
