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
using System.Threading.Tasks;

namespace OpenSearch.Client;

/// <summary>
/// Simple back pressure implementation that makes sure the minimum max concurrency between producer and consumer
/// is not amplified by the greedier of the two by more then the backPressureFactor which defaults to 4
/// </summary>
public class ProducerConsumerBackPressure
{
    private readonly int _backPressureFactor;
    private readonly SemaphoreSlim _consumerLimiter;
    private readonly int _slots;

    /// <summary>
    /// Simple back pressure implementation that makes sure the minimum max concurrency between producer and consumer
    /// is not amplified by the greedier of the two by more then the backPressureFactor
    /// </summary>
    /// <param name="backPressureFactor">
    /// The maximum amplification back pressure of the greedier part of the producer consumer pipeline, if null
    /// defaults to 4
    /// </param>
    /// <param name="maxConcurrency">The minimum maximum concurrency which would be the bottleneck of the producer consumer pipeline</param>
    internal ProducerConsumerBackPressure(int? backPressureFactor, int maxConcurrency)
    {
        _backPressureFactor = backPressureFactor.GetValueOrDefault(4);
        _slots = maxConcurrency * _backPressureFactor;
        _consumerLimiter = new SemaphoreSlim(_slots, _slots);
    }

    public Task WaitAsync(CancellationToken token = default(CancellationToken)) =>
        _consumerLimiter.WaitAsync(token);

    public void Release()
    {
        var minimumRelease = _slots - _consumerLimiter.CurrentCount;
        var release = Math.Min(minimumRelease, _backPressureFactor);
        if (release > 0)
            _consumerLimiter.Release(release);
    }
}
