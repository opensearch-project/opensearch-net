/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Client;
using OpenSearch.Net;
using FluentAssertions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Ml.DeployModel
{
	// Exercises the generated ml.deploy_model operation end-to-end.
	// deploy_model is a no-body POST that takes a model_id path parameter.
	//
	// [U] cases prove URL construction and HTTP method correctness without a live cluster.
	// [I] cases require an actual model id; because registering + loading a real model is a
	// multi-step asynchronous process that is out of scope for a unit/integration smoke test,
	// we use a synthetic id for [U] and accept an error status for the [I] path
	// (the operation is invoked, the generated plumbing is exercised end-to-end).
	// The synthetic id will not match any model, so the response is an error; the exact status
	// code varies by server version and cluster state (404 when the .plugins-ml-model index
	// exists but the id is unknown, 500 with IndexNotFoundException on a fresh cluster where the
	// index has not been created yet), so we assert on the error *family* rather than a single code.
	//
	// The ml.deploy_model endpoint (/_plugins/_ml/models/{id}/_deploy) stabilised alongside the
	// model-management surface in ml-commons 2.8.0; below that the URL returns "no handler found"
	// (400), which is outside the 404/500 error family asserted here. SkipVersion suppresses only
	// the [I] cases below 2.8.0 — the [U] cases run on every version.
	[SkipVersion("<2.8.0", "ml-commons deploy_model endpoint stabilised in 2.8.0")]
	public class DeployModelApiTests
		: ApiIntegrationTestBase<WritableCluster, DeployModelResponse, IDeployModelRequest,
			DeployModelDescriptor, DeployModelRequest>
	{
		// A stable, URL-safe id used in both [U] and [I] paths
		private static readonly Id SyntheticModelId = new Id("osnet-deploy-test-model");

		public DeployModelApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		// [I]: the model does not exist, so the call returns an error.
		protected override bool ExpectIsValid => false;

		// No request body for this POST.
		protected override object ExpectJson => null;

		// Not asserted directly; ReturnsExpectedStatusCode is overridden below to accept the
		// version-dependent error family (404 or 500). Kept as a nominal value for base wiring.
		protected override int ExpectStatusCode => 404;

		// Override the exact-code assertion: a missing model surfaces as 404 (id not found) or
		// 500 (IndexNotFoundException on a fresh cluster). Both are valid server rejections that
		// prove the generated call reached ml-commons; asserting a single code is brittle across
		// the OpenSearch versions the CI matrix runs (1.1.0 -> 3.6.0).
		[I] public override async Task ReturnsExpectedStatusCode() =>
			await AssertOnAllResponses(r =>
				r.ApiCall.HttpStatusCode.Should().BeOneOf(new[] { 404, 500 },
					"a deploy of a non-existent model is rejected as 404 (unknown id) or 500 " +
					"(IndexNotFoundException on a fresh cluster) depending on server version/state"));

		protected override Func<DeployModelDescriptor, IDeployModelRequest> Fluent => d => d;

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override DeployModelRequest Initializer => new(SyntheticModelId);

		protected override bool SupportsDeserialization => false;

		protected override string UrlPath => $"/_plugins/_ml/models/{SyntheticModelId}/_deploy";

		protected override DeployModelDescriptor NewDescriptor() =>
			new(modelId: SyntheticModelId);

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Ml.DeployModel(SyntheticModelId, f),
			(client, f) => client.Ml.DeployModelAsync(SyntheticModelId, f),
			(client, r) => client.Ml.DeployModel(r),
			(client, r) => client.Ml.DeployModelAsync(r)
		);
	}
}
