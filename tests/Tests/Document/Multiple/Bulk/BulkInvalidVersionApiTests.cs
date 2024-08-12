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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Document.Multiple.Bulk;

public class BulkInvalidVersionApiTests : ApiIntegrationTestBase<WritableCluster, BulkResponse, IBulkRequest, BulkDescriptor, BulkRequest>
{
    public BulkInvalidVersionApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;

    protected override object ExpectJson { get; } = new[]
    {
        new Dictionary<string, object> { { "index", new { _id = Project.Instance.Name, routing = Project.Instance.Name } } },
        Project.InstanceAnonymous,
        new Dictionary<string, object> { { "index", new { _id = Project.Instance.Name, routing = Project.Instance.Name, if_seq_no = -1, if_primary_term = 0 } } },
        Project.InstanceAnonymous,
    };

    protected override int ExpectStatusCode => 400;

    protected override Func<BulkDescriptor, IBulkRequest> Fluent => d => d
        .Index(CallIsolatedValue)
        .Index<Project>(i => i.Document(Project.Instance))
        .Index<Project>(i => i.IfSequenceNumber(-1).IfPrimaryTerm(0).Document(Project.Instance));

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override BulkRequest Initializer => new BulkRequest(CallIsolatedValue)
    {
        Operations = new List<IBulkOperation>
        {
            new BulkIndexOperation<Project>(Project.Instance),
            new BulkIndexOperation<Project>(Project.Instance) { IfSequenceNumber = -1, IfPrimaryTerm = 0 }
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
        response.ShouldNotBeValid();
        response.ServerError.Should().NotBeNull();
        response.ServerError.Status.Should().Be(400);
        response.ServerError.Error.Type.Should().Be("illegal_argument_exception");
        response.ServerError.Error.Reason.Should().StartWith("sequence numbers must be non negative.");
    }
}
