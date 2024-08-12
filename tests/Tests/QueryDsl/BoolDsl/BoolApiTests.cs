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
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using static OpenSearch.Client.Infer;


namespace Tests.QueryDsl.BoolDsl;

internal static class BoolDslTestExtensions
{
    public static QueryContainer Id(this QueryContainerDescriptor<BoolCluster.A> q, int id) => q.Term(p => p.Id, id);

    public static QueryContainer O(this QueryContainerDescriptor<BoolCluster.A> q, BoolCluster.E option) => q.Term(p => p.Option, option);
}

public class BoolCluster : ClientTestClusterBase
{

    public enum E
    {
        Option1,
        Option2
    }

    protected override void SeedNode()
    {
        var client = Client;
        var index = client.Indices.Create(Index<A>(), i => i
            .Map<A>(m => m
                .AutoMap()
                .Properties(props => props
                    .Keyword(s => s.Name(p => p.Option))
                )
            )
        );
        var bulkResponse = client.Bulk(b => b.IndexMany(A.Documents));
        if (!bulkResponse.IsValid) throw new Exception("Could not bootstrap bool cluster, bulk was invalid");

        client.Indices.Refresh(Indices<A>());
    }

    public class A
    {
        private static readonly E[] Options = new[] { E.Option1, E.Option2 };
        public static IList<A> Documents => Enumerable.Range(0, 20).Select(i => new A { Id = i + 1, Option = Options[i % 2] }).ToList();
        public int Id { get; set; }
        public E Option { get; set; }
    }
}

public class BoolsInPractice : IClusterFixture<BoolCluster>
{
    private readonly BoolCluster _cluster;

    public BoolsInPractice(BoolCluster cluster) => _cluster = cluster;

    private async Task Bool(
        Func<BoolCluster.A, bool> programmatic,
        Func<QueryContainerDescriptor<BoolCluster.A>, QueryContainer> fluentQuery,
        QueryContainer initializerQuery,
        int expectedCount
    )
    {
        var documents = BoolCluster.A.Documents.Where(programmatic).ToList();
        documents.Count().Should().Be(expectedCount, " filtering the documents in memory did not yield the expected count");

        var client = _cluster.Client;

        var fluent = client.Search<BoolCluster.A>(s => s.Query(fluentQuery));
        var fluentAsync = await client.SearchAsync<BoolCluster.A>(s => s.Query(fluentQuery));

        var initializer = client.Search<BoolCluster.A>(new SearchRequest<BoolCluster.A> { Query = initializerQuery });
        var initializerAsync = await client.SearchAsync<BoolCluster.A>(new SearchRequest<BoolCluster.A> { Query = initializerQuery });

        var responses = new[] { fluent, fluentAsync, initializer, initializerAsync };
        foreach (var response in responses)
        {
            response.ShouldBeValid();
            response.Total.Should().Be(expectedCount);
        }
    }

    private TermQuery Id(int id) => new TermQuery { Field = "id", Value = id };

    private TermQuery O(BoolCluster.E option) => new TermQuery { Field = "option", Value = option };

    [I]
    public async Task CompareBoolQueryTranslationsToRealBooleanLogic()
    {
        await Bool(
            a => a.Id == 1 && a.Option == BoolCluster.E.Option1,
            a => a.Id(1) && a.O(BoolCluster.E.Option1),
            Id(1) && O(BoolCluster.E.Option1),
            1
        );

        await Bool(
            a => a.Id == 1 || a.Id == 2 || a.Id == 3 || a.Id == 4,
            a => +a.Id(1) || +a.Id(2) || +a.Id(3) || +a.Id(4),
            +Id(1) || +Id(2) || +Id(3) || +Id(4),
            4
        );

        await Bool(
            a => a.Id == 1 || a.Id == 2 || a.Id == 3 || a.Id == 4 && (a.Option != BoolCluster.E.Option1 || a.Option == BoolCluster.E.Option2),
            a => +a.Id(1) || +a.Id(2) || +a.Id(3) || +a.Id(4) && (!a.O(BoolCluster.E.Option1) || a.O(BoolCluster.E.Option2)),
            +Id(1) || +Id(2) || +Id(3) || +Id(4) && (!O(BoolCluster.E.Option1) || O(BoolCluster.E.Option2)),
            4
        );

        await Bool(
            a => a.Id == 1 || a.Id == 2 || a.Id == 3 || a.Id == 4 && a.Option != BoolCluster.E.Option1 || a.Option == BoolCluster.E.Option2,
            a => +a.Id(1) || +a.Id(2) || +a.Id(3) || +a.Id(4) && !a.O(BoolCluster.E.Option1) || a.O(BoolCluster.E.Option2),
            +Id(1) || +Id(2) || +Id(3) || +Id(4) && !O(BoolCluster.E.Option1) || O(BoolCluster.E.Option2),
            12
        );

        await Bool(
            a => a.Option != BoolCluster.E.Option1 && a.Id != 2 && a.Id != 3,
            a => !a.O(BoolCluster.E.Option1) && !a.Id(2) && !+a.Id(3),
            !O(BoolCluster.E.Option1) && !Id(2) && !+Id(3),
            9
        );
    }
}
