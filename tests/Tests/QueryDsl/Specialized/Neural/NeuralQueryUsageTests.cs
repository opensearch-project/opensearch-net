/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests.TestState;
using Version = SemanticVersioning.Version;

namespace Tests.QueryDsl.Specialized.Neural;

public class NeuralSearchDoc
{
    [PropertyName("id")] public string Id { get; set; }
    [PropertyName("text")] public string Text { get; set; }
    [PropertyName("passage_embedding")] public float[] PassageEmbedding { get; set; }
}

[SkipVersion("<2.6.0", "Avoid the various early permutations of the ML APIs")]
[SkipPrereleaseVersions("Prerelease versions of OpenSearch do not include the ML & Neural Search plugins")]
public class NeuralQueryUsageTests
    : QueryDslUsageTestsBase<WritableCluster, NeuralSearchDoc>
{
    private static readonly string TestName = nameof(NeuralQueryUsageTests).ToLowerInvariant();

    private string _modelGroupId;
    private string _modelId = "default-for-unit-tests";

    public NeuralQueryUsageTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override IndexName IndexName => TestName;
    protected override string ExpectedIndexString => TestName;

    protected override QueryContainer QueryInitializer => new NeuralQuery
    {
        Field = Infer.Field<NeuralSearchDoc>(d => d.PassageEmbedding),
        QueryText = "wild west",
        K = 5,
        ModelId = _modelId
    };

    protected override object QueryJson => new
    {
        neural = new
        {
            passage_embedding = new
            {
                query_text = "wild west",
                k = 5,
                model_id = _modelId
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<NeuralSearchDoc> q) => q
        .Neural(n => n
            .Field(f => f.PassageEmbedding)
            .QueryText("wild west")
            .K(5)
            .ModelId(_modelId));

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<INeuralQuery>(a => a.Neural)
    {
        q =>
        {
            q.Field = null;
            q.QueryText = "wild west";
            q.K = 5;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = null;
            q.K = 5;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "";
            q.K = 5;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "wild west";
            q.K = null;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "wild west";
            q.K = 0;
            q.ModelId = "aFcV879";
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "wild west";
            q.K = 5;
            q.ModelId = null;
        },
        q =>
        {
            q.Field = "passage_embedding";
            q.QueryText = "wild west";
            q.K = 5;
            q.ModelId = "";
        }
    };

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        var baseVersion = Cluster.ClusterConfiguration.Version.BaseVersion();
        var renamedToRegisterDeploy = baseVersion >= new Version("2.7.0");
        var hasModelAccessControl = baseVersion >= new Version("2.8.0");

        var settings = new Dictionary<string, object>
        {
            ["plugins.ml_commons.only_run_on_ml_node"] = false,
            ["plugins.ml_commons.native_memory_threshold"] = 99
        };

        if (hasModelAccessControl)
            settings["plugins.ml_commons.model_access_control_enabled"] = true;

        if (settings.Count > 0)
        {
            var putSettingsResp = client.Cluster.PutSettings(new ClusterPutSettingsRequest
            {
                Transient = settings
            });
            putSettingsResp.ShouldBeValid();
        }

        if (hasModelAccessControl)
        {
            var registerModelGroupResp = client.Http.Post<DynamicResponse>(
                "/_plugins/_ml/model_groups/_register",
                r => r.SerializableBody(new
                {
                    name = TestName,
                    access_mode = "public",
                    model_access_mode = "public"
                }));
            registerModelGroupResp.ShouldBeCreated();
            _modelGroupId = (string)registerModelGroupResp.Body.model_group_id;
        }

        var registerModelResp = client.Http.Post<DynamicResponse>(
            $"/_plugins/_ml/models/{(renamedToRegisterDeploy ? "_register" : "_upload")}",
            r => r.SerializableBody(new
            {
                name = "huggingface/sentence-transformers/msmarco-distilbert-base-tas-b",
                version = "1.0.1",
                model_group_id = _modelGroupId,
                model_format = "TORCH_SCRIPT"
            }));
        registerModelResp.ShouldBeCreated();
        var modelRegistrationTaskId = (string) registerModelResp.Body.task_id;

        while (true)
        {
            var getTaskResp = client.Http.Get<DynamicResponse>($"/_plugins/_ml/tasks/{modelRegistrationTaskId}");
            getTaskResp.ShouldNotBeFailed();
            if (((string)getTaskResp.Body.state).StartsWith("COMPLETED"))
            {
                _modelId = getTaskResp.Body.model_id;
                break;
            }
            Thread.Sleep(5000);
        }

        var deployModelResp = client.Http.Post<DynamicResponse>($"/_plugins/_ml/models/{_modelId}/{(renamedToRegisterDeploy ? "_deploy" : "_load")}");
        deployModelResp.ShouldBeCreated();
        var modelDeployTaskId = (string) deployModelResp.Body.task_id;

        while (true)
        {
            var getTaskResp = client.Http.Get<DynamicResponse>($"/_plugins/_ml/tasks/{modelDeployTaskId}");
            getTaskResp.ShouldNotBeFailed();
            if (((string)getTaskResp.Body.state).StartsWith("COMPLETED")) break;
            Thread.Sleep(5000);
        }

        var putIngestPipelineResp = client.Ingest.PutPipeline(TestName, p => p
            .Processors(pp => pp
                .TextEmbedding<NeuralSearchDoc>(te => te
                    .ModelId(_modelId)
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
            new() { Id = "4319130149.jpg", Text = "A West Virginia university women 's basketball team , officials , and a small gathering of fans are in a West Virginia arena ." },
            new() { Id = "1775029934.jpg", Text = "A wild animal races across an uncut field with a minimal amount of trees ." },
            new() { Id = "2664027527.jpg", Text = "People line the stands which advertise Freemont 's orthopedics , a cowboy rides a light brown bucking bronco ." },
            new() { Id = "4427058951.jpg", Text = "A man who is riding a wild horse in the rodeo is very near to falling off ." },
            new() { Id = "2691147709.jpg", Text = "A rodeo cowboy , wearing a cowboy hat , is being thrown off of a wild white horse ." }
        };
        var bulkResp = client.Bulk(b => b
            .Index(IndexName)
            .IndexMany(documents)
            .Refresh(Refresh.WaitFor));
        bulkResp.ShouldBeValid();
    }

    protected override void AssertQueryResponseValid(ISearchResponse<NeuralSearchDoc> response)
    {
        base.AssertQueryResponseValid(response);

        response.Hits.Should().HaveCount(5);
        var hit = response.Hits.First();

        hit.Id.Should().Be("4427058951.jpg");
        hit.Score.Should().BeApproximately(0.01585195, 0.00000001);
        hit.Source.Text.Should().Be("A man who is riding a wild horse in the rodeo is very near to falling off .");
        hit.Source.PassageEmbedding.Should().HaveCount(768);
    }

    protected override void IntegrationTeardown(IOpenSearchClient client, CallUniqueValues values)
    {
        client.Indices.Delete(IndexName);
        client.Ingest.DeletePipeline(TestName);

        if (_modelId != "default-for-unit-tests")
        {
            while (true)
            {
                var deleteModelResp = client.Http.Delete<DynamicResponse>($"/_plugins/_ml/models/{_modelId}");
                if (deleteModelResp.Success || !(((string)deleteModelResp.Body.error?.reason)?.Contains("Try undeploy") ?? false)) break;

                client.Http.Post<DynamicResponse>($"/_plugins/_ml/models/{_modelId}/_undeploy");
                Thread.Sleep(5000);
            }
        }

        if (_modelGroupId != null)
        {
            client.Http.Delete<DynamicResponse>($"/_plugins/_ml/model_groups/{_modelGroupId}");
        }
    }
}

internal static class Helpers
{
    public static void ShouldBeCreated(this DynamicResponse r)
    {
        if (!r.Success || r.Body.status != "CREATED") throw new Exception("Expected to be created, was: " + r.DebugInformation);
    }

    public static void ShouldNotBeFailed(this DynamicResponse r)
    {
        if (!r.Success || r.Body.state == "FAILED") throw new Exception("Expected to not be failed, was: " + r.DebugInformation);
    }
}
