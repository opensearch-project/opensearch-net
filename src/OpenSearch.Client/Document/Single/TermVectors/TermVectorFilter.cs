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
/// Filter terms returned based on their TF-IDF scores.
/// This can be useful in order find out a good characteristic vector of a document.
/// </summary>
[InterfaceDataContract]
public interface ITermVectorFilter
{
    /// <summary>
    /// Ignore words which occur in more than this many docs. Defaults to unbounded.
    /// </summary>
    [DataMember(Name = "max_doc_freq")]
    int? MaximumDocumentFrequency { get; set; }

    /// <summary>
    /// Maximum number of terms that must be returned per field. Defaults to 25.
    /// </summary>
    [DataMember(Name = "max_num_terms")]
    int? MaximumNumberOfTerms { get; set; }

    /// <summary>
    /// Ignore words with more than this frequency in the source doc. Defaults to unbounded.
    /// </summary>
    [DataMember(Name = "max_term_freq")]
    int? MaximumTermFrequency { get; set; }

    /// <summary>
    /// The maximum word length above which words will be ignored. Defaults to unbounded.
    /// </summary>
    [DataMember(Name = "max_word_length")]
    int? MaximumWordLength { get; set; }

    /// <summary>
    /// Ignore terms which do not occur in at least this many docs. Defaults to 1.
    /// </summary>
    [DataMember(Name = "min_doc_freq")]
    int? MinimumDocumentFrequency { get; set; }

    /// <summary>
    /// Ignore words with less than this frequency in the source doc. Defaults to 1.
    /// </summary>
    [DataMember(Name = "min_term_freq")]
    int? MinimumTermFrequency { get; set; }

    /// <summary>
    /// The minimum word length below which words will be ignored. Defaults to 0.
    /// </summary>
    [DataMember(Name = "min_word_length")]
    int? MinimumWordLength { get; set; }
}

/// <summary>
/// Filter terms returned based on their TF-IDF scores.
/// This can be useful in order find out a good characteristic vector of a document.
/// </summary>
public class TermVectorFilter : ITermVectorFilter
{
    /// <summary>
    /// Ignore words which occur in more than this many docs. Defaults to unbounded.
    /// </summary>
    public int? MaximumDocumentFrequency { get; set; }

    /// <summary>
    /// Maximum number of terms that must be returned per field. Defaults to 25.
    /// </summary>
    public int? MaximumNumberOfTerms { get; set; }

    /// <summary>
    /// Ignore words with more than this frequency in the source doc. Defaults to unbounded.
    /// </summary>
    public int? MaximumTermFrequency { get; set; }

    /// <summary>
    /// The maximum word length above which words will be ignored. Defaults to unbounded.
    /// </summary>
    public int? MaximumWordLength { get; set; }

    /// <summary>
    /// Ignore terms which do not occur in at least this many docs. Defaults to 1.
    /// </summary>
    public int? MinimumDocumentFrequency { get; set; }

    /// <summary>
    /// Ignore words with less than this frequency in the source doc. Defaults to 1.
    /// </summary>
    public int? MinimumTermFrequency { get; set; }

    /// <summary>
    /// The minimum word length below which words will be ignored. Defaults to 0.
    /// </summary>
    public int? MinimumWordLength { get; set; }
}

/// <summary>
/// Filter terms returned based on their TF-IDF scores.
/// This can be useful in order find out a good characteristic vector of a document.
/// </summary>
public class TermVectorFilterDescriptor
    : DescriptorBase<TermVectorFilterDescriptor, ITermVectorFilter>, ITermVectorFilter
{
    int? ITermVectorFilter.MaximumDocumentFrequency { get; set; }
    int? ITermVectorFilter.MaximumNumberOfTerms { get; set; }
    int? ITermVectorFilter.MaximumTermFrequency { get; set; }
    int? ITermVectorFilter.MaximumWordLength { get; set; }
    int? ITermVectorFilter.MinimumDocumentFrequency { get; set; }
    int? ITermVectorFilter.MinimumTermFrequency { get; set; }
    int? ITermVectorFilter.MinimumWordLength { get; set; }

    /// <summary>
    /// Maximum number of terms that must be returned per field. Defaults to 25.
    /// </summary>
    public TermVectorFilterDescriptor MaximimumNumberOfTerms(int? maxNumTerms) => Assign(maxNumTerms, (a, v) => a.MaximumNumberOfTerms = v);

    /// <summary>
    /// Ignore words with less than this frequency in the source doc. Defaults to 1.
    /// </summary>
    public TermVectorFilterDescriptor MinimumTermFrequency(int? minTermFreq) => Assign(minTermFreq, (a, v) => a.MinimumTermFrequency = v);

    /// <summary>
    /// Ignore words with more than this frequency in the source doc. Defaults to unbounded.
    /// </summary>
    public TermVectorFilterDescriptor MaximumTermFrequency(int? maxTermFreq) => Assign(maxTermFreq, (a, v) => a.MaximumTermFrequency = v);

    /// <summary>
    /// Ignore terms which do not occur in at least this many docs. Defaults to 1.
    /// </summary>
    public TermVectorFilterDescriptor MinimumDocumentFrequency(int? minDocFreq) => Assign(minDocFreq, (a, v) => a.MinimumDocumentFrequency = v);

    /// <summary>
    /// Ignore words which occur in more than this many docs. Defaults to unbounded.
    /// </summary>
    public TermVectorFilterDescriptor MaximumDocumentFrequency(int? maxDocFreq) => Assign(maxDocFreq, (a, v) => a.MaximumDocumentFrequency = v);

    /// <summary>
    /// The minimum word length below which words will be ignored. Defaults to 0.
    /// </summary>
    public TermVectorFilterDescriptor MinimumWordLength(int? minWordLength) => Assign(minWordLength, (a, v) => a.MinimumWordLength = v);

    /// <summary>
    /// The maximum word length above which words will be ignored. Defaults to unbounded.
    /// </summary>
    public TermVectorFilterDescriptor MaximumWordLength(int? maxWordLength) => Assign(maxWordLength, (a, v) => a.MaximumWordLength = v);
}
