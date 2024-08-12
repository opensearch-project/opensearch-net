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
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// The purpose of this processor is to point documents to the right time
/// based index based on a date or timestamp field in a document
/// by using the date math index name support.
/// </summary>
[InterfaceDataContract]
public interface IDateIndexNameProcessor : IProcessor
{
    /// <summary>
    /// An array of the expected date formats for parsing
    /// dates / timestamps in the document being preprocessed.
    /// Default is yyyy-MM-dd'T'HH:mm:ss.SSSZ
    /// </summary>
    [DataMember(Name = "date_formats")]
    IEnumerable<string> DateFormats { get; set; }

    /// <summary>
    /// How to round the date when formatting the date into the index name.
    /// </summary>
    [DataMember(Name = "date_rounding")]
    DateRounding? DateRounding { get; set; }

    /// <summary>
    /// The field to get the date or timestamp from.
    /// </summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }

    /// <summary>
    /// The format to be used when printing the parsed date into
    /// the index name.
    /// </summary>
    [DataMember(Name = "index_name_format")]
    string IndexNameFormat { get; set; }

    /// <summary>
    /// A prefix of the index name to be prepended before the printed date.
    /// </summary>
    [DataMember(Name = "index_name_prefix")]
    string IndexNamePrefix { get; set; }

    /// <summary>
    /// The locale to use when parsing the date from the document
    /// being preprocessed, relevant when parsing month names or
    /// week days.
    /// </summary>
    [DataMember(Name = "locale")]
    string Locale { get; set; }

    /// <summary>
    /// The timezone to use when parsing the date and when date
    /// math index supports resolves expressions into concrete
    /// index names.
    /// </summary>
    [DataMember(Name = "timezone")]
    string TimeZone { get; set; }
}

public class DateIndexNameProcessor : ProcessorBase, IDateIndexNameProcessor
{
    /// <summary>
    /// An array of the expected date formats for parsing
    /// dates / timestamps in the document being preprocessed.
    /// Default is yyyy-MM-dd'T'HH:mm:ss.SSSZ
    /// </summary>
    public IEnumerable<string> DateFormats { get; set; }

    /// <summary>
    /// How to round the date when formatting the date into the index name.
    /// </summary>
    public DateRounding? DateRounding { get; set; }

    /// <summary>
    /// The field to get the date or timestamp from.
    /// </summary>
    public Field Field { get; set; }

    /// <summary>
    /// The format to be used when printing the parsed date into
    /// the index name.
    /// </summary>
    public string IndexNameFormat { get; set; }

    /// <summary>
    /// A prefix of the index name to be prepended before the printed date.
    /// </summary>
    public string IndexNamePrefix { get; set; }

    /// <summary>
    /// The locale to use when parsing the date from the document
    /// being preprocessed, relevant when parsing month names or
    /// week days.
    /// </summary>
    public string Locale { get; set; }

    /// <summary>
    /// The timezone to use when parsing the date and when date
    /// math index supports resolves expressions into concrete
    /// index names.
    /// </summary>
    public string TimeZone { get; set; }

    protected override string Name => "date_index_name";
}

public class DateIndexNameProcessorDescriptor<T>
    : ProcessorDescriptorBase<DateIndexNameProcessorDescriptor<T>, IDateIndexNameProcessor>, IDateIndexNameProcessor
    where T : class
{
    protected override string Name => "date_index_name";
    IEnumerable<string> IDateIndexNameProcessor.DateFormats { get; set; }
    DateRounding? IDateIndexNameProcessor.DateRounding { get; set; }

    Field IDateIndexNameProcessor.Field { get; set; }
    string IDateIndexNameProcessor.IndexNameFormat { get; set; }
    string IDateIndexNameProcessor.IndexNamePrefix { get; set; }
    string IDateIndexNameProcessor.Locale { get; set; }
    string IDateIndexNameProcessor.TimeZone { get; set; }

    /// <summary>
    /// The field to get the date or timestamp from.
    /// </summary>
    public DateIndexNameProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <summary>
    /// The field to get the date or timestamp from.
    /// </summary>
    public DateIndexNameProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.Field = v);

    /// <summary>
    /// A prefix of the index name to be prepended before the printed date.
    /// </summary>
    public DateIndexNameProcessorDescriptor<T> IndexNamePrefix(string indexNamePrefix) =>
        Assign(indexNamePrefix, (a, v) => a.IndexNamePrefix = v);

    /// <summary>
    /// How to round the date when formatting the date into the index name.
    /// </summary>
    public DateIndexNameProcessorDescriptor<T> DateRounding(DateRounding? dateRounding) =>
        Assign(dateRounding, (a, v) => a.DateRounding = v);

    /// <summary>
    /// An array of the expected date formats for parsing
    /// dates / timestamps in the document being preprocessed.
    /// Default is yyyy-MM-dd'T'HH:mm:ss.SSSZ
    /// </summary>
    public DateIndexNameProcessorDescriptor<T> DateFormats(IEnumerable<string> dateFormats) =>
        Assign(dateFormats, (a, v) => a.DateFormats = v);

    /// <summary>
    /// An array of the expected date formats for parsing
    /// dates / timestamps in the document being preprocessed.
    /// Default is yyyy-MM-dd'T'HH:mm:ss.SSSZ
    /// </summary>
    public DateIndexNameProcessorDescriptor<T> DateFormats(params string[] dateFormats) =>
        Assign(dateFormats, (a, v) => a.DateFormats = v);

    /// <summary>
    /// The timezone to use when parsing the date and when date
    /// math index supports resolves expressions into concrete
    /// index names.
    /// </summary>
    public DateIndexNameProcessorDescriptor<T> TimeZone(string timeZone) =>
        Assign(timeZone, (a, v) => a.TimeZone = v);

    /// <summary>
    /// The locale to use when parsing the date from the document
    /// being preprocessed, relevant when parsing month names or
    /// week days.
    /// </summary>
    public DateIndexNameProcessorDescriptor<T> Locale(string locale) =>
        Assign(locale, (a, v) => a.Locale = v);

    /// <summary>
    /// The format to be used when printing the parsed date into
    /// the index name.
    /// </summary>
    public DateIndexNameProcessorDescriptor<T> IndexNameFormat(string indexNameFormat) =>
        Assign(indexNameFormat, (a, v) => a.IndexNameFormat = v);
}

[StringEnum]
public enum DateRounding
{
    [EnumMember(Value = "s")]
    Second,

    [EnumMember(Value = "m")]
    Minute,

    [EnumMember(Value = "h")]
    Hour,

    [EnumMember(Value = "d")]
    Day,

    [EnumMember(Value = "w")]
    Week,

    [EnumMember(Value = "M")]
    Month,

    [EnumMember(Value = "y")]
    Year
}
