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
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.Stats;

public class IndicesStatsApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, IndicesStatsResponse, IIndicesStatsRequest, IndicesStatsDescriptor, IndicesStatsRequest>
{
    private static readonly IndexName ProjectIndex = Infer.Index<Project>();

    public IndicesStatsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override IndicesStatsRequest Initializer => new (ProjectIndex, IndicesStatsMetric.Docs | IndicesStatsMetric.Segments);

    protected override Func<IndicesStatsDescriptor, IIndicesStatsRequest> Fluent =>
        d => d.Metric(IndicesStatsMetric.Docs | IndicesStatsMetric.Segments);

    protected override string UrlPath => "/project/_stats/docs%2Csegments";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.Stats(typeof(Project), f),
        (client, f) => client.Indices.StatsAsync(typeof(Project), f),
        (client, r) => client.Indices.Stats(r),
        (client, r) => client.Indices.StatsAsync(r)
    );

    protected override void ExpectResponse(IndicesStatsResponse response)
    {
        response.ShouldBeValid();

        response.Indices.Should().NotBeNull();
        response.Indices.Count.Should().BeGreaterThan(0);

        var projectIndex = response.Indices[ProjectIndex];
        projectIndex.Should().NotBeNull();

        var primaries = projectIndex.Primaries;
        primaries.Should().NotBeNull();

        var documents = primaries.Documents;
        documents.Should().NotBeNull();
        documents.Count.Should().BeGreaterThan(0);

        var segments = primaries.Segments;
        segments.Should().NotBeNull();
        segments.MemoryInBytes.Should().BeGreaterThan(0);
    }
}
