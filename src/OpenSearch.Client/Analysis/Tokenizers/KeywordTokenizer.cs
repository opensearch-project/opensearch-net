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
/// A tokenizer of type keyword that emits the entire input as a single input.
/// </summary>
public interface IKeywordTokenizer : ITokenizer
{
    /// <summary>
    /// The term buffer size. Defaults to 256.
    /// </summary>
    [DataMember(Name = "buffer_size")]
    [JsonFormatter(typeof(NullableStringIntFormatter))]
    int? BufferSize { get; set; }
}

/// <inheritdoc />
public class KeywordTokenizer : TokenizerBase, IKeywordTokenizer
{
    public KeywordTokenizer() => Type = "keyword";

    /// <inheritdoc />
    public int? BufferSize { get; set; }
}

/// <inheritdoc />
public class KeywordTokenizerDescriptor
    : TokenizerDescriptorBase<KeywordTokenizerDescriptor, IKeywordTokenizer>, IKeywordTokenizer
{
    protected override string Type => "keyword";

    int? IKeywordTokenizer.BufferSize { get; set; }

    /// <inheritdoc />
    public KeywordTokenizerDescriptor BufferSize(int? size) => Assign(size, (a, v) => a.BufferSize = v);
}
