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
using System.Linq;
using System.Threading.Tasks;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Ephemeral;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Configuration;
using Tests.Core.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Core.Serialization;
using Tests.Domain.Helpers;
using Tests.Framework.EndpointTests.TestState;
using Xunit;

namespace Tests.Framework.EndpointTests;

public abstract class RequestResponseApiTestBase<TCluster, TResponse, TInterface, TDescriptor, TInitializer>
    : ExpectJsonTestBase, IClusterFixture<TCluster>, IClassFixture<EndpointUsage>
    where TCluster : IEphemeralCluster<EphemeralClusterConfiguration>, IOpenSearchClientTestCluster, new()
    where TResponse : class, IResponse
    where TInterface : class
    where TDescriptor : class, TInterface
    where TInitializer : class, TInterface
{
    private readonly EndpointUsage _usage;

    protected RequestResponseApiTestBase(TCluster cluster, EndpointUsage usage) : base(cluster.Client)
    {
        _usage = usage ?? throw new ArgumentNullException(nameof(usage));

        if (cluster == null) throw new ArgumentNullException(nameof(cluster));

        Cluster = cluster;
        Responses = usage.CallOnce(ClientUsage);
        UniqueValues = usage.CallUniqueValues;
    }

    public virtual IOpenSearchClient Client =>
        TestConfiguration.Instance.RunIntegrationTests ? Cluster.Client : TestClient.DefaultInMemoryClient;

    public TCluster Cluster { get; }

    protected virtual string CallIsolatedValue => UniqueValues.Value;
    protected virtual Func<TDescriptor, TInterface> Fluent { get; } = null;
    protected virtual TInitializer Initializer { get; } = null;
    protected bool RanIntegrationSetup => _usage?.CalledSetup ?? false;
    protected LazyResponses Responses { get; }

    protected virtual bool TestOnlyOne => TestClient.Configuration.TestOnlyOne;

    protected CallUniqueValues UniqueValues { get; }

    protected static string RandomString() => Guid.NewGuid().ToString("N").Substring(0, 8);

    protected string U(string s) => Uri.EscapeDataString(s);

    protected T ExtendedValue<T>(string key) where T : class => UniqueValues.ExtendedValue<T>(key);

    protected bool TryGetExtendedValue<T>(string key, out T t) where T : class => UniqueValues.TryGetExtendedValue(key, out t);

    protected void ExtendedValue<T>(string key, T value) where T : class => UniqueValues.ExtendedValue(key, value);

    protected virtual TDescriptor NewDescriptor() => Activator.CreateInstance<TDescriptor>();

    protected virtual void IntegrationSetup(IOpenSearchClient client, CallUniqueValues values) { }

    protected virtual void IntegrationTeardown(IOpenSearchClient client, CallUniqueValues values) { }

    protected virtual void OnBeforeCall(IOpenSearchClient client) { }

    protected virtual void OnAfterCall(IOpenSearchClient client, TResponse response) => OnAfterCall(client);

    protected virtual void OnAfterCall(IOpenSearchClient client) { }

    protected abstract LazyResponses ClientUsage();

    protected LazyResponses Calls(
        Func<IOpenSearchClient, Func<TDescriptor, TInterface>, TResponse> fluent,
        Func<IOpenSearchClient, Func<TDescriptor, TInterface>, Task<TResponse>> fluentAsync,
        Func<IOpenSearchClient, TInitializer, TResponse> request,
        Func<IOpenSearchClient, TInitializer, Task<TResponse>> requestAsync
    ) => new LazyResponses(async () =>
    {
        var client = Client;

        void IntegrateOnly(Action<IOpenSearchClient> act)
        {
            if (!TestClient.Configuration.RunIntegrationTests) return;

            act(client);
        }

        if (TestClient.Configuration.RunIntegrationTests)
        {
            IntegrationSetup(client, UniqueValues);
            _usage.CalledSetup = true;
        }

        (ClientMethod, Func<ValueTask<TResponse>>) Api(ClientMethod method, Func<ValueTask<TResponse>> action) => (method, action);

        var dict = new Dictionary<ClientMethod, IResponse>();
        var views = new[]
        {
            Api(ClientMethod.Fluent, () => new ValueTask<TResponse>(fluent(client, Fluent))),
            Api(ClientMethod.Initializer, () => new ValueTask<TResponse>(request(client, Initializer))),
            Api(ClientMethod.FluentAsync, async () => await fluentAsync(client, Fluent)),
            Api(ClientMethod.InitializerAsync, async () => await requestAsync(client, Initializer)),
        };
        foreach (var (v, m) in views.OrderBy((t) => Gimme.Random.Int()))
        {
            UniqueValues.CurrentView = v;

            IntegrateOnly(OnBeforeCall);
            var resp = await m();
            dict.Add(v, resp);
            IntegrateOnly(c => OnAfterCall(c, resp));
            if (TestOnlyOne) break;
        }

        if (TestClient.Configuration.RunIntegrationTests)
        {
            IntegrationTeardown(client, UniqueValues);
            _usage.CalledTeardown = true;
        }

        return dict;
    });

    protected virtual async Task AssertOnAllResponses(Action<TResponse> assert)
    {
        var responses = await Responses;
        foreach (var kv in responses)
        {
            var response = kv.Value as TResponse;
            try
            {
                UniqueValues.CurrentView = kv.Key;
                assert(response);
            }
#pragma warning disable 7095 //enable this if you expect a single overload to act up
#pragma warning disable 8360
            catch (Exception ex) when (false)
#pragma warning restore 7095
#pragma warning restore 8360
#pragma warning disable 0162 //dead code while the previous exception filter is false
            {
                throw new Exception($"asserting over the response from: {kv.Key} failed: {ex.Message}", ex);
            }
#pragma warning restore 0162
        }
    }
}
