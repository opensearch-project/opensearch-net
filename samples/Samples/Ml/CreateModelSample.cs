/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using OpenSearch.Client;

namespace Samples.Ml;

/// <summary>
/// Demonstrates creating an Amazon Bedrock Titan embedding connector on
/// Amazon OpenSearch Service (AOS), registering a remote model backed by
/// that connector, and deploying it.
///
/// On AOS the connector authenticates via IAM role assumption (aws_sigv4),
/// so no static credentials are needed — only a roleArn.
///
/// Prerequisites
/// -------------
///   1. An IAM role (e.g. "opensearch-bedrock-role") whose trust policy
///      allows ml.opensearchservice.amazonaws.com to assume it, and whose
///      permissions policy grants bedrock:InvokeModel on the Titan model ARN.
///   2. The caller must have iam:PassRole on that role and es:ESHttpPost
///      on the target AOS domain.
///   3. If fine-grained access control is enabled, map the caller's IAM ARN
///      to the ml_full_access role in OpenSearch Dashboards.
///
/// Usage
/// -----
///   dotnet run -- create-model-aos \
///     --host https://&lt;your-aos-domain&gt;.&lt;region&gt;.es.amazonaws.com
/// </summary>
public class CreateModelSample : Sample
{
    private const string SampleName = "create-model-aos";

    // Replace these with values for your environment.
    private const string RoleArn = "arn:aws:iam::<account-id>:role/opensearch-bedrock-role";
    private const string Region = "<region>";
    private const string BedrockModel = "amazon.titan-embed-text-v2:0";
    private const long EmbeddingDimension = 1024;

    private string? _connectorId;
    private string? _modelId;

    public CreateModelSample()
        : base(SampleName, "Creates a Bedrock Titan connector on AOS, registers and deploys a remote model") { }

    protected override async Task Run(IOpenSearchClient client)
    {
        // ----------------------------------------------------------------
        // Step 1 – Create the Bedrock connector.
        //
        // Uses aws_sigv4 so OpenSearch assumes the IAM role when calling
        // Bedrock Runtime — no static credentials required on AOS.
        //
        // Fields absent from the fixed schema (roleArn, region,
        // service_name, model, dimensions, normalize, embeddingTypes, and
        // the action headers) live in AdditionalProperties so they are
        // serialized as top-level siblings in the JSON object.
        // ----------------------------------------------------------------
        var createResp = await client.Ml.CreateConnectorAsync(c => c
            .Name("Amazon Bedrock Connector: embedding")
            .Description("The connector to Bedrock Titan Text Embedding V2")
            .Version(1)
            .Protocol(ConnectorProtocol.AwsSigv4)
            .Credential(new Credential
            {
                // On AOS the service assumes roleArn; no access/secret keys.
                AdditionalProperties = new Dictionary<string, object>
                {
                    ["roleArn"] = RoleArn
                }
            })
            .Parameters(new Parameters
            {
                AdditionalProperties = new Dictionary<string, object>
                {
                    ["region"] = Region,
                    ["service_name"] = "bedrock",
                    ["model"] = BedrockModel,
                    ["dimensions"] = EmbeddingDimension,
                    ["normalize"] = true,
                    ["embeddingTypes"] = new[] { "float" }
                }
            })
            .Actions(new List<IMlAction>
            {
                new MlAction
                {
                    ActionType = "predict",
                    Method = "POST",
                    Url = "https://bedrock-runtime.${parameters.region}.amazonaws.com"
                        + "/model/${parameters.model}/invoke",
                    Headers = new Headers
                    {
                        AdditionalProperties = new Dictionary<string, object>
                        {
                            ["content-type"] = "application/json",
                            ["x-amz-content-sha256"] = "required"
                        }
                    },
                    RequestBody =
                        "{ \"inputText\": \"${parameters.inputText}\","
                        + " \"dimensions\": ${parameters.dimensions},"
                        + " \"normalize\": ${parameters.normalize},"
                        + " \"embeddingTypes\": ${parameters.embeddingTypes} }",
                    PreProcessFunction = "connector.pre_process.bedrock.embedding",
                    PostProcessFunction = "connector.post_process.bedrock.embedding"
                }
            }));

        AssertValid(createResp);
        _connectorId = createResp.ConnectorId;
        Console.WriteLine($"Created connector: {_connectorId}");

        // ----------------------------------------------------------------
        // Step 2 – Register the remote model.
        //
        // model_config is required for semantic search; without it index
        // creation fails with "Model config is null for remote model".
        // embedding_dimension must match the dimensions parameter above.
        // ----------------------------------------------------------------
        var registerResp = await client.Ml.RegisterModelAsync(m => m
            .Name("bedrock-titan-embedding-model")
            .FunctionName("remote")
            .ConnectorId(_connectorId)
            .ModelConfig(new ModelConfig
            {
                ModelType = "TEXT_EMBEDDING",
                EmbeddingDimension = EmbeddingDimension,
                FrameworkType = "SENTENCE_TRANSFORMERS",
                AdditionalConfig = new AdditionalConfig
                {
                    SpaceType = "l2"
                }
            }));

        AssertValid(registerResp);
        Console.WriteLine($"Register model task: {registerResp.TaskId} ({registerResp.OperationStatus})");

        _modelId = await WaitForTaskAsync(client, registerResp.TaskId, "model registration");
        Console.WriteLine($"Model registered: {_modelId}");

        // ----------------------------------------------------------------
        // Step 3 – Deploy the model so it can serve inference requests.
        // ----------------------------------------------------------------
        var deployResp = await client.Ml.DeployModelAsync(new DeployModelRequest(_modelId));
        AssertValid(deployResp);
        Console.WriteLine($"Deploy model task: {deployResp.TaskId} ({deployResp.OperationStatus})");

        await WaitForTaskAsync(client, deployResp.TaskId, "model deployment");
        Console.WriteLine($"Model deployed: {_modelId}");
    }

    protected override async Task Cleanup(IOpenSearchClient client)
    {
        Console.WriteLine("\n\n-- CLEANING UP --");

        if (_modelId != null)
        {
            var undeployResp = await client.Ml.UndeployModelAsync(new UndeployModelRequest(_modelId));
            if (undeployResp.IsValid) Console.WriteLine($"Undeployed model: {_modelId}");

            var deleteModelResp = await client.Ml.DeleteModelAsync(new DeleteModelRequest(_modelId));
            if (deleteModelResp.IsValid) Console.WriteLine($"Deleted model: {_modelId}");
        }

        if (_connectorId != null)
        {
            var deleteConnectorResp = await client.Ml.DeleteConnectorAsync(new DeleteConnectorRequest(_connectorId));
            if (deleteConnectorResp.IsValid) Console.WriteLine($"Deleted connector: {_connectorId}");
        }
    }

    private static async Task<string> WaitForTaskAsync(IOpenSearchClient client, string taskId, string label)
    {
        while (true)
        {
            var taskResp = await client.Ml.GetTaskAsync(taskId);
            AssertValid(taskResp);

            var state = taskResp.State;
            Console.WriteLine($"  {label}: {state}");

            if (state == MlTaskState.Failed)
                throw new Exception($"{label} task {taskId} failed: {taskResp.OperationError}");

            if (state is MlTaskState.Completed or MlTaskState.CompletedWithError)
                return taskResp.ModelId
                    ?? throw new Exception($"{label} task completed but returned no model_id");

            await Task.Delay(5_000);
        }
    }
}
