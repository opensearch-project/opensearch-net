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

namespace Tests.Cluster.ComponentTemplate;

public class PutComponentTemplateApiTests
    : ApiIntegrationTestBase<WritableCluster, PutComponentTemplateResponse, IPutComponentTemplateRequest, PutComponentTemplateDescriptor,
        PutComponentTemplateRequest>
{
    public PutComponentTemplateApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson { get; } = new
    {
        version = 2,
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

    protected override Func<PutComponentTemplateDescriptor, IPutComponentTemplateRequest> Fluent => d => d
        .Version(2)
        .Create(false)
        .Template(t => t
            .Settings(p => p.NumberOfShards(1))
            .Map(tm => tm
                .DynamicTemplates(dts => dts
                    .DynamicTemplate("base", dt => dt
                        .Match("*")
                        .MatchMappingType("*")
                        .Mapping(mm => mm
                            .Generic(g => g
                                .Index(false)))))));

    protected override HttpMethod HttpMethod => HttpMethod.PUT;

    protected override PutComponentTemplateRequest Initializer => new(CallIsolatedValue)
    {
        Version = 2,
        Create = false,
        Template = new Template
        {
            Settings = new IndexSettings
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
    protected override string UrlPath => $"/_component_template/{CallIsolatedValue}?create=false";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cluster.PutComponentTemplate(CallIsolatedValue, f),
        (client, f) => client.Cluster.PutComponentTemplateAsync(CallIsolatedValue, f),
        (client, r) => client.Cluster.PutComponentTemplate(r),
        (client, r) => client.Cluster.PutComponentTemplateAsync(r)
    );

    protected override PutComponentTemplateDescriptor NewDescriptor() => new(CallIsolatedValue);

    protected override void ExpectResponse(PutComponentTemplateResponse response)
    {
        response.ShouldBeValid();
        response.Acknowledged.Should().BeTrue();
    }
}
