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
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Domain;

namespace Tests.CodeStandards;

/**
	 * Combining base URI with the API path results in a URI that respects the relative path defined in base URI
	 */
public class Requests
{
    [U]
    public void BaseUriIsRespected()
    {
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var settings = new ConnectionSettings(pool, new InMemoryConnection());
        var client = new OpenSearchClient(settings);
        var searchResponse = client.Search<Project>(s => s.AllIndices());

        searchResponse.ApiCall.Uri.ToString().Should().Be("http://localhost:9200/_all/_search?typed_keys=true");
    }

    [U]
    public void BaseUriWithTrailingSlashIsRespected()
    {
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200/"));
        var settings = new ConnectionSettings(pool, new InMemoryConnection());
        var client = new OpenSearchClient(settings);
        var searchResponse = client.Search<Project>(s => s.AllIndices());

        searchResponse.ApiCall.Uri.ToString().Should().Be("http://localhost:9200/_all/_search?typed_keys=true");
    }

    [U]
    public void BaseUriWithRelativePathIsRespected()
    {
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200/opensearch"));
        var settings = new ConnectionSettings(pool, new InMemoryConnection());
        var client = new OpenSearchClient(settings);
        var searchResponse = client.Search<Project>(s => s.AllIndices());

        searchResponse.ApiCall.Uri.ToString().Should().Be("http://localhost:9200/opensearch/_all/_search?typed_keys=true");
    }

    [U]
    public void BaseUriWithRelativePathAndTrailingSlashIsRespected()
    {
        var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200/opensearch/"));
        var settings = new ConnectionSettings(pool, new InMemoryConnection());
        var client = new OpenSearchClient(settings);
        var searchResponse = client.Search<Project>(s => s.AllIndices());

        searchResponse.ApiCall.Uri.ToString().Should().Be("http://localhost:9200/opensearch/_all/_search?typed_keys=true");
    }
}
