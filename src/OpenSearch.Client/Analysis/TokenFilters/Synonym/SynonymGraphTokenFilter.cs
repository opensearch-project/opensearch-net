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
/// The synonym_graph token filter allows to easily handle synonyms,
/// including multi-word synonyms correctly during the analysis process.
/// </summary>
public interface ISynonymGraphTokenFilter : ITokenFilter
{
    [DataMember(Name = "expand")]
    [JsonFormatter(typeof(NullableStringBooleanFormatter))]
    bool? Expand { get; set; }

    [DataMember(Name = "format")]
    SynonymFormat? Format { get; set; }

    /// <inheritdoc cref="ISynonymTokenFilter.Lenient" />
    [DataMember(Name = "lenient")]
    [JsonFormatter(typeof(NullableStringBooleanFormatter))]
    bool? Lenient { get; set; }

    [DataMember(Name = "synonyms")]
    IEnumerable<string> Synonyms { get; set; }

    /// <summary>
    ///  a path a synonyms file relative to the node's `config` location.
    /// </summary>
    [DataMember(Name = "synonyms_path")]
    string SynonymsPath { get; set; }

    [DataMember(Name = "tokenizer")]
    string Tokenizer { get; set; }

    /// <summary>
    /// Whether this token filter can reload changes to synonym files
    /// on demand.
    /// Marking as updateable means this component is only usable at search time
    /// </summary>
    [DataMember(Name = "updateable")]
    [JsonFormatter(typeof(NullableStringBooleanFormatter))]
    bool? Updateable { get; set; }
}

/// <inheritdoc />
public class SynonymGraphTokenFilter : TokenFilterBase, ISynonymGraphTokenFilter
{
    public SynonymGraphTokenFilter() : base("synonym_graph") { }

    /// <inheritdoc />
    public bool? Expand { get; set; }

    /// <inheritdoc />
    public SynonymFormat? Format { get; set; }

    /// <inheritdoc cref="ISynonymTokenFilter.Lenient" />
    public bool? Lenient { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> Synonyms { get; set; }

    /// <inheritdoc />
    public string SynonymsPath { get; set; }

    /// <inheritdoc />
    public string Tokenizer { get; set; }

    /// <inheritdoc />
    public bool? Updateable { get; set; }
}

/// <inheritdoc />
public class SynonymGraphTokenFilterDescriptor
    : TokenFilterDescriptorBase<SynonymGraphTokenFilterDescriptor, ISynonymGraphTokenFilter>, ISynonymGraphTokenFilter
{
    protected override string Type => "synonym_graph";
    bool? ISynonymGraphTokenFilter.Expand { get; set; }
    SynonymFormat? ISynonymGraphTokenFilter.Format { get; set; }
    bool? ISynonymGraphTokenFilter.Lenient { get; set; }
    IEnumerable<string> ISynonymGraphTokenFilter.Synonyms { get; set; }
    string ISynonymGraphTokenFilter.SynonymsPath { get; set; }
    string ISynonymGraphTokenFilter.Tokenizer { get; set; }
    bool? ISynonymGraphTokenFilter.Updateable { get; set; }

    /// <inheritdoc cref="ISynonymGraphTokenFilter.Expand"/>
    public SynonymGraphTokenFilterDescriptor Expand(bool? expand = true) => Assign(expand, (a, v) => a.Expand = v);

    /// <inheritdoc cref="ISynonymGraphTokenFilter.Lenient" />
    public SynonymGraphTokenFilterDescriptor Lenient(bool? lenient = true) => Assign(lenient, (a, v) => a.Lenient = v);

    /// <inheritdoc cref="ISynonymGraphTokenFilter.Tokenizer"/>
    public SynonymGraphTokenFilterDescriptor Tokenizer(string tokenizer) => Assign(tokenizer, (a, v) => a.Tokenizer = v);

    /// <inheritdoc cref="ISynonymGraphTokenFilter.SynonymsPath"/>
    public SynonymGraphTokenFilterDescriptor SynonymsPath(string path) => Assign(path, (a, v) => a.SynonymsPath = v);

    /// <inheritdoc cref="ISynonymGraphTokenFilter.Format"/>
    public SynonymGraphTokenFilterDescriptor Format(SynonymFormat? format) => Assign(format, (a, v) => a.Format = v);

    /// <inheritdoc cref="ISynonymGraphTokenFilter.Synonyms"/>
    public SynonymGraphTokenFilterDescriptor Synonyms(IEnumerable<string> synonymGraphs) => Assign(synonymGraphs, (a, v) => a.Synonyms = v);

    /// <inheritdoc cref="ISynonymGraphTokenFilter.Synonyms"/>
    public SynonymGraphTokenFilterDescriptor Synonyms(params string[] synonymGraphs) => Assign(synonymGraphs, (a, v) => a.Synonyms = v);

    /// <inheritdoc cref="ISynonymGraphTokenFilter.Updateable"/>
    public SynonymGraphTokenFilterDescriptor Updateable(bool? updateable = true) => Assign(updateable, (a, v) => a.Updateable = v);
}
