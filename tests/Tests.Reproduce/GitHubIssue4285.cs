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
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Reproduce;

public class GitHubIssue4285
{
    [U]
    public void CanReadAggBucketWithLongKey()
    {
        var json = @"{
				""took"" : 2612,
				""timed_out"" : false,
				""_shards"" : {
					""total"" : 3,
					""successful"" : 3,
					""skipped"" : 0,
					""failed"" : 0
				},
				""hits"" : {
					""total"" : {
						""value"" : 10000,
						""relation"" : ""gte""
					},
					""max_score"" : null,
					""hits"" : [ ]
				},
				""aggregations"" : {
					""terms#top_tags"" : {
						""buckets"" : [
						{
							""key"" : 3515753798950990007,
							""doc_count"" : 3
						}
						]
					}
				}
			}";

        var bytes = Encoding.UTF8.GetBytes(json);
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var connectionSettings = new ConnectionSettings(pool, new InMemoryConnection(bytes)).DefaultIndex("default_index");
        var client = new OpenSearchClient(connectionSettings);

        var response = client.Search<object>(s => s);

        var longTerms = response.Aggregations.Terms<long>("top_tags");
        longTerms.Buckets.Should().HaveCount(1);
        longTerms.Buckets.First().Key.Should().Be(3515753798950990007);

        var stringTerms = response.Aggregations.Terms("top_tags");
        stringTerms.Buckets.Should().HaveCount(1);
        stringTerms.Buckets.First().Key.Should().Be("3515753798950990007");
    }
}
