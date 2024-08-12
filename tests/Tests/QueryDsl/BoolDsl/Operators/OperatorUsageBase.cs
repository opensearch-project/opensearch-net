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
using FluentAssertions;
using OpenSearch.Client;
using Tests.Domain;

namespace Tests.QueryDsl.BoolDsl.Operators;

public abstract class OperatorUsageBase
{
    protected static readonly TermQuery ConditionlessQuery = new TermQuery();
    protected static readonly TermQuery NullQuery = null;
    protected static readonly TermQuery Query = new TermQuery { Field = "x", Value = "y" };

    protected void ReturnsNull(QueryContainer combined, Func<QueryContainerDescriptor<Project>, QueryContainer> selector)
    {
        combined.Should().BeNull();
        selector.Invoke(new QueryContainerDescriptor<Project>()).Should().BeNull();
    }

    protected void ReturnsBool(QueryContainer combined, Func<QueryContainerDescriptor<Project>, QueryContainer> selector,
        Action<IBoolQuery> boolQueryAssert
    )
    {
        ReturnsBool(combined, boolQueryAssert);
        ReturnsBool(selector.Invoke(new QueryContainerDescriptor<Project>()), boolQueryAssert);
    }

    private void ReturnsBool(QueryContainer combined, Action<IBoolQuery> boolQueryAssert)
    {
        combined.Should().NotBeNull();
        IQueryContainer c = combined;
        c.Bool.Should().NotBeNull();
        boolQueryAssert(c.Bool);
    }

    protected void ReturnsSingleQuery(QueryContainer combined, Func<QueryContainerDescriptor<Project>, QueryContainer> selector,
        Action<IQueryContainer> containerAssert
    )
    {
        ReturnsSingleQuery(combined, containerAssert);
        ReturnsSingleQuery(selector.Invoke(new QueryContainerDescriptor<Project>()), containerAssert);
    }

    private void ReturnsSingleQuery(QueryContainer combined, Action<IQueryContainer> containerAssert)
    {
        combined.Should().NotBeNull();
        IQueryContainer c = combined;
        containerAssert(c);
    }
}
