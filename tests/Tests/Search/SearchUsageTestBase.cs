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
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search;

public abstract class SearchUsageTestBase
    : ApiIntegrationTestBase<ReadOnlyCluster, ISearchResponse<Project>, ISearchRequest, SearchDescriptor<Project>, SearchRequest<Project>>
{
    protected TermQuery ProjectFilter = new TermQuery
    {
        Field = Infer.Field<Project>(p => p.Type),
        Value = Project.TypeName
    };

    protected object ProjectFilterExpectedJson = new { term = new { type = new { value = Project.TypeName } } };

    protected SearchUsageTestBase(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.POST;
    protected override string UrlPath => "/project/_search";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Search(f),
        (client, f) => client.SearchAsync(f),
        (client, r) => client.Search<Project>(r),
        (client, r) => client.SearchAsync<Project>(r)
    );

    // https://youtrack.jetbrains.com/issue/RIDER-19912
    [U] protected override Task HitsTheCorrectUrl() => base.HitsTheCorrectUrl();

    [U] protected override Task UsesCorrectHttpMethod() => base.UsesCorrectHttpMethod();

    [U] protected override void SerializesInitializer() => base.SerializesInitializer();

    [U] protected override void SerializesFluent() => base.SerializesFluent();

    [I] public override Task ReturnsExpectedStatusCode() => base.ReturnsExpectedStatusCode();

    [I] public override Task ReturnsExpectedIsValid() => base.ReturnsExpectedIsValid();

    [I] public override Task ReturnsExpectedResponse() => base.ReturnsExpectedResponse();
}
