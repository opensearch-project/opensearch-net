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
using System.Net.Http;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using static OpenSearch.Client.Infer;

namespace Tests.ClientConcepts.Connection;

public class ConnectionReuseAndBalancing : ClusterTestClassBase<ConnectionReuseCluster>
{
    public ConnectionReuseAndBalancing(ConnectionReuseCluster cluster) : base(cluster) { }

    private static bool IsCurlHandler { get; } = typeof(HttpClientHandler).Assembly.GetType("System.Net.Http.CurlHandler") != null;

    public IEnumerable<Project> MockDataGenerator(int numDocuments)
    {
        foreach (var i in Enumerable.Range(0, numDocuments))
            yield return new Project { Name = $"project-{i}" };
    }

    [I]
    public async Task IndexAndSearchABunch()
    {
        const int requestsPerIteration = 1000;
        var client = Client;

        await IndexMockData(client, requestsPerIteration);

        var statsRequest = new NodesStatsRequest(NodesStatsMetric.Http);
        for (var i = 0; i < 20; i++)
        {
            var tasks = Enumerable.Range(0, requestsPerIteration)
                .Select(async (r) => await client.SearchAsync<Project>())
                .Cast<Task>()
                .ToArray();
            Task.WaitAll(tasks);

            var nodeStats = await client.Nodes.StatsAsync(statsRequest);
            AssertHttpStats(client, nodeStats, i, requestsPerIteration);
        }
    }

    private static void AssertHttpStats(IOpenSearchClient c, NodesStatsResponse r, int i, int requestsPerIteration)
    {
        const int leeWay = 10;
        var connectionLimit = c.ConnectionSettings.ConnectionLimit;
        var maxCurrent = connectionLimit;
        var maxCurrentOpen = connectionLimit + 1; //cluster bootstrap opens it own connections

        foreach (var node in r.Nodes.Values) //in our cluster we only have 1 node
        {
            node.Http.TotalOpened.Should().BeGreaterThan(2, "We want to see some concurrency");
            var h = node.Http;
            node.Http.CurrentOpen.Should().BeLessOrEqualTo(maxCurrentOpen, $"CurrentOpen exceed our connection limit {maxCurrent}");

            string errorMessage;
            int iterationMax;

            if (!IsCurlHandler)
            {
                //on non curl connections we expect full connection reuse
                //we allow some leeway on the maxOpened because of connections setup and teared down
                //during the initial bootstrap procudure from the test framework getting the cluster up.
                iterationMax = maxCurrent + leeWay;
                errorMessage = $"Total openend exceeded {maxCurrent} + leighway factor {leeWay}";
            }
            else
            {
                var m = Math.Max(2, i + 1) + 1;
                iterationMax = maxCurrent * m / 2 + leeWay;
                errorMessage =
                    $"Expected some socket bleeding but iteration {i} exceeded iteration specific max {iterationMax} = (({maxCurrent} * {m}) / 2) + {leeWay}";
            }
            node.Http.TotalOpened.Should().BeLessOrEqualTo(iterationMax, errorMessage);
            if (i == -1) return;

            Console.WriteLine(
                $"Current Open: {h.CurrentOpen}, Total Opened: {h.TotalOpened}, Iteration Max = {iterationMax}, Iteration: {i}, Total Searches {(i + 1) * requestsPerIteration}");
        }
    }

    private async Task IndexMockData(IOpenSearchClient c, int requestsPerIteration)
    {
        var tokenSource = new CancellationTokenSource();
        await c.Indices.DeleteAsync(Index<Project>(), ct: tokenSource.Token);
        var observableBulk = c.BulkAll(MockDataGenerator(100000), f => f
                .MaxDegreeOfParallelism(10)
                .BackOffTime(TimeSpan.FromSeconds(10))
                .BackOffRetries(2)
                .Size(1000)
                .RefreshOnCompleted()
            , tokenSource.Token);
        await observableBulk.ForEachAsync(x => { }, tokenSource.Token);
        var statsRequest = new NodesStatsRequest(NodesStatsMetric.Http);
        var nodeStats = await c.Nodes.StatsAsync(statsRequest, tokenSource.Token);
        AssertHttpStats(c, nodeStats, -1, requestsPerIteration);
    }
}
