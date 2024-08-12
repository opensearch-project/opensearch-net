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

using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// Range that defines a bucket for either the <see cref="RangeAggregation" /> or
/// <see cref="GeoDistanceAggregation" />. If you are looking to store ranges as
/// part of your document please use explicit range class e.g DateRange, FloatRange etc
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(AggregationRange))]
public interface IAggregationRange
{
    [DataMember(Name = "from")]
    double? From { get; set; }

    [DataMember(Name = "key")]
    string Key { get; set; }

    [DataMember(Name = "to")]
    double? To { get; set; }
}

/// <inheritdoc />
public class AggregationRange : IAggregationRange
{
    public double? From { get; set; }
    public string Key { get; set; }
    public double? To { get; set; }
}

/// <inheritdoc />
public class AggregationRangeDescriptor : DescriptorBase<AggregationRangeDescriptor, IAggregationRange>, IAggregationRange
{
    double? IAggregationRange.From { get; set; }
    string IAggregationRange.Key { get; set; }
    double? IAggregationRange.To { get; set; }

    public AggregationRangeDescriptor Key(string key) => Assign(key, (a, v) => a.Key = v);

    public AggregationRangeDescriptor From(double? from) => Assign(from, (a, v) => a.From = v);

    public AggregationRangeDescriptor To(double? to) => Assign(to, (a, v) => a.To = v);
}
