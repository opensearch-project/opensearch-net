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
[ReadAs(typeof(SpanMultiTermQuery))]
public interface ISpanMultiTermQuery : ISpanSubQuery
{
    [DataMember(Name = "match")]
    QueryContainer Match { get; set; }
}

public class SpanMultiTermQuery : QueryBase, ISpanMultiTermQuery
{
    public QueryContainer Match { get; set; }
    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.SpanMultiTerm = this;

    internal static bool IsConditionless(ISpanMultiTermQuery q) => q.Match == null || q.Match.IsConditionless;
}

public class SpanMultiTermQueryDescriptor<T>
    : QueryDescriptorBase<SpanMultiTermQueryDescriptor<T>, ISpanMultiTermQuery>
        , ISpanMultiTermQuery
    where T : class
{
    protected override bool Conditionless => SpanMultiTermQuery.IsConditionless(this);
    QueryContainer ISpanMultiTermQuery.Match { get; set; }

    public SpanMultiTermQueryDescriptor<T> Match(Func<QueryContainerDescriptor<T>, QueryContainer> selector) =>
        Assign(selector, (a, v) => a.Match = v?.Invoke(new QueryContainerDescriptor<T>()));
}
