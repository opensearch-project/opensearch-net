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
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary> Allows you to sort based on a proximity to one or more <see cref="GeoLocation" /> </summary>
[InterfaceDataContract]
public interface IGeoDistanceSort : ISort
{
    /// <summary>
    /// How to compute the distance. Can either be arc (default), or plane (faster, but
    /// inaccurate on long distances and close to the poles).
    /// </summary>
    [DataMember(Name = "distance_type")]
    GeoDistanceType? DistanceType { get; set; }

    Field Field { get; set; }

    /// <summary> The unit to use when computing sort values. The default is m (meters) </summary>
    [DataMember(Name = "unit")]
    DistanceUnit? Unit { get; set; }

    /// <summary>
    /// Indicates if the unmapped field should be treated as a missing value. Setting it to `true` is equivalent to specifying
    /// an `unmapped_type` in the field sort. The default is `false` (unmapped field are causing the search to fail)
    /// </summary>
    [DataMember(Name = "ignore_unmapped")]
    bool? IgnoreUnmapped { get; set; }

    IEnumerable<GeoLocation> Points { get; set; }
}

/// <inheritdoc cref="IGeoDistanceSort" />
public class GeoDistanceSort : SortBase, IGeoDistanceSort
{
    /// <inheritdoc cref="IGeoDistanceSort.DistanceType" />
    public GeoDistanceType? DistanceType { get; set; }

    public Field Field { get; set; }

    /// <inheritdoc cref="IGeoDistanceSort.Unit" />
    public DistanceUnit? Unit { get; set; }

    /// <inheritdoc cref="IGeoDistanceSort.IgnoreUnmapped" />
    public bool? IgnoreUnmapped { get; set; }

    public IEnumerable<GeoLocation> Points { get; set; }
    protected override Field SortKey => "_geo_distance";
}

/// <inheritdoc cref="IGeoDistanceSort" />
public class GeoDistanceSortDescriptor<T> : SortDescriptorBase<GeoDistanceSortDescriptor<T>, IGeoDistanceSort, T>, IGeoDistanceSort
    where T : class
{
    protected override Field SortKey => "_geo_distance";
    GeoDistanceType? IGeoDistanceSort.DistanceType { get; set; }

    Field IGeoDistanceSort.Field { get; set; }
    DistanceUnit? IGeoDistanceSort.Unit { get; set; }
    bool? IGeoDistanceSort.IgnoreUnmapped { get; set; }
    IEnumerable<GeoLocation> IGeoDistanceSort.Points { get; set; }

    public GeoDistanceSortDescriptor<T> Points(params GeoLocation[] geoLocations) => Assign(geoLocations, (a, v) => a.Points = v);

    public GeoDistanceSortDescriptor<T> Points(IEnumerable<GeoLocation> geoLocations) => Assign(geoLocations, (a, v) => a.Points = v);

    /// <inheritdoc cref="IGeoDistanceSort.Unit" />
    public GeoDistanceSortDescriptor<T> Unit(DistanceUnit? unit) => Assign(unit, (a, v) => a.Unit = v);

    /// <inheritdoc cref="IGeoDistanceSort.DistanceType" />
    public GeoDistanceSortDescriptor<T> DistanceType(GeoDistanceType? distanceType) => Assign(distanceType, (a, v) => a.DistanceType = v);

    public GeoDistanceSortDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    public GeoDistanceSortDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) => Assign(objectPath, (a, v) => a.Field = v);

    /// <inheritdoc cref="IGeoDistanceSort.IgnoreUnmapped" />
    public GeoDistanceSortDescriptor<T> IgnoreUnmapped(bool? ignoreUnmapped = true) => Assign(ignoreUnmapped, (a, v) => a.IgnoreUnmapped = v);
}
