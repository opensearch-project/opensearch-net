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
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Document.Single.SourceExists;

public class SourceExistsApiTests
    : ApiIntegrationTestBase<WritableCluster, ExistsResponse, ISourceExistsRequest, SourceExistsDescriptor<Project>, SourceExistsRequest<Project>
    >
{
    public SourceExistsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;
    protected override Func<SourceExistsDescriptor<Project>, ISourceExistsRequest> Fluent => d => d.Routing(CallIsolatedValue);
    protected override HttpMethod HttpMethod => HttpMethod.HEAD;

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/project/_source/{CallIsolatedValue}?routing={CallIsolatedValue}";

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        foreach (var id in values.Values)
            Client.Index(Project.Instance, i => i.Id(id).Routing(id));
    }

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.SourceExists(CallIsolatedValue, f),
        (client, f) => client.SourceExistsAsync(CallIsolatedValue, f),
        (client, r) => client.SourceExists(r),
        (client, r) => client.SourceExistsAsync(r)
    );

    protected override SourceExistsDescriptor<Project> NewDescriptor() => new SourceExistsDescriptor<Project>(CallIsolatedValue);

    protected override SourceExistsRequest<Project> Initializer => new SourceExistsRequest<Project>(CallIsolatedValue) { Routing = CallIsolatedValue };

}

public class SourceExistsNotFoundApiTests
    : ApiIntegrationTestBase<WritableCluster, ExistsResponse, ISourceExistsRequest, SourceExistsDescriptor<Project>, SourceExistsRequest<Project>
    >
{
    public SourceExistsNotFoundApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;
    protected override int ExpectStatusCode => 404;
    protected override Func<SourceExistsDescriptor<Project>, ISourceExistsRequest> Fluent => d => d.Routing(CallIsolatedValue);
    protected override HttpMethod HttpMethod => HttpMethod.HEAD;

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/{IndexWithNoSource.Name}/_source/{CallIsolatedValue}?routing={CallIsolatedValue}";

    private static IndexName IndexWithNoSource { get; } = "project-with-no-source";

    private static DocumentPath<Project> Doc(string id) => new DocumentPath<Project>(id).Index(IndexWithNoSource);

    protected override void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values)
    {
        var index = client.Indices.Create(IndexWithNoSource, i => i
            .Map<Project>(mm => mm
                .SourceField(sf => sf.Enabled(false))
            )
        );
        index.ShouldBeValid();

        foreach (var id in values.Values)
            Client.Index(Project.Instance, i => i.Id(id).Index(IndexWithNoSource).Routing(id));
    }

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.SourceExists(Doc(CallIsolatedValue), f),
        (client, f) => client.SourceExistsAsync(Doc(CallIsolatedValue), f),
        (client, r) => client.SourceExists(r),
        (client, r) => client.SourceExistsAsync(r)
    );

    protected override SourceExistsDescriptor<Project> NewDescriptor() =>
        new SourceExistsDescriptor<Project>(index: IndexWithNoSource, id: CallIsolatedValue);

    protected override SourceExistsRequest<Project> Initializer => new SourceExistsRequest<Project>(IndexWithNoSource, CallIsolatedValue) { Routing = CallIsolatedValue };

}


public class SourceExistsIndexNotFoundApiTests
    : ApiIntegrationTestBase<WritableCluster, ExistsResponse, ISourceExistsRequest, SourceExistsDescriptor<Project>, SourceExistsRequest<Project>
    >
{
    public SourceExistsIndexNotFoundApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => false;
    protected override int ExpectStatusCode => 404;
    protected override Func<SourceExistsDescriptor<Project>, ISourceExistsRequest> Fluent => f => null;
    protected override HttpMethod HttpMethod => HttpMethod.HEAD;
    protected override SourceExistsRequest<Project> Initializer => new SourceExistsRequest<Project>(IndexWithNoSource, CallIsolatedValue);
    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/{IndexWithNoSource.Name}/_source/{CallIsolatedValue}";

    private static IndexName IndexWithNoSource { get; } = "source-no-index";

    protected override SourceExistsDescriptor<Project> NewDescriptor() =>
        new SourceExistsDescriptor<Project>(index: IndexWithNoSource, id: CallIsolatedValue);

    private static DocumentPath<Project> Doc(string id) => new DocumentPath<Project>(id).Index(IndexWithNoSource);

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.SourceExists(Doc(CallIsolatedValue), f),
        (client, f) => client.SourceExistsAsync(Doc(CallIsolatedValue), f),
        (client, r) => client.SourceExists(r),
        (client, r) => client.SourceExistsAsync(r)
    );
}
