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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Stack.ArtifactsApi;

namespace OpenSearch.OpenSearch.Xunit;

/// <summary>
///     The Xunit test runner options
/// </summary>
public class OpenSearchXunitRunOptions
{
    /// <summary>
    ///     Informs the runner whether we expect to run integration tests. Defaults to <c>true</c>
    /// </summary>
    public bool RunIntegrationTests { get; set; } = true;

    /// <summary>
    ///     Setting this to true will assume the cluster that is currently running was started for the purpose of these tests
    ///     Defaults to <c>false</c>
    /// </summary>
    public bool IntegrationTestsMayUseAlreadyRunningNode { get; set; } = false;

    /// <summary>
    ///     Informs the runner whether unit tests will be run. Defaults to <c>false</c>.
    ///     If set to <c>true</c> and <see cref="RunIntegrationTests" /> is <c>false</c>, the runner will run all the
    ///     tests in parallel with the maximum degree of parallelism
    /// </summary>
    public bool RunUnitTests { get; set; }

    /// <summary>
    ///     A global test filter that can be used to only run certain tests.
    ///     Accepts a comma separated list of filters
    /// </summary>
    public string TestFilter { get; set; }

    /// <summary>
    ///     A global cluster filter that can be used to only run certain cluster's tests.
    ///     Accepts a comma separated list of filters
    /// </summary>
    public string ClusterFilter { get; set; }

    /// <summary>
    ///     Informs the runner what version of OpenSearch is under test. Required for
    ///     <see cref="SkipVersionAttribute" /> to kick in
    /// </summary>
    public OpenSearchVersion Version { get; set; }

    /// <summary>
    ///     Called when the tests have finished running successfully
    /// </summary>
    /// <param name="runnerClusterTotals">Per cluster timings of the total test time, including starting OpenSearch</param>
    /// <param name="runnerFailedCollections">All collection of failed cluster, failed tests tuples</param>
    public virtual void OnTestsFinished(Dictionary<string, Stopwatch> runnerClusterTotals,
        ConcurrentBag<Tuple<string, string>> runnerFailedCollections)
    {
    }

    /// <summary>
    ///     Called before tests run. An ideal place to perform actions such as writing information to
    ///     <see cref="Console" />.
    /// </summary>
    public virtual void OnBeforeTestsRun()
    {
    }
}
