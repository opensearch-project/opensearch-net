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
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexSettings.ComposableIndexTemplates.GetComposableIndexTemplate;

public class GetComposableIndexTemplateApiTests
    : ApiIntegrationTestBase<WritableCluster, GetComposableIndexTemplateResponse, IGetComposableIndexTemplateRequest,
        GetComposableIndexTemplateDescriptor, GetComposableIndexTemplateRequest>
{
    public GetComposableIndexTemplateApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override object ExpectJson => null;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.GET;
    protected override string UrlPath => $"/_index_template/{CallIsolatedValue}";

    protected override GetComposableIndexTemplateRequest Initializer => new(CallIsolatedValue);

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.GetComposableTemplate(CallIsolatedValue, f),
        (client, f) => client.Indices.GetComposableTemplateAsync(CallIsolatedValue, f),
        (client, r) => client.Indices.GetComposableTemplate(r),
        (client, r) => client.Indices.GetComposableTemplateAsync(r)
    );

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var value in values.Values)
        {
            var putTemplateResponse = client.Indices.PutComposableTemplate(value, d => d
                .IndexPatterns($"{value}-*")
                .Version(1)
                .Template(t => t
                    .Settings(s => s.NumberOfShards(2))));

            if (!putTemplateResponse.IsValid)
                throw new Exception($"Problem putting index template for integration test: {putTemplateResponse.DebugInformation}");
        }
    }

    protected override void IntegrationTeardown(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var value in values.Values) client.Indices.DeleteComposableTemplate(value);
    }

    protected override void ExpectResponse(GetComposableIndexTemplateResponse response)
    {
        response.ShouldBeValid();

        response.IndexTemplates.Should().NotBeNull().And.HaveCount(1);

        var namedTemplate = response.IndexTemplates.First();
        namedTemplate.Name.Should().Be(CallIsolatedValue);
        var template = namedTemplate.IndexTemplate;

        template.Should().NotBeNull();
        template.IndexPatterns.Should().NotBeNull().And.HaveCount(1);
        template.IndexPatterns.First().Should().Be($"{CallIsolatedValue}-*");

        template.Version.Should().Be(1);

        template.Template.Should().NotBeNull();
        template.Template.Settings.Should().NotBeNull();
        template.Template.Settings.NumberOfShards.Should().Be(2);
    }
}
