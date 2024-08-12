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
/// Suggest option
/// </summary>
[InterfaceDataContract]
[ReadAs(typeof(SuggestOption<>))]
public interface ISuggestOption<out TDocument> where TDocument : class
{
    /// <summary>
    /// Phrase suggestions only, true if matching documents for the collate query were found,
    /// </summary>
    [DataMember(Name = "collate_match")]
    bool CollateMatch { get; }

    /// <summary>
    /// Completion suggester only, the contexts associated with the completed document
    /// </summary>
    [DataMember(Name = "contexts")]
    IDictionary<string, IEnumerable<Context>> Contexts { get; }

    // TODO this should be reported to opensearch-project/opensearch-net
    [DataMember(Name = "_score")]
    double? DocumentScore { get; }

    /// <inheritdoc />
    [DataMember(Name = "fields")]
    FieldValues Fields { get; }

    /// <summary>
    /// Term suggester only
    /// </summary>
    [DataMember(Name = "freq")]
    long Frequency { get; set; }

    /// <summary>
    /// Phrase suggester only, highlighted version of text
    /// </summary>
    [DataMember(Name = "highlighted")]
    string Highlighted { get; }

    /// <summary>
    /// Completion suggester only, the id of the completed document
    /// </summary>
    [DataMember(Name = "_id")]
    string Id { get; }

    /// <summary>
    /// Completion suggester only, the index of the completed document
    /// </summary>
    [DataMember(Name = "_index")]
    IndexName Index { get; }

    /// <summary> Either the <see cref="DocumentScore" /> or the <see cref="SuggestScore" /></summary>
    [IgnoreDataMember]
    double Score { get; }

    /// <summary>
    /// Completion suggester only, the source of the completed document
    /// </summary>
    [DataMember(Name = "_source")]
    [JsonFormatter(typeof(SourceFormatter<>))]
    TDocument Source { get; }

    [DataMember(Name = "score")]
    double? SuggestScore { get; }

    [DataMember(Name = "text")]
    string Text { get; }
}

/// <inheritdoc />
public class SuggestOption<TDocument> : ISuggestOption<TDocument>
    where TDocument : class
{
    /// <inheritdoc />
    [DataMember(Name = "collate_match")]
    public bool CollateMatch { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "contexts")]
    public IDictionary<string, IEnumerable<Context>> Contexts { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "_score")]
    public double? DocumentScore { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "fields")]
    public FieldValues Fields { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "freq")]
    public long Frequency { get; set; }

    /// <inheritdoc />
    [DataMember(Name = "highlighted")]
    public string Highlighted { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "_id")]
    public string Id { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "_index")]
    public IndexName Index { get; internal set; }

    /// <inheritdoc />
    [IgnoreDataMember]
    public double Score => DocumentScore ?? SuggestScore ?? 0;

    /// <inheritdoc />
    [DataMember(Name = "_source")]
    [JsonFormatter(typeof(SourceFormatter<>))]
    public TDocument Source { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "score")]
    public double? SuggestScore { get; internal set; }

    /// <inheritdoc />
    [DataMember(Name = "text")]
    public string Text { get; internal set; }
}
