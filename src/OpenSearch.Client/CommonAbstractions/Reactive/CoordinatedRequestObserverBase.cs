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
using OpenSearch.Net;

namespace OpenSearch.Client;

internal static class CoordinatedRequestDefaults
{
    public static int BulkAllBackOffRetriesDefault = 0;
    public static TimeSpan BulkAllBackOffTimeDefault = TimeSpan.FromMinutes(1);
    public static int BulkAllMaxDegreeOfParallelismDefault = 4;
    public static int BulkAllSizeDefault = 1000;
    public static int ReindexBackPressureFactor = 4;
    public static int ReindexScrollSize = 500;
}

public abstract class CoordinatedRequestObserverBase<T> : IObserver<T>
{
    private readonly Action _completed;
    private readonly Action<Exception> _onError;
    private readonly Action<T> _onNext;

    protected CoordinatedRequestObserverBase(Action<T> onNext = null, Action<Exception> onError = null, Action completed = null)
    {
        _onNext = onNext;
        _onError = onError;
        _completed = completed;
    }

    public void OnCompleted() => _completed?.Invoke();

    public void OnError(Exception error)
    {
        // This normalizes task cancellation exceptions for observables
        // If a task cancellation happens in the client it bubbles out as a UnexpectedOpenSearchClientException
        // where as inside our IObservable implementation we .ThrowIfCancellationRequested() directly.
        if (error is UnexpectedOpenSearchClientException opensearch && opensearch.InnerException != null && opensearch.InnerException is OperationCanceledException c)
            _onError?.Invoke(c);
        else _onError?.Invoke(error);
    }

    public void OnNext(T value) => _onNext?.Invoke(value);
}
