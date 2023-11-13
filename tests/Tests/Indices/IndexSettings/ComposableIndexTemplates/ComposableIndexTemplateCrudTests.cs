/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexSettings.ComposableIndexTemplates;

public class ComposableIndexTemplateCrudTests
	: CrudTestBase<WritableCluster, PutComposableIndexTemplateResponse, GetComposableIndexTemplateResponse, PutComposableIndexTemplateResponse,
		DeleteComposableIndexTemplateResponse, ExistsResponse>
{
	public ComposableIndexTemplateCrudTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	protected override LazyResponses Exists() =>
		Calls<ComposableIndexTemplateExistsDescriptor, ComposableIndexTemplateExistsRequest, IComposableIndexTemplateExistsRequest, ExistsResponse>(
			id => new ComposableIndexTemplateExistsRequest(id),
			(id, d) => d,
			(s, c, f) => c.Indices.ComposableTemplateExists(s, f),
			(s, c, f) => c.Indices.ComposableTemplateExistsAsync(s, f),
			(s, c, r) => c.Indices.ComposableTemplateExists(r),
			(s, c, r) => c.Indices.ComposableTemplateExistsAsync(r)
		);

	protected override LazyResponses Create() =>
		Calls<PutComposableIndexTemplateDescriptor, PutComposableIndexTemplateRequest, IPutComposableIndexTemplateRequest,
			PutComposableIndexTemplateResponse>(
			CreateInitializer,
			CreateFluent,
			(s, c, f) => c.Indices.PutComposableTemplate(s, f),
			(s, c, f) => c.Indices.PutComposableTemplateAsync(s, f),
			(s, c, r) => c.Indices.PutComposableTemplate(r),
			(s, c, r) => c.Indices.PutComposableTemplateAsync(r)
		);

	private PutComposableIndexTemplateRequest CreateInitializer(string name) => new(name)
	{
		IndexPatterns = new[] { "startingwiththis-*" },
		Template = new Template { Settings = new OpenSearch.Client.IndexSettings { NumberOfShards = 2 } }
	};

	private IPutComposableIndexTemplateRequest CreateFluent(string name, PutComposableIndexTemplateDescriptor d) => d
		.IndexPatterns("startingwiththis-*")
		.Template(t => t
			.Settings(s => s
				.NumberOfShards(2)
			)
		);

	protected override LazyResponses Read() =>
		Calls<GetComposableIndexTemplateDescriptor, GetComposableIndexTemplateRequest, IGetComposableIndexTemplateRequest,
			GetComposableIndexTemplateResponse>(
			name => new GetComposableIndexTemplateRequest(name),
			(name, d) => d.Name(name),
			(s, c, f) => c.Indices.GetComposableTemplate(s, f),
			(s, c, f) => c.Indices.GetComposableTemplateAsync(s, f),
			(s, c, r) => c.Indices.GetComposableTemplate(r),
			(s, c, r) => c.Indices.GetComposableTemplateAsync(r)
		);

	protected override void ExpectAfterCreate(GetComposableIndexTemplateResponse response)
	{
		response.IndexTemplates.Should().NotBeNull().And.HaveCount(1);
		var template = response.IndexTemplates.First().IndexTemplate;
		template.IndexPatterns.Should().NotBeNullOrEmpty().And.Contain(t => t.StartsWith("startingwith"));
		template.Template.Should().NotBeNull();
		template.Template.Settings.Should().NotBeNull().And.NotBeEmpty();
		template.Template.Settings.NumberOfShards.Should().Be(2);
	}

	protected override LazyResponses Update() =>
		Calls<PutComposableIndexTemplateDescriptor, PutComposableIndexTemplateRequest, IPutComposableIndexTemplateRequest,
			PutComposableIndexTemplateResponse>(
			PutInitializer,
			PutFluent,
			(s, c, f) => c.Indices.PutComposableTemplate(s, f),
			(s, c, f) => c.Indices.PutComposableTemplateAsync(s, f),
			(s, c, r) => c.Indices.PutComposableTemplate(r),
			(s, c, r) => c.Indices.PutComposableTemplateAsync(r)
		);

	private PutComposableIndexTemplateRequest PutInitializer(string name) => new(name)
	{
		IndexPatterns = new[] { "startingwiththis-*" },
		Template = new Template { Settings = new OpenSearch.Client.IndexSettings { NumberOfShards = 1 } }
	};

	private IPutComposableIndexTemplateRequest PutFluent(string name, PutComposableIndexTemplateDescriptor d) => d
		.IndexPatterns("startingwiththis-*")
		.Template(t => t
			.Settings(s => s
				.NumberOfShards(1)
			)
		);

	protected override void ExpectAfterUpdate(GetComposableIndexTemplateResponse response)
	{
		response.IndexTemplates.Should().NotBeNull().And.HaveCount(1);
		var template = response.IndexTemplates.First().IndexTemplate;
		template.IndexPatterns.Should().NotBeNullOrEmpty().And.Contain(t => t.StartsWith("startingwith"));
		template.Template.Should().NotBeNull();
		template.Template.Settings.Should().NotBeNull().And.NotBeEmpty();
		template.Template.Settings.NumberOfShards.Should().Be(1);
	}

	protected override LazyResponses Delete() =>
		Calls<DeleteComposableIndexTemplateDescriptor, DeleteComposableIndexTemplateRequest, IDeleteComposableIndexTemplateRequest,
			DeleteComposableIndexTemplateResponse>(
			name => new DeleteComposableIndexTemplateRequest(name),
			(name, d) => d,
			(s, c, f) => c.Indices.DeleteComposableTemplate(s, f),
			(s, c, f) => c.Indices.DeleteComposableTemplateAsync(s, f),
			(s, c, r) => c.Indices.DeleteComposableTemplate(r),
			(s, c, r) => c.Indices.DeleteComposableTemplateAsync(r)
		);

	protected override async Task GetAfterDeleteIsValid() => await AssertOnGetAfterDelete(r => r.ShouldNotBeValid());

	protected override void ExpectDeleteNotFoundResponse(DeleteComposableIndexTemplateResponse response)
	{
		response.ServerError.Should().NotBeNull();
		response.ServerError.Status.Should().Be(404);
		response.ServerError.Error.Reason.Should().Contain("missing");
	}
}
