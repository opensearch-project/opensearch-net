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
[ReadAs(typeof(MovingFunctionAggregation))]
public interface IMovingFunctionAggregation : IPipelineAggregation
{
    [DataMember(Name = "script")]
    string Script { get; set; }

    [DataMember(Name = "window")]
    int? Window { get; set; }

    [DataMember(Name = "shift")]
    int? Shift { get; set; }
}

public class MovingFunctionAggregation
    : PipelineAggregationBase, IMovingFunctionAggregation
{
    internal MovingFunctionAggregation() { }

    public MovingFunctionAggregation(string name, SingleBucketsPath bucketsPath)
        : base(name, bucketsPath) { }

    public string Script { get; set; }

    public int? Window { get; set; }

    public int? Shift { get; set; }

    internal override void WrapInContainer(AggregationContainer c) => c.MovingFunction = this;
}

public class MovingFunctionAggregationDescriptor
    : PipelineAggregationDescriptorBase<MovingFunctionAggregationDescriptor, IMovingFunctionAggregation, SingleBucketsPath>
        , IMovingFunctionAggregation
{
    string IMovingFunctionAggregation.Script { get; set; }
    int? IMovingFunctionAggregation.Window { get; set; }
    int? IMovingFunctionAggregation.Shift { get; set; }

    public MovingFunctionAggregationDescriptor Window(int? windowSize) => Assign(windowSize, (a, v) => a.Window = v);

    public MovingFunctionAggregationDescriptor Script(string script) => Assign(script, (a, v) => a.Script = v);

    public MovingFunctionAggregationDescriptor Shift(int? shift) => Assign(shift, (a, v) => a.Shift = v);

}
