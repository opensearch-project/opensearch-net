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

namespace OpenSearch.Client;

/// <summary>
/// The nori analyzer consists of the following tokenizer and token filters:
/// <para> - nori_tokenizer</para>
/// <para> - nori_part_of_speech token filter</para>
/// <para> - nori_readingform token filter</para>
/// <para> - nori_number token filter</para>
/// <para> - lowercase token filter</para>
/// </summary>
public interface INoriAnalyzer : IAnalyzer
{
    /// <inheritdoc cref="INoriTokenizer.DecompoundMode" />
    [DataMember(Name = "decompound_mode")]
    NoriDecompoundMode? DecompoundMode { get; set; }

    /// <inheritdoc cref="INoriPartOfSpeechTokenFilter.StopTags" />
    [DataMember(Name = "stoptags")]
    IEnumerable<string> StopTags { get; set; }

    /// <inheritdoc cref="INoriTokenizer.UserDictionary" />
    [DataMember(Name = "user_dictionary")]
    string UserDictionary { get; set; }
}

/// <inheritdoc cref="INoriAnalyzer" />
public class NoriAnalyzer : AnalyzerBase, INoriAnalyzer
{
    public NoriAnalyzer() : base("nori") { }

    /// <inheritdoc cref="INoriTokenizer.DecompoundMode" />
    public NoriDecompoundMode? DecompoundMode { get; set; }

    ///<inheritdoc cref="INoriPartOfSpeechTokenFilter.StopTags" />
    public IEnumerable<string> StopTags { get; set; }

    /// <inheritdoc cref="INoriTokenizer.UserDictionary" />
    public string UserDictionary { get; set; }
}

/// <inheritdoc cref="INoriAnalyzer" />
public class NoriAnalyzerDescriptor : AnalyzerDescriptorBase<NoriAnalyzerDescriptor, INoriAnalyzer>, INoriAnalyzer
{
    protected override string Type => "nori";

    NoriDecompoundMode? INoriAnalyzer.DecompoundMode { get; set; }
    IEnumerable<string> INoriAnalyzer.StopTags { get; set; }
    string INoriAnalyzer.UserDictionary { get; set; }

    /// <inheritdoc cref="INoriTokenizer.DecompoundMode" />
    public NoriAnalyzerDescriptor DecompoundMode(NoriDecompoundMode? mode) => Assign(mode, (a, v) => a.DecompoundMode = v);

    /// <inheritdoc cref="INoriTokenizer.UserDictionary" />
    public NoriAnalyzerDescriptor UserDictionary(string path) => Assign(path, (a, v) => a.UserDictionary = v);

    ///<inheritdoc cref="INoriPartOfSpeechTokenFilter.StopTags" />
    public NoriAnalyzerDescriptor StopTags(IEnumerable<string> stopTags) => Assign(stopTags, (a, v) => a.StopTags = v);

    ///<inheritdoc cref="INoriPartOfSpeechTokenFilter.StopTags" />
    public NoriAnalyzerDescriptor StopTags(params string[] stopTags) => Assign(stopTags, (a, v) => a.StopTags = v);
}
