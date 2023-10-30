/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.Search.PointInTime;

[SkipVersion("<2.4.0", "Point-In-Time search support was added in version 2.4.0")]
public sealed class PointInTimeIntegrationTests : IClusterFixture<WritableCluster>, IDisposable
{
	private readonly WritableCluster _cluster;

	public PointInTimeIntegrationTests(WritableCluster cluster) => _cluster = cluster;

	public void Dispose() => _cluster.Client.DeleteAllPits();

	[I] public async Task PointInTimeQuery()
	{
		var client = _cluster.Client;
		var index = nameof(PointInTimeQuery).ToLowerInvariant();

		var bulkResp = await client.BulkAsync(b => b
			.Index(index)
			.IndexMany(Enumerable.Range(0, 10).Select(i => new Doc { Id = i }))
			.Refresh(Refresh.WaitFor));
		bulkResp.ShouldBeValid();

		var pitResp = await client.CreatePitAsync(index, p => p.KeepAlive("1h"));
		pitResp.ShouldBeValid();

		bulkResp = await client.BulkAsync(b => b
			.Index(index)
			.IndexMany(Enumerable.Range(10, 10).Select(i => new Doc { Id = i }))
			.Refresh(Refresh.WaitFor));
		bulkResp.ShouldBeValid();

		var liveSearch = await client.SearchAsync<Doc>(s => s
			.Index(index)
			.MatchAll()
			.TrackTotalHits());
		liveSearch.ShouldBeValid();
		liveSearch.Total.Should().Be(20);

		var pitSearch = await client.SearchAsync<Doc>(s => s
			.MatchAll()
			.PointInTime(p => p.PitId(pitResp.PitId))
			.TrackTotalHits());
		pitSearch.ShouldBeValid();
		pitSearch.Total.Should().Be(10);
		pitSearch.Documents.Should().AllSatisfy(d => d.Id.Should().BeLessThan(10));

		var deleteResp = await client.DeletePitAsync(d => d.PitId(pitResp.PitId));
		deleteResp.ShouldBeValid();
		deleteResp.Pits.Should().BeEquivalentTo(new[]
		{
			new DeletedPit
			{
				PitId = pitResp.PitId,
				Successful = true
			}
		});
	}

	[I] public async Task PointInTimeGetAllDeleteAll()
	{
		var client = _cluster.Client;
		var index = nameof(PointInTimeGetAllDeleteAll).ToLowerInvariant();

		var createIndexResp = await client.Indices.CreateAsync(index);
		createIndexResp.ShouldBeValid();

		var pits = new List<(string id, long creationTime)>();
		for (var i = 0; i < 5; ++i)
		{
			var createResp = await client.CreatePitAsync(index, c => c.KeepAlive("1h"));
			createResp.ShouldBeValid();
			pits.Add((createResp.PitId, createResp.CreationTime));
		}

		var getAllResp = await client.GetAllPitsAsync();
		getAllResp.ShouldBeValid();
		getAllResp.Pits.Should()
			.BeEquivalentTo(pits.Select(p => new PitDetail
			{
				PitId = p.id,
				CreationTime = p.creationTime,
				KeepAlive = 60 * 60 * 1000
			}));

		var deleteAllResp = await client.DeleteAllPitsAsync();
		deleteAllResp.ShouldBeValid();
		deleteAllResp.Pits.Should()
			.BeEquivalentTo(pits.Select(p => new DeletedPit
			{
				PitId = p.id,
				Successful = true
			}));
	}

	[I] public async Task PointInTimeSearchExtendKeepAlive()
	{
		var client = _cluster.Client;
		var index = nameof(PointInTimeSearchExtendKeepAlive).ToLowerInvariant();

		var createIndexResp = await client.Indices.CreateAsync(index);
		createIndexResp.ShouldBeValid();

		var createResp = await client.CreatePitAsync(index, c => c.KeepAlive("1h"));
		createResp.ShouldBeValid();

		var searchResp = await client.SearchAsync<Doc>(s => s
			.MatchAll()
			.PointInTime(p => p.PitId(createResp.PitId).KeepAlive("4h")));
		searchResp.ShouldBeValid();

		var getAllResp = await client.GetAllPitsAsync();
		getAllResp.ShouldBeValid();
		var pit = getAllResp.Pits.FirstOrDefault(p => p.PitId == createResp.PitId);

		pit.Should().NotBeNull();
		pit.CreationTime.Should().Be(createResp.CreationTime);
		pit.KeepAlive.Should().Be(4 * 60 * 60 * 1000);
	}

	private class Doc
	{
		public long Id { get; set; }
	}
}
