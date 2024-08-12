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
using System.Threading;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Cluster.TaskManagement.GetTask;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Core.ManagedOpenSearch.NodeSeeders;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Cluster.VotingConfigExclusions.DeleteVotingConfigExclusions;

public class DeleteVotingConfigExclusionsApiTests : ApiIntegrationTestBase<WritableCluster, DeleteVotingConfigExclusionsResponse, IDeleteVotingConfigExclusionsRequest, DeleteVotingConfigExclusionsDescriptor, DeleteVotingConfigExclusionsRequest>
{
    public DeleteVotingConfigExclusionsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;

    protected override Func<DeleteVotingConfigExclusionsDescriptor, IDeleteVotingConfigExclusionsRequest> Fluent => s => s
        .WaitForRemoval();

    protected override HttpMethod HttpMethod => HttpMethod.DELETE;

    protected override DeleteVotingConfigExclusionsRequest Initializer => new DeleteVotingConfigExclusionsRequest { WaitForRemoval = true };
    protected override string UrlPath => $"/_cluster/voting_config_exclusions?wait_for_removal=true";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Cluster.DeleteVotingConfigExclusions(f),
        (client, f) => client.Cluster.DeleteVotingConfigExclusionsAsync(f),
        (client, r) => client.Cluster.DeleteVotingConfigExclusions(r),
        (client, r) => client.Cluster.DeleteVotingConfigExclusionsAsync(r)
    );

    protected override void ExpectResponse(DeleteVotingConfigExclusionsResponse response) => response.ShouldBeValid();
}
