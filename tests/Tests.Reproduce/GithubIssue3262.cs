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

namespace Tests.Reproduce;

public class GithubIssue3262
{
    [U]
    public void CanDeserializeSingleFilterAndCharacterFilter()
    {
        var json = @"{
			  ""products_purchasing"": {
				""settings"": {
				  ""index"": {
					""number_of_shards"": ""5"",
					""provided_name"": ""products_purchasing"",
					""creation_date"": ""1510930394462"",
					""analysis"": {
					  ""normalizer"": {
						""lowercase"": {
						  ""filter"": ""lowercase"",
						  ""type"": ""custom"",
						  ""char_filter"": ""words""
						}
					  },
					  ""analyzer"": {
						""whitespace_lowercase"": {
						  ""filter"": ""lowercase"",
						  ""type"": ""custom"",
						  ""tokenizer"": ""whitespace""
						},
						""keyword_lowercase"": {
						  ""filter"": ""lowercase"",
						  ""type"": ""custom"",
						  ""tokenizer"": ""keyword""
						}
					  },
					  ""char_filter"": {
						""words"": {
						  ""pattern"": ""[(^([^A-Za-z\\s]*\\s)*|[^A-Za-z\\s])]"",
						  ""type"": ""pattern_replace"",
						  ""replacement"": """"
						}
					  }
					},
					""number_of_replicas"": ""1"",
					""uuid"": ""peH2yqVLTIOc4ScTUYTtEA"",
					""version"": {
					  ""created"": ""5050099""
					}
				  }
				}
			  }
			}";

        var bytes = Encoding.UTF8.GetBytes(json);

        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var connectionSettings = new ConnectionSettings(pool, new InMemoryConnection(bytes));
        var client = new OpenSearchClient(connectionSettings);

        Action getIndexRequest = () => client.Indices.Get(new GetIndexRequest("products_purchasing"));

        getIndexRequest.Should().NotThrow();

        var response = client.Indices.Get(new GetIndexRequest("products_purchasing"));

        var normalizer = response.Indices["products_purchasing"].Settings.Analysis.Normalizers["lowercase"] as ICustomNormalizer;
        normalizer.Should().NotBeNull();
        normalizer.Filter.Should().NotBeNull().And.HaveCount(1);
    }
}
