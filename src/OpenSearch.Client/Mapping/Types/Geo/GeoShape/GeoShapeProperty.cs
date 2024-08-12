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

using System.Diagnostics;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// Maps a property as a geo_shape field
/// </summary>
[InterfaceDataContract]
public interface IGeoShapeProperty : IDocValuesProperty
{
    /// <summary>
    /// If <c>true</c>, malformed geojson shapes are ignored. If false (default),
    /// malformed geojson shapes throw an exception and reject the whole document.
    /// </summary>
    [DataMember(Name = "ignore_malformed")]
    bool? IgnoreMalformed { get; set; }

    /// <summary>
    /// If true (default) three dimension points will be accepted (stored in source) but
    /// only latitude and longitude values will be indexed; the third dimension is ignored. If false,
    /// geo-points containing any more than latitude and longitude (two dimensions) values throw
    /// an exception and reject the whole document.
    /// </summary>
    [DataMember(Name = "ignore_z_value")]
    bool? IgnoreZValue { get; set; }

    /// <summary>
    /// Defines how to interpret vertex order for polygons and multipolygons.
    /// Defaults to <see cref="GeoOrientation.CounterClockWise" />
    /// </summary>
    [DataMember(Name = "orientation")]
    GeoOrientation? Orientation { get; set; }

    /// <summary>
    /// Defines the approach for how to represent shapes at indexing and search time.
    /// It also influences the capabilities available so it is recommended to let
    /// OpenSearch set this parameter automatically.
    /// </summary>
    [DataMember(Name = "strategy")]
    GeoStrategy? Strategy { get; set; }

    /// <summary>
    /// Should the data be coerced into becoming a valid geo shape (for instance closing a polygon)
    /// </summary>
    [DataMember(Name = "coerce")]
    bool? Coerce { get; set; }
}

/// <inheritdoc cref="IGeoShapeProperty" />
[DebuggerDisplay("{DebugDisplay}")]
public class GeoShapeProperty : DocValuesPropertyBase, IGeoShapeProperty
{
    public GeoShapeProperty() : base(FieldType.GeoShape) { }

    /// <inheritdoc />
    public bool? IgnoreMalformed { get; set; }

    /// <inheritdoc />
    public bool? IgnoreZValue { get; set; }

    /// <inheritdoc />
    public GeoOrientation? Orientation { get; set; }

    /// <inheritdoc />
    public GeoStrategy? Strategy { get; set; }

    /// <inheritdoc />
    public bool? Coerce { get; set; }
}

/// <inheritdoc cref="IGeoShapeProperty" />
[DebuggerDisplay("{DebugDisplay}")]
public class GeoShapePropertyDescriptor<T>
    : DocValuesPropertyDescriptorBase<GeoShapePropertyDescriptor<T>, IGeoShapeProperty, T>, IGeoShapeProperty
    where T : class
{
    public GeoShapePropertyDescriptor() : base(FieldType.GeoShape) { }


    bool? IGeoShapeProperty.IgnoreMalformed { get; set; }
    bool? IGeoShapeProperty.IgnoreZValue { get; set; }
    GeoOrientation? IGeoShapeProperty.Orientation { get; set; }
    GeoStrategy? IGeoShapeProperty.Strategy { get; set; }
    bool? IGeoShapeProperty.Coerce { get; set; }

    /// <inheritdoc cref="IGeoShapeProperty.Strategy" />
    public GeoShapePropertyDescriptor<T> Strategy(GeoStrategy? strategy) => Assign(strategy, (a, v) => a.Strategy = v);

    /// <inheritdoc cref="IGeoShapeProperty.Orientation" />
    public GeoShapePropertyDescriptor<T> Orientation(GeoOrientation? orientation) => Assign(orientation, (a, v) => a.Orientation = v);

    /// <inheritdoc cref="IGeoShapeProperty.IgnoreMalformed" />
    public GeoShapePropertyDescriptor<T> IgnoreMalformed(bool? ignoreMalformed = true) =>
        Assign(ignoreMalformed, (a, v) => a.IgnoreMalformed = v);

    /// <inheritdoc cref="IGeoShapeProperty.IgnoreZValue" />
    public GeoShapePropertyDescriptor<T> IgnoreZValue(bool? ignoreZValue = true) =>
        Assign(ignoreZValue, (a, v) => a.IgnoreZValue = v);

    /// <inheritdoc cref="IGeoShapeProperty.Coerce" />
    public GeoShapePropertyDescriptor<T> Coerce(bool? coerce = true) =>
        Assign(coerce, (a, v) => a.Coerce = v);
}
