/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Linq;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Client.Specification.IndicesApi;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexSettings.IndexTemplates.GetIndexTemplate
{
	public class GetIndexTemplateApiTests
		: ApiIntegrationTestBase<WritableCluster, GetIndexTemplateResponse, IGetIndexTemplateRequest, GetIndexTemplateDescriptor,
			GetIndexTemplateRequest>
	{
		public GetIndexTemplateApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override object ExpectJson => null;
		protected override int ExpectStatusCode => 200;
		protected override HttpMethod HttpMethod => HttpMethod.GET;
		protected override string UrlPath => $"/_template/{CallIsolatedValue}";
		
		protected override GetIndexTemplateRequest Initializer => new GetIndexTemplateRequest(CallIsolatedValue);

		protected override LazyResponses ClientUsage() => Calls(
			(client, f) => client.Indices.GetTemplate(CallIsolatedValue, f),
			(client, f) => client.Indices.GetTemplateAsync(CallIsolatedValue, f),
			(client, r) => client.Indices.GetTemplate(r),
			(client, r) => client.Indices.GetTemplateAsync(r)
		);

		protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
		{
			foreach (var callUniqueValue in values)
			{
				var putTemplateResponse = client.Indices.PutTemplate(callUniqueValue.Value, d =>
					d.IndexPatterns("startingwiththis-*")
						.Settings(s => s.NumberOfShards(2))
						.Version(1)
				);

				if (!putTemplateResponse.IsValid)
					throw new Exception($"Problem putting index template for integration test: {putTemplateResponse.DebugInformation}");
			}
		}

		protected override void ExpectResponse(GetIndexTemplateResponse response)
		{
			response.ShouldBeValid();

			response.TemplateMappings.Should().NotBeNull();
			response.TemplateMappings.Should().HaveCount(1);

			var responseTemplateMapping = response.TemplateMappings[CallIsolatedValue];

			responseTemplateMapping.IndexPatterns.Should().NotBeNull();
			responseTemplateMapping.IndexPatterns.Should().HaveCount(1);
			responseTemplateMapping.IndexPatterns.First().Should().Be("startingwiththis-*");

			responseTemplateMapping.Version.Should().Be(1);

			responseTemplateMapping.Settings.Should().NotBeNull();
			responseTemplateMapping.Settings.NumberOfShards.Should().Be(2);
		}
	}
}
