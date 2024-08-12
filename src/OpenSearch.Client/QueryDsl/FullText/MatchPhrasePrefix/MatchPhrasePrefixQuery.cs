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

[InterfaceDataContract]
[JsonFormatter(typeof(FieldNameQueryFormatter<MatchPhrasePrefixQuery, IMatchPhrasePrefixQuery>))]
public interface IMatchPhrasePrefixQuery : IFieldNameQuery
{
    [DataMember(Name = "analyzer")]
    string Analyzer { get; set; }

    [DataMember(Name = "max_expansions")]
    int? MaxExpansions { get; set; }

    [DataMember(Name = "query")]
    string Query { get; set; }

    [DataMember(Name = "slop")]
    int? Slop { get; set; }

    /// <summary>
    /// If the analyzer used removes all tokens in a query like a stop filter does, the default behavior is
    /// to match no documents at all. In order to change that, <see cref="Client.ZeroTermsQuery" /> can be used,
    /// which accepts <see cref="Client.ZeroTermsQuery.None" /> (default) and <see cref="Client.ZeroTermsQuery.All" />
    /// which corresponds to a match_all query.
    /// </summary>
    [DataMember(Name = "zero_terms_query")]
    ZeroTermsQuery? ZeroTermsQuery { get; set; }
}

public class MatchPhrasePrefixQuery : FieldNameQueryBase, IMatchPhrasePrefixQuery
{
    public string Analyzer { get; set; }
    public int? MaxExpansions { get; set; }
    public string Query { get; set; }
    public int? Slop { get; set; }

    /// <inheritdoc />
    public ZeroTermsQuery? ZeroTermsQuery { get; set; }

    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.MatchPhrasePrefix = this;

    internal static bool IsConditionless(IMatchPhrasePrefixQuery q) => q.Field.IsConditionless() || q.Query.IsNullOrEmpty();
}

public class MatchPhrasePrefixQueryDescriptor<T>
    : FieldNameQueryDescriptorBase<MatchPhrasePrefixQueryDescriptor<T>, IMatchPhrasePrefixQuery, T>, IMatchPhrasePrefixQuery
    where T : class
{
    protected override bool Conditionless => MatchPhrasePrefixQuery.IsConditionless(this);
    string IMatchPhrasePrefixQuery.Analyzer { get; set; }
    int? IMatchPhrasePrefixQuery.MaxExpansions { get; set; }

    string IMatchPhrasePrefixQuery.Query { get; set; }
    int? IMatchPhrasePrefixQuery.Slop { get; set; }
    ZeroTermsQuery? IMatchPhrasePrefixQuery.ZeroTermsQuery { get; set; }

    public MatchPhrasePrefixQueryDescriptor<T> Query(string query) => Assign(query, (a, v) => a.Query = v);

    public MatchPhrasePrefixQueryDescriptor<T> Analyzer(string analyzer) => Assign(analyzer, (a, v) => a.Analyzer = v);

    public MatchPhrasePrefixQueryDescriptor<T> MaxExpansions(int? maxExpansions) => Assign(maxExpansions, (a, v) => a.MaxExpansions = v);

    public MatchPhrasePrefixQueryDescriptor<T> Slop(int? slop) => Assign(slop, (a, v) => a.Slop = v);

    /// <inheritdoc cref="IMatchQuery.ZeroTermsQuery" />
    public MatchPhrasePrefixQueryDescriptor<T> ZeroTermsQuery(ZeroTermsQuery? zeroTermsQuery) => Assign(zeroTermsQuery, (a, v) => a.ZeroTermsQuery = v);
}
