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
using System.Runtime.Serialization;

namespace OpenSearch.Client;

/// <summary>
/// A values source that can be applied on date values to build fixed size interval over the values.
/// The interval parameter defines a date/time expression that determines how values should be transformed.
/// For instance an interval set to month will translate any values to its closest month interval..
/// </summary>
public interface IDateHistogramCompositeAggregationSource : ICompositeAggregationSource
{
    /// <summary>
    /// Return a formatted date string as the key instead an epoch long
    /// </summary>
    [DataMember(Name = "format")]
    string Format { get; set; }

    /// <summary>
    /// The calendar interval to use when bucketing documents
    /// </summary>
    [DataMember(Name = "calendar_interval")]
    public Union<DateInterval?, DateMathTime> CalendarInterval { get; set; }

    /// <summary>
    /// The fixed interval to use when bucketing documents
    /// </summary>
    [DataMember(Name = "fixed_interval")]
    public Time FixedInterval { get; set; }

    /// <summary>
    /// Used to indicate that bucketing should use a different time zone.
    /// Time zones may either be specified as an ISO 8601 UTC offset (e.g. +01:00 or -08:00)
    /// or as a timezone id, an identifier used in the TZ database like America/Los_Angeles.
    /// </summary>
    [DataMember(Name = "time_zone")]
    string TimeZone { get; set; }
}

/// <inheritdoc cref="IDateHistogramCompositeAggregationSource" />
public class DateHistogramCompositeAggregationSource : CompositeAggregationSourceBase, IDateHistogramCompositeAggregationSource
{
    public DateHistogramCompositeAggregationSource(string name) : base(name) { }

    /// <inheritdoc />
    public string Format { get; set; }

    /// <inheritdoc />
    public Union<DateInterval?, DateMathTime> CalendarInterval { get; set; }

    /// <inheritdoc />
    public Time FixedInterval { get; set; }

    /// <inheritdoc />
    public string TimeZone { get; set; }

    /// <inheritdoc />
    protected override string SourceType => "date_histogram";
}

/// <inheritdoc cref="IDateHistogramCompositeAggregationSource" />
public class DateHistogramCompositeAggregationSourceDescriptor<T>
    : CompositeAggregationSourceDescriptorBase<DateHistogramCompositeAggregationSourceDescriptor<T>, IDateHistogramCompositeAggregationSource, T>,
        IDateHistogramCompositeAggregationSource
{
    public DateHistogramCompositeAggregationSourceDescriptor(string name) : base(name, "date_histogram") { }

    string IDateHistogramCompositeAggregationSource.Format { get; set; }
    Union<DateInterval?, DateMathTime> IDateHistogramCompositeAggregationSource.CalendarInterval { get; set; }
    Time IDateHistogramCompositeAggregationSource.FixedInterval { get; set; }
    string IDateHistogramCompositeAggregationSource.TimeZone { get; set; }

    /// <inheritdoc cref="IDateHistogramCompositeAggregationSource.CalendarInterval" />
    public DateHistogramCompositeAggregationSourceDescriptor<T> CalendarInterval(DateInterval? interval) =>
        Assign(interval, (a, v) => a.CalendarInterval = v);

    /// <inheritdoc cref="IDateHistogramCompositeAggregationSource.CalendarInterval" />
    public DateHistogramCompositeAggregationSourceDescriptor<T> CalendarInterval(DateMathTime interval) =>
        Assign(interval, (a, v) => a.CalendarInterval = v);

    /// <inheritdoc cref="IDateHistogramCompositeAggregationSource.FixedInterval" />
    public DateHistogramCompositeAggregationSourceDescriptor<T> FixedInterval(Time interval) =>
        Assign(interval, (a, v) => a.FixedInterval = v);

    /// <inheritdoc cref="IDateHistogramCompositeAggregationSource.TimeZone" />
    public DateHistogramCompositeAggregationSourceDescriptor<T> TimeZone(string timezone) => Assign(timezone, (a, v) => a.TimeZone = v);

    /// <inheritdoc cref="IDateHistogramCompositeAggregationSource.TimeZone" />
    public DateHistogramCompositeAggregationSourceDescriptor<T> Format(string format) => Assign(format, (a, v) => a.Format = v);
}
