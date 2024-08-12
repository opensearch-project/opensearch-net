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

/// <summary>
/// A tokenizer of type pattern that can flexibly separate text into terms via a regular expression.
/// </summary>
public interface IPatternTokenizer : ITokenizer
{
    /// <summary>
    /// The regular expression flags.
    /// </summary>
    [DataMember(Name = "flags")]
    string Flags { get; set; }

    /// <summary>
    /// Which group to extract into tokens. Defaults to -1 (split).
    /// </summary>
    [DataMember(Name = "group")]
    [JsonFormatter(typeof(NullableStringIntFormatter))]
    int? Group { get; set; }

    /// <summary>
    /// The regular expression pattern, defaults to \W+.
    /// </summary>
    [DataMember(Name = "pattern")]
    string Pattern { get; set; }
}

/// <inheritdoc />
public class PatternTokenizer : TokenizerBase, IPatternTokenizer
{
    public PatternTokenizer() => Type = "pattern";

    /// <inheritdoc />
    public string Flags { get; set; }

    /// <summary />
    public int? Group { get; set; }

    /// <inheritdoc />
    public string Pattern { get; set; }
}

/// <inheritdoc />
public class PatternTokenizerDescriptor
    : TokenizerDescriptorBase<PatternTokenizerDescriptor, IPatternTokenizer>, IPatternTokenizer
{
    protected override string Type => "pattern";
    string IPatternTokenizer.Flags { get; set; }

    int? IPatternTokenizer.Group { get; set; }
    string IPatternTokenizer.Pattern { get; set; }

    /// <inheritdoc />
    public PatternTokenizerDescriptor Group(int? group) => Assign(group, (a, v) => a.Group = v);

    /// <inheritdoc />
    public PatternTokenizerDescriptor Pattern(string pattern) => Assign(pattern, (a, v) => a.Pattern = v);

    /// <inheritdoc />
    public PatternTokenizerDescriptor Flags(string flags) => Assign(flags, (a, v) => a.Flags = v);
}
