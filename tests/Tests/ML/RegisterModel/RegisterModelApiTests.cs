/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.ML.RegisterModel
{
	// Exercises the generated ml.register_model operation end-to-end.
	//
	// Key aspect: the request body contains a ModelConfig property typed as IModelConfig
	// (a generated $ref type).  The [U] serialization tests prove that:
	//   - the nested object serializes as a JSON object with the correct wire names
	//     (model_type, embedding_dimension, framework_type, all_config), and
	//   - the ModelFormat enum member serializes as its [EnumMember] wire value
	//     ("TORCH_SCRIPT").
	//
	// For [I]: registering a real model requires a reachable URL or pre-uploaded chunks;
	// on a fresh cluster with no external model registry the call is expected to fail with
	// an error response (404 / 400 range), so ExpectIsValid is false.
	//
	// The ml.register_model endpoint (/_plugins/_ml/models/_register) stabilised alongside the
	// model-management surface in ml-commons 2.8.0; below that the URL returns "no handler found".
	// SkipVersion suppresses only the [I] cases below 2.8.0 — the [U] cases run on every version.
	[SkipVersion("<2.8.0", "ml-commons register_model endpoint stabilised in 2.8.0")]
	public class RegisterModelApiTests
		: ApiIntegrationTestBase<WritableCluster, RegisterModelResponse, IRegisterModelRequest,
			RegisterModelDescriptor, RegisterModelRequest>
	{
		public RegisterModelApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		// [I]: no real model artefact → server will return an error
		protected override bool ExpectIsValid => false;

		protected override object ExpectJson => new
		{
			name = "osnet-test-bert",
			version = "1",
			model_group_id = "test-group-id",
			model_format = "TORCH_SCRIPT",
			model_config = new
			{
				model_type = "bert",
				embedding_dimension = 768,
				framework_type = "sentence_transformers",
				all_config = "{\"architectures\":[\"BertModel\"]}",
			},
		};

		// Fresh cluster returns 400 for an unknown URL-based model registration. On a cluster
		// where the ML index does not yet exist (IndexNotFoundException) the engine may return 500.
		protected override int ExpectStatusCode => 400;

		[I] public override async Task ReturnsExpectedStatusCode() =>
			await AssertOnAllResponses(r =>
				r.ApiCall.HttpStatusCode.Should().BeOneOf(400, 500));

		protected override Func<RegisterModelDescriptor, IRegisterModelRequest> Fluent => d => d
			.Name("osnet-test-bert")
			.Version("1")
			.ModelGroupId("test-group-id")
			.ModelFormat(ModelFormat.TorchScript)
			.ModelConfig(new ModelConfig
			{
				ModelType = "bert",
				EmbeddingDimension = 768,
				FrameworkType = "sentence_transformers",
				AllConfig = "{\"architectures\":[\"BertModel\"]}",
			});

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override RegisterModelRequest Initializer => new()
		{
			Name = "osnet-test-bert",
			Version = "1",
			ModelGroupId = "test-group-id",
			ModelFormat = ModelFormat.TorchScript,
			ModelConfig = new ModelConfig
			{
				ModelType = "bert",
				EmbeddingDimension = 768,
				FrameworkType = "sentence_transformers",
				AllConfig = "{\"architectures\":[\"BertModel\"]}",
			},
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => "/_plugins/_ml/models/_register";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Ml.RegisterModel(f),
			(client, f) => client.Ml.RegisterModelAsync(f),
			(client, r) => client.Ml.RegisterModel(r),
			(client, r) => client.Ml.RegisterModelAsync(r)
		);
	}
}
