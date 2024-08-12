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
using OpenSearch.Net;

namespace OpenSearch.Client;

/// <summary>
/// A set of analyzers aimed at analyzing specific language text.
/// </summary>
public interface ILanguageAnalyzer : IAnalyzer
{
    /// <summary>
    /// The stem_exclusion parameter allows you to specify an array of lowercase words that should not be stemmed.
    /// </summary>
    [DataMember(Name = "stem_exclusion")]
    IEnumerable<string> StemExclusionList { get; set; }

    /// <summary>
    /// A list of stopword to initialize the stop filter with. Defaults to the english stop words.
    /// </summary>
    [DataMember(Name = "stopwords")]
    StopWords StopWords { get; set; }

    /// <summary>
    /// A path (either relative to config location, or absolute) to a stopwords file configuration.
    /// </summary>
    [DataMember(Name = "stopwords_path")]
    string StopwordsPath { get; set; }
}

/// <inheritdoc />
public class LanguageAnalyzer : AnalyzerBase, ILanguageAnalyzer
{
    private string _type = "language";

    /// <inheritdoc />
    [IgnoreDataMember]
    public Language? Language
    {
        get => _type.ToEnum<Language>();
        set => _type = value.GetStringValue().ToLowerInvariant();
    }

    /// <inheritdoc />
    public IEnumerable<string> StemExclusionList { get; set; }

    /// <inheritdoc />
    public StopWords StopWords { get; set; }

    /// <inheritdoc />
    public string StopwordsPath { get; set; }

    public override string Type
    {
        get => _type;
        protected set
        {
            _type = value;
            Language = value.ToEnum<Language>();
        }
    }
}

/// <inheritdoc />
public class LanguageAnalyzerDescriptor : AnalyzerDescriptorBase<LanguageAnalyzerDescriptor, ILanguageAnalyzer>, ILanguageAnalyzer
{
    private string _type = "language";
    protected override string Type => _type;
    IEnumerable<string> ILanguageAnalyzer.StemExclusionList { get; set; }

    StopWords ILanguageAnalyzer.StopWords { get; set; }
    string ILanguageAnalyzer.StopwordsPath { get; set; }

    public LanguageAnalyzerDescriptor Language(Language? language)
    {
        _type = language?.GetStringValue().ToLowerInvariant();
        return this;
    }

    public LanguageAnalyzerDescriptor StopWords(StopWords stopWords) => Assign(stopWords, (a, v) => a.StopWords = v);

    public LanguageAnalyzerDescriptor StopWords(params string[] stopWords) => Assign(stopWords, (a, v) => a.StopWords = v);

    public LanguageAnalyzerDescriptor StopWords(IEnumerable<string> stopWords) => Assign(stopWords.ToListOrNullIfEmpty(), (a, v) => a.StopWords = v);
}
