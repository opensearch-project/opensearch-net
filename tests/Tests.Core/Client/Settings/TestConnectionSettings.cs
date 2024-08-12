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
using System.Diagnostics;
using System.Linq;
using OpenSearch.Client;
using OpenSearch.Net;
using Tests.Configuration;
using Tests.Core.Client.Serializers;
using Tests.Core.Extensions;
using Tests.Core.Xunit;

namespace Tests.Core.Client.Settings;

public class TestConnectionSettings : ConnectionSettings
{
    public static readonly bool RunningMitmProxy = Process.GetProcessesByName("mitmproxy").Any();
    public static readonly bool RunningFiddler = Process.GetProcessesByName("fiddler").Any();

    public TestConnectionSettings(
        Func<ICollection<Uri>, IConnectionPool> createPool = null,
        SourceSerializerFactory sourceSerializerFactory = null,
        IPropertyMappingProvider propertyMappingProvider = null,
        bool forceInMemory = false,
        int port = 9200,
        byte[] response = null
    )
        : base(
            CreatePool(createPool, port),
            TestConfiguration.Instance.CreateConnection(forceInMemory, response),
            CreateSerializerFactory(sourceSerializerFactory),
            propertyMappingProvider
        ) =>
        ApplyTestSettings();

    public static string LocalOrProxyHost => RunningFiddler || RunningMitmProxy ? "ipv4.fiddler" : LocalHost;

    private static int ConnectionLimitDefault =>
        int.TryParse(Environment.GetEnvironmentVariable("OSC_NUMBER_OF_CONNECTIONS"), out var x)
            ? x
            : ConnectionConfiguration.DefaultConnectionLimit;

    private static string LocalHost => "localhost";

    private void ApplyTestSettings() =>
        RerouteToProxyIfNeeded()
        .EnableDebugMode()
        .EnableHttpCompression(TestConfiguration.Instance.Random.HttpCompression)
#if DEBUG
        .EnableDebugMode()
#endif
        .ConnectionLimit(ConnectionLimitDefault)
        .OnRequestCompleted(r =>
        {
            //r.HttpMethod;


            if (!r.DeprecationWarnings.Any()) return;

            var q = r.Uri.Query;
            //hack to prevent the deprecation warnings from the deprecation response test to be reported
            if (!string.IsNullOrWhiteSpace(q) && q.Contains("routing=ignoredefaultcompletedhandler")) return;

            foreach (var d in r.DeprecationWarnings) XunitRunState.SeenDeprecations.Add(d);
        });

    private ConnectionSettings RerouteToProxyIfNeeded()
    {
        if (!RunningMitmProxy) return this;

        return Proxy(new Uri("http://127.0.0.1:8080"), (string)null, (string)null);
    }

    private static SourceSerializerFactory CreateSerializerFactory(SourceSerializerFactory provided)
    {
        if (provided != null) return provided;
        if (!TestConfiguration.Instance.Random.SourceSerializer) return null;

        return (builtin, values) => new TestSourceSerializerBase(builtin, values);
    }

    private static IConnectionPool CreatePool(Func<ICollection<Uri>, IConnectionPool> createPool = null, int port = 9200)
    {
        createPool = createPool ?? (uris => new StaticConnectionPool(uris));
        var connectionPool = createPool(new[] { CreateUri(port) });
        return connectionPool;
    }

    public static Uri CreateUri(int port = 9200) => new UriBuilder("http://", LocalOrProxyHost, port).Uri;
}
