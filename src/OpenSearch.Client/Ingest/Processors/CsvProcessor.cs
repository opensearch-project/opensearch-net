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
/// Extracts fields from CSV line out of a single text field within a document.
/// Any empty field in CSV will be skipped.
/// </summary>
[InterfaceDataContract]
public interface ICsvProcessor : IProcessor
{
    /// <summary>
    /// The field to extract data from
    /// </summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }

    /// <summary>
    /// The array of fields to assign extracted values to.
    /// </summary>
    [DataMember(Name = "target_fields")]
    Fields TargetFields { get; set; }

    /// <summary>
    /// Separator used in CSV, has to be single character string. Defaults to <c>,</c>
    /// </summary>
    [DataMember(Name = "separator")]
    string Separator { get; set; }

    /// <summary>
    /// Quote used in CSV, has to be single character string. Defaults to <c>"</c>
    /// </summary>
    [DataMember(Name = "quote")]
    string Quote { get; set; }

    /// <summary>
    /// If <c>true</c> and <see cref="Field" /> does not exist or is null,
    /// the processor quietly exits without modifying the document. Default is <c>false</c>
    /// </summary>
    [DataMember(Name = "ignore_missing")]
    bool? IgnoreMissing { get; set; }

    /// <summary>
    /// Trim whitespaces in unquoted fields. Default is <c>false</c>;
    /// </summary>
    [DataMember(Name = "trim")]
    bool? Trim { get; set; }

    /// <summary>
    /// Value used to fill empty fields, empty fields will be skipped if this is not provided.
    /// Empty field is one with no value (2 consecutive separators) or empty quotes (`""`)
    /// </summary>
    [DataMember(Name = "empty_value")]
    object EmptyValue { get; set; }
}

/// <inheritdoc cref="ICsvProcessor"/>
public class CsvProcessor : ProcessorBase, ICsvProcessor
{
    /// <inheritdoc />
    public Field Field { get; set; }
    /// <inheritdoc />
    public Fields TargetFields { get; set; }
    /// <inheritdoc />
    public string Separator { get; set; }
    /// <inheritdoc />
    public string Quote { get; set; }
    /// <inheritdoc />
    public bool? IgnoreMissing { get; set; }
    /// <inheritdoc />
    public bool? Trim { get; set; }
    /// <inheritdoc />
    public object EmptyValue { get; set; }
    /// <inheritdoc />
    protected override string Name => "csv";
}

/// <inheritdoc cref="ICsvProcessor"/>
public class CsvProcessorDescriptor<T> : ProcessorDescriptorBase<CsvProcessorDescriptor<T>, ICsvProcessor>, ICsvProcessor
    where T : class
{
    protected override string Name => "csv";
    Field ICsvProcessor.Field { get; set; }
    Fields ICsvProcessor.TargetFields { get; set; }
    bool? ICsvProcessor.IgnoreMissing { get; set; }
    string ICsvProcessor.Quote { get; set; }
    string ICsvProcessor.Separator { get; set; }
    bool? ICsvProcessor.Trim { get; set; }
    object ICsvProcessor.EmptyValue { get; set; }

    /// <inheritdoc cref="ICsvProcessor.Field" />
    public CsvProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="ICsvProcessor.Field" />
    public CsvProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.Field = v);

    /// <inheritdoc cref="ICsvProcessor.TargetFields" />
    public CsvProcessorDescriptor<T> TargetFields(Func<FieldsDescriptor<T>, IPromise<Fields>> targetFields) =>
        Assign(targetFields, (a, v) => a.TargetFields = v?.Invoke(new FieldsDescriptor<T>())?.Value);

    /// <inheritdoc cref="ICsvProcessor.TargetFields" />
    public CsvProcessorDescriptor<T> TargetFields(Fields targetFields) => Assign(targetFields, (a, v) => a.TargetFields = v);

    /// <inheritdoc cref="ICsvProcessor.IgnoreMissing" />
    public CsvProcessorDescriptor<T> IgnoreMissing(bool? ignoreMissing = true) => Assign(ignoreMissing, (a, v) => a.IgnoreMissing = v);

    /// <inheritdoc cref="ICsvProcessor.Trim" />
    public CsvProcessorDescriptor<T> Trim(bool? trim = true) => Assign(trim, (a, v) => a.Trim = v);

    /// <inheritdoc cref="ICsvProcessor.Quote" />
    public CsvProcessorDescriptor<T> Quote(string quote) => Assign(quote, (a, v) => a.Quote = v);

    /// <inheritdoc cref="ICsvProcessor.Separator" />
    public CsvProcessorDescriptor<T> Separator(string separator) => Assign(separator, (a, v) => a.Separator = v);

    /// <inheritdoc cref="ICsvProcessor.EmptyValue" />
    public CsvProcessorDescriptor<T> EmptyValue(object value) => Assign(value, (a, v) => a.EmptyValue = v);
}
