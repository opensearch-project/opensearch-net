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

[InterfaceDataContract]
public interface IUriPartsProcessor : IProcessor
{
    /// <summary>
    /// The field to decode
    /// </summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }

    /// <summary>
    /// If <c>true</c> the processor copies the unparsed URI to <c>&lt;target_field&gt;.original</c>.
    /// </summary>
    [DataMember(Name = "keep_original")]
    bool? KeepOriginal { get; set; }

    /// <summary>
    /// The field to assign the converted value to, by default <see cref="Field" /> is updated in-place
    /// </summary>
    [DataMember(Name = "target_field")]
    Field TargetField { get; set; }

    /// <summary>
    /// If <c>true</c> the processor removes the <see cref="Field" /> after parsing the URI string.
    /// If parsing fails, the processor does not remove the <see cref="Field" />.
    /// </summary>
    [DataMember(Name = "remove_if_successful")]
    bool? RemoveIfSuccessful { get; set; }
}

/// <inheritdoc cref="IUriPartsProcessor" />
public class UriPartsProcessor : ProcessorBase, IUriPartsProcessor
{
    /// <inheritdoc />
    [DataMember(Name = "field")]
    public Field Field { get; set; }

    /// <inheritdoc />
    [DataMember(Name = "keep_original")]
    public bool? KeepOriginal { get; set; }

    /// <inheritdoc />
    [DataMember(Name = "target_field")]
    public Field TargetField { get; set; }

    /// <inheritdoc />
    [DataMember(Name = "remove_if_successful")]
    public bool? RemoveIfSuccessful { get; set; }

    protected override string Name => "uri_parts";
}

/// <inheritdoc cref="IUriPartsProcessor" />
public class UriPartsProcessorDescriptor<T>
    : ProcessorDescriptorBase<UriPartsProcessorDescriptor<T>, IUriPartsProcessor>, IUriPartsProcessor
    where T : class
{
    protected override string Name => "uri_parts";

    Field IUriPartsProcessor.Field { get; set; }
    bool? IUriPartsProcessor.KeepOriginal { get; set; }
    Field IUriPartsProcessor.TargetField { get; set; }
    bool? IUriPartsProcessor.RemoveIfSuccessful { get; set; }

    /// <inheritdoc cref="IUriPartsProcessor.Field" />
    public UriPartsProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="IUriPartsProcessor.Field" />
    public UriPartsProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.Field = v);

    /// <inheritdoc cref="IUriPartsProcessor.TargetField" />
    public UriPartsProcessorDescriptor<T> TargetField(Field field) => Assign(field, (a, v) => a.TargetField = v);

    /// <inheritdoc cref="IUriPartsProcessor.TargetField" />
    public UriPartsProcessorDescriptor<T> TargetField<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.TargetField = v);

    /// <inheritdoc cref="IUriPartsProcessor.KeepOriginal" />
    public UriPartsProcessorDescriptor<T> KeepOriginal(bool? keepOriginal = true) =>
        Assign(keepOriginal, (a, v) => a.KeepOriginal = v);

    /// <inheritdoc cref="IUriPartsProcessor.RemoveIfSuccessful" />
    public UriPartsProcessorDescriptor<T> RemoveIfSuccessful(bool? removeIfSuccessful = true) =>
        Assign(removeIfSuccessful, (a, v) => a.RemoveIfSuccessful = v);
}
