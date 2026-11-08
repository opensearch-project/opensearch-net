/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Client;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Cat.CatPitSegments;

public class CatPitSegmentsUrlTests : UrlTestsBase
{
	[U] public override async Task Urls()
	{
		await GET("/_cat/pit_segments")
				.Fluent(c => c.Cat.PitSegments())
				.Request(c => c.Cat.PitSegments(new CatPitSegmentsRequest()))
				.FluentAsync(c => c.Cat.PitSegmentsAsync())
				.RequestAsync(c => c.Cat.PitSegmentsAsync(new CatPitSegmentsRequest()))
			;

		await GET("/_cat/pit_segments/_all")
			.Fluent(c => c.Cat.AllPitSegments())
			.Request(c => c.Cat.AllPitSegments(new CatAllPitSegmentsRequest()))
			.FluentAsync(c => c.Cat.AllPitSegmentsAsync())
			.RequestAsync(c => c.Cat.AllPitSegmentsAsync(new CatAllPitSegmentsRequest()));
	}
}
