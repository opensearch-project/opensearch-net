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
/// Converts a human readable byte value (e.g. 1kb) to its value in bytes (e.g. 1024).
/// Supported human readable units are "b", "kb", "mb", "gb", "tb", "pb" case insensitive.
/// An error will occur if the field is not a supported format or resultant value exceeds 2^63.
/// </summary>
[InterfaceDataContract]
public interface IBytesProcessor : IProcessor
{
    /// <summary> The field to convert bytes from </summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }

    /// <summary>
    /// If <c>true</c> and <see cref="Field" /> does not exist or is null,
    /// the processor quietly exits without modifying the document. Default is <c>false</c>
    /// </summary>
    [DataMember(Name = "ignore_missing")]
    bool? IgnoreMissing { get; set; }

    /// <summary> The field to assign the converted value to, by default <see cref="Field" /> is updated in-place </summary>
    [DataMember(Name = "target_field")]
    Field TargetField { get; set; }
}

/// <inheritdoc cref="IBytesProcessor" />
public class BytesProcessor : ProcessorBase, IBytesProcessor
{
    /// <inheritdoc />
    [DataMember(Name = "field")]
    public Field Field { get; set; }

    /// <inheritdoc />
    [DataMember(Name = "ignore_missing")]
    public bool? IgnoreMissing { get; set; }

    /// <inheritdoc />
    [DataMember(Name = "target_field")]
    public Field TargetField { get; set; }

    protected override string Name => "bytes";
}

/// <inheritdoc cref="IBytesProcessor" />
public class BytesProcessorDescriptor<T>
    : ProcessorDescriptorBase<BytesProcessorDescriptor<T>, IBytesProcessor>, IBytesProcessor
    where T : class
{
    protected override string Name => "bytes";

    Field IBytesProcessor.Field { get; set; }
    bool? IBytesProcessor.IgnoreMissing { get; set; }
    Field IBytesProcessor.TargetField { get; set; }

    /// <inheritdoc cref="IBytesProcessor.Field" />
    public BytesProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="IBytesProcessor.Field" />
    public BytesProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) => Assign(objectPath, (a, v) => a.Field = v);

    /// <inheritdoc cref="IBytesProcessor.TargetField" />
    public BytesProcessorDescriptor<T> TargetField(Field field) => Assign(field, (a, v) => a.TargetField = v);

    /// <inheritdoc cref="IBytesProcessor.TargetField" />
    public BytesProcessorDescriptor<T> TargetField<TValue>(Expression<Func<T, TValue>> objectPath) => Assign(objectPath, (a, v) => a.TargetField = v);

    /// <inheritdoc cref="IBytesProcessor.IgnoreMissing" />
    public BytesProcessorDescriptor<T> IgnoreMissing(bool? ignoreMissing = true) => Assign(ignoreMissing, (a, v) => a.IgnoreMissing = v);
}
