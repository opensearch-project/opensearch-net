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

using System.Linq;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.DocumentationTests;

namespace Tests.ClientConcepts.Troubleshooting;

/**
	 * === Debug mode
	 *
	 * The <<debug-information, Debug information>> explains that every response from OpenSearch.Net
	 * and OSC contains a `DebugInformation` property, and properties on `ConnectionSettings` and
	 * `RequestConfiguration` can control which additional information is included in debug information,
	 * for all requests or on a per request basis, respectively.
	 *
	 * During development, it can be useful to enable the most verbose debug information, to help
	 * identify and troubleshoot problems, or simply ensure that the client is behaving as expected.
	 * The `EnableDebugMode` setting on `ConnectionSettings` is a convenient shorthand for enabling
	 * verbose debug information, configuring a number of settings like
	 *
	 * * disabling direct streaming to capture request and response bytes
	 * * prettyfying JSON responses from OpenSearch
	 * * collecting TCP statistics when a request is made
	 * * collecting thread pool statistics when a request is made
	 * * including the OpenSearch stack trace in the response if there is a an error on the server side
	 */
public class DebugMode : IntegrationDocumentationTestBase, IClusterFixture<ReadOnlyCluster>
{
    public DebugMode(ReadOnlyCluster cluster) : base(cluster) { }

    [I]
    public void EnableDebugMode()
    {
        var pool = new StaticConnectionPool(Cluster.NodesUris());

        var settings = new ConnectionSettings(pool)
            .DefaultIndex(Client.ConnectionSettings.DefaultIndex)
            .EnableDebugMode(); // <1> configure debug mode
        settings = (ConnectionSettings)Cluster.UpdateSettings(settings);

        var client = new OpenSearchClient(settings);

        var response = client.Search<Project>(s => s
            .Query(q => q
                .MatchAll()
            )
        );

        var debugInformation = response.DebugInformation; // <2> verbose debug information

        debugInformation.Should().Contain("TCP states:");
        debugInformation.Should().Contain("ThreadPool statistics:");
    }

    /**
		 * In addition to exposing debug information on the response, debug mode will also cause the debug
		 * information to be written to the trace listeners in the `System.Diagnostics.Debug.Listeners` collection
		 * by default, when the request has completed. A delegate can be passed when enabling debug mode to perform
		 * a different action when a request has completed, using <<logging-with-on-request-completed, `OnRequestCompleted`>>
		 */
    public void DebugModeOnRequestCompleted()
    {
        var pool = new SingleNodeConnectionPool(Cluster.NodesUris().First());
        var settings = new ConnectionSettings(pool)
            .EnableDebugMode(apiCallDetails =>
            {
                // do something with the call details e.g. send with logging framework
            });
        settings = (ConnectionSettings)Cluster.UpdateSettings(settings);
        var client = new OpenSearchClient(settings);
    }
}
