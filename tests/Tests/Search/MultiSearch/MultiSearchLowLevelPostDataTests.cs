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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.Search.MultiSearch;

public class MultiSearchLowLevelPostDataTests : IClusterFixture<ReadOnlyCluster>
{
    private readonly IOpenSearchClient _client;

    public MultiSearchLowLevelPostDataTests(ReadOnlyCluster cluster) => _client = cluster.Client;

    protected static List<object> Search => new object[]
    {
        new { },
        new { from = 0, size = 10, query = new { match_all = new { } } },
        new { search_type = "query_then_fetch" },
        new { },
        new { index = "devs" },
        new { from = 0, size = 5, query = new { match_all = new { } } },
        new { index = "devs" },
        new { from = 0, size = 5, query = new { match_all = new { } } }
    }.ToList();


    [I]
    public void PostEnumerableOfObjects()
    {
        var response = _client.LowLevel.MultiSearch<DynamicResponse>("project", PostData.MultiJson(Search));
        AssertResponse(response);
    }

    [I]
    public void PostEnumerableOfStrings()
    {
        var listOfStrings = Search
            .Select(s => _client.RequestResponseSerializer.SerializeToString(s, _client.ConnectionSettings.MemoryStreamFactory, SerializationFormatting.None))
            .ToList();

        var response = _client.LowLevel.MultiSearch<DynamicResponse>("project", PostData.MultiJson(listOfStrings));
        AssertResponse(response);
    }

    [I]
    public void PostString()
    {
        var str = Search
            .Select(s => _client.RequestResponseSerializer.SerializeToString(s, _client.ConnectionSettings.MemoryStreamFactory, SerializationFormatting.None))
            .ToList()
            .Aggregate(new StringBuilder(), (sb, s) => sb.Append(s + "\n"), sb => sb.ToString());

        var response = _client.LowLevel.MultiSearch<DynamicResponse>("project", str);
        AssertResponse(response);
    }

    [I]
    public void PostByteArray()
    {
        var str = Search
            .Select(s => _client.RequestResponseSerializer.SerializeToString(s, _client.ConnectionSettings.MemoryStreamFactory, SerializationFormatting.None))
            .ToList()
            .Aggregate(new StringBuilder(), (sb, s) => sb.Append(s + "\n"), sb => sb.ToString());

        var bytes = Encoding.UTF8.GetBytes(str);

        var response = _client.LowLevel.MultiSearch<DynamicResponse>("project", bytes);
        AssertResponse(response);
    }

    private static void AssertResponse(DynamicResponse response)
    {
        response.Success.Should().BeTrue();
        object o = response.Body;
        o.Should().NotBeNull();

        var b = response.Body;
        List<object> responses = b.responses;
        response.Should().NotBeNull("{0}", response.DebugInformation);
        responses.Count().Should().Be(4, "{0}", response.DebugInformation);

        object r = b.responses[0];
        r.Should().NotBeNull();

        object shards = b.responses[0]._shards;
        shards.Should().NotBeNull();

        int totalShards = b.responses[0]._shards.total;
        totalShards.Should().BeGreaterThan(0);
        //			JArray responses = r.responses;
        //
        //			responses.Count().Should().Be(4);
    }
}
