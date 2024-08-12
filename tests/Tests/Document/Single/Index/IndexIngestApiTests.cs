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
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Core.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Domain.Extensions;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;
using static Tests.Domain.Helpers.TestValueHelper;

namespace Tests.Document.Single.Index;

public class IndexIngestApiTests
    : ApiIntegrationTestBase<IntrusiveOperationCluster, IndexResponse, IIndexRequest<Project>, IndexDescriptor<Project>, IndexRequest<Project>>
{
    public IndexIngestApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;

    protected override object ExpectJson =>
        new
        {
            name = CallIsolatedValue,
            type = Project.TypeName,
            join = Document.Join.ToAnonymousObject(),
            state = "Stable",
            visibility = "Public",
            startedOn = FixedDate,
            lastActivity = FixedDate,
            numberOfContributors = 0,
            sourceOnly = Dependant(null, new { notWrittenByDefaultSerializer = "written" }),
            curatedTags = new[] { new { name = "x", added = FixedDate } },
        };

    protected override int ExpectStatusCode => 201;

    protected override Func<IndexDescriptor<Project>, IIndexRequest<Project>> Fluent => s => s
        .WaitForActiveShards("1")
        .OpType(OpType.Index)
        .Refresh(Refresh.True)
        .Pipeline(PipelineId)
        .Routing("route");

    protected override HttpMethod HttpMethod => HttpMethod.PUT;
    protected override bool IncludeNullInExpected => false;

    protected override IndexRequest<Project> Initializer =>
        new IndexRequest<Project>(Document)
        {
            Refresh = Refresh.True,
            OpType = OpType.Index,
            WaitForActiveShards = "1",
            Routing = "route",
            Pipeline = PipelineId
        };

    protected override bool SupportsDeserialization => false;

    protected override string UrlPath
        => $"/project/_doc/{CallIsolatedValue}?wait_for_active_shards=1&op_type=index&refresh=true&routing=route&pipeline={PipelineId}";

    private Project Document => new Project
    {
        State = StateOfBeing.Stable,
        Name = CallIsolatedValue,
        StartedOn = FixedDate,
        LastActivity = FixedDate,
        CuratedTags = new List<Tag> { new Tag { Name = "x", Added = FixedDate } },
        SourceOnly = TestClient.Configuration.Random.SourceSerializer ? new SourceOnlyObject() : null
    };

    private static string PipelineId { get; } = "pipeline-" + Guid.NewGuid().ToString("N").Substring(0, 8);

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values) => client.Ingest.PutPipeline(
        new PutPipelineRequest(PipelineId)
        {
            Description = "Index pipeline test",
            Processors = new List<IProcessor>
            {
                new RenameProcessor
                {
                    TargetField = "lastSeen",
                    Field = "lastActivity"
                }
            }
        });

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Index(Document, f),
        (client, f) => client.IndexAsync(Document, f),
        (client, r) => client.Index(r),
        (client, r) => client.IndexAsync(r)
    );

    protected override IndexDescriptor<Project> NewDescriptor() => new IndexDescriptor<Project>(Document);
}
