/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cat.CatPitSegments;

[SkipVersion("<2.4.0", "Point-In-Time search support was added in version 2.4.0")]
public class CatPitSegmentsApiTests
	: ApiIntegrationTestBase<ReadOnlyCluster, CatResponse<CatPitSegmentsRecord>, ICatPitSegmentsRequest, CatPitSegmentsDescriptor, CatPitSegmentsRequest>
{
	public CatPitSegmentsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	private static readonly string IndexName = nameof(CatPitSegmentsApiTests).ToLower();
	private string _pitId = "default-for-unit-tests";

	protected override bool ExpectIsValid => true;
	protected override int ExpectStatusCode => 200;
	protected override HttpMethod HttpMethod => HttpMethod.GET;
	protected override string UrlPath => "/_cat/pit_segments";

	protected override Func<CatPitSegmentsDescriptor, ICatPitSegmentsRequest> Fluent => d => d.PitId(_pitId);
	protected override CatPitSegmentsRequest Initializer => new(_pitId);

	protected override object ExpectJson =>
		new { pit_id = new[] { _pitId } };

	protected override LazyResponses ClientUsage() => Calls(
		(client, f) => client.Cat.PitSegments(f),
		(client, f) => client.Cat.PitSegmentsAsync(f),
		(client, r) => client.Cat.PitSegments(r),
		(client, r) => client.Cat.PitSegmentsAsync(r)
	);

	protected override void ExpectResponse(CatResponse<CatPitSegmentsRecord> response) =>
		response.Records.Should().NotBeEmpty().And.AllSatisfy(r => r.Index.Should().NotBeNullOrEmpty());

	protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
	{
		var bulkResp = client.Bulk(b => b
			.Index(IndexName)
			.IndexMany(Enumerable.Range(0, 10).Select(i => new Doc { Id = i }))
			.Refresh(Refresh.WaitFor));
		bulkResp.ShouldBeValid();

		var pitResp = client.CreatePit(IndexName, p => p.KeepAlive("1h"));
		pitResp.ShouldBeValid();
		_pitId = pitResp.PitId;
	}

	protected override void IntegrationTeardown(IOpenSearchClient client, CallUniqueValues values)
	{
		client.DeletePit(d => d.PitId(_pitId));
		client.Indices.Delete(IndexName);
	}

	public class Doc
	{
		public long Id { get; set; }
	}
}

[SkipVersion("<2.4.0", "Point-In-Time search support was added in version 2.4.0")]
public class CatAllPitSegmentsApiTests
	: ApiIntegrationTestBase<ReadOnlyCluster, CatResponse<CatAllPitSegmentsRecord>, ICatAllPitSegmentsRequest, CatAllPitSegmentsDescriptor, CatAllPitSegmentsRequest>
{
	public CatAllPitSegmentsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	private static readonly string IndexName = nameof(CatAllPitSegmentsApiTests).ToLower();
	private string _pitId;

	protected override bool ExpectIsValid => true;
	protected override int ExpectStatusCode => 200;
	protected override HttpMethod HttpMethod => HttpMethod.GET;
	protected override string UrlPath => "/_cat/pit_segments/_all";

	protected override LazyResponses ClientUsage() => Calls(
		(client, f) => client.Cat.AllPitSegments(f),
		(client, f) => client.Cat.AllPitSegmentsAsync(f),
		(client, r) => client.Cat.AllPitSegments(r),
		(client, r) => client.Cat.AllPitSegmentsAsync(r)
	);

	protected override void ExpectResponse(CatResponse<CatAllPitSegmentsRecord> response) =>
		response.Records.Should().NotBeEmpty().And.AllSatisfy(r => r.Index.Should().NotBeNullOrEmpty());

	protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
	{
		var bulkResp = client.Bulk(b => b
			.Index(IndexName)
			.IndexMany(Enumerable.Range(0, 10).Select(i => new Doc { Id = i }))
			.Refresh(Refresh.WaitFor));
		bulkResp.ShouldBeValid();

		var pitResp = client.CreatePit(IndexName, p => p.KeepAlive("1h"));
		pitResp.ShouldBeValid();
		_pitId = pitResp.PitId;
	}

	protected override void IntegrationTeardown(IOpenSearchClient client, CallUniqueValues values)
	{
		client.DeletePit(d => d.PitId(_pitId));
		client.Indices.Delete(IndexName);
	}

	public class Doc
	{
		public long Id { get; set; }
	}
}
