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
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl;

public abstract class QueryDslUsageTestsBase
    : ApiTestBase<ReadOnlyCluster, ISearchResponse<Project>, ISearchRequest, SearchDescriptor<Project>, SearchRequest<Project>>
{
    protected readonly QueryContainer ConditionlessQuery = new QueryContainer(new TermQuery());

    protected readonly QueryContainer VerbatimQuery = new QueryContainer(new TermQuery { IsVerbatim = true });

    protected byte[] ShortFormQuery => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { description = "project description" }));

    protected QueryDslUsageTestsBase(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected virtual ConditionlessWhen ConditionlessWhen => null;

    protected override object ExpectJson => new { query = QueryJson };

    protected override Func<SearchDescriptor<Project>, ISearchRequest> Fluent => s => s
        .Query(q => QueryFluent(q));

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override SearchRequest<Project> Initializer =>
        new SearchRequest<Project>
        {
            Query = QueryInitializer
        };

    protected virtual bool KnownParseException => false;

    protected virtual NotConditionlessWhen NotConditionlessWhen => null;

    protected abstract QueryContainer QueryInitializer { get; }

    protected abstract object QueryJson { get; }
    protected override string UrlPath => "/project/_search";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Search(f),
        (client, f) => client.SearchAsync(f),
        (client, r) => client.Search<Project>(r),
        (client, r) => client.SearchAsync<Project>(r)
    );

    protected abstract QueryContainer QueryFluent(QueryContainerDescriptor<Project> q);

    [U]
    public void FluentIsNotConditionless() =>
        AssertIsNotConditionless(QueryFluent(new QueryContainerDescriptor<Project>()));

    [U] public void InitializerIsNotConditionless() => AssertIsNotConditionless(QueryInitializer);

    private void AssertIsNotConditionless(IQueryContainer c)
    {
        if (!c.IsVerbatim)
            c.IsConditionless.Should().BeFalse();
    }

    [U]
    public void SeenByVisitor()
    {
        var visitor = new DslPrettyPrintVisitor(TestClient.DefaultInMemoryClient.ConnectionSettings);
        var query = QueryFluent(new QueryContainerDescriptor<Project>());
        query.Should().NotBeNull("query evaluated to null which implies it may be conditionless");
        query.Accept(visitor);
        var pretty = visitor.PrettyPrint;
        pretty.Should().NotBeNullOrWhiteSpace();
    }

    [U]
    public void ConditionlessWhenExpectedToBe()
    {
        if (ConditionlessWhen == null) return;

        foreach (var when in ConditionlessWhen)
        {
            when(QueryFluent(new QueryContainerDescriptor<Project>()));
            //this.JsonEquals(query, new { });
            when(QueryInitializer);
            //this.JsonEquals(query, new { });
        }

        ((IQueryContainer)QueryInitializer).IsConditionless.Should().BeFalse();
    }

    [U]
    public void NotConditionlessWhenExpectedToBe()
    {
        if (NotConditionlessWhen == null) return;

        foreach (var when in NotConditionlessWhen)
        {
            var query = QueryFluent(new QueryContainerDescriptor<Project>());
            when(query);

            query = QueryInitializer;
            when(query);
        }
    }

    [I]
    protected async Task AssertQueryResponse() => await AssertOnAllResponses(r =>
    {
        r.ShouldBeValid();
    });
}
