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
public class DeleteAllPitsApiTests
	: ApiIntegrationTestBase<WritableCluster, DeleteAllPitsResponse, IDeleteAllPitsRequest, DeleteAllPitsDescriptor, DeleteAllPitsRequest>
{
	private readonly List<string> _pitIds = new();

	public DeleteAllPitsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	protected override bool ExpectIsValid => true;

	protected override object ExpectJson => null;

	protected override int ExpectStatusCode => 200;

	protected override Func<DeleteAllPitsDescriptor, IDeleteAllPitsRequest> Fluent => d => d;
	protected override HttpMethod HttpMethod => HttpMethod.DELETE;

	protected override DeleteAllPitsRequest Initializer => new();
	protected override bool SupportsDeserialization => false;
	protected override string UrlPath => "/_search/point_in_time/_all";

	protected override LazyResponses ClientUsage() => Calls(
		(c, f) => c.DeleteAllPits(f),
		(c, f) => c.DeleteAllPitsAsync(f),
		(c, r) => c.DeleteAllPits(r),
		(c, r) => c.DeleteAllPitsAsync(r)
	);

	protected override DeleteAllPitsDescriptor NewDescriptor() => new();

	protected override void ExpectResponse(DeleteAllPitsResponse response)
	{
		response.ShouldBeValid();
		response.Pits.Should()
			.NotBeNull()
			.And.HaveCount(5)
			.And.BeEquivalentTo(_pitIds.Select(p => new DeletedPit
			{
				PitId = p,
				Successful = true
			}));
	}

	protected override void OnBeforeCall(IOpenSearchClient client)
	{
		for (var i = 0; i < 5; i++)
		{
			var pit = Client.CreatePit(OpenSearch.Client.Indices.Index<Project>(), c => c.KeepAlive("1h"));
			if (!pit.IsValid)
				throw new Exception("Setup: Initial PIT failed.");

			_pitIds.Add(pit.PitId);
		}
	}
}
