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

using System.Collections.Generic;
using System.Linq;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.Span.Or;

public class SpanOrUsageTests : QueryDslUsageTestsBase
{
    public SpanOrUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<ISpanOrQuery>(a => a.SpanOr)
    {
        q => q.Clauses = null,
        q => q.Clauses = Enumerable.Empty<ISpanQuery>(),
        q => q.Clauses = new[] { new SpanQuery() },
    };

    protected override QueryContainer QueryInitializer => new SpanOrQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Clauses = new List<ISpanQuery>
        {
            new SpanQuery { SpanTerm = new SpanTermQuery { Field = "field", Value = "value1" } },
            new SpanQuery { SpanTerm = new SpanTermQuery { Field = "field", Value = "value2" } },
            new SpanQuery { SpanTerm = new SpanTermQuery { Field = "field", Value = "value3" } }
        },
    };

    protected override object QueryJson => new
    {
        span_or = new
        {
            _name = "named_query",
            boost = 1.1,
            clauses = new[]
            {
                new { span_term = new { field = new { value = "value1" } } },
                new { span_term = new { field = new { value = "value2" } } },
                new { span_term = new { field = new { value = "value3" } } }
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .SpanOr(sn => sn
            .Name("named_query")
            .Boost(1.1)
            .Clauses(
                c => c.SpanTerm(st => st.Field("field").Value("value1")),
                c => c.SpanTerm(st => st.Field("field").Value("value2")),
                c => c.SpanTerm(st => st.Field("field").Value("value3"))
            )
        );
}
