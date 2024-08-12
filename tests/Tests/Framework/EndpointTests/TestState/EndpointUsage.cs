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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Bogus;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Configuration;
using Tests.Core.Client;

namespace Tests.Framework.EndpointTests.TestState;

public class EndpointUsage
{
    private readonly object _lock = new object();
    private readonly ConcurrentDictionary<string, LazyResponses> _usages = new ConcurrentDictionary<string, LazyResponses>();

    public EndpointUsage() : this("osc") { }

    protected EndpointUsage(string prefix) => CallUniqueValues = new CallUniqueValues(prefix);

    public bool CalledSetup { get; internal set; }
    public bool CalledTeardown { get; internal set; }

    public CallUniqueValues CallUniqueValues { get; }

    public LazyResponses CallOnce(Func<LazyResponses> clientUsage, string key)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        if (_usages.TryGetValue(key, out var lazyResponses)) return lazyResponses;

        lock (_lock)
        {
            if (_usages.TryGetValue(key, out lazyResponses)) return lazyResponses;

            var response = clientUsage();
            _usages.TryAdd(key, response);
            return response;
        }
    }

    public LazyResponses CallOnce(Func<LazyResponses> clientUsage, int k = 0) => CallOnce(clientUsage, k.ToString());
}

public class SingleEndpointUsage<TResponse> : EndpointUsage
    where TResponse : class, IResponse
{
    private readonly Func<string, IOpenSearchClient, TResponse> _fluent;
    private readonly Func<string, IOpenSearchClient, Task<TResponse>> _fluentAsync;
    private readonly Func<string, IOpenSearchClient, TResponse> _request;
    private readonly Func<string, IOpenSearchClient, Task<TResponse>> _requestAsync;

    public SingleEndpointUsage(
        Func<string, IOpenSearchClient, TResponse> fluent,
        Func<string, IOpenSearchClient, Task<TResponse>> fluentAsync,
        Func<string, IOpenSearchClient, TResponse> request,
        Func<string, IOpenSearchClient, Task<TResponse>> requestAsync,
        string valuePrefix = null
    ) : base(valuePrefix)
    {
        _fluent = fluent;
        _fluentAsync = fluentAsync;
        _request = request;
        _requestAsync = requestAsync;
    }

    public Action<IOpenSearchClient, CallUniqueValues> IntegrationSetup { get; set; }
    public Action<IOpenSearchClient, CallUniqueValues> IntegrationTeardown { get; set; }
    public Action<IOpenSearchClient> OnAfterCall { get; set; }
    public Action<IOpenSearchClient> OnBeforeCall { get; set; }

    // ReSharper disable once StaticMemberInGenericType
    public static Randomizer Random { get; } = new Randomizer(TestConfiguration.Instance.Seed);

    private LazyResponses Responses { get; set; }

    public void KickOffOnce(IOpenSearchClient client, bool oneRandomCall = false) =>
        Responses = CallOnce(() => new LazyResponses(async () =>
        {
            if (TestClient.Configuration.RunIntegrationTests)
            {
                IntegrationSetup?.Invoke(client, CallUniqueValues);
                CalledSetup = true;
            }

            var randomCall = Random.Number(0, 3);

            var dict = new Dictionary<ClientMethod, IResponse>();

            if (!oneRandomCall || randomCall == 0)
                Call(client, dict, ClientMethod.Fluent, v => _fluent(v, client));

            if (!oneRandomCall || randomCall == 1)
                await CallAsync(client, dict, ClientMethod.FluentAsync, v => _fluentAsync(v, client));

            if (!oneRandomCall || randomCall == 2)
                Call(client, dict, ClientMethod.Initializer, v => _request(v, client));

            if (!oneRandomCall || randomCall == 3)
                await CallAsync(client, dict, ClientMethod.InitializerAsync, v => _requestAsync(v, client));

            if (TestClient.Configuration.RunIntegrationTests)
            {
                foreach (var v in CallUniqueValues.Values.SelectMany(d => d))
                    IntegrationTeardown?.Invoke(client, CallUniqueValues);
                CalledTeardown = true;
            }

            return dict;
        }));

    private void Call(IOpenSearchClient client, IDictionary<ClientMethod, IResponse> dict, ClientMethod method, Func<string, TResponse> call)
    {
        CallUniqueValues.CurrentView = method;
        OnBeforeCall?.Invoke(client);
        dict.Add(method, call(CallUniqueValues.Value));
        OnAfterCall?.Invoke(client);
    }

    private async Task CallAsync(IOpenSearchClient client, IDictionary<ClientMethod, IResponse> dict, ClientMethod method,
        Func<string, Task<TResponse>> call
    )
    {
        CallUniqueValues.CurrentView = method;
        OnBeforeCall?.Invoke(client);
        dict.Add(method, await call(CallUniqueValues.Value));
        OnAfterCall?.Invoke(client);
    }

    public async Task AssertOnAllResponses(Action<TResponse> assert)
    {
        var responses = await Responses;
        foreach (var kv in responses)
        {
            var r = kv.Value as TResponse;

            //this is to make sure any unexpected exceptions on the response are rethrown and shown during testing
            if (TestClient.Configuration.RunIntegrationTests && !r.IsValid && r.ApiCall.OriginalException != null
                && !(r.ApiCall.OriginalException is OpenSearchClientException))
            {
                var e = ExceptionDispatchInfo.Capture(r.ApiCall.OriginalException.Demystify());
                throw new ResponseAssertionException(e.SourceException, r);
            }

            try
            {
                assert(r);
            }
            catch (Exception e)
            {
                throw new ResponseAssertionException(e, r);
            }
        }
    }
}
