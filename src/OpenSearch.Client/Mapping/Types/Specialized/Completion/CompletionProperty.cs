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
using System.Diagnostics;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[InterfaceDataContract]
public interface ICompletionProperty : IDocValuesProperty
{
    [DataMember(Name = "analyzer")]
    string Analyzer { get; set; }

    [DataMember(Name = "contexts")]
    IList<ISuggestContext> Contexts { get; set; }

    [DataMember(Name = "max_input_length")]
    int? MaxInputLength { get; set; }

    [DataMember(Name = "preserve_position_increments")]
    bool? PreservePositionIncrements { get; set; }

    [DataMember(Name = "preserve_separators")]
    bool? PreserveSeparators { get; set; }

    [DataMember(Name = "search_analyzer")]
    string SearchAnalyzer { get; set; }
}

[DataContract]
[DebuggerDisplay("{DebugDisplay}")]
public class CompletionProperty : DocValuesPropertyBase, ICompletionProperty
{
    public CompletionProperty() : base(FieldType.Completion) { }

    public string Analyzer { get; set; }
    public IList<ISuggestContext> Contexts { get; set; }
    public int? MaxInputLength { get; set; }
    public bool? PreservePositionIncrements { get; set; }
    public bool? PreserveSeparators { get; set; }

    public string SearchAnalyzer { get; set; }
}

[DebuggerDisplay("{DebugDisplay}")]
public class CompletionPropertyDescriptor<T>
    : DocValuesPropertyDescriptorBase<CompletionPropertyDescriptor<T>, ICompletionProperty, T>, ICompletionProperty
    where T : class
{
    public CompletionPropertyDescriptor() : base(FieldType.Completion) { }

    string ICompletionProperty.Analyzer { get; set; }
    IList<ISuggestContext> ICompletionProperty.Contexts { get; set; }
    int? ICompletionProperty.MaxInputLength { get; set; }
    bool? ICompletionProperty.PreservePositionIncrements { get; set; }
    bool? ICompletionProperty.PreserveSeparators { get; set; }
    string ICompletionProperty.SearchAnalyzer { get; set; }

    public CompletionPropertyDescriptor<T> SearchAnalyzer(string searchAnalyzer) =>
        Assign(searchAnalyzer, (a, v) => a.SearchAnalyzer = v);

    public CompletionPropertyDescriptor<T> Analyzer(string analyzer) => Assign(analyzer, (a, v) => a.Analyzer = v);

    public CompletionPropertyDescriptor<T> PreserveSeparators(bool? preserveSeparators = true) =>
        Assign(preserveSeparators, (a, v) => a.PreserveSeparators = v);

    public CompletionPropertyDescriptor<T> PreservePositionIncrements(bool? preservePositionIncrements = true) =>
        Assign(preservePositionIncrements, (a, v) => a.PreservePositionIncrements = v);

    public CompletionPropertyDescriptor<T> MaxInputLength(int? maxInputLength) => Assign(maxInputLength, (a, v) => a.MaxInputLength = v);

    public CompletionPropertyDescriptor<T> Contexts(Func<SuggestContextsDescriptor<T>, IPromise<IList<ISuggestContext>>> contexts) =>
        Assign(contexts, (a, v) => a.Contexts = v?.Invoke(new SuggestContextsDescriptor<T>()).Value);
}
