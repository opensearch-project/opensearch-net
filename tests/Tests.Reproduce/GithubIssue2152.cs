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
using Tests.Core.Extensions;

namespace Tests.Reproduce;

public class GithubIssue2152
{
    [U]
    public void CanDeserializeNestedBulkError()
    {
        var nestedCausedByError = @"{
			   ""took"": 4,
			   ""errors"": true,
			   ""items"": [{
					""update"": {
						""_index"": ""index-name"",
						""_type"": ""type-name"",
						""_id"": ""1"",
						""status"": 400,
						""error"": {
							""type"": ""illegal_argument_exception"",
							""reason"": ""failed to execute script"",
							""caused_by"": {
								""type"": ""script_exception"",
								""reason"": ""failed to run inline script [use(java.lang.Exception) {throw new Exception(\""Customized Exception\"")}] using lang [groovy]"",
								""caused_by"": {
									""type"": ""privileged_action_exception"",
									""reason"": null,
									""caused_by"": {
										""type"": ""exception"",
										""reason"": ""Custom Exception""
									}
								}
							}
						}
					}
				}]
			}";

        var bytes = Encoding.UTF8.GetBytes(nestedCausedByError);
        var connection = new InMemoryConnection(bytes);
        var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection);
        var client = new OpenSearchClient(settings);

        var bulkResponse = client.Bulk(new BulkDescriptor());

        bulkResponse.Errors.Should().BeTrue();

        var firstOperation = bulkResponse.ItemsWithErrors.First();
        firstOperation.Status.Should().Be(400);

        ShouldDeserialize(firstOperation.Error);
        ShouldDeserialize(firstOperation.Error.CausedBy);
        ShouldDeserialize(firstOperation.Error.CausedBy.CausedBy, true);
        ShouldDeserialize(firstOperation.Error.CausedBy.CausedBy.CausedBy);
    }

    [U]
    public void CanDeserializeNestedError()
    {
        var nestedCausedByError = @"{
				""status"": 400,
				""error"": {
					""type"": ""illegal_argument_exception"",
					""reason"": ""failed to execute script"",
					""caused_by"": {
						""type"": ""script_exception"",
						""reason"": ""failed to run inline script [use(java.lang.Exception) {throw new Exception(\""Customized Exception\"")}] using lang [groovy]"",
						""caused_by"": {
							""type"": ""privileged_action_exception"",
							""reason"": null,
							""caused_by"": {
								""type"": ""exception"",
								""reason"": ""Custom Exception""
							}
						}
					}
				}
			}";

        var bytes = Encoding.UTF8.GetBytes(nestedCausedByError);
        var connection = new InMemoryConnection(bytes, 400);
        var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection);
        var client = new OpenSearchClient(settings);

        var searchResponse = client.Search<object>(s => s.Index("index"));

        searchResponse.ShouldNotBeValid();
        var se = searchResponse.ServerError;
        se.Status.Should().Be(400);
        se.Should().NotBeNull();
        ShouldDeserialize(se.Error);
        ShouldDeserialize(se.Error.CausedBy);
        ShouldDeserialize(se.Error.CausedBy.CausedBy, true);
        ShouldDeserialize(se.Error.CausedBy.CausedBy.CausedBy);
    }

    private static void ShouldDeserialize(ErrorCause error, bool nullReason = false)
    {
        error.Should().NotBeNull();
        error.Type.Should().NotBeNullOrEmpty();
        if (nullReason)
            error.Reason.Should().BeNull();
        else
            error.Reason.Should().NotBeNullOrEmpty();
    }
}
