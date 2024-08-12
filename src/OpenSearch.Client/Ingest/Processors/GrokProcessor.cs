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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
public interface IGrokProcessor : IProcessor
{
    /// <summary>
    /// The field to use for grok expression parsing
    /// </summary>
    [DataMember(Name = "field")]
    Field Field { get; set; }

    /// <summary>
    /// A map of pattern-name and pattern tuples defining custom patterns
    /// to be used by the current processor. Patterns matching existing
    /// names will override the pre-existing definition.
    /// </summary>
    [DataMember(Name = "pattern_definitions")]
    IDictionary<string, string> PatternDefinitions { get; set; }

    /// <summary>
    /// An ordered list of grok expression to match and extract named captures with.
    /// Returns on the first expression in the list that matches.
    /// </summary>
    [DataMember(Name = "patterns")]
    IEnumerable<string> Patterns { get; set; }

    /// <summary>
    /// when <c>true</c>, _ingest._grok_match_index will be inserted into your matched document’s
    /// metadata with the index into the pattern found in patterns that matched.
    /// </summary>
    [DataMember(Name = "trace_match")]
    bool? TraceMatch { get; set; }

    /// <summary>
    /// If <c>true</c> and <see cref="Field" /> does not exist or is null,
    /// the processor quietly exits without modifying the document. Default is <c>false</c>
    /// </summary>
    [DataMember(Name = "ignore_missing")]
    bool? IgnoreMissing { get; set; }
}

/// <inheritdoc cref="IGrokProcessor" />
public class GrokProcessor : ProcessorBase, IGrokProcessor
{
    /// <inheritdoc />
    public Field Field { get; set; }
    /// <inheritdoc />
    public IDictionary<string, string> PatternDefinitions { get; set; }
    /// <inheritdoc />
    public IEnumerable<string> Patterns { get; set; }
    /// <inheritdoc />
    public bool? TraceMatch { get; set; }
    /// <inheritdoc />
    public bool? IgnoreMissing { get; set; }
    protected override string Name => "grok";
}

/// <inheritdoc cref="IGrokProcessor" />
public class GrokProcessorDescriptor<T>
    : ProcessorDescriptorBase<GrokProcessorDescriptor<T>, IGrokProcessor>, IGrokProcessor
    where T : class
{
    protected override string Name => "grok";

    Field IGrokProcessor.Field { get; set; }
    IDictionary<string, string> IGrokProcessor.PatternDefinitions { get; set; }
    IEnumerable<string> IGrokProcessor.Patterns { get; set; }
    bool? IGrokProcessor.TraceMatch { get; set; }
    bool? IGrokProcessor.IgnoreMissing { get; set; }

    /// <inheritdoc cref="IGrokProcessor.Field" />
    public GrokProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

    /// <inheritdoc cref="IGrokProcessor.Field" />
    public GrokProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) =>
        Assign(objectPath, (a, v) => a.Field = v);

    /// <inheritdoc cref="IGrokProcessor.Patterns" />
    public GrokProcessorDescriptor<T> Patterns(IEnumerable<string> patterns) => Assign(patterns, (a, v) => a.Patterns = v);

    /// <inheritdoc cref="IGrokProcessor.Patterns" />
    public GrokProcessorDescriptor<T> Patterns(params string[] patterns) => Assign(patterns, (a, v) => a.Patterns = v);

    /// <inheritdoc cref="IGrokProcessor.PatternDefinitions" />
    public GrokProcessorDescriptor<T> PatternDefinitions(
        Func<FluentDictionary<string, string>, FluentDictionary<string, string>> patternDefinitions
    ) =>
        Assign(patternDefinitions, (a, v) => a.PatternDefinitions = v?.Invoke(new FluentDictionary<string, string>()));

    /// <inheritdoc cref="IGrokProcessor.TraceMatch" />
    public GrokProcessorDescriptor<T> TraceMatch(bool? traceMatch = true) =>
        Assign(traceMatch, (a, v) => a.TraceMatch = v);

    /// <inheritdoc cref="IGrokProcessor.IgnoreMissing" />
    public GrokProcessorDescriptor<T> IgnoreMissing(bool? ignoreMissing = true) => Assign(ignoreMissing, (a, v) => a.IgnoreMissing = v);
}
