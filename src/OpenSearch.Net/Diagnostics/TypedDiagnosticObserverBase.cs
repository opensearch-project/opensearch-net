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

namespace OpenSearch.Net.Diagnostics;

public abstract class TypedDiagnosticObserverBase<TOnNext> : IObserver<KeyValuePair<string, object>>
{
    private readonly Action<KeyValuePair<string, TOnNext>> _onNext;
    private readonly Action<Exception> _onError;
    private readonly Action _onCompleted;

    protected TypedDiagnosticObserverBase(
        Action<KeyValuePair<string, TOnNext>> onNext,
        Action<Exception> onError = null,
        Action onCompleted = null
    )
    {
        _onNext = onNext;
        _onError = onError;
        _onCompleted = onCompleted;
    }

    void IObserver<KeyValuePair<string, object>>.OnCompleted() => _onCompleted?.Invoke();

    void IObserver<KeyValuePair<string, object>>.OnError(Exception error) => _onError?.Invoke(error);

    void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> value)
    {
        if (value.Value is TOnNext next) _onNext?.Invoke(new KeyValuePair<string, TOnNext>(value.Key, next));
        else if (value.Value == null) _onNext?.Invoke(new KeyValuePair<string, TOnNext>(value.Key, default));

        else throw new Exception($"{value.Key} received unexpected type {value.Value.GetType()}");

    }
}
public abstract class TypedDiagnosticObserverBase<TOnNextStart, TOnNextEnd> : IObserver<KeyValuePair<string, object>>
{
    private readonly Action<KeyValuePair<string, TOnNextStart>> _onNextStart;
    private readonly Action<KeyValuePair<string, TOnNextEnd>> _onNextEnd;
    private readonly Action<Exception> _onError;
    private readonly Action _onCompleted;

    protected TypedDiagnosticObserverBase(
        Action<KeyValuePair<string, TOnNextStart>> onNextStart,
        Action<KeyValuePair<string, TOnNextEnd>> onNextEnd,
        Action<Exception> onError = null,
        Action onCompleted = null
    )
    {
        _onNextStart = onNextStart;
        _onNextEnd = onNextEnd;
        _onError = onError;
        _onCompleted = onCompleted;
    }

    void IObserver<KeyValuePair<string, object>>.OnCompleted() => _onCompleted?.Invoke();

    void IObserver<KeyValuePair<string, object>>.OnError(Exception error) => _onError?.Invoke(error);

    void IObserver<KeyValuePair<string, object>>.OnNext(KeyValuePair<string, object> value)
    {
        if (value.Value is TOnNextStart nextStart) _onNextStart?.Invoke(new KeyValuePair<string, TOnNextStart>(value.Key, nextStart));
        else if (value.Key.EndsWith(".Start") && value.Value is null) _onNextStart?.Invoke(new KeyValuePair<string, TOnNextStart>(value.Key, default));

        else if (value.Value is TOnNextEnd nextEnd) _onNextEnd?.Invoke(new KeyValuePair<string, TOnNextEnd>(value.Key, nextEnd));
        else if (value.Key.EndsWith(".Stop") && value.Value is null) _onNextEnd?.Invoke(new KeyValuePair<string, TOnNextEnd>(value.Key, default));

        else throw new Exception($"{value.Key} received unexpected type {value.Value.GetType()}");

    }
}
