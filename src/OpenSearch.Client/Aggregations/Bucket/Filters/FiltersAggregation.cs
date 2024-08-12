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
using System.Linq;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
[ReadAs(typeof(FiltersAggregation))]
public interface IFiltersAggregation : IBucketAggregation
{
    [DataMember(Name = "filters")]
    Union<INamedFiltersContainer, IEnumerable<QueryContainer>> Filters { get; set; }

    [DataMember(Name = "other_bucket")]
    bool? OtherBucket { get; set; }

    [DataMember(Name = "other_bucket_key")]
    string OtherBucketKey { get; set; }
}

public class FiltersAggregation : BucketAggregationBase, IFiltersAggregation
{
    internal FiltersAggregation() { }

    public FiltersAggregation(string name) : base(name) { }

    public Union<INamedFiltersContainer, IEnumerable<QueryContainer>> Filters { get; set; }

    /// <summary>
    /// Gets or sets whether to add a bucket to the response which will contain all documents
    /// that do not match any of the given filters.
    /// When set to <c>true</c>, the other bucket will be returned either in a bucket
    /// (named "_other_" by default) if named filters are being used,
    ///  or as the last bucket if anonymous filters are being used
    /// When set to <c>false</c>, does not compute
    /// the other bucket.
    /// </summary>
    public bool? OtherBucket { get; set; }

    /// <summary>
    /// Gets or sets the key for the other bucket to a value other than the default "_other_".
    /// Setting this parameter will implicitly set the <see cref="OtherBucket" /> parameter to true
    /// </summary>
    public string OtherBucketKey { get; set; }

    internal override void WrapInContainer(AggregationContainer c) => c.Filters = this;
}

public class FiltersAggregationDescriptor<T>
    : BucketAggregationDescriptorBase<FiltersAggregationDescriptor<T>, IFiltersAggregation, T>
        , IFiltersAggregation
    where T : class
{
    Union<INamedFiltersContainer, IEnumerable<QueryContainer>> IFiltersAggregation.Filters { get; set; }

    bool? IFiltersAggregation.OtherBucket { get; set; }

    string IFiltersAggregation.OtherBucketKey { get; set; }

    /// <summary>
    /// Adds a bucket to the response which will contain all documents
    /// that do not match any of the given filters.
    /// When set to <c>true</c>, the other bucket will be returned either in a bucket
    /// (named "_other_" by default) if named filters are being used,
    ///  or as the last bucket if anonymous filters are being used
    /// When set to <c>false</c>, does not compute
    /// the other bucket.
    /// </summary>
    /// <param name="otherBucket">whether to set the other bucket</param>
    /// <returns>the <see cref="FiltersAggregationDescriptor{T}" /></returns>
    public FiltersAggregationDescriptor<T> OtherBucket(bool? otherBucket = true) =>
        Assign(otherBucket, (a, v) => a.OtherBucket = v);

    /// <summary>
    /// Sets the key for the other bucket to a value other than the default "_other_".
    /// Setting this parameter will implicitly set the <see cref="OtherBucket" /> parameter to true
    /// </summary>
    /// <param name="otherBucketKey">the name for the other bucket</param>
    /// <returns>the <see cref="FiltersAggregationDescriptor{T}" /></returns>
    public FiltersAggregationDescriptor<T> OtherBucketKey(string otherBucketKey) =>
        Assign(otherBucketKey, (a, v) => a.OtherBucketKey = v);

    public FiltersAggregationDescriptor<T> NamedFilters(Func<NamedFiltersContainerDescriptor<T>, IPromise<INamedFiltersContainer>> selector) =>
        Assign(selector, (a, v) => a.Filters =
            new Union<INamedFiltersContainer, IEnumerable<QueryContainer>>(v?.Invoke(new NamedFiltersContainerDescriptor<T>())?.Value));

    public FiltersAggregationDescriptor<T> AnonymousFilters(params Func<QueryContainerDescriptor<T>, QueryContainer>[] selectors) =>
        Assign(selectors, (a, v) => a.Filters = v.Select(vv => vv?.Invoke(new QueryContainerDescriptor<T>())).ToList());

    public FiltersAggregationDescriptor<T> AnonymousFilters(IEnumerable<Func<QueryContainerDescriptor<T>, QueryContainer>> selectors) =>
        Assign(selectors, (a, v) => a.Filters = v.Select(vv => vv?.Invoke(new QueryContainerDescriptor<T>())).ToList());
}
