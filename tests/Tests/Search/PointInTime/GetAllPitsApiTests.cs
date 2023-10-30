/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.PointInTime;

[SkipVersion("<2.4.0", "Point-In-Time search support was added in version 2.4.0")]
public class GetAllPitsApiTests
	: ApiIntegrationTestBase<ReadOnlyCluster, GetAllPitsResponse, IGetAllPitsRequest, GetAllPitsDescriptor, GetAllPitsRequest>
{
	private readonly List<CreatePitResponse> _pits = new();

	public GetAllPitsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	protected override bool ExpectIsValid => true;

	protected override object ExpectJson => null;

	protected override int ExpectStatusCode => 200;

	protected override Func<GetAllPitsDescriptor, IGetAllPitsRequest> Fluent => d => d;
	protected override HttpMethod HttpMethod => HttpMethod.GET;

	protected override GetAllPitsRequest Initializer => new();
	protected override bool SupportsDeserialization => false;
	protected override string UrlPath => "/_search/point_in_time/_all";

	protected override LazyResponses ClientUsage() => Calls(
		(c, f) => c.GetAllPits(f),
		(c, f) => c.GetAllPitsAsync(f),
		(c, r) => c.GetAllPits(r),
		(c, r) => c.GetAllPitsAsync(r)
	);

	protected override GetAllPitsDescriptor NewDescriptor() => new();

	protected override void ExpectResponse(GetAllPitsResponse response)
	{
		response.ShouldBeValid();
		response.Pits.Should()
			.NotBeNull()
			.And.HaveCount(5)
			.And.BeEquivalentTo(_pits.Select(p => new PitDetail
			{
				PitId = p.PitId,
				CreationTime = p.CreationTime,
				KeepAlive = 60 * 60 * 1000
			}));
	}

	protected override void OnBeforeCall(IOpenSearchClient client)
	{
		_pits.Clear();

		for (var i = 0; i < 5; i++)
		{
			var pit = Client.CreatePit(OpenSearch.Client.Indices.Index<Project>(), c => c.KeepAlive("1h"));
			if (!pit.IsValid)
				throw new Exception("Setup: Initial PIT failed.");

			_pits.Add(pit);
		}
	}

	protected override void OnAfterCall(IOpenSearchClient client) =>
		client.DeletePit(d => d.PitId(_pits.Select(p => p.PitId)));
}
