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

using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.QueryDsl.BoolDsl.Operators;

public class UnaryAddOperatorUsageTests : OperatorUsageBase
{
    [U]
    public void UnaryAdd()
    {
        ReturnsBool(+Query && +Query, q => +q.Query() && +q.Query(), b =>
        {
            b.Filter.Should().NotBeEmpty().And.HaveCount(2);
            b.Must.Should().BeNull();
            b.Should.Should().BeNull();
            b.MustNot.Should().BeNull();
        });

        ReturnsBool(+Query || +Query || +ConditionlessQuery, q => +q.Query() || +q.Query() || +q.ConditionlessQuery(), b =>
        {
            b.Should.Should().NotBeEmpty().And.HaveCount(2);
            b.Must.Should().BeNull();
            b.MustNot.Should().BeNull();
            b.MustNot.Should().BeNull();
            foreach (IQueryContainer q in b.Should)
            {
                q.Bool.Should().NotBeNull();
                q.Bool.Filter.Should().NotBeEmpty().And.HaveCount(1);
            }
        });

        ReturnsSingleQuery(+Query || +ConditionlessQuery, q => +q.Query() || +q.ConditionlessQuery(),
            c => c.Bool.Filter.Should().NotBeNull().And.HaveCount(1));

        ReturnsSingleQuery(+ConditionlessQuery || +Query, q => +q.ConditionlessQuery() || +q.Query(),
            c => c.Bool.Filter.Should().NotBeNull().And.HaveCount(1));

        ReturnsSingleQuery(+Query || +NullQuery, q => +q.Query() || +q.NullQuery(),
            c => c.Bool.Filter.Should().NotBeNull().And.HaveCount(1));

        ReturnsSingleQuery(+NullQuery && +Query, q => +q.NullQuery() && +q.Query(),
            c => c.Bool.Filter.Should().NotBeNull().And.HaveCount(1));

        ReturnsSingleQuery(+ConditionlessQuery || +ConditionlessQuery && +ConditionlessQuery || +Query,
            q => +q.ConditionlessQuery() || +q.ConditionlessQuery() && +q.ConditionlessQuery() || +q.Query(),
            c => c.Bool.Filter.Should().NotBeNull().And.HaveCount(1));

        ReturnsSingleQuery(
            +NullQuery || +NullQuery || +ConditionlessQuery || +Query,
            q => +q.NullQuery() || +q.NullQuery() || +q.ConditionlessQuery() || +q.Query(),
            c => c.Bool.Filter.Should().NotBeNull());

        ReturnsNull(+NullQuery || +ConditionlessQuery, q => +q.NullQuery() || +q.ConditionlessQuery());
        ReturnsNull(+ConditionlessQuery && +NullQuery, q => +q.ConditionlessQuery() && +q.NullQuery());
        ReturnsNull(+ConditionlessQuery || +ConditionlessQuery, q => +q.ConditionlessQuery() || +q.ConditionlessQuery());
        ReturnsNull(
            +ConditionlessQuery || +ConditionlessQuery || +ConditionlessQuery || +ConditionlessQuery,
            q => +q.ConditionlessQuery() || +q.ConditionlessQuery() || +q.ConditionlessQuery() || +q.ConditionlessQuery()
        );
        ReturnsNull(
            +NullQuery || +ConditionlessQuery || +ConditionlessQuery || +ConditionlessQuery,
            q => +q.NullQuery() || +q.ConditionlessQuery() || +q.ConditionlessQuery() || +q.ConditionlessQuery()
        );
    }

    [U]
    public void CombiningManyUsingAggregate()
    {
        var lotsOfUnaryAdds = Enumerable.Range(0, 100).Aggregate(new QueryContainer(), (q, c) => q && +Query, q => q);
        LotsOfUnaryAdds(lotsOfUnaryAdds);
    }

    [U]
    public void CombiningManyUsingForeachInitializingWithNull()
    {
        QueryContainer container = null;
        foreach (var i in Enumerable.Range(0, 100))
            container &= +Query;
        LotsOfUnaryAdds(container);
    }

    [U]
    public void CombiningManyUsingForeachInitializingWithDefault()
    {
        var container = new QueryContainer();
        foreach (var i in Enumerable.Range(0, 100))
            container &= +Query;
        LotsOfUnaryAdds(container);
    }

    private void LotsOfUnaryAdds(IQueryContainer lotsOfUnaryAdds)
    {
        lotsOfUnaryAdds.Should().NotBeNull();
        lotsOfUnaryAdds.Bool.Should().NotBeNull();
        lotsOfUnaryAdds.Bool.Filter.Should().NotBeEmpty().And.HaveCount(100);
    }

    [U]
    public void CombindingManyBoolFiltersUsingOrsShouldFlatten()
    {
        var container = new QueryContainer();
        foreach (var i in Enumerable.Range(0, 100))
            container |= +Query;
        var c = container as IQueryContainer;

        c.Bool.Should.Should().NotBeEmpty().And.HaveCount(100);
    }
}
