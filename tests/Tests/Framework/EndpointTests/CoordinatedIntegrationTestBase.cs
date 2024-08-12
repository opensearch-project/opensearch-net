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
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Framework.EndpointTests.TestState;
using Xunit;

namespace Tests.Framework.EndpointTests;

public abstract class CoordinatedIntegrationTestBase<TCluster>
    : IClusterFixture<TCluster>, IClassFixture<EndpointUsage>
    where TCluster : IEphemeralCluster<EphemeralClusterConfiguration>, IOpenSearchClientTestCluster, new()
{
    private readonly CoordinatedUsage _coordinatedUsage;

    protected CoordinatedIntegrationTestBase(CoordinatedUsage coordinatedUsage) => _coordinatedUsage = coordinatedUsage;

    protected async Task Assert<TResponse>(string name, Action<TResponse> assert)
        where TResponse : class, IResponse
    {
        if (_coordinatedUsage.Skips(name)) return;

        var lazyResponses = await ExecuteOnceInOrderUntil(name) ?? throw new Exception($"{name} is defined but it yields no LazyResponses object");
        await AssertOnAllResponses<TResponse>(name, lazyResponses, (v, r) => assert(r));
    }

    protected async Task Assert<TResponse>(string name, Action<string, TResponse> assert)
        where TResponse : class, IResponse
    {
        var lazyResponses = await ExecuteOnceInOrderUntil(name) ?? throw new Exception($"{name} is defined but it yields no LazyResponses object");
        await AssertOnAllResponses(name, lazyResponses, assert);
    }

    protected async Task AssertRunsToCompletion(string name)
    {
        var lazyResponses = await ExecuteOnceInOrderUntil(name) ?? throw new Exception($"{name} is defined but it yields no LazyResponses object");
    }

    private async Task AssertOnAllResponses<TResponse>(string name, LazyResponses responses, Action<string, TResponse> assert)
        where TResponse : class, IResponse
    {
        foreach (var (key, value) in await responses)
        {
            if (!(value is TResponse response))
                throw new Exception($"{value.GetType()} is not expected response type {typeof(TResponse)}");

            if (!_coordinatedUsage.MethodIsolatedValues.TryGetValue(key, out var isolatedValue))
                throw new Exception($"{name} is not a request observed and so no call isolated values could be located for it");

            var r = response;
            if (TestClient.Configuration.RunIntegrationTests && !r.IsValid && r.ApiCall.OriginalException != null
                && !(r.ApiCall.OriginalException is OpenSearchClientException))
            {
                var e = ExceptionDispatchInfo.Capture(r.ApiCall.OriginalException.Demystify());
                throw new ResponseAssertionException(e.SourceException, r).Demystify();
            }

            try
            {
                assert(isolatedValue, response);
            }
            catch (Exception e)
            {
                var ex = ExceptionDispatchInfo.Capture(e.Demystify());
                throw new ResponseAssertionException(ex.SourceException, r).Demystify();
            }
        }
    }

    private async Task<LazyResponses> ExecuteOnceInOrderUntil(string name)
    {
        if (!_coordinatedUsage.Contains(name)) throw new Exception($"{name} is not a keyed after create response");

        foreach (var lazyResponses in _coordinatedUsage)
        {
            await lazyResponses;
            if (lazyResponses.Name == name) return lazyResponses;
        }
        return null;
    }
}
