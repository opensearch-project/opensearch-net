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
using System.Threading;

namespace OpenSearch.Client;

public static class BlockingSubscribeExtensions
{
    public static BulkAllObserver Wait<T>(this BulkAllObservable<T> observable, TimeSpan maximumRunTime, Action<BulkAllResponse> onNext)
        where T : class =>
        WaitOnObservable<BulkAllObservable<T>, BulkAllResponse, BulkAllObserver>(
            observable, maximumRunTime, (e, c) => new BulkAllObserver(onNext, e, c));

    public static ScrollAllObserver<T> Wait<T>(this IObservable<IScrollAllResponse<T>> observable, TimeSpan maximumRunTime,
        Action<IScrollAllResponse<T>> onNext
    )
        where T : class =>
        WaitOnObservable<IObservable<IScrollAllResponse<T>>, IScrollAllResponse<T>, ScrollAllObserver<T>>(
            observable, maximumRunTime, (e, c) => new ScrollAllObserver<T>(onNext, e, c));

    public static ReindexObserver Wait(this IObservable<BulkAllResponse> observable, TimeSpan maximumRunTime, Action<BulkAllResponse> onNext) =>
        WaitOnObservable<IObservable<BulkAllResponse>, BulkAllResponse, ReindexObserver>(
            observable, maximumRunTime, (e, c) => new ReindexObserver(onNext, e, c));

    private static TObserver WaitOnObservable<TObservable, TObserve, TObserver>(
        TObservable observable,
        TimeSpan maximumRunTime,
        Func<Action<Exception>, Action, TObserver> factory
    )
        where TObservable : IObservable<TObserve>
        where TObserver : IObserver<TObserve>
    {
        observable.ThrowIfNull(nameof(observable));
        maximumRunTime.ThrowIfNull(nameof(maximumRunTime));
        Exception ex = null;
        var handle = new ManualResetEvent(false);
        var observer = factory(
            e =>
            {
                ex = e;
                handle.Set();
            },
            () => handle.Set()
        );
        observable.Subscribe(observer);
        handle.WaitOne(maximumRunTime);
        if (ex != null) throw ex;

        return observer;
    }
}
