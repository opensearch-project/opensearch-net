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
[ReadAs(typeof(SpanFirstQueryDescriptor<object>))]
public interface ISpanFirstQuery : ISpanSubQuery
{
    [DataMember(Name = "end")]
    int? End { get; set; }

    [DataMember(Name = "match")]
    ISpanQuery Match { get; set; }
}

public class SpanFirstQuery : QueryBase, ISpanFirstQuery
{
    public int? End { get; set; }
    public ISpanQuery Match { get; set; }
    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.SpanFirst = this;

    internal static bool IsConditionless(ISpanFirstQuery q) => q.Match == null || q.Match.Conditionless;
}

public class SpanFirstQueryDescriptor<T>
    : QueryDescriptorBase<SpanFirstQueryDescriptor<T>, ISpanFirstQuery>
        , ISpanFirstQuery where T : class
{
    protected override bool Conditionless => SpanFirstQuery.IsConditionless(this);
    int? ISpanFirstQuery.End { get; set; }
    ISpanQuery ISpanFirstQuery.Match { get; set; }

    public SpanFirstQueryDescriptor<T> Match(Func<SpanQueryDescriptor<T>, ISpanQuery> selector) =>
        Assign(selector(new SpanQueryDescriptor<T>()), (a, v) => a.Match = v);

    public SpanFirstQueryDescriptor<T> End(int? end) => Assign(end, (a, v) => a.End = v);
}
