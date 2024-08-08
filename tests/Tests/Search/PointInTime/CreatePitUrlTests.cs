/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Search.PointInTime;

public class CreatePitUrlTests
{
    [U]
    public async Task Urls()
    {
        const string index = "temp";
        const string keepAlive = "1h";

        await POST($"/{index}/_search/point_in_time?keep_alive={keepAlive}")
                .Fluent(c => c.CreatePit(index, c => c.KeepAlive(keepAlive)))
                .Request(c => c.CreatePit(new CreatePitRequest(index) { KeepAlive = keepAlive }))
                .FluentAsync(c => c.CreatePitAsync(index, c => c.KeepAlive(keepAlive)))
                .RequestAsync(c => c.CreatePitAsync(new CreatePitRequest(index) { KeepAlive = keepAlive }))
            ;
    }
}
