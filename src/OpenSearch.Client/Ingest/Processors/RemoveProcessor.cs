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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// Removes existing fields. If one field doesn't exist, an exception will be thrown.
/// </summary>
[InterfaceDataContract]
public interface IRemoveProcessor : IProcessor
{
    /// <summary>
    /// fields to be removed. Supports template snippets.
    /// </summary>
    [DataMember(Name = "field")]
    Fields Field { get; set; }

    /// <summary>
    /// If <c>true</c> and <see cref="Client.Field" /> does not exist or is null,
    /// the processor quietly exits without modifying the document. Default is <c>false</c>
    /// </summary>
    [DataMember(Name = "ignore_missing")]
    bool? IgnoreMissing { get; set; }
}

/// <inheritdoc cref="IRemoveProcessor" />
public class RemoveProcessor : ProcessorBase, IRemoveProcessor
{
    /// <inheritdoc />
    public Fields Field { get; set; }

    /// <inheritdoc />
    public bool? IgnoreMissing { get; set; }

    protected override string Name => "remove";
}

/// <inheritdoc cref="IRemoveProcessor" />
public class RemoveProcessorDescriptor<T>
    : ProcessorDescriptorBase<RemoveProcessorDescriptor<T>, IRemoveProcessor>, IRemoveProcessor
    where T : class
{
    protected override string Name => "remove";

    Fields IRemoveProcessor.Field { get; set; }
    bool? IRemoveProcessor.IgnoreMissing { get; set; }

    /// <inheritdoc cref="IRemoveProcessor.Field" />
    public RemoveProcessorDescriptor<T> Field(Fields fields) => Assign(fields, (a, v) => a.Field = v);

    /// <inheritdoc cref="IRemoveProcessor.Field" />
    public RemoveProcessorDescriptor<T> Field(Func<FieldsDescriptor<T>, IPromise<Fields>> selector) =>
        Assign(selector, (a, v) => a.Field = v?.Invoke(new FieldsDescriptor<T>())?.Value);

    /// <inheritdoc cref="IRemoveProcessor.IgnoreMissing" />
    public RemoveProcessorDescriptor<T> IgnoreMissing(bool? ignoreMissing = true) => Assign(ignoreMissing, (a, v) => a.IgnoreMissing = v);
}
