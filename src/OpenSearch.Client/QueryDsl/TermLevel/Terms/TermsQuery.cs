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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
[JsonFormatter(typeof(TermsQueryFormatter))]
public interface ITermsQuery : IFieldNameQuery
{
    IEnumerable<object> Terms { get; set; }
    IFieldLookup TermsLookup { get; set; }
}

[DataContract]
public class TermsQuery : FieldNameQueryBase, ITermsQuery
{
    public IEnumerable<object> Terms { get; set; }
    public IFieldLookup TermsLookup { get; set; }
    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.Terms = this;

    internal static bool IsConditionless(ITermsQuery q) => q.Field.IsConditionless()
        || (q.Terms == null
            || !q.Terms.HasAny()
            || q.Terms.All(t => t == null
                || ((t as string)?.IsNullOrEmpty()).GetValueOrDefault(false))
        )
        &&
        (q.TermsLookup == null
            || q.TermsLookup.Id == null
            || q.TermsLookup.Path.IsConditionless()
            || q.TermsLookup.Index == null
        );
}

/// <summary>
/// A query that match on any (configurable) of the provided terms.
/// This is a simpler syntax query for using a bool query with several term queries in the should clauses.
/// </summary>
/// <typeparam name="T">The type that represents the expected hit type</typeparam>
[DataContract]
public class TermsQueryDescriptor<T>
    : FieldNameQueryDescriptorBase<TermsQueryDescriptor<T>, ITermsQuery, T>
        , ITermsQuery where T : class
{
    protected override bool Conditionless => TermsQuery.IsConditionless(this);
    IEnumerable<object> ITermsQuery.Terms { get; set; }
    IFieldLookup ITermsQuery.TermsLookup { get; set; }

    public TermsQueryDescriptor<T> TermsLookup<TOther>(Func<FieldLookupDescriptor<TOther>, IFieldLookup> selector)
        where TOther : class => Assign(selector(new FieldLookupDescriptor<TOther>()), (a, v) => a.TermsLookup = v);

    public TermsQueryDescriptor<T> Terms<TValue>(IEnumerable<TValue> terms) => Assign(terms?.Cast<object>(), (a, v) => a.Terms = v);

    public TermsQueryDescriptor<T> Terms<TValue>(params TValue[] terms) => Assign(terms, (a, v) =>
    {
        if (v?.Length == 1 && typeof(IEnumerable).IsAssignableFrom(typeof(TValue)) && typeof(TValue) != typeof(string))
            a.Terms = (v.First() as IEnumerable)?.Cast<object>();
        else a.Terms = v?.Cast<object>();
    });
}
