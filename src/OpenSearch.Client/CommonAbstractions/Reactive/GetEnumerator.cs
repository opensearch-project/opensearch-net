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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OpenSearch.Client;

internal class GetEnumerator<TSource> : IEnumerator<TSource>, IObserver<TSource>
{
    private readonly SemaphoreSlim _gate;
    private readonly ConcurrentQueue<TSource> _queue;
    private TSource _current;
    private bool _disposed;
    private Exception _error;
    private IDisposable _subscription;

    public GetEnumerator()
    {
        _queue = new ConcurrentQueue<TSource>();
        _gate = new SemaphoreSlim(0);
    }

    public TSource Current => _current;

    object IEnumerator.Current => _current;

    public void Dispose()
    {
        _subscription.Dispose();

        _disposed = true;
        _gate.Release();
    }

    public bool MoveNext()
    {
        _gate.Wait();

        if (_disposed)
            throw new ObjectDisposedException("");

        if (_queue.TryDequeue(out _current))
            return true;

        if (_error != null) throw _error;

        _gate.Release(); // In the (rare) case the user calls MoveNext again we shouldn't block!
        return false;
    }

    public void Reset() => throw new NotSupportedException();

    public void OnCompleted()
    {
        _subscription.Dispose();
        _gate.Release();
    }

    public void OnError(Exception error)
    {
        _error = error;
        _subscription.Dispose();
        _gate.Release();
    }

    public virtual void OnNext(TSource value)
    {
        _queue.Enqueue(value);
        _gate.Release();
    }

    private IEnumerator<TSource> Run(IObservable<TSource> source)
    {
        //
        // [OK] Use of unsafe Subscribe: non-pretentious exact mirror with the dual GetEnumerator method.
        //
        _subscription = source.Subscribe /*Unsafe*/(this);
        return this;
    }

    public IEnumerable<TSource> ToEnumerable(IObservable<TSource> source) =>
        new AnonymousEnumerable<TSource>(() => Run(source));

    internal sealed class AnonymousEnumerable<T> : IEnumerable<T>
    {
        private readonly Func<IEnumerator<T>> _getEnumerator;

        public AnonymousEnumerable(Func<IEnumerator<T>> getEnumerator) => _getEnumerator = getEnumerator;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => _getEnumerator();
    }
}
