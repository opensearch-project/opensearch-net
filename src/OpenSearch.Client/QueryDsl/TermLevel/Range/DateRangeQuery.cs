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
[JsonFormatter(typeof(FieldNameQueryFormatter<DateRangeQuery, IDateRangeQuery>))]
public interface IDateRangeQuery : IRangeQuery
{
    [DataMember(Name = "format")]
    string Format { get; set; }

    [DataMember(Name = "gt")]
    DateMath GreaterThan { get; set; }

    [DataMember(Name = "gte")]
    DateMath GreaterThanOrEqualTo { get; set; }

    [DataMember(Name = "lt")]
    DateMath LessThan { get; set; }

    [DataMember(Name = "lte")]
    DateMath LessThanOrEqualTo { get; set; }

    [DataMember(Name = "relation")]
    RangeRelation? Relation { get; set; }

    [DataMember(Name = "time_zone")]
    string TimeZone { get; set; }
}

public class DateRangeQuery : FieldNameQueryBase, IDateRangeQuery
{
    public string Format { get; set; }
    public DateMath GreaterThan { get; set; }

    public DateMath GreaterThanOrEqualTo { get; set; }
    public DateMath LessThan { get; set; }
    public DateMath LessThanOrEqualTo { get; set; }
    public RangeRelation? Relation { get; set; }
    public string TimeZone { get; set; }
    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.Range = this;

    internal static bool IsConditionless(IDateRangeQuery q) => q.Field.IsConditionless()
        || ((q.GreaterThanOrEqualTo == null || !q.GreaterThanOrEqualTo.IsValid)
        && (q.LessThanOrEqualTo == null || !q.LessThanOrEqualTo.IsValid)
        && (q.GreaterThan == null || !q.GreaterThan.IsValid)
        && (q.LessThan == null || !q.LessThan.IsValid));
}

[DataContract]
public class DateRangeQueryDescriptor<T>
    : FieldNameQueryDescriptorBase<DateRangeQueryDescriptor<T>, IDateRangeQuery, T>
        , IDateRangeQuery where T : class
{
    protected override bool Conditionless => DateRangeQuery.IsConditionless(this);
    string IDateRangeQuery.Format { get; set; }
    DateMath IDateRangeQuery.GreaterThan { get; set; }
    DateMath IDateRangeQuery.GreaterThanOrEqualTo { get; set; }
    DateMath IDateRangeQuery.LessThan { get; set; }
    DateMath IDateRangeQuery.LessThanOrEqualTo { get; set; }
    RangeRelation? IDateRangeQuery.Relation { get; set; }
    string IDateRangeQuery.TimeZone { get; set; }

    public DateRangeQueryDescriptor<T> GreaterThan(DateMath from) => Assign(from, (a, v) => a.GreaterThan = v);

    public DateRangeQueryDescriptor<T> GreaterThanOrEquals(DateMath from) => Assign(from, (a, v) => a.GreaterThanOrEqualTo = v);

    public DateRangeQueryDescriptor<T> LessThan(DateMath to) => Assign(to, (a, v) => a.LessThan = v);

    public DateRangeQueryDescriptor<T> LessThanOrEquals(DateMath to) => Assign(to, (a, v) => a.LessThanOrEqualTo = v);

    public DateRangeQueryDescriptor<T> TimeZone(string timeZone) => Assign(timeZone, (a, v) => a.TimeZone = v);

    public DateRangeQueryDescriptor<T> Format(string format) => Assign(format, (a, v) => a.Format = v);

    public DateRangeQueryDescriptor<T> Relation(RangeRelation? relation) => Assign(relation, (a, v) => a.Relation = v);
}
