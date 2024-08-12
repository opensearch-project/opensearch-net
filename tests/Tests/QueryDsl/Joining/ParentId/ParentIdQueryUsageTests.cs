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

namespace Tests.QueryDsl.Joining.ParentId;

/**
	 * The `parent_id` query can be used to find child documents which belong to a particular parent.
	 *
	 * See the OpenSearch documentation on {ref_current}/query-dsl-parent-id-query.html[parent_id query] for more details.
	 */
public class ParentIdQueryUsageTests : QueryDslUsageTestsBase
{
    public ParentIdQueryUsageTests(ReadOnlyCluster i, EndpointUsage usage) : base(i, usage) { }

    protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<IParentIdQuery>(a => a.ParentId)
    {
        q => q.Id = null,
        q => q.Type = null,
    };

    protected override QueryContainer QueryInitializer => new ParentIdQuery
    {
        Name = "named_query",
        Type = Infer.Relation<CommitActivity>(),
        Id = Project.Instance.Name
    };

    protected override object QueryJson => new
    {
        parent_id = new
        {
            _name = "named_query",
            type = "commits",
            id = Project.Instance.Name
        }
    };

    protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
        .ParentId(p => p
            .Name("named_query")
            .Type<CommitActivity>()
            .Id(Project.Instance.Name)
        );
}
