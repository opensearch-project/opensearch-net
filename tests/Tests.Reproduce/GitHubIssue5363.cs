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
using System.Threading.Tasks;
using OpenSearch.Net;
using OpenSearch.Net.Diagnostics;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Core.Xunit;
using Xunit;

namespace Tests.Reproduce;

[Skip("When run in parallel with other tests the diagnostic listener may throw null pointer exceptions")]
public class GitHubIssue5363
{
    internal class TestDiagnosticListener : IObserver<DiagnosticListener>, IDisposable
    {
        private ConcurrentBag<IDisposable> Disposables { get; } = new();

        public Action<IApiCallDetails> OnEnded { get; }

        public TestDiagnosticListener(Action<IApiCallDetails> onEnded) => OnEnded = onEnded;

        public void OnError(Exception error) { }
        public void OnCompleted() { }

        public void OnNext(DiagnosticListener value) =>
            TrySubscribe(DiagnosticSources.RequestPipeline.SourceName,
                () => new RequestPipelineDiagnosticObserver(null, v => OnEnded(v.Value)), value);

        private void TrySubscribe(string sourceName, Func<IObserver<KeyValuePair<string, object>>> listener, DiagnosticListener value)
        {
            if (value.Name != sourceName)
                return;
            var d = value.Subscribe(listener());

            Disposables.Add(d);
        }

        public void Dispose()
        {
            foreach (var d in Disposables)
            {
                d.Dispose();
            }
        }
    }

    [U]
    public async Task DiagnosticListener_AuditTrailIsValid()
    {
        using var listener = new TestDiagnosticListener(data =>
        {
            var auditTrailEvent = data.AuditTrail[0];

            Assert.True(auditTrailEvent.Ended != default);
        });

        using var foo = DiagnosticListener.AllListeners.Subscribe(listener);

        var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
        var settings = new ConnectionConfiguration(connectionPool, new InMemoryConnection());

        var client = new OpenSearchLowLevelClient(settings);
        var person = new { Id = "1" };

        await client.IndexAsync<BytesResponse>("test-index", PostData.Serializable(person));
    }
}
