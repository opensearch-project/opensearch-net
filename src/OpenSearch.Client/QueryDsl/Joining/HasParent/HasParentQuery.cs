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

[InterfaceDataContract]
[ReadAs(typeof(HasParentQueryDescriptor<object>))]
public interface IHasParentQuery : IQuery
{
    [DataMember(Name = "ignore_unmapped")]
    bool? IgnoreUnmapped { get; set; }

    [DataMember(Name = "inner_hits")]
    IInnerHits InnerHits { get; set; }

    [DataMember(Name = "parent_type")]
    RelationName ParentType { get; set; }

    [DataMember(Name = "query")]
    QueryContainer Query { get; set; }

    /// <summary>
    /// Determines whether the score of the matching parent document is aggregated into the child documents belonging to the matching parent
    /// document.
    /// The default is false which ignores the score from the parent document.
    /// </summary>
    [DataMember(Name = "score")]
    bool? Score { get; set; }
}

public class HasParentQuery : QueryBase, IHasParentQuery
{
    public bool? IgnoreUnmapped { get; set; }
    public IInnerHits InnerHits { get; set; }
    public RelationName ParentType { get; set; }
    public QueryContainer Query { get; set; }

    /// <summary>
    /// Determines whether the score of the matching parent document is aggregated into the child documents belonging to the matching parent
    /// document.
    /// The default is false which ignores the score from the parent document.
    /// </summary>
    public bool? Score { get; set; }

    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.HasParent = this;

    internal static bool IsConditionless(IHasParentQuery q) => q.Query == null || q.Query.IsConditionless || q.ParentType == null;
}

public class HasParentQueryDescriptor<T>
    : QueryDescriptorBase<HasParentQueryDescriptor<T>, IHasParentQuery>
        , IHasParentQuery where T : class
{
    public HasParentQueryDescriptor() => Self.ParentType = RelationName.Create<T>();

    protected override bool Conditionless => HasParentQuery.IsConditionless(this);
    bool? IHasParentQuery.IgnoreUnmapped { get; set; }
    IInnerHits IHasParentQuery.InnerHits { get; set; }
    RelationName IHasParentQuery.ParentType { get; set; }
    QueryContainer IHasParentQuery.Query { get; set; }

    /// <summary>
    /// Determines whether the score of the matching parent document is aggregated into the child documents belonging to the matching parent
    /// document.
    /// The default is false which ignores the score from the parent document.
    /// </summary>
    bool? IHasParentQuery.Score { get; set; }

    public HasParentQueryDescriptor<T> Query(Func<QueryContainerDescriptor<T>, QueryContainer> selector) =>
        Assign(selector, (a, v) => a.Query = v?.Invoke(new QueryContainerDescriptor<T>()));

    public HasParentQueryDescriptor<T> ParentType(string type) => Assign(type, (a, v) => a.ParentType = v);

    /// <summary>
    /// Determines whether the score of the matching parent document is aggregated into the child documents belonging to the matching parent
    /// document.
    /// The default is false which ignores the score from the parent document.
    /// </summary>
    public HasParentQueryDescriptor<T> Score(bool? score = true) => Assign(score, (a, v) => a.Score = v);

    public HasParentQueryDescriptor<T> InnerHits(Func<InnerHitsDescriptor<T>, IInnerHits> selector = null) =>
        Assign(selector.InvokeOrDefault(new InnerHitsDescriptor<T>()), (a, v) => a.InnerHits = v);

    public HasParentQueryDescriptor<T> IgnoreUnmapped(bool? ignoreUnmapped = true) =>
        Assign(ignoreUnmapped, (a, v) => a.IgnoreUnmapped = v);
}
