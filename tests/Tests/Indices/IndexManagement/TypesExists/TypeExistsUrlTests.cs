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

namespace Tests.Indices.IndexManagement.TypesExists;

public class TypeExistsUrlTests
{
    [U]
    public async Task Urls()
    {
        var indices = OpenSearch.Client.Indices.Index<Project>().And<Developer>();
        var index = "project%2Cdevs";
        var types = "_doc";
        var type = "_doc";
        await UrlTester.HEAD($"/{index}/_mapping/{type}")
                .Fluent(c => c.Indices.TypeExists(indices, types))
                .Request(c => c.Indices.TypeExists(new TypeExistsRequest(indices, types)))
                .FluentAsync(c => c.Indices.TypeExistsAsync(indices, types))
                .RequestAsync(c => c.Indices.TypeExistsAsync(new TypeExistsRequest(indices, types)))
            ;
    }
}
