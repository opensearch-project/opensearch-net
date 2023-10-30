/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
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
public class CreatePitApiTests
	: ApiIntegrationTestBase<ReadOnlyCluster, CreatePitResponse, ICreatePitRequest, CreatePitDescriptor, CreatePitRequest>
{
	public CreatePitApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	private string _pitId;

	protected override bool ExpectIsValid => true;

	protected override object ExpectJson => null;

	protected override int ExpectStatusCode => 200;

	protected override Func<CreatePitDescriptor, ICreatePitRequest> Fluent => e => e
		.KeepAlive("1h");

	protected override HttpMethod HttpMethod => HttpMethod.POST;

	protected override CreatePitRequest Initializer => new(OpenSearch.Client.Indices.Index<Project>())
	{
		KeepAlive = "1h"
	};

	protected override bool SupportsDeserialization => false;

	protected override string UrlPath =>
		"/project/_search/point_in_time?keep_alive=1h";

	protected override LazyResponses ClientUsage() => Calls(
		(c, f) => c.CreatePit(OpenSearch.Client.Indices.Index<Project>(), f),
		(c, f) => c.CreatePitAsync(OpenSearch.Client.Indices.Index<Project>(), f),
		(c, r) => c.CreatePit(r),
		(c, r) => c.CreatePitAsync(r)
	);

	protected override CreatePitDescriptor NewDescriptor() => new(CallIsolatedValue);

	protected override void ExpectResponse(CreatePitResponse response)
	{
		_pitId = response.PitId;
		response.ShouldBeValid();
		response.PitId.Should().NotBeNullOrEmpty();
		response.CreationTime.Should().BeCloseTo(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 10000);
		response.Shards.Should().NotBeNull();
	}

	protected override void OnAfterCall(IOpenSearchClient client)
	{
		if (string.IsNullOrEmpty(_pitId)) return;
		client.DeletePit(d => d.PitId(_pitId));
		_pitId = null;
	}
}
