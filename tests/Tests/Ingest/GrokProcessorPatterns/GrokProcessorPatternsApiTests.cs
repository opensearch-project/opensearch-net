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

namespace Tests.Ingest.GrokProcessorPatterns;

public class GrokProcessorPatternsApiTests
    : ApiIntegrationTestBase<ReadOnlyCluster, GrokProcessorPatternsResponse, IGrokProcessorPatternsRequest, GrokProcessorPatternsDescriptor,
        GrokProcessorPatternsRequest>
{
    public GrokProcessorPatternsApiTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override bool ExpectIsValid => true;
    protected override int ExpectStatusCode => 200;

    protected override Func<GrokProcessorPatternsDescriptor, IGrokProcessorPatternsRequest> Fluent => d => d;

    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override GrokProcessorPatternsRequest Initializer => new GrokProcessorPatternsRequest();
    protected override string UrlPath => $"/_ingest/processor/grok";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.Ingest.GrokProcessorPatterns(f),
        (client, f) => client.Ingest.GrokProcessorPatternsAsync(f),
        (client, r) => client.Ingest.GrokProcessorPatterns(r),
        (client, r) => client.Ingest.GrokProcessorPatternsAsync(r)
    );
}
