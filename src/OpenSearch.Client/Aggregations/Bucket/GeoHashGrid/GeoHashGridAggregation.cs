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
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A multi-bucket aggregation that works on geo_point fields and groups points into buckets that represent cells in a grid.
/// The resulting grid can be sparse and only contains cells that have matching data.
/// Each cell is labeled using a geohash which is of user-definable precision.
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(GeoHashGridAggregation))]
public interface IGeoHashGridAggregation : IBucketAggregation
{
    /// <summary>
    /// The name of the field indexed with geopoints.
    /// </summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }

    /// <summary>
    /// The string length of the geohashes used to define cells/buckets in the results.
    /// Defaults to <see cref="GeoHashPrecision.Precision5"/>.
    /// </summary>
    [DataMember(Name = "precision")]
    GeoHashPrecision? Precision { get; set; }

    /// <summary>
    /// To allow for more accurate counting of the top cells returned in the final result the aggregation defaults to returning
    /// <c>max(10,(size x number-of-shards))</c> buckets from each shard. If this heuristic is undesirable,
    /// the number considered from each shard can be over-ridden using this parameter.
    /// </summary>
    [DataMember(Name = "shard_size")]
    int? ShardSize { get; set; }

    /// <summary>
    /// The maximum number of geohash buckets to return. Defaults to <c>10,000</c>. When results are trimmed,
    /// buckets are prioritised based on the volumes of documents they contain.
    /// </summary>
    [DataMember(Name = "size")]
    int? Size { get; set; }

    /// <summary>
    /// Restricts the points considered to those that fall within the bounds provided.
    /// </summary>
    [DataMember(Name = "bounds")]
    IBoundingBox Bounds { get; set; }
}

/// <inheritdoc cref="IGeoHashGridAggregation"/>
public class GeoHashGridAggregation : BucketAggregationBase, IGeoHashGridAggregation
{
    internal GeoHashGridAggregation() { }

    public GeoHashGridAggregation(string name) : base(name) { }

    /// <inheritdoc cref="IGeoHashGridAggregation.Field"/>
    public Field Field { get; set; }

    /// <inheritdoc cref="IGeoHashGridAggregation.Precision"/>
    public GeoHashPrecision? Precision { get; set; }

    /// <inheritdoc cref="IGeoHashGridAggregation.ShardSize"/>
    public int? ShardSize { get; set; }

    /// <inheritdoc cref="IGeoHashGridAggregation.Size"/>
    public int? Size { get; set; }

    /// <inheritdoc cref="IGeoHashGridAggregation.Bounds"/>
    public IBoundingBox Bounds { get; set; }

    internal override void WrapInContainer(AggregationContainer c) => c.GeoHash = this;
}

public class GeoHashGridAggregationDescriptor<T>
    : BucketAggregationDescriptorBase<GeoHashGridAggregationDescriptor<T>, IGeoHashGridAggregation, T>
        , IGeoHashGridAggregation
    where T : class
{
    Field IGeoHashGridAggregation.Field { get; set; }
    GeoHashPrecision? IGeoHashGridAggregation.Precision { get; set; }
    int? IGeoHashGridAggregation.ShardSize { get; set; }
    int? IGeoHashGridAggregation.Size { get; set; }
    IBoundingBox IGeoHashGridAggregation.Bounds { get; set; }

    /// <inheritdoc cref="IGeoHashGridAggregation.Field"/>
    public GeoHashGridAggregationDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="IGeoHashGridAggregation.Field"/>
    public GeoHashGridAggregationDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="IGeoHashGridAggregation.Size"/>
    public GeoHashGridAggregationDescriptor<T> Size(int? size) => Assign(size, (a, v) => a.Size = v);

    /// <inheritdoc cref="IGeoHashGridAggregation.ShardSize"/>
    public GeoHashGridAggregationDescriptor<T> ShardSize(int? shardSize) => Assign(shardSize, (a, v) => a.ShardSize = v);

    /// <inheritdoc cref="IGeoHashGridAggregation.Precision"/>
    // TODO: Rename to precision in next major.
    public GeoHashGridAggregationDescriptor<T> GeoHashPrecision(GeoHashPrecision? precision) =>
        Assign(precision, (a, v) => a.Precision = v);

    /// <inheritdoc cref="IGeoHashGridAggregation.Bounds"/>
    public GeoHashGridAggregationDescriptor<T> Bounds(Func<BoundingBoxDescriptor, IBoundingBox> selector) =>
        Assign(selector, (a, v) => a.Bounds = v?.Invoke(new BoundingBoxDescriptor()));
}
