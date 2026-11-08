/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Indices.Stats;

public class IndexStatsUrlsTests
{
    [U] public async Task Urls()
    {
        // "_stats", "{index}/_stats", "{index}/_stats/{metric}", "_stats/{metric}"
        const string index = "test_index_1";
        const IndicesStatsMetric metrics = IndicesStatsMetric.Docs | IndicesStatsMetric.Segments;

        await GET("/_stats")
                .Fluent(c => c.Indices.Stats())
                .Request(c => c.Indices.Stats(new IndicesStatsRequest()))
                .FluentAsync(c => c.Indices.StatsAsync())
                .RequestAsync(c => c.Indices.StatsAsync(new IndicesStatsRequest()));

        await GET("/_stats/docs%2Csegments")
            .Fluent(c => c.Indices.Stats(null, d => d.Metric(metrics)))
            .Request(c => c.Indices.Stats(new IndicesStatsRequest(metrics)))
            .FluentAsync(c => c.Indices.StatsAsync(null, d => d.Metric(metrics)))
            .RequestAsync(c => c.Indices.StatsAsync(new IndicesStatsRequest(metrics)));

        await GET($"/{index}/_stats")
            .Fluent(c => c.Indices.Stats(index, d => d))
            .Request(c => c.Indices.Stats(new IndicesStatsRequest(index)))
            .FluentAsync(c => c.Indices.StatsAsync(index, d => d))
            .RequestAsync(c => c.Indices.StatsAsync(new IndicesStatsRequest(index)));

        await GET($"/{index}/_stats/docs%2Csegments")
            .Fluent(c => c.Indices.Stats(index, d => d.Metric(metrics)))
            .Request(c => c.Indices.Stats(new IndicesStatsRequest(index, metrics)))
            .FluentAsync(c => c.Indices.StatsAsync(index, d => d.Metric(metrics)))
            .RequestAsync(c => c.Indices.StatsAsync(new IndicesStatsRequest(index, metrics)));
    }
}
