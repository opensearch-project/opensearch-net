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
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.PointInTime;

public class CreatePitApiTests
	: ApiIntegrationTestBase<WritableCluster, CreatePitResponse, ICreatePitRequest, CreatePitDescriptor, CreatePitRequest>
{
	public CreatePitApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	protected override bool ExpectIsValid => true;

	protected override object ExpectJson => null;

	protected override int ExpectStatusCode => 200;

	protected override Func<CreatePitDescriptor, ICreatePitRequest> Fluent => e => e
		.KeepAlive("1h");

	protected override HttpMethod HttpMethod => HttpMethod.POST;

	protected override CreatePitRequest Initializer => new(CallIsolatedValue)
	{
		KeepAlive = "1h"
	};

	protected override bool SupportsDeserialization => false;

	protected override string UrlPath =>
		$"/{CallIsolatedValue}/_search/point_in_time?keep_alive=1h";

	protected override LazyResponses ClientUsage() => Calls(
		(c, f) => c.CreatePit(CallIsolatedValue, f),
		(c, f) => c.CreatePitAsync(CallIsolatedValue, f),
		(c, r) => c.CreatePit(r),
		(c, r) => c.CreatePitAsync(r)
	);

	protected override CreatePitDescriptor NewDescriptor() => new(CallIsolatedValue);

	protected override void ExpectResponse(CreatePitResponse response)
	{
		response.IsValid.Should().BeTrue();
		response.PitId.Should().NotBeNullOrEmpty();
		response.CreationTime.Should().BeCloseTo(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 10000);
		response.Shards.Should().NotBeNull();
	}
}
