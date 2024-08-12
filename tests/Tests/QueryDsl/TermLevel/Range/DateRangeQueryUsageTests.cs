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

using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;
using static Tests.Domain.Helpers.TestValueHelper;

namespace Tests.QueryDsl.TermLevel.Range;

public class DateRangeQueryUsageTests : QueryDslUsageTestsBase
{
    public DateRangeQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IDateRangeQuery>(q => q.Range as IDateRangeQuery)
    {
        q => q.Field = null,
        q =>
        {
            q.GreaterThan = null;
            q.GreaterThanOrEqualTo = null;
            q.LessThan = null;
            q.LessThanOrEqualTo = null;
        },
        q =>
        {
            q.GreaterThan = default;
            q.GreaterThanOrEqualTo = default;
            q.LessThan = default;
            q.LessThanOrEqualTo = default;
        }
    };

    protected override QueryContainer QueryInitializer => new DateRangeQuery
    {
        Name = "named_query",
        Boost = 1.1,
        Field = "description",
        GreaterThan = FixedDate,
        GreaterThanOrEqualTo = DateMath.Anchored(FixedDate).RoundTo(DateMathTimeUnit.Month),
        LessThan = "01/01/2012",
        LessThanOrEqualTo = DateMath.Now,
        TimeZone = "+01:00",
        Format = "dd/MM/yyyy||yyyy"
    };

    protected override object QueryJson => new
    {
        range = new
        {
            description = new
            {
                _name = "named_query",
                boost = 1.1,
                format = "dd/MM/yyyy||yyyy",
                gt = "2015-06-06T12:01:02.123",
                gte = "2015-06-06T12:01:02.123||/M",
                lt = "01/01/2012",
                lte = "now",
                time_zone = "+01:00"
            }
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .DateRange(c => c
            .Name("named_query")
            .Boost(1.1)
            .Field(p => p.Description)
            .GreaterThan(FixedDate)
            .GreaterThanOrEquals(DateMath.Anchored(FixedDate).RoundTo(DateMathTimeUnit.Month))
            .LessThan("01/01/2012")
            .LessThanOrEquals(DateMath.Now)
            .Format("dd/MM/yyyy||yyyy")
            .TimeZone("+01:00")
        );
}
