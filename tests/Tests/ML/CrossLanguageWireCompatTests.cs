/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Serialization;
using static Tests.Core.Serialization.SerializationTestHelper;

namespace Tests.ML
{
	/// <summary>
	/// Cross-language wire-compatibility tests.
	///
	/// These tests use canonical JSON fixtures that represent the exact wire format
	/// produced by opensearch-java's generated client for the same operations.
	/// Both clients generate from the same OpenAPI spec (opensearch-api-specification),
	/// so their serialized output MUST be byte-compatible on the wire.
	///
	/// Methodology:
	///   1. Construct the .NET object with the same field values
	///   2. Serialize → assert JSON matches the canonical fixture (Java-compatible output)
	///   3. Deserialize the fixture → assert .NET object fields match
	///
	/// If a test here fails, it means the .NET codegen produces wire-incompatible
	/// output vs opensearch-java for the same spec operation.
	/// </summary>
	public class CrossLanguageWireCompatTests
	{
		/// <summary>
		/// ml.register_model — Java produces:
		/// <code>
		/// RegisterModelRequest req = new RegisterModelRequest.Builder()
		///     .name("bert-embedding")
		///     .version("1.0.0")
		///     .modelGroupId("group-abc")
		///     .modelFormat(ModelFormat.TorchScript)
		///     .modelConfig(c -> c
		///         .modelType("bert")
		///         .embeddingDimension(768L)
		///         .frameworkType("sentence_transformers")
		///         .allConfig("{\"architectures\":[\"BertModel\"]}")
		///     )
		///     .connectorId("conn-123")
		///     .description("BERT embedding model")
		///     .url("https://artifacts.example.com/bert.zip")
		///     .build();
		/// </code>
		/// </summary>
		private const string RegisterModelCanonicalJson =
			@"{""connector_id"":""conn-123"","
			+ @"""description"":""BERT embedding model"","
			+ @"""model_config"":{""all_config"":""{\""architectures\"":[\""BertModel\""]}"","
			+ @"""embedding_dimension"":768,"
			+ @"""framework_type"":""sentence_transformers"","
			+ @"""model_type"":""bert""},"
			+ @"""model_format"":""TORCH_SCRIPT"","
			+ @"""model_group_id"":""group-abc"","
			+ @"""name"":""bert-embedding"","
			+ @"""url"":""https://artifacts.example.com/bert.zip"","
			+ @"""version"":""1.0.0""}";

		[U]
		public void RegisterModel_Serializes_JavaCompatible()
		{
			var request = new RegisterModelRequest
			{
				Name = "bert-embedding",
				Version = "1.0.0",
				ModelGroupId = "group-abc",
				ModelFormat = ModelFormat.TorchScript,
				ModelConfig = new ModelConfig
				{
					ModelType = "bert",
					EmbeddingDimension = 768,
					FrameworkType = "sentence_transformers",
					AllConfig = "{\"architectures\":[\"BertModel\"]}",
				},
				ConnectorId = "conn-123",
				Description = "BERT embedding model",
				Url = "https://artifacts.example.com/bert.zip",
			};

			Expect(RegisterModelCanonicalJson).FromRequest(c => c.Ml.RegisterModel(request));
		}

		/// <summary>
		/// ml.create_connector — Java produces:
		/// <code>
		/// CreateConnectorRequest req = new CreateConnectorRequest.Builder()
		///     .name("bedrock-titan")
		///     .description("Bedrock Titan embedding connector")
		///     .version(1)
		///     .protocol(ConnectorProtocol.AwsSigv4)
		///     .credential(c -> c)  // empty object {}
		///     .parameters(p -> p)  // empty object {}
		///     .actions(List.of(new Action.Builder()
		///         .actionType("predict")
		///         .method("POST")
		///         .url("https://bedrock-runtime.us-east-1.amazonaws.com/model/invoke")
		///         .requestBody("{\"inputText\":\"${parameters.inputText}\"}")
		///         .build()))
		///     .build();
		/// </code>
		/// </summary>
		private const string CreateConnectorCanonicalJson =
			@"{""actions"":[{""action_type"":""predict"","
			+ @"""method"":""POST"","
			+ @"""request_body"":""{\""inputText\"":\""${parameters.inputText}\""}"""
			+ @",""url"":""https://bedrock-runtime.us-east-1.amazonaws.com/model/invoke""}],"
			+ @"""description"":""Bedrock Titan embedding connector"","
			+ @"""name"":""bedrock-titan"","
			+ @"""protocol"":""aws_sigv4"","
			+ @"""version"":1}";

		[U]
		public void CreateConnector_Serializes_JavaCompatible()
		{
			var request = new CreateConnectorRequest
			{
				Name = "bedrock-titan",
				Description = "Bedrock Titan embedding connector",
				Version = 1,
				Protocol = ConnectorProtocol.AwsSigv4,
				Actions = new List<IMLAction>
				{
					new MLAction
					{
						ActionType = "predict",
						Method = "POST",
						Url = "https://bedrock-runtime.us-east-1.amazonaws.com/model/invoke",
						RequestBody = "{\"inputText\":\"${parameters.inputText}\"}",
					},
				},
			};

			Expect(CreateConnectorCanonicalJson).FromRequest(c => c.Ml.CreateConnector(request));
		}

		/// <summary>
		/// ml.get_task response — same JSON both clients must deserialize identically.
		/// Java's GetMlTaskResponse fields: model_id, state, task_type, function_name, etc.
		/// </summary>
		private const string GetTaskResponseJson =
			@"{""model_id"":""model-xyz"","
			+ @"""state"":""COMPLETED"","
			+ @"""task_type"":""REGISTER_MODEL"","
			+ @"""function_name"":""REMOTE""}";

		[U]
		public void GetTaskResponse_Deserializes_JavaCompatible()
		{
			var response = Expect(GetTaskResponseJson)
				.NoRoundTrip()
				.DeserializesTo<GetMLTaskResponse>();

			response.Should().NotBeNull();
			response.ModelId.Should().Be("model-xyz");
			response.State.Should().Be(MLTaskState.Completed);
			response.TaskType.Should().Be(MlTaskType.RegisterModel);
			response.FunctionName.Should().Be(FunctionName.Remote);
		}
	}
}
