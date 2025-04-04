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
using Tests.Framework.EndpointTests.TestState;
using Tests.QueryDsl.Specialized.Neural;

namespace Tests.QueryDsl.Specialized.Hybrid;

[SkipVersion("<2.9.0", "Search pipelines were stabilized in 2.9.0")]
public class HybridQueryUsageTests
    : QueryDslUsageTestsBase<NeuralQueryCluster, NeuralSearchDoc>
{
    private static readonly string TestName = nameof(HybridQueryUsageTests).ToLowerInvariant();

    public string ModelId => Cluster?.ModelId ?? "default-for-unit-tests";

    public HybridQueryUsageTests(NeuralQueryCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override IndexName IndexName => TestName;
    protected override string ExpectedIndexString => TestName;

    protected override string SearchPipeline => TestName;

    protected override QueryContainer QueryInitializer => new HybridQuery
    {
        Queries =
        [
            new MatchQuery { Field = Infer.Field<NeuralSearchDoc>(d => d.Text), Query = "cowboy rodeo bronco" },
            new NeuralQuery { Field = Infer.Field<NeuralSearchDoc>(d => d.PassageEmbedding), QueryText = "wild west", K = 5, ModelId = ModelId }
        ]
    };

    protected override object QueryJson => new
    {
        hybrid = new
        {
            queries = new object[]
            {
                new { match = new { text = new { query = "cowboy rodeo bronco" } } },
                new { neural = new { passage_embedding = new { query_text = "wild west", k = 5, model_id = ModelId } } }
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<NeuralSearchDoc> q) => q
        .Hybrid(h => h
            .Queries(qq => qq
                    .Match(m => m
                        .Field(f => f.Text)
                        .Query("cowboy rodeo bronco")),
                qq => qq
                    .Neural(n => n
                        .Field(f => f.PassageEmbedding)
                        .QueryText("wild west")
                        .K(5)
                        .ModelId(ModelId))));

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IHybridQuery>(a => a.Hybrid)
    {
        q =>
        {
            q.Queries = null;
        },
        q =>
        {
            q.Queries = Array.Empty<QueryContainer>();
        },
        q =>
        {
            q.Queries =
            [
                new BoolQuery()
            ];
        }
    };

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        var putIngestPipelineResp = client.Ingest.PutPipeline(TestName, p => p
            .Processors(pp => pp
                .TextEmbedding<NeuralSearchDoc>(te => te
                    .ModelId(ModelId)
                    .FieldMap(fm => fm
                        .Map(d => d.Text, d => d.PassageEmbedding)))));
        putIngestPipelineResp.ShouldBeValid();

        var createIndexResp = client.Indices.Create(
            IndexName,
            i => i
                .Settings(s => s
                    .Setting("index.knn", true)
                    .DefaultPipeline(TestName))
                .Map<NeuralSearchDoc>(m => m
                    .Properties(p => p
                        .Text(t => t.Name(d => d.Id))
                        .Text(t => t.Name(d => d.Text))
                        .KnnVector(k => k
                            .Name(d => d.PassageEmbedding)
                            .Dimension(768)
                            .Method(km => km
                                .Engine("lucene")
                                .SpaceType("l2")
                                .Name("hnsw"))))));
        createIndexResp.ShouldBeValid();

        var documents = new NeuralSearchDoc[]
        {
            new()
            {
                Id = "4319130149.jpg",
                Text =
                    "A West Virginia university women 's basketball team , officials , and a small gathering of fans are in a West Virginia arena ."
            },
            new() { Id = "1775029934.jpg", Text = "A wild animal races across an uncut field with a minimal amount of trees ." },
            new()
            {
                Id = "2664027527.jpg",
                Text = "People line the stands which advertise Freemont 's orthopedics , a cowboy rides a light brown bucking bronco ."
            },
            new() { Id = "4427058951.jpg", Text = "A man who is riding a wild horse in the rodeo is very near to falling off ." },
            new() { Id = "2691147709.jpg", Text = "A rodeo cowboy , wearing a cowboy hat , is being thrown off of a wild white horse ." }
        };
        var bulkResp = client.Bulk(b => b
            .Index(IndexName)
            .IndexMany(documents)
            .Refresh(Refresh.WaitFor));
        bulkResp.ShouldBeValid();

        var putSearchPipelineResp = client.LowLevel.SearchPipeline.Put<DynamicResponse>(SearchPipeline,
            PostData.Serializable(new
            {
                description = "Post processor for hybrid search",
                phase_results_processors = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["normalization-processor"] = new
                        {
                            normalization = new { technique = "min_max" },
                            combination = new { technique = "arithmetic_mean", parameters = new { weights = new[] { 0.3, 0.7 } } }
                        }
                    }
                }
            }));
        putSearchPipelineResp.Success.Should().BeTrue("{0}", putSearchPipelineResp.DebugInformation);
    }

    protected override void AssertQueryResponseValid(ISearchResponse<NeuralSearchDoc> response)
    {
        base.AssertQueryResponseValid(response);

        response.Hits.Should().HaveCount(5);
        var hit = response.Hits.First();

        hit.Id.Should().Be("2691147709.jpg");
        hit.Score.Should().BeApproximately(0.86481035, 0.0000002);
        hit.Source.Text.Should().Be("A rodeo cowboy , wearing a cowboy hat , is being thrown off of a wild white horse .");
        hit.Source.PassageEmbedding.Should().HaveCount(768);
    }

    protected override void IntegrationTeardown(IOpenSearchClient client, CallUniqueValues values)
    {
        client.LowLevel.SearchPipeline.Delete<VoidResponse>(SearchPipeline);
        client.Indices.Delete(IndexName);
        client.Ingest.DeletePipeline(TestName);
    }
}
