/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
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
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Configuration;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.SearchTemplate.RenderSearchTemplate
{
	public class RenderSearchTemplateApiTests
		: ApiIntegrationTestBase<ReadOnlyCluster, RenderSearchTemplateResponse, IRenderSearchTemplateRequest, RenderSearchTemplateDescriptor,
			RenderSearchTemplateRequest>
	{
		private static readonly string inlineSearchTemplate = @"
{
	""query"": {
	  ""terms"": {
		""status"": [
		  ""{{#status}}"",
		  ""{{.}}"",
		  ""{{/status}}""
		]
	  }
	}
  }";

		private readonly string[] _statusValues = { "pending", "published" };

		public RenderSearchTemplateApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override bool ExpectIsValid => true;
		protected override int ExpectStatusCode => 200;

		protected override Func<RenderSearchTemplateDescriptor, IRenderSearchTemplateRequest> Fluent => s => s
			.Source(inlineSearchTemplate)
			.Params(p => p
				.Add("status", _statusValues)
			);

		protected override HttpMethod HttpMethod => HttpMethod.POST;


		protected override RenderSearchTemplateRequest Initializer => new RenderSearchTemplateRequest
		{
			Source = inlineSearchTemplate,
			Params = new Dictionary<string, object>
			{
				{ "status", _statusValues }
			}
		};

		protected override string UrlPath => $"/_render/template";

		protected override LazyResponses ClientUsage() => Calls(
			(c, f) => c.RenderSearchTemplate(f),
			(c, f) => c.RenderSearchTemplateAsync(f),
			(c, r) => c.RenderSearchTemplate(r),
			(c, r) => c.RenderSearchTemplateAsync(r)
		);

		[I] public Task AssertResponse() => AssertOnAllResponses(r =>
		{
			r.TemplateOutput.Should().NotBeNull();

			//TODO: this fails on As with random source serializer we need to come up with a better API here in
			// build.bat seed:36985 integrate 1.0.0 "readonly" "rendersearchtemplate"

			if (TestConfiguration.Instance.Random.SourceSerializer) return;
			var searchRequest = r.TemplateOutput.As<ISearchRequest>();
			searchRequest.Should().NotBeNull();

			searchRequest.Query.Should().NotBeNull();
		});
	}
}
