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
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.Document.Multiple.ReindexOnServer;

public class ReindexOnServerRemoteApiTests
    : ApiTestBase<IntrusiveOperationCluster, ReindexOnServerResponse, IReindexOnServerRequest, ReindexOnServerDescriptor, ReindexOnServerRequest>
{
    private readonly Uri _host = new Uri("http://myremoteserver.example:9200");

    public ReindexOnServerRemoteApiTests(IntrusiveOperationCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

    protected override object ExpectJson =>
        new
        {
            dest = new
            {
                index = $"{CallIsolatedValue}-clone",
            },
            source = new
            {
                remote = new
                {
                    host = "http://myremoteserver.example:9200",
                    username = "user",
                    password = "changeme",
                    socket_timeout = "1m",
                    connect_timeout = "10s"
                },
                index = CallIsolatedValue,
                size = 100
            }
        };

    protected override Func<ReindexOnServerDescriptor, IReindexOnServerRequest> Fluent => d => d
        .Source(s => s
            .Remote(r => r.Host(_host).Username("user").Password("changeme").SocketTimeout("1m").ConnectTimeout("10s"))
            .Index(CallIsolatedValue)
            .Size(100)
        )
        .Destination(s => s
            .Index(CallIsolatedValue + "-clone")
        );

    protected override HttpMethod HttpMethod => HttpMethod.POST;

    protected override ReindexOnServerRequest Initializer => new ReindexOnServerRequest()
    {
        Source = new ReindexSource
        {
            Remote = new RemoteSource
            {
                Host = _host,
                Username = "user",
                Password = "changeme",
                SocketTimeout = "1m",
                ConnectTimeout = "10s"
            },
            Index = CallIsolatedValue,
            Size = 100
        },
        Destination = new ReindexDestination
        {
            Index = CallIsolatedValue + "-clone",
        }
    };

    protected override bool SupportsDeserialization => false;
    protected override string UrlPath => $"/_reindex";

    protected override LazyResponses ClientUsage() => Calls(
        (client, f) => client.ReindexOnServer(f),
        (client, f) => client.ReindexOnServerAsync(f),
        (client, r) => client.ReindexOnServer(r),
        (client, r) => client.ReindexOnServerAsync(r)
    );
}
