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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// The ingest attachment plugin lets OpenSearch extract file attachments in common formats
/// (such as PPT, XLS, and PDF) by using the Apache text extraction library Tika.
/// You can use the ingest attachment plugin as a replacement for the mapper attachment plugin.
/// </summary>
/// <remarks>
/// Requires the Ingest Attachment Processor Plugin to be installed on the cluster.
/// </remarks>
[InterfaceDataContract]
public interface IAttachmentProcessor : IProcessor
{
    /// <summary> The field to get the base64 encoded field from.</summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }


    /// <summary> If `true` and `field` does not exist, the processor quietly exits without modifying the document.</summary>
    [DataMember(Name = "ignore_missing")]
    bool? IgnoreMissing { get; set; }

    /// <summary>
    /// The number of chars being used for extraction to prevent huge fields. Use -1 for no limit.
    /// Defaults to 100000.
    /// </summary>
    [DataMember(Name = "indexed_chars")]
    long? IndexedCharacters { get; set; }

    /// <summary> Field name from which you can overwrite the number of chars being used for extraction.</summary>
    [DataMember(Name = "indexed_chars_field")]
    Field IndexedCharactersField { get; set; }

    /// <summary>
    /// Properties to select to be stored. Can be content, title, name, author,
    /// keywords, date, content_type, content_length, language. Defaults to all.
    /// </summary>
    [DataMember(Name = "properties")]
    IEnumerable<string> Properties { get; set; }

    /// <summary> The field that will hold the attachment information.</summary>
    [DataMember(Name = "target_field")]
    Field TargetField { get; set; }

    /// <summary> The field containing the name of the resource to decode.
    /// If specified, the processor passes this resource name to the underlying
    /// Tika library to enable 'Resource Name Based Detection'.</summary>
    [DataMember(Name = "resource_name")]
    Field ResourceName { get; set; }
}

/// <inheritdoc cref="IAttachmentProcessor" />
public class AttachmentProcessor : ProcessorBase, IAttachmentProcessor
{
    /// <inheritdoc cref="IAttachmentProcessor.Field" />
    public Field Field { get; set; }

    /// <inheritdoc />
    /// <inheritdoc cref="IAttachmentProcessor.IgnoreMissing" />
    public bool? IgnoreMissing { get; set; }

    /// <inheritdoc cref="IAttachmentProcessor.IndexedCharacters" />
    public long? IndexedCharacters { get; set; }

    /// <inheritdoc cref="IAttachmentProcessor.IndexedCharactersField" />
    public Field IndexedCharactersField { get; set; }

    /// <inheritdoc cref="IAttachmentProcessor.Properties" />
    public IEnumerable<string> Properties { get; set; }

    /// <inheritdoc cref="IAttachmentProcessor.TargetField" />
    public Field TargetField { get; set; }

    /// <inheritdoc cref="IAttachmentProcessor.ResourceName" />
    public Field ResourceName { get; set; }

    protected override string Name => "attachment";
}

/// <inheritdoc cref="IAttachmentProcessor" />
public class AttachmentProcessorDescriptor<T>
    : ProcessorDescriptorBase<AttachmentProcessorDescriptor<T>, IAttachmentProcessor>, IAttachmentProcessor
    where T : class
{
    protected override string Name => "attachment";

    Field IAttachmentProcessor.Field { get; set; }
    bool? IAttachmentProcessor.IgnoreMissing { get; set; }
    long? IAttachmentProcessor.IndexedCharacters { get; set; }
    Field IAttachmentProcessor.IndexedCharactersField { get; set; }
    IEnumerable<string> IAttachmentProcessor.Properties { get; set; }
    Field IAttachmentProcessor.TargetField { get; set; }
    Field IAttachmentProcessor.ResourceName { get; set; }

    /// <inheritdoc cref="IAttachmentProcessor.Field" />
    public AttachmentProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="IAttachmentProcessor.Field" />
    public AttachmentProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) => Assign(objectPath, (a, v) => a.Field = v);

    /// <inheritdoc cref="IAttachmentProcessor.TargetField" />
    public AttachmentProcessorDescriptor<T> TargetField(Field field) => Assign(field, (a, v) => a.TargetField = v);

    /// <inheritdoc cref="IAttachmentProcessor.TargetField" />
    public AttachmentProcessorDescriptor<T> TargetField<TValue>(Expression<Func<T, TValue>> objectPath) => Assign(objectPath, (a, v) => a.TargetField = v);

    /// <inheritdoc cref="IAttachmentProcessor.IndexedCharacters" />
    public AttachmentProcessorDescriptor<T> IndexedCharacters(long? indexedCharacters) => Assign(indexedCharacters, (a, v) => a.IndexedCharacters = v);

    /// <inheritdoc cref="IAttachmentProcessor.IndexedCharactersField" />
    public AttachmentProcessorDescriptor<T> IndexedCharactersField(Field field) => Assign(field, (a, v) => a.IndexedCharactersField = v);

    /// <inheritdoc cref="IAttachmentProcessor.IndexedCharactersField" />
    public AttachmentProcessorDescriptor<T> IndexedCharactersField<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.IndexedCharactersField = v);

    /// <inheritdoc cref="IAttachmentProcessor.IgnoreMissing" />
    public AttachmentProcessorDescriptor<T> IgnoreMissing(bool? ignoreMissing = true) => Assign(ignoreMissing, (a, v) => a.IgnoreMissing = v);

    /// <inheritdoc cref="IAttachmentProcessor.Properties" />
    public AttachmentProcessorDescriptor<T> Properties(IEnumerable<string> properties) => Assign(properties, (a, v) => a.Properties = v);

    /// <inheritdoc cref="IAttachmentProcessor.Properties" />
    public AttachmentProcessorDescriptor<T> Properties(params string[] properties) => Assign(properties, (a, v) => a.Properties = v);

    /// <inheritdoc cref="IAttachmentProcessor.ResourceName" />
    public AttachmentProcessorDescriptor<T> ResourceName(Field field) => Assign(field, (a, v) => a.ResourceName = v);

    /// <inheritdoc cref="IAttachmentProcessor.TargetField" />
    public AttachmentProcessorDescriptor<T> ResourceName<TValue>(Expression<Func<T, TValue>> objectPath) => Assign(objectPath, (a, v) => a.ResourceName = v);
}
