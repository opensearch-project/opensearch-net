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
using OpenSearch.Net;


namespace OpenSearch.Client;

/// <summary>
/// A request to Reindex API
/// </summary>
[MapsApi("reindex.json")]
public partial interface IReindexOnServerRequest
{
    /// <summary>
    /// Determine what to do in the event of version conflicts.
    /// Defaults to <see cref="OpenSearch.Net.Conflicts.Abort" />
    /// </summary>
    [DataMember(Name = "conflicts")]
    Conflicts? Conflicts { get; set; }

    /// <summary>
    /// The destination for the reindex operation.
    /// </summary>
    /// <remarks>
    /// Reindex does not attempt to set up the destination index. It does not copy the settings
    /// of the source index. You should set up the destination index beforehand
    /// </remarks>
    [DataMember(Name = "dest")]
    IReindexDestination Destination { get; set; }

    /// <summary>
    /// A script that can modify documents from source, including metadata, before reindexing
    /// </summary>
    [DataMember(Name = "script")]
    IScript Script { get; set; }

    /// <summary>
    /// Limit the number of processed documents
    /// </summary>
    [DataMember(Name = "max_docs")]
    long? MaximumDocuments { get; set; }

    /// <summary>
    /// The source for the reindex operation
    /// </summary>
    [DataMember(Name = "source")]
    IReindexSource Source { get; set; }
}

/// <inheritdoc cref="IReindexOnServerRequest" />
public partial class ReindexOnServerRequest
{
    /// <inheritdoc />
    public Conflicts? Conflicts { get; set; }

    /// <inheritdoc />
    public IReindexDestination Destination { get; set; }

    /// <inheritdoc />
    public IScript Script { get; set; }

    /// <inheritdoc />
    public long? MaximumDocuments { get; set; }

    /// <inheritdoc />
    public IReindexSource Source { get; set; }
}

public partial class ReindexOnServerDescriptor
{
    Conflicts? IReindexOnServerRequest.Conflicts { get; set; }
    IReindexDestination IReindexOnServerRequest.Destination { get; set; }
    IScript IReindexOnServerRequest.Script { get; set; }
    IReindexSource IReindexOnServerRequest.Source { get; set; }
    long? IReindexOnServerRequest.MaximumDocuments { get; set; }

    /// <inheritdoc cref="IReindexOnServerRequest.Source" />
    public ReindexOnServerDescriptor Source(Func<ReindexSourceDescriptor, IReindexSource> selector = null) =>
        Assign(selector.InvokeOrDefault(new ReindexSourceDescriptor()), (a, v) => a.Source = v);

    /// <inheritdoc cref="IReindexOnServerRequest.Destination" />
    public ReindexOnServerDescriptor Destination(Func<ReindexDestinationDescriptor, IReindexDestination> selector) =>
        Assign(selector, (a, v) => a.Destination = v?.Invoke(new ReindexDestinationDescriptor()));

    /// <inheritdoc cref="IReindexOnServerRequest.Script" />
    public ReindexOnServerDescriptor Script(string script) => Assign((InlineScript)script, (a, v) => a.Script = v);

    /// <inheritdoc cref="IReindexOnServerRequest.Script" />
    public ReindexOnServerDescriptor Script(Func<ScriptDescriptor, IScript> scriptSelector) =>
        Assign(scriptSelector, (a, v) => a.Script = v?.Invoke(new ScriptDescriptor()));

    /// <inheritdoc cref="IReindexOnServerRequest.Conflicts" />
    public ReindexOnServerDescriptor Conflicts(Conflicts? conflicts) => Assign(conflicts, (a, v) => a.Conflicts = v);

    /// <inheritdoc cref="IReindexOnServerRequest.MaximumDocuments"/>
    public ReindexOnServerDescriptor MaximumDocuments(long? maximumDocuments) =>
        Assign(maximumDocuments, (a, v) => a.MaximumDocuments = v);
}
