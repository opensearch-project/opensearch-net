/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Diagnostics;
using OpenSearch.Client;
using OpenSearch.Net;

namespace Samples.NeuralSearch;

/// <summary>
/// Sample based off of the <a href="https://opensearch.org/docs/latest/search-plugins/neural-search-tutorial">Neural Search Tutorial</a>
/// </summary>
public class NeuralSearchSample : Sample
{
    private const string SampleName = "neural-search";
    private const string ResourceNamePrefix = "csharp-" + SampleName;
    private const string MlModelGroupName = ResourceNamePrefix + "-model-group";
    private const string IngestPipelineName = ResourceNamePrefix + "-ingest-pipeline";
    private const string IndexName = ResourceNamePrefix + "-index";

    private string? _modelGroupId;
    private string? _modelRegistrationTaskId;
    private string? _modelId;
    private string? _modelDeployTaskId;
    private bool _putIngestPipeline;
    private bool _createdIndex;

    public NeuralSearchSample() : base(SampleName, "A sample demonstrating how to perform a neural search query") { }

    public class NeuralSearchDoc
    {
        [PropertyName("id")] public string? Id { get; set; }
        [PropertyName("text")] public string? Text { get; set; }
        [PropertyName("passage_embedding")] public float[]? PassageEmbedding { get; set; }
    }

    protected override async Task Run(IOpenSearchClient client)
    {
        // Temporarily configure the cluster to allow local running of the ML model
        var putSettingsResp = await client.Cluster.PutSettingsAsync(s => s
            .Transient(p => p
                .Add("plugins.ml_commons.only_run_on_ml_node", false)
                .Add("plugins.ml_commons.model_access_control_enabled", true)
                .Add("plugins.ml_commons.native_memory_threshold", 99)));
        Assert(putSettingsResp, r => r.IsValid);
        Console.WriteLine("Configured cluster to allow local execution of the ML model");

        // Register an ML model group
        var registerModelGroupResp = await client.Http.PostAsync<DynamicResponse>(
            "/_plugins/_ml/model_groups/_register",
            r => r.SerializableBody(new
            {
                name = MlModelGroupName,
                description = $"A model group for the opensearch-net {SampleName} sample",
                access_mode = "public"
            }));
        AssertCreatedStatus(registerModelGroupResp);
        Console.WriteLine($"Model group named {MlModelGroupName} {registerModelGroupResp.Body.status}: {registerModelGroupResp.Body.model_group_id}");
        _modelGroupId = (string) registerModelGroupResp.Body.model_group_id;

        // Register the ML model
        var registerModelResp = await client.Http.PostAsync<DynamicResponse>(
            "/_plugins/_ml/models/_register",
            r => r.SerializableBody(new
            {
                name = "huggingface/sentence-transformers/msmarco-distilbert-base-tas-b",
                version = "1.0.1",
                model_group_id = _modelGroupId,
                model_format = "TORCH_SCRIPT"
            }));
        AssertCreatedStatus(registerModelResp);
        Console.WriteLine($"Model registration task {registerModelResp.Body.status}: {registerModelResp.Body.task_id}");
        _modelRegistrationTaskId = (string) registerModelResp.Body.task_id;

        // Wait for ML model registration to complete
        while (true)
        {
            var getTaskResp = await client.Http.GetAsync<DynamicResponse>($"/_plugins/_ml/tasks/{_modelRegistrationTaskId}");
            Console.WriteLine($"Model registration: {getTaskResp.Body.state}");
            AssertNotFailedState(getTaskResp);
            if (((string)getTaskResp.Body.state).StartsWith("COMPLETED"))
            {
                _modelId = getTaskResp.Body.model_id;
                break;
            }
            await Task.Delay(10000);
        }
        Console.WriteLine($"Model registered: {_modelId}");

        // Deploy the ML model
        var deployModelResp = await client.Http.PostAsync<DynamicResponse>($"/_plugins/_ml/models/{_modelId}/_deploy");
        AssertCreatedStatus(deployModelResp);
        Console.WriteLine($"Model deployment task {deployModelResp.Body.status}: {deployModelResp.Body.task_id}");
        _modelDeployTaskId = (string) deployModelResp.Body.task_id;

        // Wait for ML model deployment to complete
        while (true)
        {
            var getTaskResp = await client.Http.GetAsync<DynamicResponse>($"/_plugins/_ml/tasks/{_modelDeployTaskId}");
            Console.WriteLine($"Model deployment: {getTaskResp.Body.state}");
            AssertNotFailedState(getTaskResp);
            if (((string)getTaskResp.Body.state).StartsWith("COMPLETED")) break;
            await Task.Delay(10000);
        }
        Console.WriteLine($"Model deployed: {_modelId}");

        // Create the text_embedding ingest pipeline
        var putIngestPipelineResp = await client.Ingest.PutPipelineAsync(IngestPipelineName, p => p
            .Description($"A text_embedding ingest pipeline for the opensearch-net {SampleName} sample")
            .Processors(pp => pp
                .TextEmbedding<NeuralSearchDoc>(te => te
                    .ModelId(_modelId)
                    .FieldMap(fm => fm
                        .Map(d => d.Text, d => d.PassageEmbedding)))));
        AssertValid(putIngestPipelineResp);
        Console.WriteLine($"Put ingest pipeline {IngestPipelineName}: {putIngestPipelineResp.Acknowledged}");
        _putIngestPipeline = true;

        // Create the index
        var createIndexResp = await client.Indices.CreateAsync(
            IndexName,
            i => i
                .Settings(s => s
                    .Setting("index.knn", true)
                    .DefaultPipeline(IngestPipelineName))
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
        AssertValid(createIndexResp);
        Console.WriteLine($"Created index {IndexName}: {createIndexResp.Acknowledged}");
        _createdIndex = true;

        // Index some documents
        var documents = new NeuralSearchDoc[]
        {
            new() { Id = "4319130149.jpg", Text = "A West Virginia university women 's basketball team , officials , and a small gathering of fans are in a West Virginia arena ." },
            new() { Id = "1775029934.jpg", Text = "A wild animal races across an uncut field with a minimal amount of trees ." },
            new() { Id = "2664027527.jpg", Text = "People line the stands which advertise Freemont 's orthopedics , a cowboy rides a light brown bucking bronco ." },
            new() { Id = "4427058951.jpg", Text = "A man who is riding a wild horse in the rodeo is very near to falling off ." },
            new() { Id = "2691147709.jpg", Text = "A rodeo cowboy , wearing a cowboy hat , is being thrown off of a wild white horse ." }
        };
        var bulkResp = await client.BulkAsync(b => b
            .Index(IndexName)
            .IndexMany(documents)
            .Refresh(Refresh.WaitFor));
        AssertValid(bulkResp);
        Console.WriteLine($"Indexed {documents.Length} documents");

        // Perform the neural search
        Console.WriteLine("Performing neural search for text 'wild west'");
        var searchResp = await client.SearchAsync<NeuralSearchDoc>(s => s
            .Index(IndexName)
            .Source(sf => sf
                .Excludes(f => f
                    .Field(d => d.PassageEmbedding)))
            .Query(q => q
                .Neural(n => n
                    .Field(f => f.PassageEmbedding)
                    .QueryText("wild west")
                    .ModelId(_modelId)
                    .K(5))));
        AssertValid(searchResp);
        Console.WriteLine($"Found {searchResp.Hits.Count} documents");
        foreach (var hit in searchResp.Hits) Console.WriteLine($"- Document id: {hit.Source.Id}, score: {hit.Score}, text: {hit.Source.Text}");
    }

    protected override async Task Cleanup(IOpenSearchClient client)
    {
        Console.WriteLine("\n\n-- CLEANING UP --");
        if (_createdIndex)
        {
            // Cleanup the index
            var deleteIndexResp = await client.Indices.DeleteAsync(IndexName);
            AssertValid(deleteIndexResp);
            Console.WriteLine($"Deleted index: {deleteIndexResp.Acknowledged}");
        }

        if (_putIngestPipeline)
        {
            // Cleanup the ingest pipeline
            var deleteIngestPipelineResp = await client.Ingest.DeletePipelineAsync(IngestPipelineName);
            AssertValid(deleteIngestPipelineResp);
            Console.WriteLine($"Deleted ingest pipeline: {deleteIngestPipelineResp.Acknowledged}");
        }

        if (_modelDeployTaskId != null)
        {
            // Cleanup the model deployment task
            var deleteModelDeployTaskResp = await client.Http.DeleteAsync<DynamicResponse>($"/_plugins/_ml/tasks/{_modelDeployTaskId}");
            AssertDeletedResult(deleteModelDeployTaskResp);
            Console.WriteLine($"Deleted model deployment task: {deleteModelDeployTaskResp.Body.result}");
        }

        if (_modelId != null)
        {
            while (true)
            {
                // Try cleanup the ML model
                var deleteModelResp = await client.Http.DeleteAsync<DynamicResponse>($"/_plugins/_ml/models/{_modelId}");
                if (deleteModelResp.Success)
                {
                    Console.WriteLine($"Deleted model: {deleteModelResp.Body.result}");
                    break;
                }

                Assert(deleteModelResp, r => ((string?) r.Body.error?.reason)?.Contains("Try undeploy") ?? false);

                // Undeploy the ML model
                var undeployModelResp = await client.Http.PostAsync<DynamicResponse>($"/_plugins/_ml/models/{_modelId}/_undeploy");
                Assert(undeployModelResp, r => r.Success);
                Console.WriteLine("Undeployed model");
                await Task.Delay(10000);
            }
        }

        if (_modelRegistrationTaskId != null)
        {
            // Cleanup the model registration task
            var deleteModelRegistrationTaskResp = await client.Http.DeleteAsync<DynamicResponse>($"/_plugins/_ml/tasks/{_modelRegistrationTaskId}");
            AssertDeletedResult(deleteModelRegistrationTaskResp);
            Console.WriteLine($"Deleted model registration task: {deleteModelRegistrationTaskResp.Body.result}");
        }

        if (_modelGroupId != null)
        {
            // Cleanup the model group
            var deleteModelGroupResp = await client.Http.DeleteAsync<DynamicResponse>($"/_plugins/_ml/model_groups/{_modelGroupId}");
            AssertDeletedResult(deleteModelGroupResp);
            Console.WriteLine($"Deleted model group: {deleteModelGroupResp.Body.result}");
        }
    }

    private static void AssertCreatedStatus(DynamicResponse response) =>
        Assert(response, r => r.Success && (string)r.Body.status == "CREATED");

    private static void AssertNotFailedState(DynamicResponse response) =>
        Assert(response, r => r.Success && (string) r.Body.state != "FAILED");

    private static void AssertDeletedResult(DynamicResponse response) =>
        Assert(response, r => r.Success && (string) r.Body.result == "deleted");
}
