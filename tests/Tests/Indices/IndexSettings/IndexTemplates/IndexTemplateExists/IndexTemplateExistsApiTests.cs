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
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Indices.IndexSettings.IndexTemplates.IndexTemplateExists;

public class IndexTemplateExistsApiTests
    : ApiTestBase<WritableCluster, ExistsResponse, IIndexTemplateExistsRequest, IndexTemplateExistsDescriptor, IndexTemplateExistsRequest>
{
    public IndexTemplateExistsApiTests(WritableCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override Func<IndexTemplateExistsDescriptor, IIndexTemplateExistsRequest> Fluent => d => d;

    protected override HttpMethod HttpMethod => HttpMethod.HEAD;

    protected override IndexTemplateExistsRequest Initializer => new IndexTemplateExistsRequest(CallIsolatedValue);
    protected override string UrlPath => $"/_template/{CallIsolatedValue}";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Indices.TemplateExists(CallIsolatedValue, f),
        (client, f) => client.Indices.TemplateExistsAsync(CallIsolatedValue, f),
        (client, r) => client.Indices.TemplateExists(r),
        (client, r) => client.Indices.TemplateExistsAsync(r)
    );

    protected override IndexTemplateExistsDescriptor NewDescriptor() => new IndexTemplateExistsDescriptor(CallIsolatedValue);
}
