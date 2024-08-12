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
using System.Net.NetworkInformation;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Diagnostics;
using OpenSearch.OpenSearch.Xunit.Sdk;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Core.Client.Settings;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework;
using Tests.Framework.DocumentationTests;
using Xunit;

namespace Tests.ClientConcepts.Troubleshooting;

/**
	 * === Debug information
	 *
	 * Every response from OpenSearch.Net and OpenSearch.Client contains a `DebugInformation` property
	 * that provides a human readable description of what happened during the request for both successful and
	 * failed requests
	 */
public class DebugInformation : IntegrationDocumentationTestBase, IClusterFixture<ReadOnlyCluster>
{
    public DebugInformation(ReadOnlyCluster cluster) : base(cluster) { }

    [I]
    public void DefaultDebug()
    {
        // hide
        var client = Client;

        var response = client.Search<Project>(s => s
            .Query(q => q
                .MatchAll()
            )
        );

        response.DebugInformation.Should().Contain("Valid OpenSearch.Client response");
    }
    //hide
    [U]
    public void PasswordIsNotExposedInDebugInformation()
    {
        // hide
        var client = new OpenSearchClient(new AlwaysInMemoryConnectionSettings()
            .DefaultIndex("index")
            .BasicAuthentication("user1", "pass2")
        );

        var response = client.Search<Project>(s => s
            .Query(q => q
                .MatchAll()
            )
        );

        response.DebugInformation.Should().NotContain("pass2");
    }
    //hide
    [U]
    public void ApiKeyIsNotExposedInDebugInformation()
    {
        // hide
        var client = new OpenSearchClient(new AlwaysInMemoryConnectionSettings()
            .DefaultIndex("index")
            .ApiKeyAuthentication("id1", "api_key1")
        );

        var response = client.Search<Project>(s => s
            .Query(q => q
                .MatchAll()
            )
        );

        response.DebugInformation.Should().NotContain("api_key1");
    }

    //hide
    [U]
    public void PasswordIsNotExposedInDebugInformationWhenPartOfUrl()
    {
        // hide
        var pool = new SingleNodeConnectionPool(new Uri("http://user1:pass2@localhost:9200"));
        var client = new OpenSearchClient(new ConnectionSettings(pool, new InMemoryConnection())
            .DefaultIndex("index")
        );

        var response = client.Search<Project>(s => s
            .Query(q => q
                .MatchAll()
            )
        );

        response.DebugInformation.Should().NotContain("pass2");
    }
    /**
		 * This can be useful in tracking down numerous problems and can also be useful when filing an
		 * {github}/issues[issue] on the GitHub repository.
		 *
		 * ==== Request and response bytes
		 *
		 * By default, the request and response bytes are not available within the debug information, but
		 * can be enabled globally on Connection Settings by setting `DisableDirectStreaming`. This
		 * disables direct streaming of
		 *
		 * . the serialized request type to the request stream
		 * . the response stream to a deserialized response type
		 */
    public void DisableDirectStreaming()
    {
        var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

        var settings = new ConnectionSettings(connectionPool)
            .DisableDirectStreaming(); // <1> disable direct streaming for *all* requests

        var client = new OpenSearchClient(settings);
    }

    /**
		 * or on a _per request_ basis
		 */
    [I]
    public void DisableDirectStreamingPerRequest()
    {
        // hide
        var client = TestClient.DefaultInMemoryClient;

        var response = client.Search<Project>(s => s
            .RequestConfiguration(r => r
                .DisableDirectStreaming() // <1> disable direct streaming for *this* request only
            )
            .Query(q => q
                .MatchAll()
            )
        );

        // hide
        response.DebugInformation.Should().Contain("\"match_all\":");
    }

    /**
		 * Configuring `DisableDirectStreaming` on an individual request takes precedence over
		 * any global configuration.
		 *
		 * There is typically a performance and allocation cost associated with disabling direct streaming
		 * since both the request and response bytes must be buffered in memory, to allow them to be
		 * exposed on the response call details.
		 *
		 * ==== TCP statistics
		 *
		 * It can often be useful to see the statistics for active TCP connections, particularly when
		 * trying to diagnose issues with the client. The client can collect the states of active TCP
		 * connections just before making a request, and expose these on the response and in the debug
		 * information.
		 *
		 * Similarly to `DisableDirectStreaming`, TCP statistics can be collected for every request
		 * by configuring on `ConnectionSettings`
		 */
    public void ConnectionSettingsTcpStats()
    {
        var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

        var settings = new ConnectionSettings(connectionPool)
            .EnableTcpStats(); // <1> collect TCP statistics for *all* requests

        var client = new OpenSearchClient(settings);
    }

    /**
		 * or on a _per request_ basis
		 */
    [I]
    public void RequestConfigurationTcpStats()
    {
        // hide
        var client = Client;

        var response = client.Search<Project>(s => s
            .RequestConfiguration(r => r
                .EnableTcpStats() // <1> collect TCP statistics for *this* request only
            )
            .Query(q => q
                .MatchAll()
            )
        );

        var debugInformation = response.DebugInformation;

        // hide
        debugInformation.Should().Contain("TCP states:");
    }

    /**
		 * With `EnableTcpStats` set, the states of active TCP connections will now be included
		 * on the response and in the debug information.
		 *
		 * The client includes a `TcpStats`
		 * class to help with retrieving more detail about active TCP connections should it be
		 * required
		 */
    [I]
    public void TcpStatistics()
    {
        // hide
        var client = Client;

        var tcpStatistics = TcpStats.GetActiveTcpConnections(); // <1> Retrieve details about active TCP connections, including local and remote addresses and ports
        var ipv4Stats = TcpStats.GetTcpStatistics(NetworkInterfaceComponent.IPv4); // <2> Retrieve statistics about IPv4
        var ipv6Stats = TcpStats.GetTcpStatistics(NetworkInterfaceComponent.IPv6); // <3> Retrieve statistics about IPv6

        var response = client.Search<Project>(s => s
            .Query(q => q
                .MatchAll()
            )
        );
    }

    /**
    * [NOTE]
    * --
    * Collecting TCP statistics may not be accessible in all environments, for example, Azure App Services.
    * When this is the case, `TcpStats.GetActiveTcpConnections()` returns `null`.
    * --
    * 
    * ==== ThreadPool statistics
    *
    * It can often be useful to see the statistics for thread pool threads, particularly when
    * trying to diagnose issues with the client. The client can collect statistics for both
    * worker threads and asynchronous I/O threads, and expose these on the response and
    * in debug information.
    *
    * Similar to collecting TCP statistics, ThreadPool statistics can be collected for all requests
    * by configuring `EnableThreadPoolStats` on `ConnectionSettings`
    */
    public void ConnectionSettingsThreadPoolStats()
    {
        var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

        var settings = new ConnectionSettings(connectionPool)
            .EnableThreadPoolStats(); // <1> collect thread pool statistics for *all* requests

        var client = new OpenSearchClient(settings);
    }

    /**
    * or on a _per request_ basis
    */
    [I]
    public void RequestConfigurationThreadPoolStats()
    {
        // hide
        var client = Client;

        var response = client.Search<Project>(s => s
            .RequestConfiguration(r => r
                    .EnableThreadPoolStats() // <1> collect thread pool statistics for *this* request only
            )
            .Query(q => q
                .MatchAll()
            )
        );

        var debugInformation = response.DebugInformation; // <2> contains thread pool statistics

        // hide
        debugInformation.Should().Contain("ThreadPool statistics:");
    }
    /**
    * With `EnableThreadPoolStats` set, the statistics of thread pool threads will now be included
    * on the response and in the debug information.
    */
}
