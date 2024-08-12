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

[InterfaceDataContract]
[JsonFormatter(typeof(FieldNameQueryFormatter<NumericRangeQuery, INumericRangeQuery>))]
public interface INumericRangeQuery : IRangeQuery
{
    [DataMember(Name = "gt")]
    double? GreaterThan { get; set; }

    [DataMember(Name = "gte")]
    double? GreaterThanOrEqualTo { get; set; }

    [DataMember(Name = "lt")]
    double? LessThan { get; set; }

    [DataMember(Name = "lte")]
    double? LessThanOrEqualTo { get; set; }

    [DataMember(Name = "relation")]
    RangeRelation? Relation { get; set; }
}

public class NumericRangeQuery : FieldNameQueryBase, INumericRangeQuery
{
    public double? GreaterThan { get; set; }
    public double? GreaterThanOrEqualTo { get; set; }
    public double? LessThan { get; set; }
    public double? LessThanOrEqualTo { get; set; }

    public RangeRelation? Relation { get; set; }
    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.Range = this;

    internal static bool IsConditionless(INumericRangeQuery q) => q.Field.IsConditionless()
        || q.GreaterThanOrEqualTo == null
        && q.LessThanOrEqualTo == null
        && q.GreaterThan == null
        && q.LessThan == null;
}

[DataContract]
public class NumericRangeQueryDescriptor<T>
    : FieldNameQueryDescriptorBase<NumericRangeQueryDescriptor<T>, INumericRangeQuery, T>
        , INumericRangeQuery where T : class
{
    protected override bool Conditionless => NumericRangeQuery.IsConditionless(this);
    double? INumericRangeQuery.GreaterThan { get; set; }
    double? INumericRangeQuery.GreaterThanOrEqualTo { get; set; }
    double? INumericRangeQuery.LessThan { get; set; }
    double? INumericRangeQuery.LessThanOrEqualTo { get; set; }

    RangeRelation? INumericRangeQuery.Relation { get; set; }

    public NumericRangeQueryDescriptor<T> GreaterThan(double? from) => Assign(from, (a, v) => a.GreaterThan = v);

    public NumericRangeQueryDescriptor<T> GreaterThanOrEquals(double? from) => Assign(from, (a, v) => a.GreaterThanOrEqualTo = v);

    public NumericRangeQueryDescriptor<T> LessThan(double? to) => Assign(to, (a, v) => a.LessThan = v);

    public NumericRangeQueryDescriptor<T> LessThanOrEquals(double? to) => Assign(to, (a, v) => a.LessThanOrEqualTo = v);

    public NumericRangeQueryDescriptor<T> Relation(RangeRelation? relation) => Assign(relation, (a, v) => a.Relation = v);
}
