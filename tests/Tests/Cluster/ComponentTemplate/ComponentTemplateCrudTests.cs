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

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Client.Specification.ClusterApi;
using OpenSearch.Client.Specification.IndicesApi;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.ComponentTemplate;

public class ComponentTemplateCrudTests
	: CrudTestBase<WritableCluster, PutComponentTemplateResponse, GetComponentTemplateResponse, PutComponentTemplateResponse, DeleteComponentTemplateResponse,
		ExistsResponse>
{
	public ComponentTemplateCrudTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	protected override LazyResponses Exists() =>
		Calls<ComponentTemplateExistsDescriptor, ComponentTemplateExistsRequest, IComponentTemplateExistsRequest, ExistsResponse>(
			id => new ComponentTemplateExistsRequest(id),
			(id, d) => d,
			(s, c, f) => c.Cluster.ComponentTemplateExists(s, f),
			(s, c, f) => c.Cluster.ComponentTemplateExistsAsync(s, f),
			(s, c, r) => c.Cluster.ComponentTemplateExists(r),
			(s, c, r) => c.Cluster.ComponentTemplateExistsAsync(r)
		);

	protected override LazyResponses Create() =>
		Calls<PutComponentTemplateDescriptor, PutComponentTemplateRequest, IPutComponentTemplateRequest, PutComponentTemplateResponse>(
			CreateInitializer,
			CreateFluent,
			(s, c, f) => c.Cluster.PutComponentTemplate(s, f),
			(s, c, f) => c.Cluster.PutComponentTemplateAsync(s, f),
			(s, c, r) => c.Cluster.PutComponentTemplate(r),
			(s, c, r) => c.Cluster.PutComponentTemplateAsync(r)
		);

	private PutComponentTemplateRequest CreateInitializer(string name) => new(name)
	{
		Version = 42,
		Template = new Template
		{
			Settings = new IndexSettings
			{
				NumberOfShards = 2
			}
		}
	};

	private IPutComponentTemplateRequest CreateFluent(string name, PutComponentTemplateDescriptor d) => d
		.Version(42)
		.Template(t => t
			.Settings(s => s
				.NumberOfShards(2)));

	protected override LazyResponses Read() =>
		Calls<GetComponentTemplateDescriptor, GetComponentTemplateRequest, IGetComponentTemplateRequest, GetComponentTemplateResponse>(
			name => new GetComponentTemplateRequest(name),
			(name, d) => d.Name(name),
			(s, c, f) => c.Cluster.GetComponentTemplate(s, f),
			(s, c, f) => c.Cluster.GetComponentTemplateAsync(s, f),
			(s, c, r) => c.Cluster.GetComponentTemplate(r),
			(s, c, r) => c.Cluster.GetComponentTemplateAsync(r)
		);

	protected override void ExpectAfterCreate(GetComponentTemplateResponse response)
	{
		response.ComponentTemplates.Should().NotBeNull().And.HaveCount(1);
		var template = response.ComponentTemplates.First().ComponentTemplate;
		template.Version.Should().Be(42);
		template.Template.Should().NotBeNull();
		template.Template.Settings.Should().NotBeNull().And.NotBeEmpty();
		template.Template.Settings.NumberOfShards.Should().Be(2);
	}

	protected override LazyResponses Update() =>
		Calls<PutComponentTemplateDescriptor, PutComponentTemplateRequest, IPutComponentTemplateRequest, PutComponentTemplateResponse>(
			PutInitializer,
			PutFluent,
			(s, c, f) => c.Cluster.PutComponentTemplate(s, f),
			(s, c, f) => c.Cluster.PutComponentTemplateAsync(s, f),
			(s, c, r) => c.Cluster.PutComponentTemplate(r),
			(s, c, r) => c.Cluster.PutComponentTemplateAsync(r)
		);

	private PutComponentTemplateRequest PutInitializer(string name) => new(name)
	{
		Version = 84,
		Template = new Template
		{
			Settings = new IndexSettings
			{
				NumberOfShards = 1
			}
		}
	};

	private IPutComponentTemplateRequest PutFluent(string name, PutComponentTemplateDescriptor d) => d
		.Version(84)
		.Template(t => t
			.Settings(s => s
				.NumberOfShards(1)));

	protected override void ExpectAfterUpdate(GetComponentTemplateResponse response)
	{
		response.ComponentTemplates.Should().NotBeNull().And.HaveCount(1);
		var template = response.ComponentTemplates.First().ComponentTemplate;
		template.Version.Should().Be(84);
		template.Template.Should().NotBeNull();
		template.Template.Settings.Should().NotBeNull().And.NotBeEmpty();
		template.Template.Settings.NumberOfShards.Should().Be(1);
	}

	protected override LazyResponses Delete() =>
		Calls<DeleteComponentTemplateDescriptor, DeleteComponentTemplateRequest, IDeleteComponentTemplateRequest, DeleteComponentTemplateResponse>(
			name => new DeleteComponentTemplateRequest(name),
			(name, d) => d,
			(s, c, f) => c.Cluster.DeleteComponentTemplate(s, f),
			(s, c, f) => c.Cluster.DeleteComponentTemplateAsync(s, f),
			(s, c, r) => c.Cluster.DeleteComponentTemplate(r),
			(s, c, r) => c.Cluster.DeleteComponentTemplateAsync(r)
		);

	protected override async Task GetAfterDeleteIsValid() => await AssertOnGetAfterDelete(r => r.ShouldNotBeValid());

	protected override void ExpectDeleteNotFoundResponse(DeleteComponentTemplateResponse response)
	{
		response.ServerError.Should().NotBeNull();
		response.ServerError.Status.Should().Be(404);
		response.ServerError.Error.Reason.Should().Contain("missing");
	}
}
