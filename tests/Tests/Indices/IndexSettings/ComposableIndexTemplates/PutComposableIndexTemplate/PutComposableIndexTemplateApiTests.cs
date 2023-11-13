/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexSettings.ComposableIndexTemplates.PutComposableIndexTemplate;

public class PutComposableIndexTemplateApiTests
	: ApiIntegrationTestBase<WritableCluster, PutComposableIndexTemplateResponse, IPutComposableIndexTemplateRequest,
		PutComposableIndexTemplateDescriptor, PutComposableIndexTemplateRequest>
{
	public PutComposableIndexTemplateApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

	protected override bool ExpectIsValid => true;

	protected override object ExpectJson => new
	{
		priority = 1,
		version = 2,
		index_patterns = new[] { "oscx-*" },
		composed_of = new[] { $"component_{CallIsolatedValue}" },
		template = new
		{
			settings = new Dictionary<string, object> { { "index.number_of_shards", 1 } },
			mappings = new
			{
				dynamic_templates = new object[]
				{
					new
					{
						@base = new
						{
							match = "*",
							match_mapping_type = "*",
							mapping = new
							{
								index = false
							}
						}
					}
				}
			}
		}
	};

	protected override int ExpectStatusCode => 200;

	protected override Func<PutComposableIndexTemplateDescriptor, IPutComposableIndexTemplateRequest> Fluent => d => d
		.Create(false)
		.Priority(1)
		.Version(2)
		.IndexPatterns("oscx-*")
		.ComposedOf($"component_{CallIsolatedValue}")
		.Template(t => t
			.Settings(p => p.NumberOfShards(1))
			.Map(tm => tm
				.DynamicTemplates(dts => dts
					.DynamicTemplate("base", dt => dt
						.Match("*")
						.MatchMappingType("*")
						.Mapping(mm => mm
							.Generic(g => g
								.Index(false)
							)
						)
					)
				)
			)
		);

	protected override HttpMethod HttpMethod => HttpMethod.PUT;

	protected override PutComposableIndexTemplateRequest Initializer => new(CallIsolatedValue)
	{
		Create = false,
		Priority = 1,
		Version = 2,
		IndexPatterns = new[] { "oscx-*" },
		ComposedOf = new[] { $"component_{CallIsolatedValue}" },
		Template = new Template {
			Settings = new OpenSearch.Client.IndexSettings
			{
				NumberOfShards = 1
			},
			Mappings = new TypeMapping
			{
				DynamicTemplates = new DynamicTemplateContainer
				{
					{ "base", new DynamicTemplate
					{
						Match = "*",
						MatchMappingType = "*",
						Mapping = new GenericProperty { Index = false }
					} }
				}
			}
		}
	};

	protected override bool SupportsDeserialization => false;
	protected override string UrlPath => $"/_index_template/{CallIsolatedValue}?create=false";

	protected override LazyResponses ClientUsage() => Calls(
		(client, f) => client.Indices.PutComposableTemplate(CallIsolatedValue, f),
		(client, f) => client.Indices.PutComposableTemplateAsync(CallIsolatedValue, f),
		(client, r) => client.Indices.PutComposableTemplate(r),
		(client, r) => client.Indices.PutComposableTemplateAsync(r)
	);

	protected override PutComposableIndexTemplateDescriptor NewDescriptor() => new(CallIsolatedValue);

	protected override void ExpectResponse(PutComposableIndexTemplateResponse response)
	{
		response.ShouldBeValid();
		response.Acknowledged.Should().BeTrue();
	}

	protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
	{
		foreach (var value in values.Values)
		{
			var putComponentResponse = client.Cluster.PutComponentTemplate($"component_{value}", d => d
				.Template(t => t
					.Settings(s => s
						.NumberOfReplicas(0))));

			if (!putComponentResponse.IsValid)
				throw new Exception($"Problem putting component template for integration test: {putComponentResponse.DebugInformation}");
		}
	}
}
