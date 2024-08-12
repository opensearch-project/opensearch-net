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

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A mapping for a property type to a document field in OpenSearch
/// </summary>
[InterfaceDataContract]
[JsonFormatter(typeof(PropertyFormatter))]
public interface IProperty : IFieldMapping
{
    /// <summary>
    /// Local property metadata that will not be stored in OpenSearch with the mappings
    /// </summary>
    [IgnoreDataMember]
    IDictionary<string, object> LocalMetadata { get; set; }

    /// <summary>
    /// Metadata attached to the field. This metadata is stored in but opaque to OpenSearch. It is
    /// only useful for multiple applications that work on the same indices to share
    /// meta information about fields such as units.
    ///<para></para>
    /// Field metadata enforces at most 5 entries, that keys have a length that
    /// is less than or equal to 20, and that values are strings whose length is less
    ///	than or equal to 50.
    /// </summary>
    [DataMember(Name = "meta")]
    IDictionary<string, string> Meta { get; set; }

    /// <summary>
    /// The name of the property
    /// </summary>
    [IgnoreDataMember]
    PropertyName Name { get; set; }

    /// <summary>
    /// The datatype of the property
    /// </summary>
    [DataMember(Name = "type")]
    string Type { get; set; }
}

/// <summary>
/// A mapping for a property from a CLR type
/// </summary>
public interface IPropertyWithClrOrigin
{
    /// <summary>
    /// The CLR property to which the mapping relates
    /// </summary>
    PropertyInfo ClrOrigin { get; set; }
}

/// <inheritdoc cref="IProperty" />
[DebuggerDisplay("{DebugDisplay}")]
public abstract class PropertyBase : IProperty, IPropertyWithClrOrigin
{
    protected PropertyBase(FieldType type) => ((IProperty)this).Type = type.GetStringValue();

    /// <inheritdoc />
    public IDictionary<string, object> LocalMetadata { get; set; }

    /// <inheritdoc />
    public IDictionary<string, string> Meta { get; set; }

    /// <inheritdoc />
    public PropertyName Name { get; set; }

    protected string DebugDisplay => $"Type: {((IProperty)this).Type ?? "<empty>"}, Name: {Name?.DebugDisplay ?? "<empty>"} ";

    public override string ToString() => DebugDisplay;

    /// <summary>
    /// Override for the property type, used for custom mappings
    /// </summary>
    protected string TypeOverride { get; set; }

    PropertyInfo IPropertyWithClrOrigin.ClrOrigin { get; set; }

    string IProperty.Type
    {
        get => TypeOverride;
        set => TypeOverride = value;
    }
}
