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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Domain;
using Tests.Framework;

namespace Tests.ClientConcepts.Connection;

/**[[modifying-default-connection]]
	 * === Modifying the default connection
	 *
	 * The client abstracts sending the request and creating a response behind `IConnection` and the default
	 * implementation uses https://msdn.microsoft.com/en-us/library/system.net.http.httpclient(v=vs.118).aspx[`System.Net.Http.HttpClient`].
	 *
	 * Why would you ever want to pass your own `IConnection`? Let's look at a couple of examples
	 */
public class ModifyingTheDefaultConnection
{
    /**==== Using InMemoryConnection
		 *
		 * `InMemoryConnection` is an in-built `IConnection` that makes it easy to write unit tests against. It can be
		 * configured to respond with default response bytes, HTTP status code and an exception when a call is made.
		 *
		 * `InMemoryConnection` **doesn't actually send any requests or receive any responses from OpenSearch**;
		 * requests are still serialized and the request bytes can be obtained on the response if `.DisableDirectStreaming` is
		 * set to `true` on the request or globally
		 */
    public void InMemoryConnectionDefaultCtor()
    {
        var connection = new InMemoryConnection();
        var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var settings = new ConnectionSettings(connectionPool, connection);
        var client = new OpenSearchClient(settings);
    }

    /**
		 * Here we create a new `ConnectionSettings` by using the overload that takes a `IConnectionPool` and an `IConnection`.
		 * We pass it an `InMemoryConnection` which, using the default parameterless constructor,
		 * will return 200 for everything and never actually perform any IO.
		 *
		 * Let's see a more complex example
		 */
    [U]
    public void InMemoryConnectionOverloadedCtor()
    {
        var response = new
        {
            took = 1,
            timed_out = false,
            _shards = new
            {
                total = 2,
                successful = 2,
                failed = 0
            },
            hits = new
            {
                total = new { value = 25 },
                max_score = 1.0,
                hits = Enumerable.Range(1, 25).Select(i => (object)new
                {
                    _index = "project",
                    _type = "project",
                    _id = $"Project {i}",
                    _score = 1.0,
                    _source = new { name = $"Project {i}" }
                }).ToArray()
            }
        };

        var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
        var connection = new InMemoryConnection(responseBytes, 200); // <1> `InMemoryConnection` is configured to **always** return `responseBytes` along with a 200 HTTP status code
        var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var settings = new ConnectionSettings(connectionPool, connection).DefaultIndex("project");
        var client = new OpenSearchClient(settings);

        var searchResponse = client.Search<Project>(s => s.MatchAll());

        /**
			 * We can now assert that the `searchResponse` is valid and contains documents deserialized
			 * from our fixed `InMemoryConnection` response
			 */
        searchResponse.ShouldBeValid();
        searchResponse.Documents.Count.Should().Be(25);
    }

    /**
		* ==== Changing HttpConnection
		*
		* There may be a need to change how the default `HttpConnection` works, for example, to add an X509 certificate
		* to the request, change the maximum number of connections allowed to an endpoint, etc.
		*
		* By deriving from `HttpConnection`, it is possible to change the behaviour of the connection. The following
		* provides some examples
		*
		*/
    public class MyCustomHttpConnection : HttpConnection
    {
        protected override HttpRequestMessage CreateRequestMessage(RequestData requestData)
        {
            var message = base.CreateRequestMessage(requestData);
            var header = string.Empty;
            message.Headers.Authorization = new AuthenticationHeaderValue("Negotiate", header);
            return message;
        }
    }

    /*
		* [[kerberos-authentication]]
		* ===== Kerberos Authentication
		*
		* For a lot of use cases subclassing HttpConnection is a great way to customize the http connection for your needs.
		* E.g if you want to authenticate with Kerberos, creating a custom HttpConnection as followed allows you to set the right HTTP headers.
		*
		*
		* TIP use something like https://www.nuget.org/packages/Kerberos.NET/ to fill in the actual blanks of this implementation
		*/
    public class KerberosConnection : HttpConnection
    {
        protected override HttpRequestMessage CreateRequestMessage(RequestData requestData)
        {
            var message = base.CreateRequestMessage(requestData);
            var header = string.Empty;
            message.Headers.Authorization = new AuthenticationHeaderValue("Negotiate", header);
            return message;
        }
    }
    /**
		 * See <<working-with-certificates, Working with certificates>> for further details.
		 */
}
