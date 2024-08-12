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
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Core.Extensions;
using Tests.Domain;

namespace Tests.ClientConcepts.LowLevel;

public class DirectStreaming
{
    [U]
    public void DisableDirectStreamingOnError()
    {
        Action<IResponse> assert = r =>
        {
            r.ApiCall.Should().NotBeNull();
            r.ApiCall.RequestBodyInBytes.Should().NotBeNull();
            r.ApiCall.ResponseBodyInBytes.Should().NotBeNull();
        };

        var client = FixedResponseClient.Create(new { }, 404, s => s.DisableDirectStreaming());
        var response = client.Search<object>(s => s);
        assert(response);
        response = client.SearchAsync<object>(s => s).Result;
        assert(response);
    }

    [U]
    public void EnableDirectStreamingOnError()
    {
        Action<IResponse> assert = r =>
        {
            r.ApiCall.Should().NotBeNull();
            r.ApiCall.RequestBodyInBytes.Should().BeNull();
            r.ApiCall.ResponseBodyInBytes.Should().BeNull();
        };

        var client = FixedResponseClient.Create(new { }, 404);
        var response = client.Search<object>(s => s);
        assert(response);
        response = client.SearchAsync<object>(s => s).Result;
        assert(response);
    }

    [U]
    public void DisableDirectStreamingOnSuccess()
    {
        Action<IResponse> assert = r =>
        {
            r.ApiCall.Should().NotBeNull();
            r.ApiCall.RequestBodyInBytes.Should().NotBeNull();
            r.ApiCall.ResponseBodyInBytes.Should().NotBeNull();
        };

        var client = FixedResponseClient.Create(new { }, 200, s => s.DisableDirectStreaming());
        var response = client.Search<object>(s => s);
        assert(response);
        response = client.SearchAsync<object>(s => s).Result;
        assert(response);
    }

    [U]
    public void EnableDirectStreamingOnSuccess()
    {
        Action<IResponse> assert = r =>
        {
            r.ApiCall.Should().NotBeNull();
            r.ApiCall.RequestBodyInBytes.Should().BeNull();
            r.ApiCall.ResponseBodyInBytes.Should().BeNull();
        };

        var client = FixedResponseClient.Create(new { });
        var response = client.Search<object>(s => s);
        assert(response);
        response = client.SearchAsync<object>(s => s).Result;
        assert(response);
    }

    [U]
    public void DisableDirectStreamingOnRequest()
    {
        Action<IResponse> assert = r =>
        {
            r.ApiCall.Should().NotBeNull();
            r.ApiCall.RequestBodyInBytes.Should().NotBeNull();
            r.ApiCall.ResponseBodyInBytes.Should().NotBeNull();
        };

        var client = FixedResponseClient.Create(new { });
        var response = client.Search<object>(s => s.RequestConfiguration(r => r.DisableDirectStreaming()));
        assert(response);
        response = client.SearchAsync<object>(s => s.RequestConfiguration(r => r.DisableDirectStreaming())).Result;
        assert(response);
    }

    [U]
    public void EnableDirectStreamingOnRequest()
    {
        Action<IResponse> assert = r =>
        {
            r.ApiCall.Should().NotBeNull();
            r.ApiCall.RequestBodyInBytes.Should().BeNull();
            r.ApiCall.ResponseBodyInBytes.Should().BeNull();
        };

        var client = FixedResponseClient.Create(new { }, 200, c => c.DisableDirectStreaming());
        var response = client.Search<object>(s => s.RequestConfiguration(r => r.DisableDirectStreaming(false)));
        assert(response);
        response = client.SearchAsync<object>(s => s.RequestConfiguration(r => r.DisableDirectStreaming(false))).Result;
        assert(response);
    }

    [U]
    public void DebugModeRespectsOriginalOnRequestCompleted()
    {
        var global = 0;
        var local = 0;
        var client = FixedResponseClient.Create(new { }, 200, s => s
            .EnableDebugMode(d => local++)
            .OnRequestCompleted(d => global++)
        );

        var response = client.Search<Project>(s => s
            .From(10)
            .Size(20)
            .Query(q => q
                .Match(m => m
                    .Field(p => p.Name)
                    .Query("opensearch")
                )
            )
        );
        response.ShouldBeValid();
        global.Should().Be(1);
        local.Should().Be(1);
    }
}
