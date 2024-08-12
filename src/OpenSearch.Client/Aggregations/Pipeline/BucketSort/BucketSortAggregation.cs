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
/// A parent pipeline aggregation which sorts the buckets of its parent multi-bucket aggregation.
/// Zero or more sort fields may be specified together with the corresponding sort order.
/// Each bucket may be sorted based on its _key, _count or its sub-aggregations.
/// In addition, parameters from and size may be set in order to truncate the result buckets.
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(BucketSortAggregation))]
public interface IBucketSortAggregation : IAggregation
{
    /// <summary>
    /// Buckets in positions prior to the set value will be truncated
    /// </summary>
    [DataMember(Name = "from")]
    int? From { get; set; }

    /// <summary>
    /// The policy to apply when gaps are found in the data
    /// </summary>
    [DataMember(Name = "gap_policy")]
    GapPolicy? GapPolicy { get; set; }

    /// <summary>
    /// 	The number of buckets to return. Defaults to all buckets of the parent aggregation
    /// </summary>
    [DataMember(Name = "size")]
    int? Size { get; set; }

    /// <summary>
    /// The list of fields to sort on
    /// </summary>
    [DataMember(Name = "sort")]
    IList<ISort> Sort { get; set; }
}

/// <inheritdoc cref="IBucketSortAggregation" />
public class BucketSortAggregation
    : AggregationBase, IBucketSortAggregation
{
    internal BucketSortAggregation() { }

    public BucketSortAggregation(string name)
        : base(name) { }

    /// <inheritdoc />
    public int? From { get; set; }

    /// <inheritdoc />
    public GapPolicy? GapPolicy { get; set; }

    /// <inheritdoc />
    public int? Size { get; set; }

    /// <inheritdoc />
    public IList<ISort> Sort { get; set; }

    internal override void WrapInContainer(AggregationContainer c) => c.BucketSort = this;
}

/// <inheritdoc cref="IBucketSortAggregation" />
public class BucketSortAggregationDescriptor<T>
    : DescriptorBase<BucketSortAggregationDescriptor<T>, IBucketSortAggregation>
        , IBucketSortAggregation
    where T : class
{
    int? IBucketSortAggregation.From { get; set; }
    GapPolicy? IBucketSortAggregation.GapPolicy { get; set; }
    IDictionary<string, object> IAggregation.Meta { get; set; }
    string IAggregation.Name { get; set; }
    int? IBucketSortAggregation.Size { get; set; }
    IList<ISort> IBucketSortAggregation.Sort { get; set; }

    /// <summary>
    /// The list of fields to sort on
    /// </summary>
    public BucketSortAggregationDescriptor<T> Sort(Func<SortDescriptor<T>, IPromise<IList<ISort>>> selector) =>
        Assign(selector, (a, v) => a.Sort = v?.Invoke(new SortDescriptor<T>())?.Value);

    /// <summary>
    /// Buckets in positions prior to the set value will be truncated
    /// </summary>
    public BucketSortAggregationDescriptor<T> From(int? from) => Assign(from, (a, v) => a.From = v);

    /// <summary>
    /// 	The number of buckets to return. Defaults to all buckets of the parent aggregation
    /// </summary>
    public BucketSortAggregationDescriptor<T> Size(int? size) => Assign(size, (a, v) => a.Size = v);

    /// <summary>
    /// The policy to apply when gaps are found in the data
    /// </summary>
    public BucketSortAggregationDescriptor<T> GapPolicy(GapPolicy? gapPolicy) => Assign(gapPolicy, (a, v) => a.GapPolicy = v);

    /// <summary>
    /// The metadata for the aggregation
    /// </summary>
    public BucketSortAggregationDescriptor<T> Meta(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> selector) =>
        Assign(selector, (a, v) => a.Meta = v?.Invoke(new FluentDictionary<string, object>()));
}
