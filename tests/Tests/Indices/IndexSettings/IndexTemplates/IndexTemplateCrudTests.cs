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
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexSettings.IndexTemplates;

using OpenSearch.Client;

public class IndexTemplateCrudTests
    : CrudTestBase<WritableCluster, PutIndexTemplateResponse, GetIndexTemplateResponse, PutIndexTemplateResponse, DeleteIndexTemplateResponse,
        ExistsResponse>
{
    public IndexTemplateCrudTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override LazyResponses Exists() =>
        Calls<IndexTemplateExistsDescriptor, IndexTemplateExistsRequest, IIndexTemplateExistsRequest, ExistsResponse>(
            id => new IndexTemplateExistsRequest(id),
            (id, d) => d,
            (s, c, f) => c.Indices.TemplateExists(s, f),
            (s, c, f) => c.Indices.TemplateExistsAsync(s, f),
            (s, c, r) => c.Indices.TemplateExists(r),
            (s, c, r) => c.Indices.TemplateExistsAsync(r)
        );

    protected override LazyResponses Create() =>
        Calls<PutIndexTemplateDescriptor, PutIndexTemplateRequest, IPutIndexTemplateRequest, PutIndexTemplateResponse>(
            CreateInitializer,
            CreateFluent,
            (s, c, f) => c.Indices.PutTemplate(s, f),
            (s, c, f) => c.Indices.PutTemplateAsync(s, f),
            (s, c, r) => c.Indices.PutTemplate(r),
            (s, c, r) => c.Indices.PutTemplateAsync(r)
        );

    private PutIndexTemplateRequest CreateInitializer(string name) => new(name)
    {
        IndexPatterns = new[] { "startingwiththis-*" },
        Settings = new IndexSettings
        {
            NumberOfShards = 2
        }
    };

    private IPutIndexTemplateRequest CreateFluent(string name, PutIndexTemplateDescriptor d) => d
        .IndexPatterns("startingwiththis-*")
        .Settings(s => s
            .NumberOfShards(2)
        );

    protected override LazyResponses Read() =>
        Calls<GetIndexTemplateDescriptor, GetIndexTemplateRequest, IGetIndexTemplateRequest, GetIndexTemplateResponse>(
            name => new GetIndexTemplateRequest(name),
            (name, d) => d.Name(name),
            (s, c, f) => c.Indices.GetTemplate(s, f),
            (s, c, f) => c.Indices.GetTemplateAsync(s, f),
            (s, c, r) => c.Indices.GetTemplate(r),
            (s, c, r) => c.Indices.GetTemplateAsync(r)
        );

    protected override void ExpectAfterCreate(GetIndexTemplateResponse response)
    {
        response.TemplateMappings.Should().NotBeNull().And.HaveCount(1);
        var templateMapping = response.TemplateMappings.First().Value;
        templateMapping.IndexPatterns.Should().NotBeNullOrEmpty().And.Contain(t => t.StartsWith("startingwith"));
        templateMapping.Settings.Should().NotBeNull().And.NotBeEmpty();
        templateMapping.Settings.NumberOfShards.Should().Be(2);
    }

    protected override LazyResponses Update() =>
        Calls<PutIndexTemplateDescriptor, PutIndexTemplateRequest, IPutIndexTemplateRequest, PutIndexTemplateResponse>(
            PutInitializer,
            PutFluent,
            (s, c, f) => c.Indices.PutTemplate(s, f),
            (s, c, f) => c.Indices.PutTemplateAsync(s, f),
            (s, c, r) => c.Indices.PutTemplate(r),
            (s, c, r) => c.Indices.PutTemplateAsync(r)
        );

    private PutIndexTemplateRequest PutInitializer(string name) => new(name)
    {
        IndexPatterns = new[] { "startingwiththis-*" },
        Settings = new IndexSettings
        {
            NumberOfShards = 1
        }
    };

    private IPutIndexTemplateRequest PutFluent(string name, PutIndexTemplateDescriptor d) => d
        .IndexPatterns("startingwiththis-*")
        .Settings(s => s
            .NumberOfShards(1)
        );

    protected override void ExpectAfterUpdate(GetIndexTemplateResponse response)
    {
        response.TemplateMappings.Should().NotBeNull().And.HaveCount(1);
        var templateMapping = response.TemplateMappings.First().Value;
        templateMapping.IndexPatterns.Should().NotBeNullOrEmpty().And.Contain(t => t.StartsWith("startingwith"));
        templateMapping.Settings.Should().NotBeNull().And.NotBeEmpty();
        templateMapping.Settings.NumberOfShards.Should().Be(1);
    }

    protected override LazyResponses Delete() =>
        Calls<DeleteIndexTemplateDescriptor, DeleteIndexTemplateRequest, IDeleteIndexTemplateRequest, DeleteIndexTemplateResponse>(
            name => new DeleteIndexTemplateRequest(name),
            (name, d) => d,
            (s, c, f) => c.Indices.DeleteTemplate(s, f),
            (s, c, f) => c.Indices.DeleteTemplateAsync(s, f),
            (s, c, r) => c.Indices.DeleteTemplate(r),
            (s, c, r) => c.Indices.DeleteTemplateAsync(r)
        );

    protected override async Task GetAfterDeleteIsValid() => await AssertOnGetAfterDelete(r => r.ShouldNotBeValid());

    protected override void ExpectDeleteNotFoundResponse(DeleteIndexTemplateResponse response)
    {
        response.ServerError.Should().NotBeNull();
        response.ServerError.Status.Should().Be(404);
        response.ServerError.Error.Reason.Should().Contain("missing");
    }
}
