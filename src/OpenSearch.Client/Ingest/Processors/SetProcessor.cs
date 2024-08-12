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
/// Sets one field and associates it with the specified value.
/// If the field already exists, its value will be replaced with the provided one.
/// </summary>
[InterfaceDataContract]
public interface ISetProcessor : IProcessor
{
    /// <summary>
    /// The field to insert, upsert, or update. Supports template snippets.
    /// </summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }

    /// <summary>
    /// The value to be set for the field. Supports template snippets.
    /// </summary>
    [DataMember(Name = "value")]
    [JsonFormatter(typeof(SourceWriteFormatter<>))]
    object Value { get; set; }

    /// <summary>
    /// If processor will update fields with pre-existing non-null-valued field.
    /// When set to false, such fields will not be touched.
    /// Default is <c>true</c>
    /// </summary>
    [DataMember(Name = "override")]
    bool? Override { get; set; }

    /// <summary>
    /// If <c>true</c> and value is a template snippet that evaluates to null or the
    /// empty string, the processor quietly exits without modifying the document.
    /// Defaults to <c>false</c>.
    /// </summary>
    [DataMember(Name = "ignore_empty_value")]
    bool? IgnoreEmptyValue { get; set; }
}

/// <inheritdoc cref="ISetProcessor" />
public class SetProcessor : ProcessorBase, ISetProcessor
{
    /// <inheritdoc />
    public Field Field { get; set; }
    /// <inheritdoc />
    public object Value { get; set; }
    /// <inheritdoc />
    public bool? Override { get; set; }
    /// <inheritdoc />
    public bool? IgnoreEmptyValue { get; set; }
    protected override string Name => "set";
}

/// <inheritdoc cref="ISetProcessor" />
public class SetProcessorDescriptor<T> : ProcessorDescriptorBase<SetProcessorDescriptor<T>, ISetProcessor>, ISetProcessor
    where T : class
{
    protected override string Name => "set";
    Field ISetProcessor.Field { get; set; }
    object ISetProcessor.Value { get; set; }
    bool? ISetProcessor.Override { get; set; }
    bool? ISetProcessor.IgnoreEmptyValue { get; set; }

    /// <inheritdoc cref="ISetProcessor.Field"/>
    public SetProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="ISetProcessor.Field"/>
    public SetProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.Field = v);

    /// <inheritdoc cref="ISetProcessor.Value"/>
    public SetProcessorDescriptor<T> Value<TValue>(TValue value) => Assign(value, (a, v) => a.Value = v);

    /// <inheritdoc cref="ISetProcessor.Override"/>
    public SetProcessorDescriptor<T> Override(bool? @override = true) => Assign(@override, (a, v) => a.Override = v);

    /// <inheritdoc cref="ISetProcessor.IgnoreEmptyValue"/>
    public SetProcessorDescriptor<T> IgnoreEmptyValue(bool? ignoreEmptyValue = true) =>
        Assign(ignoreEmptyValue, (a, v) => a.IgnoreEmptyValue = v);
}
