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
/// Similar to the Grok Processor, dissect also extracts structured fields out of a single text field
/// within a document. However unlike the Grok Processor, dissect does not use Regular Expressions.
/// This allows dissect’s syntax to be simple and, for some cases faster, than the Grok Processor.
/// </summary>
[InterfaceDataContract]
public interface IDissectProcessor : IProcessor
{
    /// <summary> The field to dissect </summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }

    /// <summary> The pattern to apply to the field </summary>
    [DataMember(Name = "pattern")]
    string Pattern { get; set; }

    /// <summary>
    /// If <c>true</c> and field does not exist or is null, the processor quietly exits without modifying the document
    /// </summary>
    [DataMember(Name = "ignore_missing")]
    bool? IgnoreMissing { get; set; }

    /// <summary> The character(s) that separate the appended fields. </summary>
    [DataMember(Name = "append_separator")]
    string AppendSeparator { get; set; }
}

/// <inheritdoc cref="IDissectProcessor"/>
public class DissectProcessor : ProcessorBase, IDissectProcessor
{
    protected override string Name => "dissect";

    /// <inheritdoc cref="IDissectProcessor.Field"/>
    public Field Field { get; set; }

    /// <inheritdoc cref="IDissectProcessor.Pattern"/>
    public string Pattern { get; set; }

    /// <inheritdoc cref="IDissectProcessor.IgnoreMissing"/>
    public bool? IgnoreMissing { get; set; }

    /// <inheritdoc cref="IDissectProcessor.AppendSeparator"/>
    public string AppendSeparator { get; set; }
}

/// <inheritdoc cref="IDissectProcessor"/>
public class DissectProcessorDescriptor<T>
    : ProcessorDescriptorBase<DissectProcessorDescriptor<T>, IDissectProcessor>, IDissectProcessor
    where T : class
{
    protected override string Name => "dissect";

    Field IDissectProcessor.Field { get; set; }
    string IDissectProcessor.Pattern { get; set; }
    bool? IDissectProcessor.IgnoreMissing { get; set; }
    string IDissectProcessor.AppendSeparator { get; set; }

    /// <inheritdoc cref="IDissectProcessor.Field"/>
    public DissectProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="IDissectProcessor.Field"/>
    public DissectProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.Field = v);

    /// <inheritdoc cref="IDissectProcessor.Pattern"/>
    public DissectProcessorDescriptor<T> Pattern(string pattern) =>
        Assign(pattern, (a, v) => a.Pattern = v);

    /// <inheritdoc cref="IDissectProcessor.IgnoreMissing"/>
    public DissectProcessorDescriptor<T> IgnoreMissing(bool? traceMatch = true) =>
        Assign(traceMatch, (a, v) => a.IgnoreMissing = v);

    /// <inheritdoc cref="IDissectProcessor.AppendSeparator"/>
    public DissectProcessorDescriptor<T> AppendSeparator(string appendSeparator) =>
        Assign(appendSeparator, (a, v) => a.AppendSeparator = v);

}
