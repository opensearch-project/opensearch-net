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
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A multi-bucket aggregation that creates composite buckets from different sources.
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(CompositeAggregation))]
public interface ICompositeAggregation : IBucketAggregation
{
    /// <summary>
    /// Used to retrieve the composite buckets that are after the
    /// last composite buckets returned in a previous round
    /// </summary>
    [DataMember(Name = "after")]
    CompositeKey After { get; set; }

    /// <summary>
    /// Defines how many composite buckets should be returned.
    /// Each composite bucket is considered as a single bucket so
    /// setting a size of 10 will return the first 10 composite buckets
    /// created from the values source.
    /// </summary>
    [DataMember(Name = "size")]
    int? Size { get; set; }

    /// <summary>
    /// Controls the sources that should be used to build the composite buckets
    /// </summary>
    [DataMember(Name = "sources")]
    IEnumerable<ICompositeAggregationSource> Sources { get; set; }
}

/// <inheritdoc cref="ICompositeAggregation" />
public class CompositeAggregation : BucketAggregationBase, ICompositeAggregation
{
    internal CompositeAggregation() { }

    public CompositeAggregation(string name) : base(name) { }

    /// <inheritdoc />
    public CompositeKey After { get; set; }

    /// <inheritdoc />
    public int? Size { get; set; }

    /// <inheritdoc />
    public IEnumerable<ICompositeAggregationSource> Sources { get; set; }

    internal override void WrapInContainer(AggregationContainer c) => c.Composite = this;
}

/// <inheritdoc cref="ICompositeAggregation" />
public class CompositeAggregationDescriptor<T>
    : BucketAggregationDescriptorBase<CompositeAggregationDescriptor<T>, ICompositeAggregation, T>
        , ICompositeAggregation
    where T : class
{
    CompositeKey ICompositeAggregation.After { get; set; }
    int? ICompositeAggregation.Size { get; set; }
    IEnumerable<ICompositeAggregationSource> ICompositeAggregation.Sources { get; set; }

    /// <inheritdoc cref="ICompositeAggregation.Sources" />
    public CompositeAggregationDescriptor<T> Sources(
        Func<CompositeAggregationSourcesDescriptor<T>, IPromise<IList<ICompositeAggregationSource>>> selector
    ) =>
        Assign(selector, (a, v) => a.Sources = v?.Invoke(new CompositeAggregationSourcesDescriptor<T>())?.Value);

    /// <inheritdoc cref="ICompositeAggregation.Size" />
    public CompositeAggregationDescriptor<T> Size(int? size) => Assign(size, (a, v) => a.Size = v);

    /// <inheritdoc cref="ICompositeAggregation.After" />
    public CompositeAggregationDescriptor<T> After(CompositeKey after) => Assign(after, (a, v) => a.After = v);
}
