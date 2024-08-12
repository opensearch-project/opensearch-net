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
[ReadAs(typeof(SpanNotQuery))]
public interface ISpanNotQuery : ISpanSubQuery
{
    [DataMember(Name = "dist")]
    int? Dist { get; set; }

    [DataMember(Name = "exclude")]
    ISpanQuery Exclude { get; set; }

    [DataMember(Name = "include")]
    ISpanQuery Include { get; set; }

    [DataMember(Name = "post")]
    int? Post { get; set; }

    [DataMember(Name = "pre")]
    int? Pre { get; set; }
}

public class SpanNotQuery : QueryBase, ISpanNotQuery
{
    public int? Dist { get; set; }
    public ISpanQuery Exclude { get; set; }
    public ISpanQuery Include { get; set; }
    public int? Post { get; set; }
    public int? Pre { get; set; }
    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.SpanNot = this;

    internal static bool IsConditionless(ISpanNotQuery q)
    {
        var exclude = q.Exclude as IQuery;
        var include = q.Include as IQuery;

        return exclude == null && include == null
            || include == null && exclude.Conditionless
            || exclude == null && include.Conditionless
            || exclude != null && exclude.Conditionless && include != null && include.Conditionless;
    }
}

public class SpanNotQueryDescriptor<T>
    : QueryDescriptorBase<SpanNotQueryDescriptor<T>, ISpanNotQuery>
        , ISpanNotQuery where T : class
{
    protected override bool Conditionless => SpanNotQuery.IsConditionless(this);
    int? ISpanNotQuery.Dist { get; set; }
    ISpanQuery ISpanNotQuery.Exclude { get; set; }
    ISpanQuery ISpanNotQuery.Include { get; set; }
    int? ISpanNotQuery.Post { get; set; }
    int? ISpanNotQuery.Pre { get; set; }

    public SpanNotQueryDescriptor<T> Include(Func<SpanQueryDescriptor<T>, ISpanQuery> selector) =>
        Assign(selector(new SpanQueryDescriptor<T>()), (a, v) => a.Include = v);

    public SpanNotQueryDescriptor<T> Exclude(Func<SpanQueryDescriptor<T>, ISpanQuery> selector) =>
        Assign(selector(new SpanQueryDescriptor<T>()), (a, v) => a.Exclude = v);

    public SpanNotQueryDescriptor<T> Pre(int? pre) => Assign(pre, (a, v) => a.Pre = v);

    public SpanNotQueryDescriptor<T> Post(int? post) => Assign(post, (a, v) => a.Post = v);

    public SpanNotQueryDescriptor<T> Dist(int? dist) => Assign(dist, (a, v) => a.Dist = v);
}
