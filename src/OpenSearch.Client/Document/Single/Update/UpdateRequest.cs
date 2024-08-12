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

[MapsApi("update.json")]
public partial interface IUpdateRequest<TDocument, TPartialDocument>
    where TDocument : class
    where TPartialDocument : class
{
    [DataMember(Name = "detect_noop")]
    bool? DetectNoop { get; set; }

    [DataMember(Name = "doc")]
    [JsonFormatter(typeof(SourceFormatter<>))]
    TPartialDocument Doc { get; set; }

    [DataMember(Name = "doc_as_upsert")]
    bool? DocAsUpsert { get; set; }

    [DataMember(Name = "script")]
    IScript Script { get; set; }

    /// <summary>
    /// If you would like your script to run regardless of whether the document exists or not — i.e. the script handles
    /// initializing the document instead of the upsert element — then set scripted_upsert to true
    /// </summary>
    [DataMember(Name = "scripted_upsert")]
    bool? ScriptedUpsert { get; set; }

    [DataMember(Name = "_source")]
    Union<bool, ISourceFilter> Source { get; set; }

    [DataMember(Name = "upsert")]
    [JsonFormatter(typeof(SourceFormatter<>))]
    TDocument Upsert { get; set; }
}

public partial class UpdateRequest<TDocument, TPartialDocument>
    where TDocument : class
    where TPartialDocument : class
{
    /// <inheritdoc />
    public bool? DetectNoop { get; set; }

    /// <inheritdoc />
    public TPartialDocument Doc { get; set; }

    /// <inheritdoc />
    public bool? DocAsUpsert { get; set; }

    /// <inheritdoc />
    public IScript Script { get; set; }

    /// <inheritdoc />
    public bool? ScriptedUpsert { get; set; }

    /// <inheritdoc />
    public Union<bool, ISourceFilter> Source { get; set; }

    /// <inheritdoc />
    public TDocument Upsert { get; set; }
}

public partial class UpdateDescriptor<TDocument, TPartialDocument>
    where TDocument : class
    where TPartialDocument : class
{
    bool? IUpdateRequest<TDocument, TPartialDocument>.DetectNoop { get; set; }

    TPartialDocument IUpdateRequest<TDocument, TPartialDocument>.Doc { get; set; }

    bool? IUpdateRequest<TDocument, TPartialDocument>.DocAsUpsert { get; set; }

    IScript IUpdateRequest<TDocument, TPartialDocument>.Script { get; set; }

    bool? IUpdateRequest<TDocument, TPartialDocument>.ScriptedUpsert { get; set; }

    Union<bool, ISourceFilter> IUpdateRequest<TDocument, TPartialDocument>.Source { get; set; }

    TDocument IUpdateRequest<TDocument, TPartialDocument>.Upsert { get; set; }

    /// <summary>
    /// The full document to be created if an existing document does not exist for a partial merge.
    /// </summary>
    public UpdateDescriptor<TDocument, TPartialDocument> Upsert(TDocument upsertObject) => Assign(upsertObject, (a, v) => a.Upsert = v);

    /// <summary>
    /// The partial update document to be merged on to the existing object.
    /// </summary>
    public UpdateDescriptor<TDocument, TPartialDocument> Doc(TPartialDocument @object) => Assign(@object, (a, v) => a.Doc = v);

    public UpdateDescriptor<TDocument, TPartialDocument> DocAsUpsert(bool? docAsUpsert = true) => Assign(docAsUpsert, (a, v) => a.DocAsUpsert = v);

    public UpdateDescriptor<TDocument, TPartialDocument> DetectNoop(bool? detectNoop = true) => Assign(detectNoop, (a, v) => a.DetectNoop = v);

    public UpdateDescriptor<TDocument, TPartialDocument> ScriptedUpsert(bool? scriptedUpsert = true) =>
        Assign(scriptedUpsert, (a, v) => a.ScriptedUpsert = v);

    public UpdateDescriptor<TDocument, TPartialDocument> Script(Func<ScriptDescriptor, IScript> scriptSelector) =>
        Assign(scriptSelector, (a, v) => a.Script = v?.Invoke(new ScriptDescriptor()));

    public UpdateDescriptor<TDocument, TPartialDocument> Source(bool? enabled = true) => Assign(enabled, (a, v) => a.Source = v);

    public UpdateDescriptor<TDocument, TPartialDocument> Source(Func<SourceFilterDescriptor<TDocument>, ISourceFilter> selector) =>
        Assign(selector, (a, v) => a.Source = new Union<bool, ISourceFilter>(v?.Invoke(new SourceFilterDescriptor<TDocument>())));
}
