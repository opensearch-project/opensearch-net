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
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;

namespace Tests.Reproduce;

public class GithubIssue2309
{
    [U]
    public void FailedReIndexResponseMarkedAsInvalidAndContainFailures()
    {
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

        var json = @"{
				""took"": 4,
				""timed_out"": false,
				""total"": 1,
				""updated"": 0,
				""created"": 0,
				""deleted"": 0,
				""batches"": 1,
				""version_conflicts"": 0,
				""noops"": 0,
				""retries"": {
					""bulk"": 0,
					""search"": 0
				},
				""throttled_millis"": 0,
				""requests_per_second"": -1.0,
				""throttled_until_millis"": 0,
				""failures"": [{
					""index"": ""employees-v2"",
					""id"": ""57f7ce8df8a10336a0cf935b"",
					""cause"": {
						""type"": ""mapper_parsing_exception"",
						""reason"": ""failed to parse [id]"",
						""caused_by"": {
							""type"": ""number_format_exception"",
							""reason"": ""For input string: \""57f7ce8df8a10336a0cf935b\""""
						}
					},
					""status"": 400
				}]
			}";

        var connection = new InMemoryConnection(Encoding.UTF8.GetBytes(json), 400);
        var settings = new ConnectionSettings(pool, connection);
        var client = new OpenSearchClient(settings);

        var reindexResponse = client.ReindexOnServer(r => r
            .Source(s => s
                .Index("employees-v1")
            )
            .Destination(d => d
                .Index("employees-v2")
            )
            .Conflicts(Conflicts.Proceed)
        );

        reindexResponse.ShouldNotBeValid();
        reindexResponse.Failures.Should().NotBeNull().And.HaveCount(1);
    }
}
