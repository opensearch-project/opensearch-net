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
using System.Collections.Specialized;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Framework;

namespace Tests.ClientConcepts.LowLevel;

public class Lifetimes
{
    /**== Lifetimes
		*
		* If you are using an IOC/Dependency Injection container, it's always useful to know the best practices around
		* the lifetime of your objects.
		*
		* In general, we advise folks to register an `OpenSearchClient` instance as a singleton; the client is thread safe,
		* so sharing an instance across threads is fine.
		*
		* The actual moving part that benefits from being a singleton is `ConnectionSettings` because
		* **caches are __per__ `ConnectionSettings`**.
		*
		* In some applications ,it could make perfect sense to have multiple `OpenSearchClient` instances registered with different
		* connection settings such as when your application connects to two different OpenSearch clusters.
		*
		* IMPORTANT: Due to the semantic versioning of OpenSearch.Net and OSC and their alignment to versions of OpenSearch, all instances of `OpenSearchClient` and
		* OpenSearch clusters that are connected to must be on the **same major version**
		*
		* Let's demonstrate which components are disposed by creating our own derived `ConnectionSettings`, `IConnectionPool` and `IConnection` types
		*/
    private class AConnectionSettings : ConnectionSettings
    {
        public AConnectionSettings(IConnectionPool pool, IConnection connection)
            : base(pool, connection)
        { }

        public bool IsDisposed { get; private set; }

        protected override void DisposeManagedResources()
        {
            IsDisposed = true;
            base.DisposeManagedResources();
        }
    }

    private class AConnectionPool : SingleNodeConnectionPool
    {
        public AConnectionPool(Uri uri, IDateTimeProvider dateTimeProvider = null) : base(uri, dateTimeProvider) { }

        public bool IsDisposed { get; private set; }

        protected override void DisposeManagedResources()
        {
            IsDisposed = true;
            base.DisposeManagedResources();
        }
    }

    private class AConnection : InMemoryConnection
    {
        public bool IsDisposed { get; private set; }

        protected override void DisposeManagedResources()
        {
            IsDisposed = true;
            base.DisposeManagedResources();
        }
    }

    /**
		* `ConnectionSettings`, `IConnectionPool` and `IConnection` all explicitly implement `IDisposable`
		*/
    [U]
    public void InitialDisposeState()
    {
        var connection = new AConnection();
        var connectionPool = new AConnectionPool(new Uri("http://localhost:9200"));
        var settings = new AConnectionSettings(connectionPool, connection);
        settings.IsDisposed.Should().BeFalse();
        connectionPool.IsDisposed.Should().BeFalse();
        connection.IsDisposed.Should().BeFalse();
    }

    /**
		* Disposing an instance of `ConnectionSettings` will also dispose the `IConnectionPool` and `IConnection` it uses
		*/
    [U]
    public void DisposingSettingsDisposesMovingParts()
    {
        var connection = new AConnection();
        var connectionPool = new AConnectionPool(new Uri("http://localhost:9200"));
        var settings = new AConnectionSettings(connectionPool, connection);
        using (settings) { } // <1> force the settings to be disposed
        settings.IsDisposed.Should().BeTrue();
        connectionPool.IsDisposed.Should().BeTrue();
        connection.IsDisposed.Should().BeTrue();
    }
}
