/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
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
using BenchmarkDotNet.Attributes;
using OpenSearch.Net;
using Nest;
using Tests.Benchmarking.Framework;

namespace Tests.Benchmarking
{
	[BenchmarkConfig]
	public class ConnectionMechanicsBenchmarkTests
	{
		private IOpenSearchClient Client { get; set; }
		private PostData Post { get; set; }

		[GlobalSetup]
		public void Setup()
		{
			Post = PostData.String("{}");

			var nodes = Enumerable.Range(0, 100).Select(i => new Node(new Uri($"http://localhost:{i}"))).ToList();

			var connectionPool = new SniffingConnectionPool(nodes);
			var connection = new InMemoryConnection();
			var settings = new ConnectionSettings(connectionPool, connection)
				.SniffOnStartup(false)
				.SniffOnConnectionFault(false);
			Client = new OpenSearchClient(settings);

		}


		[Benchmark(Description = "SearchResponse", OperationsPerInvoke = 1000)]
		public void SearchResponse() => Client.LowLevel.Search<VoidResponse>("index", Post);
	}
}
