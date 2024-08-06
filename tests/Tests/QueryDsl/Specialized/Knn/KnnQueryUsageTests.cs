/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.Specialized.Knn
{
    public class KnnQueryUsageTests : QueryDslUsageTestsBase
    {
        public KnnQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

        protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IKnnQuery>(a => a.Knn)
        {
            q =>
            {
                q.Field = null;
                q.Vector = new[] { 1.5f, -2.6f };
                q.K = 30;
            },
            q =>
            {
                q.Field = "knn_vector";
                q.Vector = null;
                q.K = 30;
            },
            q =>
            {
                q.Field = "knn_vector";
                q.Vector = Array.Empty<float>();
                q.K = 30;
            },
            q =>
            {
                q.Field = "knn_vector";
                q.Vector = new[] { 1.5f, 2.6f };
                q.K = null;
            },
            q =>
            {
                q.Field = "knn_vector";
                q.Vector = new[] { 1.5f, 2.6f };
                q.K = 0;
            }
        };

        protected override QueryContainer QueryInitializer => new KnnQuery
        {
            Boost = 1.1,
            Field = Infer.Field<Project>(f => f.Vector),
            Vector = new[] { 1.5f, -2.6f },
            K = 30
        };

        protected override object QueryJson =>
            new { knn = new { vector = new { boost = 1.1, vector = new[] { 1.5f, -2.6f }, k = 30 } } };

        protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
            .Knn(knn => knn
                .Boost(1.1)
                .Field(f => f.Vector)
                .Vector(1.5f, -2.6f)
                .K(30)
            );
    }

    public class KnnIntegrationTests : IClusterFixture<WritableCluster>
    {
        private readonly WritableCluster _cluster;

        public KnnIntegrationTests(WritableCluster cluster) => _cluster = cluster;

        [I]
        public async Task KnnQuery()
        {
            var client = _cluster.Client;
            const string index = "knn-index";

            var createIndexResponse = await client.Indices.CreateAsync(index, c => c
                .Settings(s => s
                    .Setting("index.knn", true)
                    .Setting("index.knn.algo_param.ef_search", 100))
                .Map<Doc>(m => m
                    .Properties(p => p
                        .KnnVector(k => k
                            .Name(d => d.Vector)
                            .Dimension(4)
                            .Method(m => m
                                .Name("hnsw")
                                .SpaceType("innerproduct")
                                .Engine("nmslib")
                                .Parameters(p => p
                                    .Parameter("ef_construction", 256)
                                    .Parameter("m", 48)
                                )
                            )
                        )
                    )
                )
            );

            createIndexResponse.ShouldBeValid();

            var bulkResponse = await client.BulkAsync(b => b
                .Index(index)
                .IndexMany(new object[]
                {
                    new Doc(new[] { 1.5f, 5.5f, 4.5f, 6.4f }, 10.3f),
                    new Doc(new[] { 2.5f, 3.5f, 5.6f, 6.7f }, 5.5f),
                    new Doc(new[] { 4.5f, 5.5f, 6.7f, 3.7f }, 4.4f),
                    new Doc(new[] { 1.5f, 5.5f, 4.5f, 6.4f }, 8.9f)
                }));

            bulkResponse.ShouldBeValid();

            var refreshResponse = await client.Indices.RefreshAsync(index);
            refreshResponse.ShouldBeValid();

            var searchResponse = await client.SearchAsync<Doc>(s => s
                .Index(index)
                .Size(2)
                .Query(q => q
                    .Knn(k => k
                        .Field(d => d.Vector)
                        .Vector(2.0f, 3.0f, 5.0f, 6.0f)
                        .K(2)
                    )
                )
            );

            searchResponse.ShouldBeValid();
            searchResponse
                .Documents
                .Should()
                .BeEquivalentTo(new[]
                {
                    new Doc(new[] { 2.5f, 3.5f, 5.6f, 6.7f }, 5.5f),
                    new Doc(new[] { 4.5f, 5.5f, 6.7f, 3.7f }, 4.4f),
                });
        }

        public class Doc
        {
            public Doc(float[] vector, float price)
            {
                Vector = vector;
                Price = price;
            }

            public float Price { get; set; }
            public float[] Vector { get; set; }
        }
    }
}
