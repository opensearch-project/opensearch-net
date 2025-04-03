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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Newtonsoft.Json;
using OpenSearch.OpenSearch.Ephemeral;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl;

public abstract class QueryDslUsageTestsBase<TCluster, TDocument>
    : ApiTestBase<TCluster, ISearchResponse<TDocument>, ISearchRequest, SearchDescriptor<TDocument>, SearchRequest<TDocument>>
    where TCluster : IEphemeralCluster<EphemeralClusterConfiguration>, IOpenSearchClientTestCluster, new()
    where TDocument : class
{
    protected QueryDslUsageTestsBase(TCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected abstract IndexName IndexName { get; }
    protected abstract string ExpectedIndexString { get; }

    protected virtual string SearchPipeline => null;

    protected virtual ConditionlessWhen ConditionlessWhen => null;

    protected override object ExpectJson => new { query = QueryJson };

    protected override Func<SearchDescriptor<TDocument>, ISearchRequest> Fluent => s => s
        .Index(IndexName)
        .SearchPipeline(SearchPipeline)
        .Query(QueryFluent);

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override SearchRequest<TDocument> Initializer =>
        new(IndexName)
        {
            SearchPipeline = SearchPipeline,
            Query = QueryInitializer
        };

    protected virtual NotConditionlessWhen NotConditionlessWhen => null;

    protected abstract QueryContainer QueryInitializer { get; }

    protected abstract object QueryJson { get; }
    protected override string UrlPath =>
        $"/{ExpectedIndexString}/_search{(string.IsNullOrWhiteSpace(SearchPipeline) ? "" : $"?search_pipeline={SearchPipeline}")}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Search(f),
        (client, f) => client.SearchAsync(f),
        (client, r) => client.Search<TDocument>(r),
        (client, r) => client.SearchAsync<TDocument>(r)
    );

    protected abstract QueryContainer QueryFluent(QueryContainerDescriptor<TDocument> q);

    [U] public void FluentIsNotConditionless() =>
        AssertIsNotConditionless(QueryFluent(new QueryContainerDescriptor<TDocument>()));

    [U] public void InitializerIsNotConditionless() => AssertIsNotConditionless(QueryInitializer);

    private void AssertIsNotConditionless(IQueryContainer c)
    {
        if (!c.IsVerbatim)
            c.IsConditionless.Should().BeFalse();
    }

    [U] public void SeenByVisitor()
    {
        var visitor = new DslPrettyPrintVisitor(TestClient.DefaultInMemoryClient.ConnectionSettings);
        var query = QueryFluent(new QueryContainerDescriptor<TDocument>());
        query.Should().NotBeNull("query evaluated to null which implies it may be conditionless");
        query.Accept(visitor);
        var pretty = visitor.PrettyPrint;
        pretty.Should().NotBeNullOrWhiteSpace();
    }

    [U] public void ConditionlessWhenExpectedToBe()
    {
        if (ConditionlessWhen == null) return;

        foreach (var when in ConditionlessWhen)
        {
            when(QueryFluent(new QueryContainerDescriptor<TDocument>()));
            when(QueryInitializer);
        }

        ((IQueryContainer)QueryInitializer).IsConditionless.Should().BeFalse();
    }

    [U] public void NotConditionlessWhenExpectedToBe()
    {
        if (NotConditionlessWhen == null) return;

        foreach (var when in NotConditionlessWhen)
        {
            when(QueryFluent(new QueryContainerDescriptor<TDocument>()));
            when(QueryInitializer);
        }
    }

    [I] protected async Task AssertQueryResponse() => await AssertOnAllResponses(AssertQueryResponseValid);

    protected virtual void AssertQueryResponseValid(ISearchResponse<TDocument> response) => response.ShouldBeValid();
}

public abstract class QueryDslUsageTestsBase
    : QueryDslUsageTestsBase<ReadOnlyCluster, Project>
{
    protected static byte[] ShortFormQuery => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { description = "project description" }));

    protected static readonly QueryContainer ConditionlessQuery = new(new TermQuery());

    protected static readonly QueryContainer VerbatimQuery = new(new TermQuery { IsVerbatim = true });

    protected QueryDslUsageTestsBase(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override IndexName IndexName => typeof(Project);
    protected override string ExpectedIndexString => "project";
}
