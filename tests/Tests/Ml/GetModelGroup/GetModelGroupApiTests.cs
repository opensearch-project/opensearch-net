/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Ml.GetModelGroup
{
	// Exercises the generated ml.get_model_group operation end-to-end.
	// [U] cases (URL, HTTP method, request serialization) run in the default suite;
	// [I] cases run only under RunIntegrationTests.
	//
	// get_model_group is a no-body GET.  ExpectJson returns null (base class default)
	// because there is nothing to serialize as a request body.
	//
	// For the [I] path we create the group in IntegrationSetup and capture its id via
	// ExtendedValue so the GET resolves to an existing resource.
	//
	// The model_groups register API arrived in ml-commons 2.8.0, but GET on model_groups/{id} was
	// added later: on 2.8.0-2.10.0 that path only supports DELETE/PUT and a GET returns 405
	// ("Incorrect HTTP method ... allowed: [DELETE, PUT]"). The GET handler is present from 2.12.0.
	// SkipVersion suppresses only the [I] cases below 2.12.0 — the [U] cases still run on every version.
	[SkipVersion("<2.12.0", "GET on ml-commons model_groups/{id} was added in 2.12.0 (405 on 2.8.0-2.10.0)")]
	public class GetModelGroupApiTests
		: ApiIntegrationTestBase<WritableCluster, GetModelGroupResponse, IGetModelGroupRequest,
			GetModelGroupDescriptor, GetModelGroupRequest>
	{
		public GetModelGroupApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		// For [U] tests we use the per-view stable value.
		// For [I] tests we use the id created in IntegrationSetup.
		private Id ModelGroupId =>
			RanIntegrationSetup
				? ExtendedValue<Id>("modelGroupId")
				: new Id(CallIsolatedValue);

		protected override bool ExpectIsValid => true;

		// No request body for this GET.
		protected override object ExpectJson => null;

		protected override int ExpectStatusCode => 200;

		protected override Func<GetModelGroupDescriptor, IGetModelGroupRequest> Fluent =>
			d => d;   // id is supplied via the path-param overload, not the body

		protected override HttpMethod HttpMethod => HttpMethod.GET;

		protected override GetModelGroupRequest Initializer => new(ModelGroupId);

		protected override bool SupportsDeserialization => false;

		protected override string UrlPath => $"/_plugins/_ml/model_groups/{ModelGroupId}";

		protected override GetModelGroupDescriptor NewDescriptor() =>
			new(ModelGroupId);

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Ml.GetModelGroup(ModelGroupId, f),
			(client, f) => client.Ml.GetModelGroupAsync(ModelGroupId, f),
			(client, r) => client.Ml.GetModelGroup(r),
			(client, r) => client.Ml.GetModelGroupAsync(r)
		);

		protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
		{
			// ExtendedValue writes into the per-view store keyed by values.CurrentView, and the
			// live [I] run exercises a single random view (random:test_only_one). We must register
			// a group and store its id under *every* view, otherwise the view the runner picks may
			// have no "modelGroupId" and get_ModelGroupId throws KeyNotFoundException. This mirrors
			// the per-view IntegrationSetup pattern in TasksCancelApiTests.
			foreach (var view in values.Views)
			{
				values.CurrentView = view;
				var groupName = $"osnet-getmg-{values.Value}";
				// access_mode omitted: ml-commons rejects access-control params with a 400 when the
				// Security plugin / model access control is disabled (the case on the test cluster).
				var reg = client.Ml.RegisterModelGroup(f => f
					.Name(groupName)
					.Description("GetModelGroup integration test group"));

				if (!reg.IsValid)
					throw new System.Exception(
						$"Failed to register model group for GetModelGroup test: {reg.DebugInformation}");

				// Store the returned id under this view so ModelGroupId can resolve it at call time.
				values.ExtendedValue("modelGroupId", new Id(reg.ModelGroupId));
			}
		}

		protected override void ExpectResponse(GetModelGroupResponse response)
		{
			response.Name.Should().NotBeNullOrEmpty();
		}
	}
}
