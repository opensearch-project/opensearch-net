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

using System.Diagnostics;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// Core properties of a mapping for a property type to a document field in OpenSearch
/// </summary>
[InterfaceDataContract]
public interface ICoreProperty : IProperty
{
    /// <summary>
    /// Copies the value of this field into another field, which can be queried as a single field.
    /// Allows for the creation of custom _all fields
    /// </summary>
    [DataMember(Name = "copy_to")]
    Fields CopyTo { get; set; }

    /// <summary>
    /// Configures multi-fields for this field. Allows one field to be indexed in different
    /// ways to serve different search and analytics purposes
    /// </summary>
    [DataMember(Name = "fields")]
    IProperties Fields { get; set; }

    /// <summary>
    /// Which relevancy scoring algorithm or similarity should be used.
    /// Defaults to BM25
    /// </summary>
    [DataMember(Name = "similarity")]
    string Similarity { get; set; }

    /// <summary>
    /// Whether the field value should be stored and retrievable separately from the _source field
    /// Default is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// Not valid on <see cref="ObjectProperty" />
    /// </remarks>
    [DataMember(Name = "store")]
    bool? Store { get; set; }
}

/// <inheritdoc cref="ICoreProperty" />
[DebuggerDisplay("{DebugDisplay}")]
public abstract class CorePropertyBase : PropertyBase, ICoreProperty
{
    protected CorePropertyBase(FieldType type) : base(type) { }

    /// <inheritdoc />
    public Fields CopyTo { get; set; }

    /// <inheritdoc />
    public IProperties Fields { get; set; }

    /// <inheritdoc />
    public string Similarity { get; set; }

    /// <inheritdoc />
    public bool? Store { get; set; }
}
