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

namespace Tests.Cat.CatSegmentReplication;

[SkipVersion("<2.7.0", "/_cat/segment_replication was added in version 2.7.0")]
public class CatSegmentReplicationApiTests
    : ApiIntegrationTestBase<ReplicatedReadOnlyCluster, CatResponse<CatSegmentReplicationRecord>, ICatSegmentReplicationRequest, CatSegmentReplicationDescriptor, CatSegmentReplicationRequest>
{
    public CatSegmentReplicationApiTests(ReplicatedReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    private static readonly string IndexName = nameof(CatSegmentReplicationApiTests).ToLower();

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;
    protected override string UrlPath => $"/_cat/segment_replication/{IndexName}";

    protected override Func<CatSegmentReplicationDescriptor, ICatSegmentReplicationRequest> Fluent => d => d.Index(IndexName);
    protected override CatSegmentReplicationRequest Initializer => new(IndexName);

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cat.SegmentReplication(f),
        (client, f) => client.Cat.SegmentReplicationAsync(f),
        (client, r) => client.Cat.SegmentReplication(r),
        (client, r) => client.Cat.SegmentReplicationAsync(r)
    );

    protected override void ExpectResponse(CatResponse<CatSegmentReplicationRecord> response) =>
        response.Records.Should().NotBeEmpty().And.AllSatisfy(r => r.ShardId.Should().StartWith($"[{IndexName}]"));

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        var resp = client.Indices.Create(IndexName, d => d
            .Settings(s => s
                .Setting("index.replication.type", "SEGMENT")));
        resp.ShouldBeValid();

        var bulkResp = client.Bulk(b => b
            .Index(IndexName)
            .IndexMany(Enumerable.Range(0, 10).Select(i => new Doc { Id = i }))
            .Refresh(Refresh.WaitFor));
        bulkResp.ShouldBeValid();
    }

    protected override void IntegrationTeardown(IOpenSearchClient client, CallUniqueValues values) => client.Indices.Delete(IndexName);

    public class Doc
    {
        public long Id { get; set; }
    }
}
