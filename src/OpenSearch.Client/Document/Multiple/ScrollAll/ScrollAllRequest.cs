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
using System.Linq.Expressions;
using OpenSearch.Net;

namespace OpenSearch.Client;

public interface IScrollAllRequest
{
    /// <summary>
    /// Simple back pressure implementation that makes sure the minimum max concurrency between producer and consumer
    /// is not amplified by the greedier of the two by more then a given back pressure factor
    /// When set each scroll request will additionally wait on <see cref="ProducerConsumerBackPressure.WaitAsync" /> as well as
    /// <see cref="MaxDegreeOfParallelism" /> if set. Not that the consumer has to call <see cref="ProducerConsumerBackPressure.Release" />
    /// on the same instance every time it is done.
    /// </summary>
    ProducerConsumerBackPressure BackPressure { get; set; }

    /// <summary>
    /// The maximum degree of parallelism we should drain the sliced scroll, defaults to the value of <see cref="Slices" />
    /// </summary>
    int? MaxDegreeOfParallelism { get; set; }

    /// <summary>
    /// Set a different routing field, has to have doc_values enabled
    /// </summary>
    Field RoutingField { get; set; }

    /// <summary>
    /// The ammount of time to keep the scroll alive on the server
    /// </summary>
    Time ScrollTime { get; set; }

    /// <summary>
    /// An optional search request that describes the search we want to scroll over.
    /// Defaults to matchall on the index and type of T in the <see cref="ScrollAllObserver{T}" />.
    /// Note: both scroll and slice information WILL be overriden.
    /// </summary>
    ISearchRequest Search { get; set; }

    /// <summary>
    /// The maximum number of slices to partition the scroll over
    /// </summary>
    int Slices { get; set; }
}

public class ScrollAllRequest : IScrollAllRequest, IHelperCallable
{
    public ScrollAllRequest(Time scrollTime, int numberOfSlices)
    {
        var i = (IScrollAllRequest)this;
        i.ScrollTime = scrollTime;
        i.Slices = numberOfSlices;
    }

    public ScrollAllRequest(Time scrollTime, int numberOfSlices, Field routingField) : this(scrollTime, numberOfSlices) =>
        RoutingField = routingField;

    /// <inheritdoc />
    public ProducerConsumerBackPressure BackPressure { get; set; }

    /// <inheritdoc />
    public int? MaxDegreeOfParallelism { get; set; }

    /// <inheritdoc />
    public Field RoutingField { get; set; }

    internal RequestMetaData ParentMetaData { get; set; }

    /// <inheritdoc />
    public ISearchRequest Search { get; set; }
    RequestMetaData IHelperCallable.ParentMetaData { get => ParentMetaData; set => ParentMetaData = value; }
    Time IScrollAllRequest.ScrollTime { get; set; }
    int IScrollAllRequest.Slices { get; set; }
}

public class ScrollAllDescriptor<T> : DescriptorBase<ScrollAllDescriptor<T>, IScrollAllRequest>, IScrollAllRequest where T : class
{
    public ScrollAllDescriptor(Time scrollTime, int numberOfSlices)
    {
        Self.ScrollTime = scrollTime;
        Self.Slices = numberOfSlices;
    }

    ProducerConsumerBackPressure IScrollAllRequest.BackPressure { get; set; }
    int? IScrollAllRequest.MaxDegreeOfParallelism { get; set; }
    Field IScrollAllRequest.RoutingField { get; set; }
    Time IScrollAllRequest.ScrollTime { get; set; }
    ISearchRequest IScrollAllRequest.Search { get; set; }
    int IScrollAllRequest.Slices { get; set; }

    /// <inheritdoc />
    public ScrollAllDescriptor<T> MaxDegreeOfParallelism(int? maxDegreeOfParallelism) =>
        Assign(maxDegreeOfParallelism, (a, v) => a.MaxDegreeOfParallelism = v);

    /// <inheritdoc />
    public ScrollAllDescriptor<T> RoutingField(Field field) => Assign(field, (a, v) => a.RoutingField = v);

    /// <inheritdoc />
    public ScrollAllDescriptor<T> RoutingField<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.RoutingField = v);

    /// <inheritdoc />
    public ScrollAllDescriptor<T> Search(Func<SearchDescriptor<T>, ISearchRequest> selector) =>
        Assign(selector, (a, v) => a.Search = v?.Invoke(new SearchDescriptor<T>()));

    /// <summary>
    /// Simple back pressure implementation that makes sure the minimum max concurrency between producer and consumer
    /// is not amplified by the greedier of the two by more then a given back pressure factor
    /// When set each bulk request will call <see cref="ProducerConsumerBackPressure.Release" />
    /// </summary>
    /// <param name="maxConcurrency">The minimum maximum concurrency which would be the bottleneck of the producer consumer pipeline</param>
    /// <param name="backPressureFactor">The maximum amplification back pressure of the greedier part of the producer consumer pipeline</param>
    public ScrollAllDescriptor<T> BackPressure(int maxConcurrency, int? backPressureFactor = null) =>
        Assign(new ProducerConsumerBackPressure(backPressureFactor, maxConcurrency), (a, v) => a.BackPressure = v);
}
