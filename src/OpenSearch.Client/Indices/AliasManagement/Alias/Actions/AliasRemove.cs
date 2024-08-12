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
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
public interface IAliasRemoveAction : IAliasAction
{
    [DataMember(Name = "remove")]
    AliasRemoveOperation Remove { get; set; }
}

public class AliasRemoveAction : IAliasRemoveAction
{
    public AliasRemoveOperation Remove { get; set; }
}

public class AliasRemoveDescriptor : DescriptorBase<AliasRemoveDescriptor, IAliasRemoveAction>, IAliasRemoveAction
{
    public AliasRemoveDescriptor() => Self.Remove = new AliasRemoveOperation();

    AliasRemoveOperation IAliasRemoveAction.Remove { get; set; }

    /// <inheritdoc cref="AliasRemoveOperation.Index"/>
    public AliasRemoveDescriptor Index(string index)
    {
        Self.Remove.Index = index;
        return this;
    }

    /// <inheritdoc cref="AliasRemoveOperation.Index"/>
    public AliasRemoveDescriptor Index(Type index)
    {
        Self.Remove.Index = index;
        return this;
    }

    /// <inheritdoc cref="AliasRemoveOperation.Index"/>
    public AliasRemoveDescriptor Index<T>() where T : class
    {
        Self.Remove.Index = typeof(T);
        return this;
    }

    /// <inheritdoc cref="AliasRemoveOperation.Indices"/>
    public AliasRemoveDescriptor Indices(Indices indices)
    {
        Self.Remove.Indices = indices;
        return this;
    }

    /// <inheritdoc cref="AliasRemoveOperation.Alias"/>
    public AliasRemoveDescriptor Alias(string alias)
    {
        Self.Remove.Alias = alias;
        return this;
    }

    /// <inheritdoc cref="AliasRemoveOperation.Aliases"/>
    public AliasRemoveDescriptor Aliases(IEnumerable<string> aliases)
    {
        Self.Remove.Aliases = aliases;
        return this;
    }

    /// <inheritdoc cref="AliasRemoveOperation.Aliases"/>
    public AliasRemoveDescriptor Aliases(params string[] aliases)
    {
        Self.Remove.Aliases = aliases;
        return this;
    }
}
