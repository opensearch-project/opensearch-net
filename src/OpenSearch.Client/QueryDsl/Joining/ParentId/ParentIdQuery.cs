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

using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// The parent_id query can be used to find child documents which belong to a particular parent.
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(ParentIdQuery))]
public interface IParentIdQuery : IQuery
{
    /// <summary>
    /// The id of the parent document to get children for.
    /// </summary>
    [DataMember(Name = "id")]
    Id Id { get; set; }

    /// <summary>
    /// When set to true this will ignore an unmapped type and will not match any documents for
    /// this query. This can be useful when querying multiple indexes which might have different mappings.
    /// </summary>
    [DataMember(Name = "ignore_unmapped")]
    bool? IgnoreUnmapped { get; set; }

    /// <summary>
    /// The child type. This must be a type with _parent field.
    /// </summary>
    [DataMember(Name = "type")]
    RelationName Type { get; set; }
}

public class ParentIdQuery : QueryBase, IParentIdQuery
{
    public Id Id { get; set; }

    public bool? IgnoreUnmapped { get; set; }

    public RelationName Type { get; set; }
    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.ParentId = this;

    internal static bool IsConditionless(IParentIdQuery q) => q.Type.IsConditionless() || q.Id.IsConditionless();
}

[DataContract]
public class ParentIdQueryDescriptor<T>
    : QueryDescriptorBase<ParentIdQueryDescriptor<T>, IParentIdQuery>
        , IParentIdQuery where T : class
{
    protected override bool Conditionless => ParentIdQuery.IsConditionless(this);
    Id IParentIdQuery.Id { get; set; }
    bool? IParentIdQuery.IgnoreUnmapped { get; set; }

    RelationName IParentIdQuery.Type { get; set; }

    public ParentIdQueryDescriptor<T> Id(Id id) => Assign(id, (a, v) => a.Id = v);

    public ParentIdQueryDescriptor<T> Type(RelationName type) => Assign(type, (a, v) => a.Type = v);

    public ParentIdQueryDescriptor<T> Type<TChild>() => Assign(typeof(TChild), (a, v) => a.Type = v);

    public ParentIdQueryDescriptor<T> IgnoreUnmapped(bool? ignoreUnmapped = true) => Assign(ignoreUnmapped, (a, v) => a.IgnoreUnmapped = v);
}
