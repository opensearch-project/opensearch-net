/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Cat.CatSegmentReplication;

public class CatSegmentReplicationUrlTests : UrlTestsBase
{
    [U]
    public override async Task Urls()
    {
        await GET("/_cat/segment_replication")
                .Fluent(c => c.Cat.SegmentReplication())
                .Request(c => c.Cat.SegmentReplication(new CatSegmentReplicationRequest()))
                .FluentAsync(c => c.Cat.SegmentReplicationAsync())
                .RequestAsync(c => c.Cat.SegmentReplicationAsync(new CatSegmentReplicationRequest()))
            ;

        await GET("/_cat/segment_replication/my-index")
            .Fluent(c => c.Cat.SegmentReplication(d => d.Index("my-index")))
            .Request(c => c.Cat.SegmentReplication(new CatSegmentReplicationRequest("my-index")))
            .FluentAsync(c => c.Cat.SegmentReplicationAsync(d => d.Index("my-index")))
            .RequestAsync(c => c.Cat.SegmentReplicationAsync(new CatSegmentReplicationRequest("my-index")));
    }
}
