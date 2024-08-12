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
[ReadAs(typeof(BucketSelectorAggregation))]
public interface IBucketSelectorAggregation : IPipelineAggregation
{
    [DataMember(Name = "script")]
    IScript Script { get; set; }
}

public class BucketSelectorAggregation
    : PipelineAggregationBase, IBucketSelectorAggregation
{
    internal BucketSelectorAggregation() { }

    public BucketSelectorAggregation(string name, MultiBucketsPath bucketsPath)
        : base(name, bucketsPath) { }

    public IScript Script { get; set; }

    internal override void WrapInContainer(AggregationContainer c) => c.BucketSelector = this;
}

public class BucketSelectorAggregationDescriptor
    : PipelineAggregationDescriptorBase<BucketSelectorAggregationDescriptor, IBucketSelectorAggregation, MultiBucketsPath>
        , IBucketSelectorAggregation
{
    IScript IBucketSelectorAggregation.Script { get; set; }

    public BucketSelectorAggregationDescriptor Script(string script) => Assign((InlineScript)script, (a, v) => a.Script = v);

    public BucketSelectorAggregationDescriptor Script(Func<ScriptDescriptor, IScript> scriptSelector) =>
        Assign(scriptSelector, (a, v) => a.Script = v?.Invoke(new ScriptDescriptor()));

    public BucketSelectorAggregationDescriptor BucketsPath(Func<MultiBucketsPathDescriptor, IPromise<IBucketsPath>> selector) =>
        Assign(selector, (a, v) => a.BucketsPath = v?.Invoke(new MultiBucketsPathDescriptor())?.Value);
}
