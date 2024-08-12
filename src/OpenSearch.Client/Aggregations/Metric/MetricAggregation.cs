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
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace OpenSearch.Client;

/// <summary>
/// An aggregation that computes metrics based on values extracted in
/// one way or another from the documents that are being aggregated.
/// </summary>
public interface IMetricAggregation : IAggregation
{
    /// <summary>
    /// The field on which to aggregate
    /// </summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }

    /// <summary>
    /// The value to use when the aggregation finds a missing value in a document
    /// </summary>
    [DataMember(Name = "missing")]
    double? Missing { get; set; }

    /// <summary>
    /// The script to use for the aggregation
    /// </summary>
    [DataMember(Name = "script")]
    IScript Script { get; set; }
}

/// <summary>
/// A metric aggregation that can accept a format to use for the output of the aggregation
/// </summary>
public interface IFormattableMetricAggregation : IMetricAggregation
{
    /// <summary>
    /// The format to use for the output of the aggregation
    /// </summary>
    [DataMember(Name = "format")]
    string Format { get; set; }
}

public abstract class MetricAggregationBase : AggregationBase, IMetricAggregation
{
    internal MetricAggregationBase() { }

    protected MetricAggregationBase(string name, Field field) : base(name) => Field = field;

    public Field Field { get; set; }
    public double? Missing { get; set; }
    public virtual IScript Script { get; set; }
}

public abstract class FormattableMetricAggregationBase : MetricAggregationBase, IFormattableMetricAggregation
{
    internal FormattableMetricAggregationBase() { }

    protected FormattableMetricAggregationBase(string name, Field field) : base(name, field) { }

    /// <inheritdoc />
    public string Format { get; set; }
}

public abstract class MetricAggregationDescriptorBase<TMetricAggregation, TMetricAggregationInterface, T>
    : DescriptorBase<TMetricAggregation, TMetricAggregationInterface>, IMetricAggregation
    where TMetricAggregation : MetricAggregationDescriptorBase<TMetricAggregation, TMetricAggregationInterface, T>
    , TMetricAggregationInterface, IMetricAggregation
    where T : class
    where TMetricAggregationInterface : class, IMetricAggregation
{
    Field IMetricAggregation.Field { get; set; }

    IDictionary<string, object> IAggregation.Meta { get; set; }

    double? IMetricAggregation.Missing { get; set; }

    string IAggregation.Name { get; set; }

    IScript IMetricAggregation.Script { get; set; }

    /// <inheritdoc cref="IMetricAggregation.Field"/>
    public TMetricAggregation Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="IMetricAggregation.Field"/>
    public TMetricAggregation Field<TValue>(Expression<Func<T, TValue>> field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="IMetricAggregation.Script"/>
    public virtual TMetricAggregation Script(string script) => Assign((InlineScript)script, (a, v) => a.Script = v);

    /// <inheritdoc cref="IMetricAggregation.Script"/>
    public virtual TMetricAggregation Script(Func<ScriptDescriptor, IScript> scriptSelector) =>
        Assign(scriptSelector, (a, v) => a.Script = v?.Invoke(new ScriptDescriptor()));

    /// <inheritdoc cref="IMetricAggregation.Missing"/>
    public TMetricAggregation Missing(double? missing) => Assign(missing, (a, v) => a.Missing = v);

    /// <inheritdoc cref="IAggregation.Meta"/>
    public TMetricAggregation Meta(Func<FluentDictionary<string, object>, FluentDictionary<string, object>> selector) =>
        Assign(selector, (a, v) => a.Meta = v?.Invoke(new FluentDictionary<string, object>()));
}

public abstract class FormattableMetricAggregationDescriptorBase<TFormattableMetricAggregation, TFormattableMetricAggregationInterface, T>
    : MetricAggregationDescriptorBase<TFormattableMetricAggregation, TFormattableMetricAggregationInterface, T>, IFormattableMetricAggregation
    where TFormattableMetricAggregation :
    FormattableMetricAggregationDescriptorBase<TFormattableMetricAggregation, TFormattableMetricAggregationInterface, T>
    , TFormattableMetricAggregationInterface, IFormattableMetricAggregation
    where T : class
    where TFormattableMetricAggregationInterface : class, IFormattableMetricAggregation
{
    string IFormattableMetricAggregation.Format { get; set; }

    /// <inheritdoc cref="IFormattableMetricAggregation.Format"/>
    public TFormattableMetricAggregation Format(string format) =>
        Assign(format, (a, v) => a.Format = v);
}
