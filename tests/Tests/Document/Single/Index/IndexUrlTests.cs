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
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Document.Single.Index;

public class IndexUrlTests
{
    [U]
    public async Task Urls()
    {
        var project = new Project { Name = "OSC" };

        await POST("/project/_doc")
            .Fluent(c => c.Index(project, i => i.Id(null)))
            .Request(c => c.Index(new IndexRequest<Project>(index: "project") { Document = project }))
            .FluentAsync(c => c.IndexAsync(project, i => i.Id(null)))
            .RequestAsync(c => c.IndexAsync(new IndexRequest<Project>(typeof(Project))
            {
                Document = project
            }));

        //no explicit ID is provided and none can be inferred on the anonymous object so this falls back to a POST to /index/type
        await POST("/project/_doc")
            .Fluent(c => c.Index(new { }, i => i.Index(typeof(Project))))
            .Request(c => c.Index(new IndexRequest<object>(index: "project") { Document = new { } }))
            .FluentAsync(c => c.IndexAsync(new { }, i => i.Index(typeof(Project))))
            .RequestAsync(c => c.IndexAsync(new IndexRequest<object>(typeof(Project))
            {
                Document = new { }
            }));

        await PUT("/project/_doc/OSC")
            .Fluent(c => c.IndexDocument(project))
            .Request(c => c.Index(new IndexRequest<Project>(index: "project", id: "OSC") { Document = project }))
            .Request(c => c.Index(new IndexRequest<Project>(project)))
            .FluentAsync(c => c.IndexDocumentAsync(project))
            .RequestAsync(c => c.IndexAsync(new IndexRequest<Project>(project)));
    }

    [U]
    public async Task LowLevelUrls()
    {
        var project = new Project { Name = "OSC" };

        await POST("/index/_doc")
            .LowLevel(c => c.Index<VoidResponse>("index", PostData.Empty))
            .LowLevelAsync(c => c.IndexAsync<VoidResponse>("index", PostData.Empty));

        await PUT("/index/_doc/id")
            .LowLevel(c => c.Index<VoidResponse>("index", "id", PostData.Empty))
            .LowLevelAsync(c => c.IndexAsync<VoidResponse>("index", "id", PostData.Empty));

    }

    [U]
    public async Task CanIndexUrlIds()
    {
        var id = "http://my.local/id?qwe=2";
        var escaped = Uri.EscapeDataString(id);
        escaped.Should().NotContain("/").And.NotContain("?");
        var project = new Project { Name = "name" };

        await PUT($"/project/_doc/{escaped}")
            .Fluent(c => c.Index(project, i => i.Id(id)))
            .Request(c => c.Index(new IndexRequest<Project>("project", id) { Document = project }))
            .FluentAsync(c => c.IndexAsync(project, i => i.Id(id)))
            .RequestAsync(c => c.IndexAsync(new IndexRequest<Project>(typeof(Project), id)
            {
                Document = project
            }));

        project = new Project { Name = id };
        await PUT($"/project/_doc/{escaped}")
            .Fluent(c => c.Index(project, i => i.Id(id)))
            .Request(c => c.Index(new IndexRequest<Project>("project", id) { Document = project }))
            .FluentAsync(c => c.IndexAsync(project, i => i.Id(id)))
            .RequestAsync(c => c.IndexAsync(new IndexRequest<Project>(typeof(Project), id)
            {
                Document = project
            }));
    }
}
