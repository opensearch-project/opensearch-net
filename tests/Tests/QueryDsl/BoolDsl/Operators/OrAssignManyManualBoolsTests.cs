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
using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Domain;

namespace Tests.QueryDsl.BoolDsl.Operators;

public class OrAssignManyManualBoolsTests : OperatorUsageBase
{
    private static readonly int Iterations = 10000;
    /** Or assigning many bool queries that are not locked should result in a single bool query with many
		 * should clauses. Consider our combining logic where we try to merge should clauses or wrap in a new container:
		 *
		 * n | left			| right	| result
		 * ------------------------------------
		 * 1 | empty		| must	| bool query with should clause and bool query inside with must clause
		 * 2 | result of 1	| must	| see below...
		 *
		 * The result of iteration 1 indicates it can merge shoulds because it only has should clauses
		 * The right operand can not merge shoulds because it only has a single must clause.
		 * So a new query container is created with the 2 should clauses containing the result of iteration 1
		 * and the must clause right operand of iteration 2.
		 *
		 * If we move to the third iteration the whole story repeat itself generating deeply nested pairs of 2.
		 *
		 * Our combining logic should spot this special case and only generate of single bool query with N should clauses
		 * containing many bool queries with single must clauses.
		 *
		 */

    private static QueryContainer ATermQuery(QueryContainerDescriptor<Project> must) => must.Term(p => p.Name, "foo");

    [U]
    public void OrAssigningManyBoolMustQueries()
    {
        var q = Query<Project>.Bool(b => b.Must(ATermQuery));
        var container = OrAssignManyBoolQueries(q);
        container.Bool.Should.Cast<IQueryContainer>().Should().OnlyContain(s => s.Bool != null && s.Bool.Must != null);
    }

    [U]
    public void OrAssigningManyBoolMustNotQueries()
    {
        var q = Query<Project>.Bool(b => b.MustNot(ATermQuery));
        var container = OrAssignManyBoolQueries(q);
        container.Bool.Should.Cast<IQueryContainer>().Should().OnlyContain(s => s.Bool != null && s.Bool.MustNot != null);
    }

    /**
		 * |= assigning many bool queries with only should clauses flattens even further to a single bool with only term
		 * queries in the should
		 */
    [U]
    public void OrAssigningBoolShouldQueries()
    {
        var q = Query<Project>.Bool(b => b.Should(ATermQuery));
        var container = OrAssignManyBoolQueries(q);
        container.Bool.Should.Cast<IQueryContainer>().Should().OnlyContain(s => s.Term != null);
    }

    [U]
    public void OrAssigningBoolShouldQueriesWithMustClauses()
    {
        var q = Query<Project>.Bool(b => b.Should(ATermQuery).Must(ATermQuery));
        var container = OrAssignManyBoolQueries(q);
        container.Bool.Should.Cast<IQueryContainer>()
            .Should()
            .OnlyContain(s => s.Bool != null && s.Bool.Should != null && s.Bool.Must != null);
    }

    /** But not if that query has other clauses */
    [U]
    public void OrAssigningBoolShouldQueriesWithMustNotClauses()
    {
        var q = Query<Project>.Bool(b => b.Should(ATermQuery).MustNot(ATermQuery));
        var container = OrAssignManyBoolQueries(q);
        container.Bool.Should.Cast<IQueryContainer>()
            .Should()
            .OnlyContain(s => s.Bool != null && s.Bool.Should != null && s.Bool.MustNot != null);
    }

    [U]
    public void OrAssigningBoolMustQueriesWithMustNotClauses()
    {
        var q = Query<Project>.Bool(b => b.Must(ATermQuery).MustNot(ATermQuery));
        var container = OrAssignManyBoolQueries(q);
        container.Bool.Should.Cast<IQueryContainer>()
            .Should()
            .OnlyContain(s => s.Bool != null && s.Bool.Must != null && s.Bool.MustNot != null);
    }

    /** Or is locked */
    [U]
    public void OrAssigningNamedBoolShouldQueries()
    {
        var q = Query<Project>.Bool(b => b.Should(ATermQuery).Name("name"));
        var container = OrAssignManyBoolQueries(q);
        container.Bool.Should.Cast<IQueryContainer>()
            .Should()
            .OnlyContain(s => s.Bool != null && s.Bool.Should != null && s.Bool.Name == "name");
    }

    private IQueryContainer OrAssignManyBoolQueries(QueryContainer q)
    {
        var container = new QueryContainer();
        Action act = () =>
        {
            for (var i = 0; i < Iterations; i++) container |= q;
        };
        act.Should().NotThrow();
        LotsOfOrs(container);
        return container;
    }

    private void LotsOfOrs(IQueryContainer lotsOfOrs)
    {
        lotsOfOrs.Should().NotBeNull();
        lotsOfOrs.Bool.Should().NotBeNull();
        lotsOfOrs.Bool.Should.Should().NotBeEmpty().And.HaveCount(Iterations);
    }
}
