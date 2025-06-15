/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Stack.ArtifactsApi.Products;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests.TestState;
using Version = SemanticVersioning.Version;

namespace Tests.QueryDsl.Specialized.Neural;

public class NeuralQueryCluster : ClientTestClusterBase
{
    public NeuralQueryCluster() : base(CreateConfiguration()) { }

    private static ClientTestClusterConfiguration CreateConfiguration()
    {
        var config = new ClientTestClusterConfiguration(
            OpenSearchPlugin.Knn,
            OpenSearchPlugin.MachineLearning,
            OpenSearchPlugin.NeuralSearch,
            OpenSearchPlugin.Security) { MaxConcurrency = 4, ValidatePluginsToInstall = false, };

        config.DefaultNodeSettings.Add("indices.breaker.total.limit", "100%");
        config.DefaultNodeSettings.Add("plugins.ml_commons.only_run_on_ml_node", "false");
        config.DefaultNodeSettings.Add("plugins.ml_commons.native_memory_threshold", "100");
        config.DefaultNodeSettings.Add("plugins.ml_commons.jvm_heap_memory_threshold", "100", ">=2.9.0");
        config.DefaultNodeSettings.Add("plugins.ml_commons.model_access_control_enabled", "true", ">=2.8.0");

        return config;
    }

    public string ModelGroupId { get; private set; }
    public string ModelId { get; private set; }

    protected override void SeedNode()
    {
        var baseVersion = ClusterConfiguration.Version.BaseVersion();
        var renamedToRegisterDeploy = baseVersion >= new Version("2.7.0");
        var hasModelAccessControl = baseVersion >= new Version("2.8.0");

        if (hasModelAccessControl)
        {
            var registerModelGroupResp = Client.Http.Post<DynamicResponse>(
                "/_plugins/_ml/model_groups/_register",
                r => r.SerializableBody(new { name = "Neural Search Model Group", access_mode = "public", model_access_mode = "public" }));
            registerModelGroupResp.ShouldBeCreated();
            ModelGroupId = (string)registerModelGroupResp.Body.model_group_id;
        }

        var registerModelResp = Client.Http.Post<DynamicResponse>(
            $"/_plugins/_ml/models/{(renamedToRegisterDeploy ? "_register" : "_upload")}",
            r => r.SerializableBody(new
            {
                name = "huggingface/sentence-transformers/msmarco-distilbert-base-tas-b",
                version = "1.0.1",
                model_group_id = ModelGroupId,
                model_format = "TORCH_SCRIPT"
            }));
        registerModelResp.ShouldBeCreated();
        var modelRegistrationTaskId = (string)registerModelResp.Body.task_id;

        while (true)
        {
            var getTaskResp = Client.Http.Get<DynamicResponse>($"/_plugins/_ml/tasks/{modelRegistrationTaskId}");
            getTaskResp.ShouldNotBeFailed();
            if (((string)getTaskResp.Body.state).StartsWith("COMPLETED"))
            {
                ModelId = getTaskResp.Body.model_id;
                break;
            }
            Thread.Sleep(5000);
        }

        var deployModelResp =
            Client.Http.Post<DynamicResponse>($"/_plugins/_ml/models/{ModelId}/{(renamedToRegisterDeploy ? "_deploy" : "_load")}");
        deployModelResp.ShouldBeCreated();
        var modelDeployTaskId = (string)deployModelResp.Body.task_id;

        while (true)
        {
            var getTaskResp = Client.Http.Get<DynamicResponse>($"/_plugins/_ml/tasks/{modelDeployTaskId}");
            getTaskResp.ShouldNotBeFailed();
            if (((string)getTaskResp.Body.state).StartsWith("COMPLETED")) break;

            Thread.Sleep(5000);
        }

        var updateIndexSettingsResp = AdminCertClient.Indices.UpdateSettings(".plugins-ml-*",
            u => u.ExpandWildcards(ExpandWildcards.All).IndexSettings(s => s.NumberOfReplicas(0)));
        updateIndexSettingsResp.ShouldBeValid();
    }

    private IOpenSearchClient _adminCertClient;

    private IOpenSearchClient AdminCertClient
    {
        get
        {
            if (_adminCertClient != null) return _adminCertClient;

            var settings = this.CreateConnectionSettings()
                .ClientCertificate(X509Certificate2.CreateFromPemFile(FileSystem.ConfigPath + "/kirk.pem", FileSystem.ConfigPath + "/kirk-key.pem"));
            _adminCertClient = new OpenSearchClient(this.UpdateSettings(settings));
            return _adminCertClient;
        }
    }
}

public class NeuralSearchDoc
{
    [PropertyName("id")] public string Id { get; set; }
    [PropertyName("text")] public string Text { get; set; }
    [PropertyName("passage_embedding")] public float[] PassageEmbedding { get; set; }
}

[SkipVersion("<2.6.0", "Avoid the various early permutations of the ML APIs")]
public class NeuralQueryUsageTests
    : QueryDslUsageTestsBase<NeuralQueryCluster, NeuralSearchDoc>
{
    private static readonly string TestName = nameof(NeuralQueryUsageTests).ToLowerInvariant();

    public string ModelId => Cluster?.ModelId ?? "default-for-unit-tests";

    public NeuralQueryUsageTests(NeuralQueryCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override IndexName IndexName => TestName;
    protected override string ExpectedIndexString => TestName;

    protected override QueryContainer QueryInitializer => new NeuralQuery
    {
        Field = Infer.Field<NeuralSearchDoc>(d => d.PassageEmbedding), QueryText = "wild west", K = 5, ModelId = ModelId
    };

    protected override object QueryJson => new { neural = new { passage_embedding = new { query_text = "wild west", k = 5, model_id = ModelId } } };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<NeuralSearchDoc> q) => q
        .Neural(n => n
            .Field(f => f.PassageEmbedding)
            .QueryText("wild west")
            .K(5)
            .ModelId(ModelId));

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
