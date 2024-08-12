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
using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
[ReadAs(typeof(FunctionScoreQuery))]
public interface IFunctionScoreQuery : IQuery
{
    [DataMember(Name = "boost_mode")]
    FunctionBoostMode? BoostMode { get; set; }

    [DataMember(Name = "functions")]
    IEnumerable<IScoreFunction> Functions { get; set; }

    [DataMember(Name = "max_boost")]
    double? MaxBoost { get; set; }

    [DataMember(Name = "min_score")]
    double? MinScore { get; set; }

    [DataMember(Name = "query")]
    QueryContainer Query { get; set; }

    [DataMember(Name = "score_mode")]
    FunctionScoreMode? ScoreMode { get; set; }
}

public class FunctionScoreQuery : QueryBase, IFunctionScoreQuery
{
    public FunctionBoostMode? BoostMode { get; set; }
    public IEnumerable<IScoreFunction> Functions { get; set; }
    public double? MaxBoost { get; set; }
    public double? MinScore { get; set; }
    public QueryContainer Query { get; set; }
    public FunctionScoreMode? ScoreMode { get; set; }
    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.FunctionScore = this;

    internal static bool IsConditionless(IFunctionScoreQuery q, bool force = false) =>
        force || !q.Functions.HasAny();
}

public class FunctionScoreQueryDescriptor<T>
    : QueryDescriptorBase<FunctionScoreQueryDescriptor<T>, IFunctionScoreQuery>
        , IFunctionScoreQuery where T : class
{
    private bool _forcedConditionless;
    protected override bool Conditionless => FunctionScoreQuery.IsConditionless(this, _forcedConditionless);
    FunctionBoostMode? IFunctionScoreQuery.BoostMode { get; set; }
    IEnumerable<IScoreFunction> IFunctionScoreQuery.Functions { get; set; }
    double? IFunctionScoreQuery.MaxBoost { get; set; }
    double? IFunctionScoreQuery.MinScore { get; set; }
    QueryContainer IFunctionScoreQuery.Query { get; set; }
    FunctionScoreMode? IFunctionScoreQuery.ScoreMode { get; set; }

    public FunctionScoreQueryDescriptor<T> ConditionlessWhen(bool isConditionless)
    {
        _forcedConditionless = isConditionless;
        return this;
    }

    public FunctionScoreQueryDescriptor<T> Query(Func<QueryContainerDescriptor<T>, QueryContainer> selector) =>
        Assign(selector, (a, v) => a.Query = v?.Invoke(new QueryContainerDescriptor<T>()));

    public FunctionScoreQueryDescriptor<T> Functions(Func<ScoreFunctionsDescriptor<T>, IPromise<IList<IScoreFunction>>> functions) =>
        Assign(functions, (a, v) => a.Functions = v?.Invoke(new ScoreFunctionsDescriptor<T>())?.Value);

    public FunctionScoreQueryDescriptor<T> Functions(IEnumerable<IScoreFunction> functions) => Assign(functions, (a, v) => a.Functions = v);

    public FunctionScoreQueryDescriptor<T> ScoreMode(FunctionScoreMode? mode) => Assign(mode, (a, v) => a.ScoreMode = v);

    public FunctionScoreQueryDescriptor<T> BoostMode(FunctionBoostMode? mode) => Assign(mode, (a, v) => a.BoostMode = v);

    public FunctionScoreQueryDescriptor<T> MaxBoost(double? maxBoost) => Assign(maxBoost, (a, v) => a.MaxBoost = v);

    public FunctionScoreQueryDescriptor<T> MinScore(double? minScore) => Assign(minScore, (a, v) => a.MinScore = v);
}
