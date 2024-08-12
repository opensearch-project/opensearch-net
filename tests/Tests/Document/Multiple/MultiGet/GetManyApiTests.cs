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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client.Settings;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;

namespace Tests.Document.Multiple.MultiGet;

public class GetManyApiTests : IClusterFixture<WritableCluster>
{
    private readonly IOpenSearchClient _client;
    private readonly WritableCluster _cluster;
    private readonly IEnumerable<long> _ids = Developer.Developers.Select(d => d.Id).Take(10);

    public GetManyApiTests(WritableCluster cluster)
    {
        _client = cluster.Client;
        _cluster = cluster;
    }

    [I]
    public void UsesDefaultIndexAndInferredType()
    {
        var response = _client.GetMany<Developer>(_ids);
        response.Count().Should().Be(10);
        foreach (var hit in response)
        {
            hit.Index.Should().NotBeNullOrWhiteSpace();
            if (_cluster.ClusterConfiguration.Version < "2.0.0")
                hit.Type.Should().NotBeNullOrWhiteSpace();
            hit.Id.Should().NotBeNullOrWhiteSpace();
            hit.Found.Should().BeTrue();
        }
    }

    [I]
    public async Task UsesDefaultIndexAndInferredTypeAsync()
    {
        var response = await _client.GetManyAsync<Developer>(_ids);
        response.Count().Should().Be(10);
        foreach (var hit in response)
        {
            hit.Index.Should().NotBeNullOrWhiteSpace();
            if (_cluster.ClusterConfiguration.Version < "2.0.0")
                hit.Type.Should().NotBeNullOrWhiteSpace();
            hit.Id.Should().NotBeNullOrWhiteSpace();
            hit.Found.Should().BeTrue();
        }
    }

    [I]
    public async Task ReturnsDocMatchingDistinctIds()
    {
        var id = _ids.First();

        var response = await _client.GetManyAsync<Developer>(new[] { id, id, id });
        response.Count().Should().Be(1);
        foreach (var hit in response)
        {
            hit.Index.Should().NotBeNullOrWhiteSpace();
            hit.Id.Should().Be(id.ToString(CultureInfo.InvariantCulture));
            hit.Found.Should().BeTrue();
        }
    }

    [I]
    public void ReturnsDocsMatchingDistinctIdsFromDifferentIndices()
    {
        var developerIndex = OpenSearch.Client.Indices.Index<Developer>();
        var indexName = developerIndex.GetString(_client.ConnectionSettings);
        var reindexName = $"{indexName}-getmany-distinctids";

        var reindexResponse = _client.ReindexOnServer(r => r
            .Source(s => s
                .Index(developerIndex)
                .Query<Developer>(q => q
                    .Ids(ids => ids.Values(_ids))
                )
            )
            .Destination(d => d
                .Index(reindexName))
            .Refresh()
        );

        if (!reindexResponse.IsValid)
            throw new Exception($"problem reindexing documents for integration test: {reindexResponse.DebugInformation}");

        var id = _ids.First();

        var multiGetResponse = _client.MultiGet(s => s
            .RequestConfiguration(r => r.ThrowExceptions())
            .Get<Developer>(m => m
                .Id(id)
                .Index(indexName)
            )
            .Get<Developer>(m => m
                .Id(id)
                .Index(reindexName)
            )
        );

        var response = multiGetResponse.GetMany<Developer>(new[] { id, id });

        response.Count().Should().Be(2);
        foreach (var hit in response)
        {
            hit.Index.Should().NotBeNullOrWhiteSpace();
            hit.Id.Should().NotBeNullOrWhiteSpace();
            hit.Found.Should().BeTrue();
        }
    }

    [I]
    public void ReturnsDocsMatchingDistinctIdsFromDifferentIndicesWithRequestLevelIndex()
    {
        var developerIndex = OpenSearch.Client.Indices.Index<Developer>();
        var indexName = developerIndex.GetString(_client.ConnectionSettings);
        var reindexName = $"{indexName}-getmany-distinctidsindex";

        var reindexResponse = _client.ReindexOnServer(r => r
            .Source(s => s
                .Index(developerIndex)
                .Query<Developer>(q => q
                    .Ids(ids => ids.Values(_ids))
                )
            )
            .Destination(d => d
                .Index(reindexName))
            .Refresh()
        );

        if (!reindexResponse.IsValid)
            throw new Exception($"problem reindexing documents for integration test: {reindexResponse.DebugInformation}");

        var id = _ids.First();

        var multiGetResponse = _client.MultiGet(s => s
            .Index(indexName)
            .RequestConfiguration(r => r.ThrowExceptions())
            .Get<Developer>(m => m
                .Id(id)
            )
            .Get<Developer>(m => m
                .Id(id)
                .Index(reindexName)
            )
        );

        var response = multiGetResponse.GetMany<Developer>(new[] { id, id });

        response.Count().Should().Be(2);
        var seenIndices = new HashSet<string>(2);

        foreach (var hit in response)
        {
            hit.Index.Should().NotBeNullOrWhiteSpace();
            seenIndices.Add(hit.Index);
            hit.Id.Should().NotBeNullOrWhiteSpace();
            hit.Found.Should().BeTrue();
        }

        seenIndices.Should().HaveCount(2).And.Contain(new[] { indexName, reindexName });
    }

    [I]
    public async Task ReturnsSourceMatchingDistinctIds()
    {
        var id = _ids.First();

        var sources = await _client.SourceManyAsync<Developer>(new[] { id, id, id });
        sources.Count().Should().Be(1);
        foreach (var hit in sources)
        {
            hit.Id.Should().Be(id);
        }
    }

    [I]
    public async Task CanHandleNotFoundResponses()
    {
        var response = await _client.GetManyAsync<Developer>(_ids.Select(i => i * 100));
        response.Count().Should().Be(10);
        foreach (var hit in response)
        {
            hit.Index.Should().NotBeNullOrWhiteSpace();
            if (_cluster.ClusterConfiguration.Version < "2.0.0")
                hit.Type.Should().NotBeNullOrWhiteSpace();
            hit.Id.Should().NotBeNullOrWhiteSpace();
            hit.Found.Should().BeFalse();
        }
    }

    [I]
    public void ThrowsExceptionOnConnectionError()
    {
        if (TestConnectionSettings.RunningFiddler) return; //fiddler meddles here

        var client = new OpenSearchClient(new TestConnectionSettings(port: 9500));
        Action response = () => client.GetMany<Developer>(_ids.Select(i => i * 100));
        response.Should().Throw<OpenSearchClientException>();
    }
}
