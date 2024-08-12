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
using System.Diagnostics;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
/// A field of type token_count is really an integer field which accepts string values,
/// analyzes them, then indexes the number of tokens in the string.
/// </summary>
[InterfaceDataContract]
public interface ITokenCountProperty : IDocValuesProperty
{
    [DataMember(Name = "analyzer")]
    string Analyzer { get; set; }

    [DataMember(Name = "enable_position_increments")]
    bool? EnablePositionIncrements { get; set; }

    [DataMember(Name = "index")]
    bool? Index { get; set; }

    [DataMember(Name = "null_value")]
    double? NullValue { get; set; }
}

/// <inheritdoc cref="ITokenCountProperty"/>
[DebuggerDisplay("{DebugDisplay}")]
public class TokenCountProperty : DocValuesPropertyBase, ITokenCountProperty
{
    public TokenCountProperty() : base(FieldType.TokenCount) { }
    public string Analyzer { get; set; }
    public bool? EnablePositionIncrements { get; set; }
    public bool? Index { get; set; }
    public double? NullValue { get; set; }
}

/// <inheritdoc cref="ITokenCountProperty"/>
[DebuggerDisplay("{DebugDisplay}")]
public class TokenCountPropertyDescriptor<T>
    : DocValuesPropertyDescriptorBase<TokenCountPropertyDescriptor<T>, ITokenCountProperty, T>, ITokenCountProperty
    where T : class
{
    public TokenCountPropertyDescriptor() : base(FieldType.TokenCount) { }
    string ITokenCountProperty.Analyzer { get; set; }
    bool? ITokenCountProperty.EnablePositionIncrements { get; set; }
    bool? ITokenCountProperty.Index { get; set; }
    double? ITokenCountProperty.NullValue { get; set; }

    public TokenCountPropertyDescriptor<T> Analyzer(string analyzer) => Assign(analyzer, (a, v) => a.Analyzer = v);

    public TokenCountPropertyDescriptor<T> EnablePositionIncrements(bool? enablePositionIncrements = true) =>
        Assign(enablePositionIncrements, (a, v) => a.EnablePositionIncrements = v);

    public TokenCountPropertyDescriptor<T> Index(bool? index = true) => Assign(index, (a, v) => a.Index = v);
    public TokenCountPropertyDescriptor<T> NullValue(double? nullValue) => Assign(nullValue, (a, v) => a.NullValue = v);
}
