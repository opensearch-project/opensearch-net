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

using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Document.Single.Get;

public class GetUrlTests
{
    [U]
    public async Task Urls()
    {
        await GET("/project/_doc/1")
                .Fluent(c => c.Get<Project>(1))
                .Request(c => c.Get<Project>(new GetRequest<Project>(1)))
                .FluentAsync(c => c.GetAsync<Project>(1))
                .RequestAsync(c => c.GetAsync<Project>(new GetRequest<Project>(1)))
            ;

        await GET("/testindex/_doc/1")
                .Fluent(c => c.Get<Project>(1, g => g.Index("testindex")))
                .Request(c => c.Get<Project>(new GetRequest<Project>("testindex", 1)))
                .FluentAsync(c => c.GetAsync<Project>(1, g => g.Index("testindex")))
                .RequestAsync(c => c.GetAsync<Project>(new GetRequest<Project>("testindex", 1)))
            ;

        var urlId = "http://id.mynamespace/metainformation?x=y,2";
        var escaped = "http%3A%2F%2Fid.mynamespace%2Fmetainformation%3Fx%3Dy%2C2";
        await GET($"/project/_doc/{escaped}")
                .Fluent(c => c.Get<Project>(urlId))
                .Request(c => c.Get<Project>(new GetRequest<Project>(urlId)))
                .FluentAsync(c => c.GetAsync<Project>(urlId))
                .RequestAsync(c => c.GetAsync<Project>(new GetRequest<Project>(urlId)))
            ;

        await GET($"/project/_doc/{escaped}?routing={escaped}")
                .Fluent(c => c.Get<Project>(urlId, s => s.Routing(urlId)))
                .Request(c => c.Get<Project>(new GetRequest<Project>(urlId) { Routing = urlId }))
                .FluentAsync(c => c.GetAsync<Project>(urlId, s => s.Routing(urlId)))
                .RequestAsync(c => c.GetAsync<Project>(new GetRequest<Project>(urlId) { Routing = urlId }))
            ;

        GET($"/project/_doc/{escaped}?routing={escaped}")
            .LowLevel(c => c.Get<DynamicResponse>("project", urlId, new GetRequestParameters
            {
                Routing = urlId
            })
            );
    }
}
