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

using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Search.Validate;

public class ValidateQueryApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, ValidateQueryResponse, IValidateQueryRequest, ValidateQueryDescriptor<Project>,
        ValidateQueryRequest<Project>>
{
    public ValidateQueryApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.POST;
    protected override string UrlPath => "/project/_validate/query";

    protected override LazyResponses ClientUsage() => Calls(
        (c, f) => c.Indices.ValidateQuery<Project>(v => v.Query(q => q.MatchAll())),
        (c, f) => c.Indices.ValidateQueryAsync<Project>(v => v.Query(q => q.MatchAll())),
        (c, r) => c.Indices.ValidateQuery(new ValidateQueryRequest<Project> { Query = new QueryContainer(new MatchAllQuery()) }),
        (c, r) => c.Indices.ValidateQueryAsync(new ValidateQueryRequest<Project> { Query = new QueryContainer(new MatchAllQuery()) })
    );
}

public class ValidateInvalidQueryApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, ValidateQueryResponse, IValidateQueryRequest, ValidateQueryDescriptor<Project>,
        ValidateQueryRequest<Project>>
{
    private readonly ValidateQueryDescriptor<Project> _descriptor = new ValidateQueryDescriptor<Project>()
        .Query(q => q
            .Match(m => m
                .Field(p => p.StartedOn)
                .Query("shouldbeadate")
            )
        );

    private readonly ValidateQueryRequest<Project> _request = new ValidateQueryRequest<Project>
    {
        Query = new QueryContainer(
            new MatchQuery
            {
                Field = "startedOn",
                Query = "shouldbeadate"
            }
        )
    };

    public ValidateInvalidQueryApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override HttpMethod HttpMethod => HttpMethod.POST;
    protected override string UrlPath => "/project/_validate/query";

    protected override LazyResponses ClientUsage() => Calls(
        (c, f) => c.Indices.ValidateQuery<Project>(v => _descriptor),
        (c, f) => c.Indices.ValidateQueryAsync<Project>(v => _descriptor),
        (c, r) => c.Indices.ValidateQuery(_request),
        (c, r) => c.Indices.ValidateQueryAsync(_request)
    );

    protected override void ExpectResponse(ValidateQueryResponse response) => response.Valid.Should().BeFalse();
}
