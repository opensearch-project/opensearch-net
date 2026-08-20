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
	/// Round-trip serialization tests for a representative sample of generated ml namespace types,
	/// covering three shapes: (1) a request with a $ref object property that resolves to a typed
	/// interface (<see cref="IModelConfig"/>), (2) a response that deserializes fields correctly,
	/// and (3) an enum property that serializes as its <c>[EnumMember]</c> wire string.
	/// </summary>
	public class MlGeneratedSerializationTests
	{
		/// <summary>
		/// Test 1: <c>ml.register_model</c> request with a nested <see cref="IModelConfig"/> object.
		/// Verifies that the $ref-resolved interface property is serialized as a real JSON object
		/// (not omitted or collapsed) and that <see cref="ModelFormat.TorchScript"/> renders as
		/// <c>"TORCH_SCRIPT"</c> (its <c>[EnumMember]</c> value).
		/// </summary>
		[U]
		public void RegisterModelRequest_NestedModelConfig_SerializesCorrectly()
		{
			var expectedJson = new
			{
				name = "bert",
				version = "1.0",
				model_format = "TORCH_SCRIPT",
				model_config = new
				{
					model_type = "bert",
					framework_type = "sentence_transformers",
					embedding_dimension = 768L,
				},
			};

			var request = new RegisterModelRequest
			{
				Name = "bert",
				Version = "1.0",
				ModelFormat = ModelFormat.TorchScript,
				ModelConfig = new ModelConfig
				{
					ModelType = "bert",
					FrameworkType = "sentence_transformers",
					EmbeddingDimension = 768,
				},
			};

			Expect(expectedJson).FromRequest(c => c.Ml.RegisterModel(request));
		}

		/// <summary>
		/// Test 2: <see cref="RegisterModelResponse"/> deserialization.
		/// Verifies that the three response fields (<c>model_id</c>, <c>status</c>, <c>task_id</c>)
		/// map to their C# properties via <c>[DataMember(Name=...)]</c>.
		/// </summary>
		[U]
		public void RegisterModelResponse_DeserializesFieldsCorrectly()
		{
			const string json = @"{""model_id"":""m1"",""task_id"":""t1"",""status"":""CREATED""}";

			var response = Expect(json).NoRoundTrip().DeserializesTo<RegisterModelResponse>();

			response.Should().NotBeNull();
			response.ModelId.Should().Be("m1");
			response.TaskId.Should().Be("t1");
			response.OperationStatus.Should().Be("CREATED");
		}

		/// <summary>
		/// Test 3: <c>ml.create_connector</c> request with <see cref="ConnectorProtocol.AwsSigv4"/>.
		/// Verifies that the <c>[StringEnum]</c> enum serializes as its <c>[EnumMember]</c> wire value
		/// (<c>"aws_sigv4"</c>, not the C# identifier <c>"AwsSigv4"</c>).
		/// Also exercises the <c>actions</c> list property (<see cref="IList{IMLAction}"/>),
		/// confirming the nested action object is rendered with its wire fields.
		/// </summary>
		[U]
		public void CreateConnectorRequest_EnumAndActionList_SerializeCorrectly()
		{
			var expectedJson = new
			{
				name = "bedrock",
				description = "Bedrock connector",
				version = 1,
				protocol = "aws_sigv4",
				actions = new[]
				{
					new
					{
						action_type = "predict",
						method = "POST",
						url = "https://bedrock.us-east-1.amazonaws.com/model/invoke",
					},
				},
			};

			var request = new CreateConnectorRequest
			{
				Name = "bedrock",
				Description = "Bedrock connector",
				Version = 1,
				Protocol = ConnectorProtocol.AwsSigv4,
				Actions = new List<IMLAction>
				{
					new MLAction
					{
						ActionType = "predict",
						Method = "POST",
						Url = "https://bedrock.us-east-1.amazonaws.com/model/invoke",
					},
				},
			};

			Expect(expectedJson).FromRequest(c => c.Ml.CreateConnector(request));
		}

		[U]
		public void ExecuteAlgorithmResponse_Variant1_SampleCalculator_Deserializes()
		{
			const string json = @"{""result"":3.14}";

			var response = Expect(json).NoRoundTrip().DeserializesTo<ExecuteAlgorithmResponse>();

			response.Should().NotBeNull();
			response.Result.Should().BeApproximately(3.14f, 0.001f);
			response.Results.Should().BeNull();
		}

		[U]
		public void ExecuteAlgorithmResponse_Variant2_AnomalyLocalization_Deserializes()
		{
			const string json = @"{""results"":[{""name"":""anomaly1""},{""name"":""anomaly2""}]}";

			var response = Expect(json).NoRoundTrip().DeserializesTo<ExecuteAlgorithmResponse>();

			response.Should().NotBeNull();
			response.Result.Should().BeNull();
			response.Results.Should().HaveCount(2);
			response.Results[0].Name.Should().Be("anomaly1");
			response.Results[1].Name.Should().Be("anomaly2");
		}
	}
}
