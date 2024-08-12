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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(PercentileRanksAggregationFormatter))]
public interface IPercentileRanksAggregation : IFormattableMetricAggregation
{
    IPercentilesMethod Method { get; set; }
    IEnumerable<double> Values { get; set; }
    bool? Keyed { get; set; }
}

public class PercentileRanksAggregation : FormattableMetricAggregationBase, IPercentileRanksAggregation
{
    internal PercentileRanksAggregation() { }

    public PercentileRanksAggregation(string name, Field field) : base(name, field) { }

    public IPercentilesMethod Method { get; set; }
    public IEnumerable<double> Values { get; set; }
    public bool? Keyed { get; set; }

    internal override void WrapInContainer(AggregationContainer c) => c.PercentileRanks = this;
}

public class PercentileRanksAggregationDescriptor<T>
    : FormattableMetricAggregationDescriptorBase<PercentileRanksAggregationDescriptor<T>, IPercentileRanksAggregation, T>, IPercentileRanksAggregation
    where T : class
{
    IPercentilesMethod IPercentileRanksAggregation.Method { get; set; }
    IEnumerable<double> IPercentileRanksAggregation.Values { get; set; }
    bool? IPercentileRanksAggregation.Keyed { get; set; }

    public PercentileRanksAggregationDescriptor<T> Values(IEnumerable<double> values) =>
        Assign(values, (a, v) => a.Values = v);

    public PercentileRanksAggregationDescriptor<T> Values(params double[] values) =>
        Assign(values, (a, v) => a.Values = v);

    public PercentileRanksAggregationDescriptor<T> Method(Func<PercentilesMethodDescriptor, IPercentilesMethod> methodSelctor) =>
        Assign(methodSelctor, (a, v) => a.Method = v?.Invoke(new PercentilesMethodDescriptor()));

    public PercentileRanksAggregationDescriptor<T> Keyed(bool? keyed = true) =>
        Assign(keyed, (a, v) => a.Keyed = v);
}
