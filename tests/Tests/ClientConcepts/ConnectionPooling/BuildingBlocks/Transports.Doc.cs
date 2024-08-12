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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Domain;

namespace Tests.ClientConcepts.ConnectionPooling.BuildingBlocks;

public class Transports
{
    /**=== Transports
		*
		* The `ITransport` interface can be seen as the motor block of the client. Its interface is
		* deceitfully simple, yet it's ultimately responsible for translating a client call to a response.
		*
		* If for some reason you do not agree with the way we wrote the internals of the client,
		* by implementing a custom `ITransport`, you can circumvent all of it and introduce your own.
		*/
    public async Task InterfaceExplained()
    {
        /**
			* Transport is generically typed to a type that implements `IConnectionConfigurationValues`
			* This is the minimum `ITransport` needs to report back for the client to function.
			*
			* In the low level client, `OpenSearchLowLevelClient`, a `Transport` is instantiated like this:
			*/
        var lowLevelTransport = new Transport<ConnectionConfiguration>(new ConnectionConfiguration());

        /** and in the high level client, `OpenSearchClient`, like this */
        var highlevelTransport = new Transport<ConnectionSettings>(new ConnectionSettings());

        var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var inMemoryTransport = new Transport<ConnectionSettings>(
            new ConnectionSettings(connectionPool, new InMemoryConnection()));

        /**
			* The only two methods on `ITransport` are `Request()` and `RequestAsync()`; the default `ITransport` implementation is responsible for introducing
			* many of the building blocks in the client. If you feel that the defaults do not work for you then you can swap them out for your own
			* custom `ITransport` implementation and if you do, {github}/issues[please let us know] as we'd love to learn why you've go down this route!
			*/
        var response = inMemoryTransport.Request<SearchResponse<Project>>(
            HttpMethod.GET,
            "/_search",
            PostData.Serializable(new { query = new { match_all = new { } } }));

        response = await inMemoryTransport.RequestAsync<SearchResponse<Project>>(
            HttpMethod.GET,
            "/_search",
            default(CancellationToken),
            PostData.Serializable(new { query = new { match_all = new { } } }));
    }
}
