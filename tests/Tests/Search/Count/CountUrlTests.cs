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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using static Tests.Framework.EndpointTests.UrlTester;

namespace Tests.Search.Count;

public class CountUrlTests
{
    [U]
    public async Task Urls()
    {
        var hardcoded = "hardcoded";
        await GET("/devs/_count")
                .Fluent(c => c.Count<Developer>())
                .Request(c => c.Count(new CountRequest<Developer>()))
                .FluentAsync(c => c.CountAsync<Developer>())
                .RequestAsync(c => c.CountAsync(new CountRequest<Developer>()))
            ;

        await GET("/devs/_count?q=querystring")
                .Fluent(c => c.Count<Developer>(s => s.QueryOnQueryString("querystring")))
                .Request(c => c.Count(new CountRequest<Developer>() { QueryOnQueryString = "querystring" }))
                .FluentAsync(c => c.CountAsync<Developer>(s => s.QueryOnQueryString("querystring")))
                .RequestAsync(c => c.CountAsync(new CountRequest<Developer>() { QueryOnQueryString = "querystring" }))
            ;
        await GET($"/devs/_count")
                .Fluent(c => c.Count<Developer>(s => s))
                .Request(c => c.Count(new CountRequest<Developer>()))
                .FluentAsync(c => c.CountAsync<Developer>(s => s))
                .RequestAsync(c => c.CountAsync(new CountRequest<Developer>()))
            ;

        await GET("/project/_count")
                .Fluent(c => c.Count<Project>(s => s))
                .Fluent(c => c.Count<Project>(s => s))
                .Request(c => c.Count(new CountRequest("project")))
                .Request(c => c.Count(new CountRequest<Project>("project")))
                .FluentAsync(c => c.CountAsync<Project>(s => s))
                .RequestAsync(c => c.CountAsync(new CountRequest<Project>(typeof(Project))))
                .FluentAsync(c => c.CountAsync<Project>(s => s))
            ;

        await GET($"/{hardcoded}/_count")
                .Fluent(c => c.Count<Project>(s => s.Index(hardcoded)))
                .Fluent(c => c.Count<Project>(s => s.Index(hardcoded)))
                .Request(c => c.Count(new CountRequest(hardcoded)))
                .Request(c => c.Count(new CountRequest<Project>(hardcoded)))
                .FluentAsync(c => c.CountAsync<Project>(s => s.Index(hardcoded)))
                .RequestAsync(c => c.CountAsync(new CountRequest<Project>(hardcoded)))
                .FluentAsync(c => c.CountAsync<Project>(s => s.Index(hardcoded)))
            ;

        await GET("/_count")
                .Request(c => c.Count(new CountRequest()))
                .RequestAsync(c => c.CountAsync(new CountRequest()))
            ;

        await POST("/_all/_count")
                .Fluent(c => c.Count<Project>(s => s.AllIndices().Query(q => q.MatchAll())))
                .Request(c => c.Count(new CountRequest<Project>(OpenSearch.Client.Indices.All) { Query = new MatchAllQuery() }))
                .FluentAsync(c => c.CountAsync<Project>(s => s.AllIndices().Query(q => q.MatchAll())))
                .RequestAsync(c => c.CountAsync(new CountRequest<Project>(OpenSearch.Client.Indices.All) { Query = new MatchAllQuery() }))
            ;
    }
}
