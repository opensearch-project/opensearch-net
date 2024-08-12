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

using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

/// <summary>
///  Token filter that generates bigrams for frequently occuring terms. Single terms are still indexed.
/// <para>Note, common_words or common_words_path field is required.</para>
/// </summary>
public interface ICommonGramsTokenFilter : ITokenFilter
{
    /// <summary>
    /// A list of common words to use.
    /// </summary>
    [DataMember(Name = "common_words")]
    [JsonFormatter(typeof(SingleOrEnumerableFormatter<string>))]
    IEnumerable<string> CommonWords { get; set; }

    /// <summary>
    /// A path (either relative to config location, or absolute) to a list of common words.
    /// </summary>
    [DataMember(Name = "common_words_path")]
    string CommonWordsPath { get; set; }

    /// <summary>
    /// If true, common words matching will be case insensitive.
    /// </summary>
    [DataMember(Name = "ignore_case")]
    [JsonFormatter(typeof(NullableStringBooleanFormatter))]
    bool? IgnoreCase { get; set; }

    /// <summary>
    /// Generates bigrams then removes common words and single terms followed by a common word.
    /// </summary>
    [DataMember(Name = "query_mode")]
    [JsonFormatter(typeof(NullableStringBooleanFormatter))]
    bool? QueryMode { get; set; }
}

/// <inheritdoc />
public class CommonGramsTokenFilter : TokenFilterBase, ICommonGramsTokenFilter
{
    public CommonGramsTokenFilter() : base("common_grams") { }

    /// <inheritdoc />
    public IEnumerable<string> CommonWords { get; set; }

    /// <inheritdoc />
    public string CommonWordsPath { get; set; }

    /// <inheritdoc />
    public bool? IgnoreCase { get; set; }

    /// <inheritdoc />
    public bool? QueryMode { get; set; }
}

/// <inheritdoc />
public class CommonGramsTokenFilterDescriptor
    : TokenFilterDescriptorBase<CommonGramsTokenFilterDescriptor, ICommonGramsTokenFilter>, ICommonGramsTokenFilter
{
    protected override string Type => "common_grams";

    IEnumerable<string> ICommonGramsTokenFilter.CommonWords { get; set; }
    string ICommonGramsTokenFilter.CommonWordsPath { get; set; }
    bool? ICommonGramsTokenFilter.IgnoreCase { get; set; }
    bool? ICommonGramsTokenFilter.QueryMode { get; set; }

    /// <inheritdoc />
    public CommonGramsTokenFilterDescriptor QueryMode(bool? queryMode = true) => Assign(queryMode, (a, v) => a.QueryMode = v);

    /// <inheritdoc />
    public CommonGramsTokenFilterDescriptor IgnoreCase(bool? ignoreCase = true) => Assign(ignoreCase, (a, v) => a.IgnoreCase = v);

    /// <inheritdoc />
    public CommonGramsTokenFilterDescriptor CommonWordsPath(string path) => Assign(path, (a, v) => a.CommonWordsPath = v);

    /// <inheritdoc />
    public CommonGramsTokenFilterDescriptor CommonWords(IEnumerable<string> commonWords) => Assign(commonWords, (a, v) => a.CommonWords = v);

    /// <inheritdoc />
    public CommonGramsTokenFilterDescriptor CommonWords(params string[] commonWords) => Assign(commonWords, (a, v) => a.CommonWords = v);
}
