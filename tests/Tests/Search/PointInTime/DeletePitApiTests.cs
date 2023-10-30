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
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.PointInTime;

[SkipVersion("<2.4.0", "Point-In-Time search support was added in version 2.4.0")]
public class DeletePitApiTests
	: ApiIntegrationTestBase<ReadOnlyCluster, DeletePitResponse, IDeletePitRequest, DeletePitDescriptor, DeletePitRequest>
{
	private string _pitId = "default-for-unit-tests";

	public DeletePitApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	protected override bool ExpectIsValid => true;

	protected override object ExpectJson => new
	{
		pit_id = new[]
		{
			_pitId
		}
	};

	protected override int ExpectStatusCode => 200;

	protected override Func<DeletePitDescriptor, IDeletePitRequest> Fluent => d => d.PitId(_pitId);
	protected override HttpMethod HttpMethod => HttpMethod.DELETE;

	protected override DeletePitRequest Initializer => new(_pitId);
	protected override bool SupportsDeserialization => false;
	protected override string UrlPath => "/_search/point_in_time";

	protected override LazyResponses ClientUsage() => Calls(
		(c, f) => c.DeletePit(f),
		(c, f) => c.DeletePitAsync(f),
		(c, r) => c.DeletePit(r),
		(c, r) => c.DeletePitAsync(r)
	);

	protected override DeletePitDescriptor NewDescriptor() => new();

	protected override void ExpectResponse(DeletePitResponse response)
	{
		response.ShouldBeValid();
		response.Pits.Should().NotBeNull().And.HaveCount(1);

		var pit = response.Pits.First();
		pit.Successful.Should().BeTrue();
		pit.PitId.Should().Be(_pitId);
	}

	protected override void OnBeforeCall(IOpenSearchClient client)
	{
		var pit = client.CreatePit(OpenSearch.Client.Indices.Index<Project>(), c => c.KeepAlive("1h"));
		if (!pit.IsValid)
			throw new Exception("Setup: Initial PIT failed.");

		_pitId = pit.PitId;
	}
}
