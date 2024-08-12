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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Document.Multiple.Bulk;

public class BulkInvalidApiTests : ApiIntegrationTestBase<WritableCluster, BulkResponse, IBulkRequest, BulkDescriptor, BulkRequest>
{
    public BulkInvalidApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;

    protected override object ExpectJson { get; } = new object[]
    {
        new Dictionary<string, object> { { "update", new { _id = Project.Instance.Name } } },
        new { doc = new { leadDeveloper = new { firstName = "martijn" } } },
        new Dictionary<string, object> { { "delete", new { _id = Project.Instance.Name + "1" } } },
    };

    protected override int ExpectStatusCode => 200;

    protected override Func<BulkDescriptor, IBulkRequest> Fluent => d => d
        .Index(CallIsolatedValue)
        .Update<Project, object>(b => b.Doc(new { leadDeveloper = new { firstName = "martijn" } }).Id(Project.Instance.Name))
        .Delete<Project>(b => b.Id(Project.Instance.Name + "1"));

    protected override HttpMethod HttpMethod => HttpMethod.POST;


    protected override BulkRequest Initializer => new BulkRequest(CallIsolatedValue)
    {
        Operations = new List<IBulkOperation>
        {
            new BulkUpdateOperation<Project, object>(Project.Instance.Name)
            {
                Doc = new { leadDeveloper = new { firstName = "martijn" } }
            },
            new BulkDeleteOperation<Project>(Project.Instance.Name + "1"),
        }
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/{CallIsolatedValue}/_bulk";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Bulk(f),
        (client, f) => client.BulkAsync(f),
        (client, r) => client.Bulk(r),
        (client, r) => client.BulkAsync(r)
    );

    protected override void ExpectResponse(BulkResponse response)
    {
        response.Took.Should().BeGreaterThan(0);
        response.Errors.Should().BeTrue();

        //a delete not found is not an error (also in OpenSearch)
        //if you do a single bulk delete on an unknown id .Errors will be false
        response.ItemsWithErrors.Should().NotBeNull().And.HaveCount(1);
        response.Items.Should().NotBeEmpty();

        var failedUpdate = response.Items.First() as BulkUpdateResponseItem;
        failedUpdate.Should().NotBeNull();
        failedUpdate.Index.Should().Be(CallIsolatedValue);
        failedUpdate.Status.Should().Be(404);
        failedUpdate.Error.Should().NotBeNull();
        failedUpdate.Error.Type.Should().Be("document_missing_exception");
        failedUpdate.IsValid.Should().BeFalse();

        var failedDelete = response.Items.Last() as BulkDeleteResponseItem;
        failedDelete.IsValid.Should().BeTrue();
    }
}
