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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client.Settings;

namespace Tests.ClientConcepts.ConnectionPooling.Dispose;

public class ResponseBuilderDisposeTests
{
    private readonly IConnectionSettingsValues _settings = new AlwaysInMemoryConnectionSettings().DisableDirectStreaming(false);
    private readonly IConnectionSettingsValues _settingsDisableDirectStream = new AlwaysInMemoryConnectionSettings().DisableDirectStreaming();

    [U] public async Task ResponseWithHttpStatusCode() => await AssertRegularResponse(false, 1);

    [U] public async Task ResponseBuilderWithNoHttpStatusCode() => await AssertRegularResponse(false);

    [U]
    public async Task ResponseWithHttpStatusCodeDisableDirectStreaming() =>
        await AssertRegularResponse(true, 1);

    [U]
    public async Task ResponseBuilderWithNoHttpStatusCodeDisableDirectStreaming() =>
        await AssertRegularResponse(true);

    private async Task AssertRegularResponse(bool disableDirectStreaming, int? statusCode = null)
    {
        var settings = disableDirectStreaming ? _settingsDisableDirectStream : _settings;
        var memoryStreamFactory = new TrackMemoryStreamFactory();
        var requestData = new RequestData(HttpMethod.GET, "/", null, settings, null, memoryStreamFactory)
        {
            Node = new Node(new Uri("http://localhost:9200"))
        };

        var stream = new TrackDisposeStream();
        var response = ResponseBuilder.ToResponse<RootNodeInfoResponse>(requestData, null, statusCode, null, stream);
        response.Should().NotBeNull();

        memoryStreamFactory.Created.Count().Should().Be(disableDirectStreaming ? 1 : 0);
        if (disableDirectStreaming)
        {
            var memoryStream = memoryStreamFactory.Created[0];
            memoryStream.IsDisposed.Should().BeTrue();
        }
        stream.IsDisposed.Should().BeTrue();


        stream = new TrackDisposeStream();
        var ct = new CancellationToken();
        response = await ResponseBuilder.ToResponseAsync<RootNodeInfoResponse>(requestData, null, statusCode, null, stream,
            cancellationToken: ct);
        response.Should().NotBeNull();
        memoryStreamFactory.Created.Count().Should().Be(disableDirectStreaming ? 2 : 0);
        if (disableDirectStreaming)
        {
            var memoryStream = memoryStreamFactory.Created[1];
            memoryStream.IsDisposed.Should().BeTrue();
        }
        stream.IsDisposed.Should().BeTrue();
    }

    [U] public async Task StreamResponseWithHttpStatusCode() => await AssertStreamResponse(false, 200);

    [U] public async Task StreamResponseBuilderWithNoHttpStatusCode() => await AssertStreamResponse(false);

    [U]
    public async Task StreamResponseWithHttpStatusCodeDisableDirectStreaming() =>
        await AssertStreamResponse(true, 1);

    [U]
    public async Task StreamResponseBuilderWithNoHttpStatusCodeDisableDirectStreaming() =>
        await AssertStreamResponse(true);

    private async Task AssertStreamResponse(bool disableDirectStreaming, int? statusCode = null)
    {
        var settings = disableDirectStreaming ? _settingsDisableDirectStream : _settings;
        var memoryStreamFactory = new TrackMemoryStreamFactory();

        var requestData = new RequestData(HttpMethod.GET, "/", null, settings, null, memoryStreamFactory)
        {
            Node = new Node(new Uri("http://localhost:9200"))
        };

        var stream = new TrackDisposeStream();
        var response = ResponseBuilder.ToResponse<RootNodeInfoResponse>(requestData, null, statusCode, null, stream);
        response.Should().NotBeNull();

        memoryStreamFactory.Created.Count().Should().Be(disableDirectStreaming ? 1 : 0);
        stream.IsDisposed.Should().Be(true);

        stream = new TrackDisposeStream();
        var ct = new CancellationToken();
        response = await ResponseBuilder.ToResponseAsync<RootNodeInfoResponse>(requestData, null, statusCode, null, stream,
            cancellationToken: ct);
        response.Should().NotBeNull();
        memoryStreamFactory.Created.Count().Should().Be(disableDirectStreaming ? 2 : 0);
        stream.IsDisposed.Should().Be(true);
    }

    private class TrackDisposeStream : MemoryStream
    {
        public TrackDisposeStream() { }

        public TrackDisposeStream(byte[] bytes) : base(bytes) { }

        public TrackDisposeStream(byte[] bytes, int index, int count) : base(bytes, index, count) { }

        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private class TrackMemoryStreamFactory : IMemoryStreamFactory
    {
        public IList<TrackDisposeStream> Created { get; } = new List<TrackDisposeStream>();

        public MemoryStream Create()
        {
            var stream = new TrackDisposeStream();
            Created.Add(stream);
            return stream;
        }

        public MemoryStream Create(byte[] bytes)
        {
            var stream = new TrackDisposeStream(bytes);
            Created.Add(stream);
            return stream;
        }

        public MemoryStream Create(byte[] bytes, int index, int count)
        {
            var stream = new TrackDisposeStream(bytes, index, count);
            Created.Add(stream);
            return stream;
        }
    }
}
