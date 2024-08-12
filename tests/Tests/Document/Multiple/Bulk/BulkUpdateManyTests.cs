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
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Document.Multiple.Bulk;

public class BulkUpdateManyTests : ApiTestBase<ReadOnlyCluster, BulkResponse, IBulkRequest, BulkDescriptor, BulkRequest>
{
    private readonly List<Project> _updates = Project.Projects.Take(10).ToList();

    public BulkUpdateManyTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson => _updates.SelectMany(ProjectToBulkJson);

    protected override Func<BulkDescriptor, IBulkRequest> Fluent => d => d
        .Index(CallIsolatedValue)
        .UpdateMany(_updates, (b, u) => b.Script(s => s.Source("_source.counter++")));

    protected override HttpMethod HttpMethod => HttpMethod.POST;


    protected override BulkRequest Initializer => new BulkRequest(CallIsolatedValue)
    {
        Operations = _updates
            .Select(u => new BulkUpdateOperation<Project, Project>(u) { Script = new InlineScript("_source.counter++") })
            .ToList<IBulkOperation>()
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/{CallIsolatedValue}/_bulk";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Bulk(f),
        (client, f) => client.BulkAsync(f),
        (client, r) => client.Bulk(r),
        (client, r) => client.BulkAsync(r)
    );

    private IEnumerable<object> ProjectToBulkJson(Project p)
    {
        yield return new Dictionary<string, object> { { "update", new { _id = p.Name, routing = p.Name } } };
        yield return new { script = new { source = "_source.counter++" } };
    }
}
