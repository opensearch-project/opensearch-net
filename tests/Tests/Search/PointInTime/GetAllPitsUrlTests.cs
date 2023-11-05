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

public class GetAllPitsUrlTests
{
	[U] public async Task Urls() =>
		await GET("/_search/point_in_time/_all")
			.Fluent(c => c.GetAllPits())
			.Request(c => c.GetAllPits(new GetAllPitsRequest()))
			.FluentAsync(c => c.GetAllPitsAsync())
			.RequestAsync(c => c.GetAllPitsAsync(new GetAllPitsRequest()));
}
