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
/// Sort on a field inside one or more nested objects.
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(NestedSort))]
public interface INestedSort
{
    /// <summary>
    /// A filter that the inner objects inside the nested path should match with in order for its field values to be taken into account
    /// by sorting. A common pattern is to repeat the query/filter inside the nested filter or query.
    /// By default no nested filter is active.
    /// </summary>
    [DataMember(Name = "filter")]
    QueryContainer Filter { get; set; }

    /// <summary>
    /// Same as top-level nested, but applies to another nested path within the current nested object.
    /// </summary>
    [DataMember(Name = "nested")]
    INestedSort Nested { get; set; }

    /// <summary>
    /// Defines on which nested object to sort. The actual sort field must be a direct field inside this nested object.
    /// When sorting by nested field, this field is mandatory.
    /// </summary>
    [DataMember(Name = "path")]
    Field Path { get; set; }

    /// <summary>
    /// The maximum number of children to consider per root document when picking the sort value. Defaults to unlimited.
    /// </summary>
    [DataMember(Name = "max_children")]
    int? MaxChildren { get; set; }
}

/// <inheritdoc />
public class NestedSort : INestedSort
{
    /// <inheritdoc />
    public QueryContainer Filter { get; set; }
    /// <inheritdoc />
    public INestedSort Nested { get; set; }
    /// <inheritdoc />
    public Field Path { get; set; }
    /// <inheritdoc />
    public int? MaxChildren { get; set; }
}

/// <inheritdoc cref="INestedSort"/>
public class NestedSortDescriptor<T>
    : DescriptorBase<NestedSortDescriptor<T>, INestedSort>, INestedSort where T : class
{
    QueryContainer INestedSort.Filter { get; set; }
    INestedSort INestedSort.Nested { get; set; }
    Field INestedSort.Path { get; set; }
    int? INestedSort.MaxChildren { get; set; }

    /// <inheritdoc cref="INestedSort.Path"/>
    public NestedSortDescriptor<T> Path(Field path) => Assign(path, (a, v) => a.Path = v);

    /// <inheritdoc cref="INestedSort.Path"/>
    public NestedSortDescriptor<T> Path<TValue>(Expression<Func<T, TValue>> objectPath) => Assign(objectPath, (a, v) => a.Path = v);

    /// <inheritdoc cref="INestedSort.Filter"/>
    public NestedSortDescriptor<T> Filter(Func<QueryContainerDescriptor<T>, QueryContainer> filterSelector) =>
        Assign(filterSelector, (a, v) => a.Filter = v?.Invoke(new QueryContainerDescriptor<T>()));

    /// <inheritdoc cref="INestedSort.Nested"/>
    public NestedSortDescriptor<T> Nested(Func<NestedSortDescriptor<T>, INestedSort> filterSelector) =>
        Assign(filterSelector, (a, v) => a.Nested = v?.Invoke(new NestedSortDescriptor<T>()));

    /// <inheritdoc cref="INestedSort.MaxChildren"/>
    public NestedSortDescriptor<T> MaxChildren(int? maxChildren) => Assign(maxChildren, (a, v) => a.MaxChildren = v);
}
