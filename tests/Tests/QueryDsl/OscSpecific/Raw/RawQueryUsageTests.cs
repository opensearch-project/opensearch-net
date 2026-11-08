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

namespace Tests.QueryDsl.OscSpecific.Raw
{
    /**
	 * Allows a query represented as a string of JSON to be passed to OSC's Fluent API or Object Initializer syntax.
	 * This can be useful when porting over a query expressed in the query DSL over to OSC.
	 */
    public class RawUsageTests : QueryDslUsageTestsBase
    {
        private static readonly string RawTermQuery = @"{""term"": { ""fieldname"":""value"" } }";

        public RawUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

        protected override QueryContainer QueryInitializer => new RawQuery(RawTermQuery);

        protected override object QueryJson => new
        {
            term = new { fieldname = "value" }
        };

        protected override bool SupportsDeserialization => false;

        protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
            .Raw(RawTermQuery);
    }
}
