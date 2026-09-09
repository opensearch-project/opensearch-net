/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.ML.RegisterModelGroup
{
	// Exercises the generated high-level ml.register_model_group operation end-to-end:
	// the [U] cases (URL, HTTP method, request serialization) run in the default suite and
	// prove the generated request/descriptor/client-method wiring; the [I] cases run against
	// an ml-commons-enabled cluster (WritableCluster installs the MachineLearning plugin).
	//
	// The model_groups (model access control) API was introduced in ml-commons 2.8.0; on older
	// clusters the endpoint returns "no handler found" (400). SkipVersion suppresses only the [I]
	// cases below 2.8.0 — the [U] serialization/URL cases still run on every version.
	[SkipVersion("<2.8.0", "ml-commons model_groups (model access control) API was added in 2.8.0")]
	public class RegisterModelGroupApiTests
		: ApiIntegrationTestBase<WritableCluster, RegisterModelGroupResponse, IRegisterModelGroupRequest,
			RegisterModelGroupDescriptor, RegisterModelGroupRequest>
	{
		public RegisterModelGroupApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		// A distinct group name per test run keeps the [I] path idempotent across reruns.
		private string GroupName => $"osnet-{CallIsolatedValue}";

		protected override bool ExpectIsValid => true;

		// NOTE: access_mode is intentionally omitted. ml-commons rejects model-access-control
		// parameters (access_mode/backend_roles/add_all_backend_roles) with a 400 when the
		// Security plugin / model access control is disabled, which is the case on the test
		// cluster. name + description are always accepted.
		protected override object ExpectJson => new
		{
			name = GroupName,
			description = "OpenSearch.NET generated-client integration test group",
		};

		protected override int ExpectStatusCode => 200;

		protected override Func<RegisterModelGroupDescriptor, IRegisterModelGroupRequest> Fluent => d => d
			.Name(GroupName)
			.Description("OpenSearch.NET generated-client integration test group");

		protected override HttpMethod HttpMethod => HttpMethod.POST;

		protected override RegisterModelGroupRequest Initializer => new()
		{
			Name = GroupName,
			Description = "OpenSearch.NET generated-client integration test group",
		};

		protected override bool SupportsDeserialization => false;
		protected override string UrlPath => "/_plugins/_ml/model_groups/_register";

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Ml.RegisterModelGroup(f),
			(client, f) => client.Ml.RegisterModelGroupAsync(f),
			(client, r) => client.Ml.RegisterModelGroup(r),
			(client, r) => client.Ml.RegisterModelGroupAsync(r)
		);

		protected override void ExpectResponse(RegisterModelGroupResponse response)
		{
			response.ModelGroupId.Should().NotBeNullOrEmpty();
			response.OperationStatus.Should().NotBeNullOrEmpty();
		}
	}
}
