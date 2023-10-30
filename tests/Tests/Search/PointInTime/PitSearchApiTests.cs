/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.PointInTime;

[SkipVersion("<2.4.0", "Point-In-Time search support was added in version 2.4.0")]
public sealed class PitSearchApiTests :
	ApiIntegrationTestBase<
		WritableCluster,
		ISearchResponse<PitSearchApiTests.Doc>,
		ISearchRequest,
		SearchDescriptor<PitSearchApiTests.Doc>,
		SearchRequest<PitSearchApiTests.Doc>
	>
{
	public PitSearchApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	private string _pitId = "default-for-unit-tests";

	protected override object ExpectJson => new
	{
		query = new
		{
			match_all = new { }
		},
		pit = new
		{
			id = _pitId,
			keep_alive = "1h"
		},
		track_total_hits = true
	};

	protected override HttpMethod HttpMethod => HttpMethod.POST;
	protected override string UrlPath => "/_search";

	protected override Func<SearchDescriptor<Doc>, ISearchRequest> Fluent => s => s
		.Query(q => q.MatchAll())
		.PointInTime(p => p
			.Id(_pitId)
			.KeepAlive("1h"))
		.TrackTotalHits();

	protected override bool ExpectIsValid => true;
	protected override int ExpectStatusCode => 200;

	protected override SearchRequest<Doc> Initializer => new(null)
	{
		Query = new QueryContainer(new MatchAllQuery()),
		PointInTime = new OpenSearch.Client.PointInTime
		{
			Id = _pitId,
			KeepAlive = "1h"
		},
		TrackTotalHits = true
	};

	protected override SearchDescriptor<Doc> NewDescriptor() => new(null);

	protected override void ExpectResponse(ISearchResponse<Doc> response)
	{
		response.ShouldBeValid();
		response.Total.Should().Be(10);
		response
			.Documents
			.Should()
			.NotBeNull()
			.And.HaveCount(10)
			.And.BeEquivalentTo(Enumerable.Range(0, 10).Select(i => new Doc { Id = i }));
	}

	protected override void OnBeforeCall(IOpenSearchClient client)
	{
		var bulkResp = client.Bulk(b => b
			.Index(CallIsolatedValue)
			.IndexMany(Enumerable.Range(0, 10).Select(i => new Doc { Id = i }))
			.Refresh(Refresh.WaitFor));
		bulkResp.ShouldBeValid();

		var pitResp = client.CreatePit(CallIsolatedValue, p => p.KeepAlive("1h"));
		pitResp.ShouldBeValid();
		_pitId = pitResp.PitId;

		bulkResp = client.Bulk(b => b
			.Index(CallIsolatedValue)
			.IndexMany(Enumerable.Range(10, 10).Select(i => new Doc { Id = i }))
			.Refresh(Refresh.WaitFor));
		bulkResp.ShouldBeValid();
	}

	protected override void OnAfterCall(IOpenSearchClient client) => client.DeletePit(d => d.PitId(_pitId));

	protected override LazyResponses ClientUsage() => Calls(
		(c, f) => c.Search(f),
		(c, f) => c.SearchAsync(f),
		(c, r) => c.Search<Doc>(r),
		(c, r) => c.SearchAsync<Doc>(r)
	);

	public class Doc
	{
		public long Id { get; set; }
	}
}
