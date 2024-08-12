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
[ReadAs(typeof(ScriptedMetricAggregation))]
public interface IScriptedMetricAggregation : IMetricAggregation
{
    [DataMember(Name = "combine_script")]
    IScript CombineScript { get; set; }

    [DataMember(Name = "init_script")]
    IScript InitScript { get; set; }

    [DataMember(Name = "map_script")]
    IScript MapScript { get; set; }

    [DataMember(Name = "params")]
    IDictionary<string, object> Params { get; set; }

    [DataMember(Name = "reduce_script")]
    IScript ReduceScript { get; set; }
}

public class ScriptedMetricAggregation : MetricAggregationBase, IScriptedMetricAggregation
{
    internal ScriptedMetricAggregation() { }

    public ScriptedMetricAggregation(string name) : base(name, null) { }

    public IScript CombineScript { get; set; }
    public IScript InitScript { get; set; }
    public IScript MapScript { get; set; }
    public IDictionary<string, object> Params { get; set; }
    public IScript ReduceScript { get; set; }

    internal override void WrapInContainer(AggregationContainer c) => c.ScriptedMetric = this;
}

public class ScriptedMetricAggregationDescriptor<T>
    : MetricAggregationDescriptorBase<ScriptedMetricAggregationDescriptor<T>, IScriptedMetricAggregation, T>
        , IScriptedMetricAggregation
    where T : class
{
    IScript IScriptedMetricAggregation.CombineScript { get; set; }
    IScript IScriptedMetricAggregation.InitScript { get; set; }
    IScript IScriptedMetricAggregation.MapScript { get; set; }
    IDictionary<string, object> IScriptedMetricAggregation.Params { get; set; }
    IScript IScriptedMetricAggregation.ReduceScript { get; set; }

    public ScriptedMetricAggregationDescriptor<T> InitScript(string script) => Assign((InlineScript)script, (a, v) => a.InitScript = v);

    public ScriptedMetricAggregationDescriptor<T> InitScript(Func<ScriptDescriptor, IScript> scriptSelector) =>
        Assign(scriptSelector, (a, v) => a.InitScript = v?.Invoke(new ScriptDescriptor()));

    public ScriptedMetricAggregationDescriptor<T> MapScript(string script) => Assign((InlineScript)script, (a, v) => a.MapScript = v);

    public ScriptedMetricAggregationDescriptor<T> MapScript(Func<ScriptDescriptor, IScript> scriptSelector) =>
        Assign(scriptSelector, (a, v) => a.MapScript = v?.Invoke(new ScriptDescriptor()));

    public ScriptedMetricAggregationDescriptor<T> CombineScript(string script) => Assign((InlineScript)script, (a, v) => a.CombineScript = v);

    public ScriptedMetricAggregationDescriptor<T> CombineScript(Func<ScriptDescriptor, IScript> scriptSelector) =>
        Assign(scriptSelector, (a, v) => a.CombineScript = v?.Invoke(new ScriptDescriptor()));

    public ScriptedMetricAggregationDescriptor<T> ReduceScript(string script) => Assign((InlineScript)script, (a, v) => a.ReduceScript = v);

    public ScriptedMetricAggregationDescriptor<T> ReduceScript(Func<ScriptDescriptor, IScript> scriptSelector) =>
        Assign(scriptSelector, (a, v) => a.ReduceScript = v?.Invoke(new ScriptDescriptor()));

    public ScriptedMetricAggregationDescriptor<T>
        Params(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> paramSelector) =>
        Assign(paramSelector, (a, v) => a.Params = v?.Invoke(new FluentDictionary<string, object>()));
}
