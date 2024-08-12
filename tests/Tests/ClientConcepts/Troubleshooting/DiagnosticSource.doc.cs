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
using System.Threading;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.Net;
using OpenSearch.Net.Diagnostics;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Extensions;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;

namespace Tests.ClientConcepts.Troubleshooting;

/**
	 * [[diagnostic-source]]
	 * === Diagnostic Source
	 *
	 * OpenSearch.Net and OSC support capturing diagnostics information using `DiagnosticSource` and `Activity` from the
	 * `System.Diagnostics` namespace.
	 *
	 * To aid with the discoverability of the topics you can subscribe to and the event names they emit,
	 * both topics and event names are exposed as strongly typed strings under `OpenSearch.Net.Diagnostics.DiagnosticSources`
	 */
public class DiagnosticSourceUsageDocumentation : IClusterFixture<ReadOnlyCluster>
{
    private readonly ReadOnlyCluster _cluster;

    // hide
    public DiagnosticSourceUsageDocumentation(ReadOnlyCluster cluster) => _cluster = cluster;

    /**
		 * Subscribing to DiagnosticSources means implementing `IObserver<DiagnosticListener>`
		 * or using `.Subscribe(observer, filter)` to opt in to the correct topic.
		 *
		 * Here we choose the more verbose `IObserver<>` implementation
		 */
    public class ListenerObserver : IObserver<DiagnosticListener>, IDisposable
    {
        private long _messagesWrittenToConsole = 0;
        public long MessagesWrittenToConsole => _messagesWrittenToConsole;

        public Exception SeenException { get; private set; }

        public void OnError(Exception error) => SeenException = error;
        public bool Completed { get; private set; }
        public void OnCompleted() => Completed = true;

        private void WriteToConsole<T>(string eventName, T data)
        {
            var a = Activity.Current;
            Interlocked.Increment(ref _messagesWrittenToConsole);
        }

        private List<IDisposable> Disposables { get; } = new List<IDisposable>();

        public void OnNext(DiagnosticListener value)
        {
            void TrySubscribe(string sourceName, Func<IObserver<KeyValuePair<string, object>>> listener) // <1> By inspecting the name, we can selectively subscribe only to the topics `OpenSearch.Net` emit
            {
                if (value.Name != sourceName) return;

                var subscription = value.Subscribe(listener());
                Disposables.Add(subscription);
            }

            TrySubscribe(DiagnosticSources.AuditTrailEvents.SourceName,
                () => new AuditDiagnosticObserver(v => WriteToConsole(v.Key, v.Value)));

            TrySubscribe(DiagnosticSources.Serializer.SourceName,
                () => new SerializerDiagnosticObserver(v => WriteToConsole(v.Key, v.Value)));

            TrySubscribe(DiagnosticSources.RequestPipeline.SourceName,
                () => new RequestPipelineDiagnosticObserver(
                    v => WriteToConsole(v.Key, v.Value),
                    v => WriteToConsole(v.Key, v.Value)
                ));

            TrySubscribe(DiagnosticSources.HttpConnection.SourceName,
                () => new HttpConnectionDiagnosticObserver(
                    v => WriteToConsole(v.Key, v.Value),
                    v => WriteToConsole(v.Key, v.Value)
                ));
        }

        public void Dispose()
        {
            foreach (var d in Disposables) d.Dispose();
        }
    }
    /**
		 * Thanks to `DiagnosticSources`, you do not have to guess the topics emitted.
		 *
		 * The `DiagnosticListener.Subscribe` method expects an `IObserver<KeyValuePair<string, object>>`
		 * which is a rather generic message contract. As a subscriber, it's useful to know what `object` is in each case.
		 * To help with this, each topic within the client has a dedicated `Observer` implementation that
		 * takes an `onNext` delegate typed to the context object actually emitted.
		 *
		 * The RequestPipeline diagnostic source emits a different context objects the start and end of the `Activity`
		 * For this reason, `RequestPipelineDiagnosticObserver` accepts two `onNext` delegates,
		 * one for the `.Start` events and one for the `.Stop` events.
		 *
		 * [[subscribing-to-topics]]
		 * ==== Subscribing to topics
		 *
		 * As a concrete example of subscribing to topics, let's hook into all diagnostic sources and use
		 * `ListenerObserver` to only listen to the ones from `OpenSearch.Net`
		 */
    [I]
    public void SubscribeToTopics()
    {
        using (var listenerObserver = new ListenerObserver())
        using (var subscription = DiagnosticListener.AllListeners.Subscribe(listenerObserver))
        {
            var pool = new SniffingConnectionPool(_cluster.NodesUris()); // <1> use a sniffing connection pool that sniffs on startup and pings before first usage, so our diagnostics will emit most topics.
            var connectionSettings = new ConnectionSettings(pool)
                .DefaultMappingFor<Project>(i => i
                    .IndexName("project")
                );
            connectionSettings = (ConnectionSettings)_cluster.UpdateSettings(connectionSettings);
            var client = new OpenSearchClient(connectionSettings);

            var response = client.Search<Project>(s => s // <2> make a search API call
                .MatchAll()
            );

            listenerObserver.SeenException.Should().BeNull(); // <3> verify that the listener is picking up events
            listenerObserver.Completed.Should().BeFalse();
            listenerObserver.MessagesWrittenToConsole.Should().BeGreaterThan(0);
        }
    }
}
