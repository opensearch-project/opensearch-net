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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;

namespace Tests.ClientConcepts.Troubleshooting;

/**
	 * === Audit trail
	 *
	 * OpenSearch.Net and OSC provide an audit trail for the events within the request pipeline that
	 * occur when a request is made. This audit trail is available on the response as demonstrated in the
	 * following example.
	 */
public class AuditTrail : IClusterFixture<ReadOnlyCluster>
{
    private readonly ReadOnlyCluster _cluster;

    public AuditTrail(ReadOnlyCluster cluster) => _cluster = cluster;

    [I]
    public void AvailableOnResponse()
    {
        /**
			 * We'll use a Sniffing connection pool here since it sniffs on startup and pings before
			 * first usage, so we can get an audit trail with a few events out
			 */
        var pool = new SniffingConnectionPool(_cluster.NodesUris());
        var connectionSettings = new ConnectionSettings(pool)
            .DefaultMappingFor<Project>(i => i
                .IndexName("project")
            );

        connectionSettings = (ConnectionSettings)_cluster.UpdateSettings(connectionSettings);
        var client = new OpenSearchClient(connectionSettings);

        /**
			 * After issuing the following request
			 */
        var response = client.Search<Project>(s => s
            .MatchAll()
        );
        /**
			 * The audit trail is provided in the <<debug-information, Debug information>> in a human
			 * readable fashion, similar to
			 *
			 * ....
			 * Valid OSC response built from a successful low level call on POST: /project/doc/_search
			 * # Audit trail of this API call:
			 *  - [1] SniffOnStartup: Took: 00:00:00.0360264
			 *  - [2] SniffSuccess: Node: http://localhost:9200/ Took: 00:00:00.0310228
			 *  - [3] PingSuccess: Node: http://127.0.0.1:9200/ Took: 00:00:00.0115074
			 *  - [4] HealthyResponse: Node: http://127.0.0.1:9200/ Took: 00:00:00.1477640
			 * # Request:
			 * <Request stream not captured or already read to completion by serializer. Set DisableDirectStreaming() on ConnectionSettings to force it to be set on the response.>
			 * # Response:
			 * <Response stream not captured or already read to completion by serializer. Set DisableDirectStreaming() on ConnectionSettings to force it to be set on the response.>
			 * ....
			 *
			 * to help with troubleshootin
			 */
        var debug = response.DebugInformation;

        /**
			 * But can also be accessed manually:
			 */
        response.ApiCall.AuditTrail.Count.Should().Be(4, "{0}", debug);
        response.ApiCall.AuditTrail[0].Event.Should().Be(AuditEvent.SniffOnStartup, "{0}", debug);
        response.ApiCall.AuditTrail[1].Event.Should().Be(AuditEvent.SniffSuccess, "{0}", debug);
        response.ApiCall.AuditTrail[2].Event.Should().Be(AuditEvent.PingSuccess, "{0}", debug);
        response.ApiCall.AuditTrail[3].Event.Should().Be(AuditEvent.HealthyResponse, "{0}", debug);

        /**
			 * Each audit has a started and ended `DateTime` on it that will provide
			 * some understanding of how long it took
			 */
        response.ApiCall.AuditTrail
            .Should().OnlyContain(a => a.Ended - a.Started >= TimeSpan.Zero);

    }
}
