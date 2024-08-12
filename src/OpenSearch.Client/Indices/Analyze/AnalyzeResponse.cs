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

[DataContract]
public class AnalyzeResponse : ResponseBase
{
    /// <summary>
    /// When <see cref="IAnalyzeRequest.Explain " /> is to true this will hold the detailed view of the analyzed tokens.
    /// </summary>
    [DataMember(Name = "detail")]
    public AnalyzeDetail Detail { get; internal set; }

    /// <summary>
    /// When <see cref="IAnalyzeRequest.Explain " /> is not true this will hold the analyzed tokens.
    /// </summary>
    [DataMember(Name = "tokens")]
    public IReadOnlyCollection<AnalyzeToken> Tokens { get; internal set; } = EmptyReadOnly<AnalyzeToken>.Collection;
}


[DataContract]
public class AnalyzeDetail
{
    [DataMember(Name = "charfilters")]
    public IReadOnlyCollection<CharFilterDetail> CharFilters { get; internal set; } = EmptyReadOnly<CharFilterDetail>.Collection;

    [DataMember(Name = "custom_analyzer")]
    public bool CustomAnalyzer { get; internal set; }

    [DataMember(Name = "tokenfilters")]
    public IReadOnlyCollection<TokenDetail> Filters { get; internal set; } = EmptyReadOnly<TokenDetail>.Collection;

    [DataMember(Name = "tokenizer")]
    public TokenDetail Tokenizer { get; internal set; }
}

[DataContract]
public class CharFilterDetail
{
    [DataMember(Name = "filtered_text")]
    public IReadOnlyCollection<string> FilteredText { get; internal set; } = EmptyReadOnly<string>.Collection;

    [DataMember(Name = "name")]
    public string Name { get; internal set; }
}

[DataContract]
public class TokenDetail
{
    [DataMember(Name = "name")]
    public string Name { get; internal set; }

    [DataMember(Name = "tokens")]
    public IReadOnlyCollection<ExplainAnalyzeToken> Tokens { get; internal set; } = EmptyReadOnly<ExplainAnalyzeToken>.Collection;
}

[DataContract]
public class ExplainAnalyzeToken
{
    [DataMember(Name = "bytes")]
    public string Bytes { get; internal set; }

    [DataMember(Name = "end_offset")]
    public long EndOffset { get; internal set; }

    [DataMember(Name = "keyword")]
    public bool? Keyword { get; internal set; }

    [DataMember(Name = "position")]
    public long Position { get; internal set; }

    [DataMember(Name = "positionLength")]
    public long? PositionLength { get; internal set; }

    [DataMember(Name = "start_offset")]
    public long StartOffset { get; internal set; }

    [DataMember(Name = "termFrequency")]
    public long? TermFrequency { get; internal set; }

    [DataMember(Name = "token")]
    public string Token { get; internal set; }

    [DataMember(Name = "type")]
    public string Type { get; internal set; }
}
