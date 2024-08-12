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
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace OpenSearch.Client;

/// <summary>
/// Matches terms that start with a specified set of characters. This prefix can expand to match at most 128 terms.
/// If the prefix matches more than 128 terms, OpenSearch returns an error.
/// You can use the index-prefixes option in the field mapping to avoid this limit.
/// </summary>
[ReadAs(typeof(IntervalsPrefix))]
public interface IIntervalsPrefix : IIntervalsNoFilter
{
    /// <summary>
    /// Analyzer used to normalize the prefix. Defaults to the top-level field's analyzer.
    /// </summary>
    [DataMember(Name = "analyzer")]
    string Analyzer { get; set; }

    /// <summary>
    /// Beginning characters of terms you wish to find in the top-level field
    /// </summary>
    [DataMember(Name = "prefix")]
    string Prefix { get; set; }

    /// <summary>
    /// If specified, then match intervals from this field rather than the top-level field.
    /// The prefix is normalized using the search analyzer from this field, unless a separate analyzer is specified.
    /// </summary>
    [DataMember(Name = "use_field")]
    Field UseField { get; set; }
}

/// <inheritdoc cref="IIntervalsPrefix" />
public class IntervalsPrefix : IntervalsNoFilterBase, IIntervalsPrefix
{
    /// <inheritdoc />
    public string Analyzer { get; set; }

    /// <inheritdoc />
    public string Prefix { get; set; }

    /// <inheritdoc />
    public Field UseField { get; set; }

    internal override void WrapInContainer(IIntervalsContainer container) => container.Prefix = this;
}

/// <inheritdoc cref="IIntervalsPrefix" />
public class IntervalsPrefixDescriptor : DescriptorBase<IntervalsPrefixDescriptor, IIntervalsPrefix>, IIntervalsPrefix
{
    string IIntervalsPrefix.Analyzer { get; set; }
    string IIntervalsPrefix.Prefix { get; set; }
    Field IIntervalsPrefix.UseField { get; set; }

    /// <inheritdoc cref="IIntervalsPrefix.Analyzer" />
    public IntervalsPrefixDescriptor Analyzer(string analyzer) => Assign(analyzer, (a, v) => a.Analyzer = v);

    /// <inheritdoc cref="IIntervalsPrefix.Prefix" />
    public IntervalsPrefixDescriptor Prefix(string prefix) => Assign(prefix, (a, v) => a.Prefix = v);

    /// <inheritdoc cref="IIntervalsPrefix.UseField" />
    public IntervalsPrefixDescriptor UseField<T>(Expression<Func<T, object>> objectPath) => Assign(objectPath, (a, v) => a.UseField = v);

    /// <inheritdoc cref="IIntervalsPrefix.UseField" />
    public IntervalsPrefixDescriptor UseField(Field useField) => Assign(useField, (a, v) => a.UseField = v);
}
