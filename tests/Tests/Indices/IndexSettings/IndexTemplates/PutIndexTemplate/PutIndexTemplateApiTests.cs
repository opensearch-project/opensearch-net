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
using System.Collections.Generic;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexSettings.IndexTemplates.PutIndexTemplate;

using OpenSearch.Client;

public class PutIndexTemplateApiTests
    : ApiIntegrationTestBase<WritableCluster, PutIndexTemplateResponse, IPutIndexTemplateRequest, PutIndexTemplateDescriptor,
        PutIndexTemplateRequest>
{
    public PutIndexTemplateApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson { get; } = new
    {
        order = 1,
        version = 2,
        index_patterns = new[] { "oscx-*" },
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
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<PutIndexTemplateDescriptor, IPutIndexTemplateRequest> Fluent => d => d
        .Order(1)
        .Version(2)
        .IndexPatterns("oscx-*")
        .Create(false)
        .Settings(p => p.NumberOfShards(1))
        .Map(tm => tm
            .DynamicTemplates(t => t
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
    );

    protected override HttpMethod HttpMethod => HttpMethod.PUT;


    protected override PutIndexTemplateRequest Initializer => new(CallIsolatedValue)
    {
        Order = 1,
        Version = 2,
        IndexPatterns = new[] { "oscx-*" },
        Create = false,
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
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/_template/{CallIsolatedValue}?create=false";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.PutTemplate(CallIsolatedValue, f),
        (client, f) => client.Indices.PutTemplateAsync(CallIsolatedValue, f),
        (client, r) => client.Indices.PutTemplate(r),
        (client, r) => client.Indices.PutTemplateAsync(r)
    );

    protected override PutIndexTemplateDescriptor NewDescriptor() => new(CallIsolatedValue);

    protected override void ExpectResponse(PutIndexTemplateResponse response)
    {
        response.ShouldBeValid();
        response.Acknowledged.Should().BeTrue();
    }
}
