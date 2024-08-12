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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.Sdk;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;
using Xunit;
// ReSharper disable SuggestVarOrType_Elsewhere
// ReSharper disable SuggestVarOrType_BuiltInTypes

namespace Tests.ClientConcepts.LowLevel;

/**[[opensearch-net-getting-started]]
	 * == Getting started
	 *
	 * OpenSearch.Net is a low level OpenSearch .NET client that has no dependencies on other libraries
	 * and is unopinionated about how you build your requests and responses.
	 *
	 */
public class GettingStarted : IClusterFixture<WritableCluster>
{
    private readonly WritableCluster _cluster;

    public GettingStarted(WritableCluster cluster) => _cluster = cluster;

    private IOpenSearchLowLevelClient lowlevelClient = new OpenSearchLowLevelClient(new ConnectionConfiguration(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), new InMemoryConnection()));

    /**[float]
		 * === Connecting
		 *
		 * To connect to OpenSearch running locally at `http://localhost:9200` is as simple as
		 * instantiating a new instance of the client
		 */
    public void SimpleInstantiation()
    {
        var lowlevelClient = new OpenSearchLowLevelClient();
    }

    /**
		 * Often you may need to pass additional configuration options to the client such as the address of OpenSearch if it's running on
		 * a remote machine. This is where `ConnectionConfiguration` comes in; an instance can be instantiated to provide
		 * the client with different configuration values.
		 */
    public void UsingConnectionSettings()
    {
        var settings = new ConnectionConfiguration(new Uri("http://example.com:9200"))
            .RequestTimeout(TimeSpan.FromMinutes(2));

        var lowlevelClient = new OpenSearchLowLevelClient(settings);
    }

    /**
		 * In this example, a default request timeout was also specified that will be applied to all requests, to determine after how long the client should cancel a request.
		 * There are many other <<configuration-options,configuration options>> on `ConnectionConfiguration` to control things such as
		 *
		 * - Basic Authentication header to send with all requests
		 * - whether the client should connect through a proxy
		 * - whether HTTP compression support should be enabled on the client
		 *
		 * [float]
		 * ==== Connection pools
		 *
		 * `ConnectionConfiguration` is not restricted to being passed a single address for OpenSearch. There are several different
		 * types of <<connection-pooling,Connection pool>> available, each with different characteristics that can be used to
		 * configure the client. The following example uses a <<sniffing-connection-pool,SniffingConnectionPool>> seeded with the addresses
		 * of three OpenSearch nodes in the cluster, and the client will use this type of pool to maintain a list of available nodes within the
		 * cluster to which it can send requests in a round-robin fashion.
		 */
    public void UsingConnectionPool()
    {
        var uris = new[]
        {
            new Uri("http://localhost:9200"),
            new Uri("http://localhost:9201"),
            new Uri("http://localhost:9202"),
        };

        var connectionPool = new SniffingConnectionPool(uris);
        var settings = new ConnectionConfiguration(connectionPool);

        var lowlevelClient = new OpenSearchLowLevelClient(settings);
    }

    /**[float]
		 * === Indexing
		 *
		 * Once a client had been configured to connect to OpenSearch, we need to get some data into the cluster to work with.
		 * Imagine we have the following http://en.wikipedia.org/wiki/Plain_Old_CLR_Object[Plain Old CLR Object (POCO)]
		 */
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    /**
		 * Indexing a single instance of this POCO, either synchronously or asynchronously, is as simple as
		 */
    public async Task Indexing()
    {
        var person = new Person
        {
            FirstName = "Martijn",
            LastName = "Laarman"
        };

        var ndexResponse = lowlevelClient.Index<BytesResponse>("people", "1", PostData.Serializable(person)); //<1> synchronous method that returns an `IndexResponse`
        var responseBytes = ndexResponse.Body;

        var asyncIndexResponse = await lowlevelClient.IndexAsync<StringResponse>("people", "1", PostData.Serializable(person)); //<2> asynchronous method that returns a `Task<IndexResponse>` that can be awaited
        var responseString = asyncIndexResponse.Body;
    }

    /**
		 * NOTE: All available methods within OpenSearch.Net are exposed as both synchronous and asynchronous versions,
		 * with the latter using the idiomatic *Async suffix for the method name.
		 *
		 * Both index requests will index the document to the endpoint `/people/person/1`.
		 *
		 * An https://msdn.microsoft.com/en-us/library/bb397696.aspx[anonymous type] can also be used to represent the document to index
		 */
    public async Task IndexingWithAnonymousType()
    {
        var person = new
        {
            FirstName = "Martijn",
            LastName = "Laarman"
        };

        var ndexResponse = await lowlevelClient.IndexAsync<BytesResponse>("people", "1", PostData.Serializable(person));
        var responseStream = ndexResponse.Body;
    }

    /**
		* For API's that take a body you can send the body as an (anonymous) object, byte[], string, stream. Additionally for API's that
		* take multilined json you can also send a list of objects or a list of bytes to help you format this. These are all encapsulated
		* by `PostData` and you can use the static methods on that class to send the body in whatever form you have it.
		* Check out the documentation on <<post-data, Post Data>> to see all of these permutations in action.
		*
		* The generic type parameter on the method specifies the type of the response body. In the last example, we return the response as a
		* string from OpenSearch, forgoing any deserialization.
		*
		* [float]
		* ==== Bulk indexing
		*
		* If you need to index many documents, OpenSearch has a {ref_current}/docs-bulk.html[Bulk API] that can be used to perform many operations in one request
		*/
    public void BulkIndexing()
    {
        var people = new object[]
        {
            new { index = new { _index = "people", _type = "person", _id = "1"  }},
            new { FirstName = "Martijn", LastName = "Laarman" },
            new { index = new { _index = "people", _type = "person", _id = "2"  }},
            new { FirstName = "Greg", LastName = "Marzouka" },
            new { index = new { _index = "people", _type = "person", _id = "3"  }},
            new { FirstName = "Russ", LastName = "Cam" },
        };

        var ndexResponse = lowlevelClient.Bulk<StringResponse>(PostData.MultiJson(people));
        var responseStream = ndexResponse.Body;
    }
    /**
		* The client will serialize each item seperately and join items up using the `\n` character as required by the Bulk API. Refer to the
		* OpenSearch Bulk API documentation for further details and supported operations.
		*
		* [float]
		* === Searching
		*
		* Now that we have indexed some documents we can begin to search for them.
		*
		* The OpenSearch Query DSL can be expressed using an anonymous type within the request
		*/
    public void SearchingWithAnonymousTypes()
    {
        var searchResponse = lowlevelClient.Search<StringResponse>("people", PostData.Serializable(new
        {
            from = 0,
            size = 10,
            query = new
            {
                match = new
                {
                    firstName = new
                    {
                        query = "Martijn"
                    }
                }
            }
        }));

        var successful = searchResponse.Success;
        var responseJson = searchResponse.Body;
    }

    /**
		 * `responseJson` now holds a JSON string for the response. The search endpoint for this query is
		 * `/people/person/_search` and it's possible to search over multiple indices and types by changing the arguments
		 * supplied in the request for `index` and `type`, respectively.
		 *
		 * Strings can also be used to express the request
		 */
    public void SearchingWithStrings()
    {
        var searchResponse = lowlevelClient.Search<BytesResponse>("people", @"
			{
				""from"": 0,
				""size"": 10,
				""query"": {
					""match"": {
						""firstName"": {
						    ""query"": ""Martijn""
					    }
					}
				}
			}");

        var responseBytes = searchResponse.Body;
    }

    /**
		* As you can see, using strings is a little more cumbersome than using anonymous types because of the need to escape
		* double quotes, but it can be useful at times nonetheless. `responseBytes` will contain
		* the bytes of the response from OpenSearch.
		*
		* [NOTE]
		* --
		* OpenSearch.Net does not provide typed objects to represent responses; if you need this, you should consider
		* using <<osc, OSC, the high level client>>, that does map all requests and responses to types. You can work with
		* strong types with OpenSearch.Net but it will be up to you as the developer to configure OpenSearch.Net so that
		* it understands how to deserialize your types, most likely by providing your own <<custom-serialization, IOpenSearchSerializer>> implementation
		* to `ConnectionConfiguration`.
		* --
		*
		* [float]
		* === Handling Errors
		*
		* By default, OpenSearch.Net is configured not to throw exceptions if a HTTP response status code is returned that is not in
		* the 200-300 range, nor an expected response status code allowed for a given request e.g. checking if an index exists
		* can return a 404.
		*
		* The response from low level client calls provides a number of properties that can be used to determine if a call
		* is successful
		*/
    public void ResponseProperties()
    {
        var searchResponse = lowlevelClient.Search<BytesResponse>("people", PostData.Serializable(new { match_all = new { } }));

        var success = searchResponse.Success; // <1> Response is in the 200 range, or an expected response for the given request
        var successOrKnownError = searchResponse.SuccessOrKnownError; // <2> Response is successful, or has a response code between 400-599 that indicates the request cannot be retried.
        var exception = searchResponse.OriginalException; // <3> If the response is unsuccessful, will hold the original exception.
    }

    /**
		* Using these details, it is possible to make decisions around what should be done in your application.
		*
		* The default behaviour of not throwing exceptions can be changed by setting `.ThrowExceptions()` on `ConnectionConfiguration`
		*/
    public void HandlingErrors()
    {
        var settings = new ConnectionConfiguration(new Uri("http://example.com:9200"))
            .ThrowExceptions();

        var lowlevelClient = new OpenSearchLowLevelClient(settings);
    }

    /**
		 * And if more fine grained control is required, custom exceptions can be thrown using `.OnRequestCompleted()` on
		 * `ConnectionConfiguration`
		 */
    public void FineGrainedControl()
    {
        var settings = new ConnectionConfiguration(new Uri("http://example.com:9200"))
            .OnRequestCompleted(apiCallDetails =>
            {
                if (apiCallDetails.HttpStatusCode == 418)
                {
                    throw new TimeForACoffeeException();
                }
            });

        var lowlevelClient = new OpenSearchLowLevelClient(settings);
    }

    // hide
    private class TimeForACoffeeException : Exception { }
}
