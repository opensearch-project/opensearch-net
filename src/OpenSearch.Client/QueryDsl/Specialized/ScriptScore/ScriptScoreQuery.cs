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

/// <summary>
/// A query allowing you to modify the score of documents that are retrieved by a query.
/// This can be useful if, for example, a score function is computationally expensive and it is sufficient to
/// compute the score on a filtered set of documents.
/// </summary>
[ReadAs(typeof(ScriptScoreQuery))]
[InterfaceDataContract]
public interface IScriptScoreQuery : IQuery
{
    /// <summary>
    /// The query to execute
    /// </summary>
    [DataMember(Name = "query")]
    QueryContainer Query { get; set; }

    /// <summary>
    /// The script to execute
    /// </summary>
    [DataMember(Name = "script")]
    IScript Script { get; set; }

    /// <summary>
		/// The score above which documents will be returned
		/// </summary>
		[DataMember(Name = "min_score")]
    double? MinScore { get; set; }
}

/// <inheritdoc cref="IScriptScoreQuery" />
public class ScriptScoreQuery : QueryBase, IScriptScoreQuery
{
    /// <inheritdoc />
    public QueryContainer Query { get; set; }

    /// <inheritdoc />
    public IScript Script { get; set; }

    /// <inheritdoc />
    public double? MinScore { get; set; }

    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.ScriptScore = this;

    internal static bool IsConditionless(IScriptScoreQuery q)
    {
        if (q.Script == null || q.Query.IsConditionless())
            return true;

        switch (q.Script)
        {
            case IInlineScript inlineScript:
                return inlineScript.Source.IsNullOrEmpty();
            case IIndexedScript indexedScript:
                return indexedScript.Id.IsNullOrEmpty();
        }

        return false;
    }
}

/// <inheritdoc cref="IScriptScoreQuery" />
public class ScriptScoreQueryDescriptor<T>
    : QueryDescriptorBase<ScriptScoreQueryDescriptor<T>, IScriptScoreQuery>
        , IScriptScoreQuery where T : class
{
    protected override bool Conditionless => ScriptScoreQuery.IsConditionless(this);
    QueryContainer IScriptScoreQuery.Query { get; set; }

    IScript IScriptScoreQuery.Script { get; set; }

    double? IScriptScoreQuery.MinScore { get; set; }

    /// <inheritdoc cref="IScriptScoreQuery.Query" />
    public ScriptScoreQueryDescriptor<T> Query(Func<QueryContainerDescriptor<T>, QueryContainer> selector) =>
        Assign(selector, (a, v) => a.Query = v?.Invoke(new QueryContainerDescriptor<T>()));

    /// <inheritdoc cref="IScriptScoreQuery.Script" />
    public ScriptScoreQueryDescriptor<T> Script(Func<ScriptDescriptor, IScript> selector) =>
        Assign(selector, (a, v) => a.Script = v?.Invoke(new ScriptDescriptor()));

    /// <inheritdoc cref="IScriptScoreQuery.MinScore" />
    public ScriptScoreQueryDescriptor<T> MinScore(double? minScore) => Assign(minScore, (a, v) => a.MinScore = v);

}
