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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;

namespace Tests.Reproduce;

public class GitHubIssue4537
{
    [U]
    public void CanDeserializeSnapshotShardFailure()
    {
        var json = @"{
						  ""snapshots"": [
						    {
						      ""snapshot"": ""snapshot_2020-03-31t00:02:18z"",
						      ""uuid"": ""P9oZzuEfS8qbT-FVZLFSgw"",
						      ""version_id"": 7040299,
						      ""version"": ""1.0.0"",
						      ""indices"": [ ""someIndices"" ],
						      ""include_global_state"": true,
						      ""state"": ""FAILED"",
						      ""reason"": ""Indices don't have primary shards [someIndex]"",
						      ""start_time"": ""2020-03-31T00:02:21.478Z"",
						      ""start_time_in_millis"": 1585612941478,
						      ""end_time"": ""2020-03-31T00:02:25.353Z"",
						      ""end_time_in_millis"": 1585612945353,
						      ""duration_in_millis"": 3875,
						      ""failures"": [
						        {
						          ""index"": ""someIndex"",
						          ""index_uuid"": ""someIndex"",
						          ""shard_id"": 1,
						          ""reason"": ""primary shard is not allocated"",
						          ""status"": ""INTERNAL_SERVER_ERROR""
						        },
						        {
						          ""index"": ""someIndex"",
						          ""index_uuid"": ""someIndex"",
						          ""shard_id"": 0,
						          ""reason"": ""primary shard is not allocated"",
						          ""status"": ""INTERNAL_SERVER_ERROR""
						        },
						        {
						          ""index"": ""someIndex"",
						          ""index_uuid"": ""someIndex"",
						          ""shard_id"": 2,
						          ""reason"": ""primary shard is not allocated"",
						          ""status"": ""INTERNAL_SERVER_ERROR""
						        }
						      ],
						      ""shards"": {
						        ""total"": 78,
						        ""failed"": 3,
						        ""successful"": 75
						      }
						    }
						  ]
						}";

        var client = TestClient.FixedInMemoryClient(Encoding.UTF8.GetBytes(json));

        var action = () => client.Snapshot.Get("repo", "snapshot_2020-03-31t00:02:18z");

        action.Should().NotThrow();

        var response = action();

        var failures = response.Snapshots.First().Failures;
        failures.Should().HaveCount(3);
        failures.First().ShardId.Should().Be("1");
    }
}
