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
/// Protects words from being modified by stemmers. Must be placed before any stemming filters.
/// </summary>
public interface IKeywordMarkerTokenFilter : ITokenFilter
{
    /// <summary>
    /// Set to true to lower case all words first. Defaults to false.
    /// </summary>
    [DataMember(Name = "ignore_case")]
    [JsonFormatter(typeof(NullableStringBooleanFormatter))]
    bool? IgnoreCase { get; set; }

    /// <summary>
    /// A list of words to use.
    /// <para></para>
    /// Cannot specify both <see cref="KeywordsPattern"/> and <see cref="Keywords"/> or <see cref="KeywordsPath"/>
    /// </summary>
    [DataMember(Name = "keywords")]
    IEnumerable<string> Keywords { get; set; }

    /// <summary>
    /// A path (either relative to config location, or absolute) to a list of words.
    /// <para></para>
    /// Cannot specify both <see cref="KeywordsPattern"/> and <see cref="Keywords"/> or <see cref="KeywordsPath"/>
    /// </summary>
    [DataMember(Name = "keywords_path")]
    string KeywordsPath { get; set; }

    /// <summary>
    /// A regular expression pattern to match against words in the text.
    /// <para></para>
    /// Cannot specify both <see cref="KeywordsPattern"/> and <see cref="Keywords"/> or <see cref="KeywordsPath"/>
    /// </summary>
    [DataMember(Name = "keywords_pattern")]
    string KeywordsPattern { get; set; }
}

/// <inheritdoc cref="IKeywordMarkerTokenFilter" />
public class KeywordMarkerTokenFilter : TokenFilterBase, IKeywordMarkerTokenFilter
{
    public KeywordMarkerTokenFilter() : base("keyword_marker") { }

    /// <inheritdoc />
    public bool? IgnoreCase { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> Keywords { get; set; }

    /// <inheritdoc />
    public string KeywordsPath { get; set; }

    /// <inheritdoc />
    public string KeywordsPattern { get; set; }
}

/// <inheritdoc cref="IKeywordMarkerTokenFilter" />
public class KeywordMarkerTokenFilterDescriptor
    : TokenFilterDescriptorBase<KeywordMarkerTokenFilterDescriptor, IKeywordMarkerTokenFilter>, IKeywordMarkerTokenFilter
{
    protected override string Type => "keyword_marker";
    bool? IKeywordMarkerTokenFilter.IgnoreCase { get; set; }

    IEnumerable<string> IKeywordMarkerTokenFilter.Keywords { get; set; }
    string IKeywordMarkerTokenFilter.KeywordsPath { get; set; }

    string IKeywordMarkerTokenFilter.KeywordsPattern { get; set; }

    /// <inheritdoc cref="IKeywordMarkerTokenFilter.IgnoreCase" />
    public KeywordMarkerTokenFilterDescriptor IgnoreCase(bool? ignoreCase = true) => Assign(ignoreCase, (a, v) => a.IgnoreCase = v);

    /// <inheritdoc cref="IKeywordMarkerTokenFilter.KeywordsPath" />
    public KeywordMarkerTokenFilterDescriptor KeywordsPath(string path) => Assign(path, (a, v) => a.KeywordsPath = v);

    /// <inheritdoc cref="IKeywordMarkerTokenFilter.KeywordsPattern" />
    public KeywordMarkerTokenFilterDescriptor KeywordsPattern(string pattern) => Assign(pattern, (a, v) => a.KeywordsPattern = v);

    /// <inheritdoc cref="IKeywordMarkerTokenFilter.Keywords" />
    public KeywordMarkerTokenFilterDescriptor Keywords(IEnumerable<string> keywords) => Assign(keywords, (a, v) => a.Keywords = v);

    /// <inheritdoc cref="IKeywordMarkerTokenFilter.Keywords" />
    public KeywordMarkerTokenFilterDescriptor Keywords(params string[] keywords) => Assign(keywords, (a, v) => a.Keywords = v);
}
