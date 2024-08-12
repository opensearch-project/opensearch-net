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

namespace OpenSearch.Client;

/// <summary>
/// An alias to one or more indices
/// </summary>
[ReadAs(typeof(Alias))]
public interface IAlias
{
    /// <summary>
    /// Provides an easy way to create different "views" of the same index. A filter can be defined using Query DSL and is
    /// applied to all Search, Count, Delete By Query and More Like This operations with this alias.
    /// </summary>
    [DataMember(Name = "filter")]
    QueryContainer Filter { get; set; }

    /// <summary>
    /// Associates routing values with aliases for index operations. This feature can be used together
    /// with filtering aliases in order to avoid unnecessary shard operations.
    /// </summary>
    [DataMember(Name = "index_routing")]
    Routing IndexRouting { get; set; }

    /// <summary>
    /// If an alias points to multiple indices, OpenSearch will reject the write operations
    /// unless one is explicitly marked as the write alias using this property.
    /// </summary>
    [DataMember(Name = "is_write_index")]
    bool? IsWriteIndex { get; set; }

    /// <summary>
    /// If true, the alias will be excluded from wildcard expressions by default, unless overriden in the request using
    /// the expand_wildcards parameter, similar to hidden indices.
    /// This property must be set to the same value on all indices that share an alias. Defaults to false.
    /// </summary>
    [DataMember(Name = "is_hidden")]
    bool? IsHidden { get; set; }

    /// <summary>
    /// Associates routing values with aliases for both index and search operations. This feature can be used together
    /// with filtering aliases in order to avoid unnecessary shard operations.
    /// </summary>
    [DataMember(Name = "routing")]
    Routing Routing { get; set; }

    /// <summary>
    /// Associates routing values with aliases for search operations. This feature can be used together
    /// with filtering aliases in order to avoid unnecessary shard operations.
    /// </summary>
    [DataMember(Name = "search_routing")]
    Routing SearchRouting { get; set; }
}

/// <inheritdoc />
public class Alias : IAlias
{
    /// <inheritdoc />
    public QueryContainer Filter { get; set; }
    /// <inheritdoc />
    public Routing IndexRouting { get; set; }
    /// <inheritdoc />
    public bool? IsWriteIndex { get; set; }
    /// <inheritdoc />
    public bool? IsHidden { get; set; }

    /// <inheritdoc />
    public Routing Routing { get; set; }
    /// <inheritdoc />
    public Routing SearchRouting { get; set; }
}

/// <inheritdoc cref="IAlias" />
public class AliasDescriptor : DescriptorBase<AliasDescriptor, IAlias>, IAlias
{
    QueryContainer IAlias.Filter { get; set; }
    Routing IAlias.IndexRouting { get; set; }
    bool? IAlias.IsWriteIndex { get; set; }
    Routing IAlias.Routing { get; set; }
    Routing IAlias.SearchRouting { get; set; }
    bool? IAlias.IsHidden { get; set; }

    /// <inheritdoc cref="IAlias.Filter" />
    public AliasDescriptor Filter<T>(Func<QueryContainerDescriptor<T>, QueryContainer> filterSelector) where T : class =>
        Assign(filterSelector, (a, v) => a.Filter = v?.Invoke(new QueryContainerDescriptor<T>()));

    /// <inheritdoc cref="IAlias.IndexRouting" />
    public AliasDescriptor IndexRouting(Routing indexRouting) => Assign(indexRouting, (a, v) => a.IndexRouting = v);

    /// <inheritdoc cref="IAlias.IsWriteIndex" />
    public AliasDescriptor IsWriteIndex(bool? isWriteIndex = true) => Assign(isWriteIndex, (a, v) => a.IsWriteIndex = v);

    /// <inheritdoc cref="IAlias.IsHidden" />
    public AliasDescriptor IsHidden(bool? isHidden = true) => Assign(isHidden, (a, v) => a.IsHidden = v);

    /// <inheritdoc cref="IAlias.Routing" />
    public AliasDescriptor Routing(Routing routing) => Assign(routing, (a, v) => a.Routing = v);

    /// <inheritdoc cref="IAlias.SearchRouting" />
    public AliasDescriptor SearchRouting(Routing searchRouting) => Assign(searchRouting, (a, v) => a.SearchRouting = v);
}
