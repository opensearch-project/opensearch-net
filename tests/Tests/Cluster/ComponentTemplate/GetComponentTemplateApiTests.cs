/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Client.Specification.ClusterApi;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.ComponentTemplate
{
	public class GetComponentTemplateApiTests
		: ApiIntegrationTestBase<WritableCluster, GetComponentTemplateResponse, IGetComponentTemplateRequest, GetComponentTemplateDescriptor,
			GetComponentTemplateRequest>
	{
		public GetComponentTemplateApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override string UrlPath => $"/_component_template/{CallIsolatedValue}";

		protected override GetComponentTemplateRequest Initializer => new GetComponentTemplateRequest(CallIsolatedValue);

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Cluster.GetComponentTemplate(CallIsolatedValue, f),
			(client, f) => client.Cluster.GetComponentTemplateAsync(CallIsolatedValue, f),
			(client, r) => client.Cluster.GetComponentTemplate(r),
			(client, r) => client.Cluster.GetComponentTemplateAsync(r)
		);

		protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				var putTemplateResponse = client.Cluster.PutComponentTemplate(callUniqueValue.Value, d => d
					.Template(t => t
						.Settings(s => s.NumberOfShards(2)))
					.Version(1)
				);

				if (!putTemplateResponse.IsValid)
					throw new Exception($"Problem putting index template for integration test: {putTemplateResponse.DebugInformation}");
			}
		}

		protected override void ExpectResponse(GetComponentTemplateResponse response)
		{
			response.ShouldBeValid();

			response.ComponentTemplates.Should().NotBeNull();
			response.ComponentTemplates.Should().HaveCount(1);

			var componentTemplate = response.ComponentTemplates.First(t => t.Name == CallIsolatedValue).ComponentTemplate;

			componentTemplate.Version.Should().Be(1);

			componentTemplate.Template.Settings.Should().NotBeNull();
			componentTemplate.Template.Settings.NumberOfShards.Should().Be(2);
		}
	}
}
