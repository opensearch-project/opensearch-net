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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using Osc;

namespace Tests.Reproduce
{
	public class GithubIssue1906
	{
		[U] public void SearchDoesNotTakeDefaultIndexIntoAccount()
		{
			var node = new Uri("http://localhost:9200");
			var connectionPool = new SingleNodeConnectionPool(node);
			var connectionSettings = new ConnectionSettings(connectionPool, new InMemoryConnection())
				.DefaultIndex("GithubIssue1906-*")
				.DefaultFieldNameInferrer(p => p)
				.OnRequestCompleted(info =>
				{
					// info.Uri is /_search/ without the default index
					// my OpenSearch instance throws an error on the .opensearchDashboards index (@timestamp field not mapped because I sort on @timestamp)
				});

			var client = new OpenSearchClient(connectionSettings);
			var response = client.Search<OpenSearchLogEvent>(s => s);

			response.ApiCall.Uri.AbsolutePath.Should().Be("/GithubIssue1906-%2A/_search");

			response = client.Search<OpenSearchLogEvent>(new SearchRequest<OpenSearchLogEvent>());
			response.ApiCall.Uri.AbsolutePath.Should().Be("/GithubIssue1906-%2A/_search");

			response = client.Search<OpenSearchLogEvent>(new SearchRequest());
			response.ApiCall.Uri.AbsolutePath.Should().Be("/_search");
		}

		private class OpenSearchLogEvent { }
	}
}
