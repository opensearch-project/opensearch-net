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
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using static OpenSearch.Client.Infer;

namespace Tests.Search.MultiSearch.MultiSearchTemplate;

public class MultiSearchTemplateApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, MultiSearchResponse, IMultiSearchTemplateRequest, MultiSearchTemplateDescriptor,
        MultiSearchTemplateRequest>
{
    public MultiSearchTemplateApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;

    protected override object ExpectJson => new object[]
    {
        new { },
        new { @params = new { state = "Stable" }, source = "{\"query\": {\"match\":  {\"state\" : \"{{state}}\" }}}" },
        new { index = "devs" },
        new { id = "template-id" },
        new { index = "devs" },
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<MultiSearchTemplateDescriptor, IMultiSearchTemplateRequest> Fluent => ms => ms
        .Index(typeof(Project))
        .Template<Project>("inline", s => s
            .Source("{\"query\": {\"match\":  {\"state\" : \"{{state}}\" }}}")
            .Params(p => p
                .Add("state", "Stable")
            )
        )
        .Template<Project>("id", s => s.Index("devs").Id("template-id"));

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override MultiSearchTemplateRequest Initializer => new MultiSearchTemplateRequest(typeof(Project))
    {
        Operations = new Dictionary<string, ISearchTemplateRequest>
        {
            {
                "inline", new SearchTemplateRequest<Project>(typeof(Project))
                {
                    Source = "{\"query\": {\"match\":  {\"state\" : \"{{state}}\" }}}",
                    Params = new Dictionary<string, object>
                    {
                        { "state", "Stable" }
                    }
                }
            },
            { "id", new SearchTemplateRequest<Project>("devs") { Id = "template-id" } },
        }
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => "/project/_msearch/template";

    protected override LazyResponses ClientUsage() => Calls(
        (c, f) => c.MultiSearchTemplate(Index<Project>(), f),
        (c, f) => c.MultiSearchTemplateAsync(Index<Project>(), f),
        (c, r) => c.MultiSearchTemplate(r),
        (c, r) => c.MultiSearchTemplateAsync(r)
    );

    protected override void ExpectResponse(MultiSearchResponse response)
    {
        var inline = response.GetResponse<Project>("inline");
        inline.Should().NotBeNull();
        inline.ShouldBeValid();
        inline.Hits.Count().Should().BeGreaterThan(0);

        var id = response.GetResponse<Project>("id");
        id.Should().NotBeNull();
        id.ShouldNotBeValid();
    }
}
