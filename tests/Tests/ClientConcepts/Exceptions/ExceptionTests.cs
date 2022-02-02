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
using OpenSearch.Net;
using FluentAssertions;
using Osc;
using Tests.Core.Client.Settings;
using Tests.Core.ManagedElasticsearch;
using Tests.Core.ManagedElasticsearch.Clusters;
using Tests.Domain;
using Xunit;

namespace Tests.ClientConcepts.Exceptions
{
	public class ExceptionTests : ClusterTestClassBase<WritableCluster>
	{
		private readonly int _port;

		public ExceptionTests(WritableCluster cluster) : base(cluster) => _port = cluster.Nodes.First().Port ?? 9200;

		//[I]
		public void ServerTestWhenThrowExceptionsEnabled()
		{
			var settings = new ConnectionSettings(TestConnectionSettings.CreateUri(_port))
				.ThrowExceptions();
			var client = new OpenSearchClient(settings);
			var exception = Assert.Throws<OpenSearchClientException>(() => client.Indices.GetMapping<Project>(s => s.Index("doesntexist")));
			// HttpClient does not throw on "known error" status codes (i.e. 404) thus the inner exception should not be set
			exception.InnerException.Should().BeNull();
			exception.Response.Should().NotBeNull();
		}

		//[I]
		public void ClientTestWhenThrowExceptionsEnabled()
		{
			var settings = new ConnectionSettings(new Uri("http://doesntexist:9200"))
				.ThrowExceptions();
			var client = new OpenSearchClient(settings);
			var exception = Assert.Throws<OpenSearchClientException>(() => client.RootNodeInfo());
			var inner = exception.InnerException;
			// HttpClient does not throw on "known error" status codes (i.e. 404) thus OriginalException should not be set
			inner.Should().BeNull();
		}

		//[I]
		public void ServerTestWhenThrowExceptionsDisabled()
		{
			var settings = new ConnectionSettings(TestConnectionSettings.CreateUri(_port));
			var client = new OpenSearchClient(settings);
			var response = client.Indices.GetMapping<Project>(s => s.Index("doesntexist"));
			// HttpClient does not throw on "known error" status codes (i.e. 404) thus OriginalException should not be set
			response.ApiCall.OriginalException.Should().BeNull();
		}

		//[I]
		public void ClientTestWhenThrowExceptionsDisabled()
		{
			var settings = new ConnectionSettings(new Uri("http://doesntexist:9200"));
			var client = new OpenSearchClient(settings);
			var response = client.RootNodeInfo();
			// HttpClient does not throw on "known error" status codes (i.e. 404) thus OriginalException should not be set
			response.ApiCall.OriginalException.Should().BeNull();
		}

		//TODO figure out a way to trigger this again
		//[U]
		public void DispatchIndicatesMissingRouteValues()
		{
			var settings = new ConnectionSettings(new Uri("http://doesntexist:9200"));
			var client = new OpenSearchClient(settings);

			Action dispatch = () => client.Index(new Project(), p => p.Index(null));
			var ce = dispatch.Should().Throw<ArgumentException>();
			ce.Should().NotBeNull();
			ce.Which.Message.Should().Contain("index=<NULL>");
		}
	}
}
