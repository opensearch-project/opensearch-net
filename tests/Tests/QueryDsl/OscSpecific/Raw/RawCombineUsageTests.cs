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

namespace Tests.QueryDsl.OscSpecific.Raw;

/**
	 * OSC's <<raw-query-usage, raw query>> can be combined with other queries using a <<compound-queries, compound query>>
	 * such as a `bool` query.
	 */
public class RawCombineUsageTests : QueryDslUsageTestsBase
{
    private static readonly string RawTermQuery = @"{""term"": { ""fieldname"":""value"" } }";

    public RawCombineUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override QueryContainer QueryInitializer =>
        new RawQuery(RawTermQuery)
        && new TermQuery { Field = "x", Value = "y" };

    protected override object QueryJson => new
    {
        @bool = new
        {
            must = new object[]
            {
                new { term = new { fieldname = "value" } },
                new { term = new { x = new { value = "y" } } }
            }
        }
    };

    protected override bool SupportsDeserialization => false;

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) =>
        q.Raw(RawTermQuery) && q.Term("x", "y");
}
