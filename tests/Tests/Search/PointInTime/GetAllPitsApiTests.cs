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
using Xunit;

namespace Tests.Search.PointInTime;

[Collection("PitApiTests")]
[SkipVersion("<2.4.0", "Point-In-Time search support was added in version 2.4.0")]
public sealed class GetAllPitsApiTests
    : ApiIntegrationTestBase<WritableCluster, GetAllPitsResponse, IGetAllPitsRequest, GetAllPitsDescriptor, GetAllPitsRequest>
{
    public GetAllPitsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    private List<(string id, long creationTime)> Pits
    {
        get => ExtendedValue<List<(string, long)>>(nameof(Pits));
        set => ExtendedValue(nameof(Pits), value);
    }

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
            .And.BeEquivalentTo(Pits.Select(p => new PitDetail
            {
                PitId = p.id,
                CreationTime = p.creationTime,
                KeepAlive = 60 * 60 * 1000
            }));
    }

    protected override void OnBeforeCall(IOpenSearchClient client)
    {
        Pits = new List<(string, long)>();
        for (var i = 0; i < 5; i++)
        {
            var pit = client.CreatePit(OpenSearch.Client.Indices.Index<Project>(), c => c.KeepAlive("1h"));
            if (!pit.IsValid)
                throw new Exception("Setup: Initial PIT failed.");

            Pits.Add((pit.PitId, pit.CreationTime));
        }
    }

    protected override void OnAfterCall(IOpenSearchClient client) => client.DeletePit(d => d.PitId(Pits.Select(p => p.id)));
}
