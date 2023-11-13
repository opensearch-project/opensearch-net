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

public class DeletePitUrlTests
{
	[U] public async Task Urls()
	{
		var pitIds = new[] { "pitid1", "pitid2" };

		await DELETE("/_search/point_in_time")
			.Fluent(c => c.DeletePit(d => d.PitId(pitIds)))
			.Request(c => c.DeletePit(new DeletePitRequest(pitIds)))
			.FluentAsync(c => c.DeletePitAsync(d => d.PitId(pitIds)))
			.RequestAsync(c => c.DeletePitAsync(new DeletePitRequest(pitIds)));
	}
}
