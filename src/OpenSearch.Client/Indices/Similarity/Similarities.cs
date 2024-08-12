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

using System;
using System.Collections.Generic;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[JsonFormatter(typeof(VerbatimDictionaryKeysFormatter<Similarities, ISimilarities, string, ISimilarity>))]
public interface ISimilarities : IIsADictionary<string, ISimilarity> { }

[JsonFormatter(typeof(VerbatimDictionaryKeysFormatter<Similarities, Similarities, string, ISimilarity>))]
public class Similarities : IsADictionaryBase<string, ISimilarity>, ISimilarities
{
    public Similarities() { }

    public Similarities(IDictionary<string, ISimilarity> container) : base(container) { }

    public Similarities(Dictionary<string, ISimilarity> container) : base(container) { }

    /// <summary>
    /// Add an <see cref="ISimilarity" />
    /// </summary>
    public void Add(string type, ISimilarity mapping) => BackingDictionary.Add(type, mapping);
}

public class SimilaritiesDescriptor : IsADictionaryDescriptorBase<SimilaritiesDescriptor, ISimilarities, string, ISimilarity>
{
    public SimilaritiesDescriptor() : base(new Similarities()) { }

    /// <summary>
    /// BM25 Similarity. Introduced in Stephen E. Robertson, Steve Walker, Susan Jones, Micheline Hancock-Beaulieu,
    /// and Mike Gatford. Okapi at TREC-3. In Proceedings of the Third Text Retrieval Conference (TREC 1994). Gaithersburg,
    /// USA, November 1994.
    /// </summary>
    public SimilaritiesDescriptor BM25(string name, Func<BM25SimilarityDescriptor, IBM25Similarity> selector) =>
        Assign(name, selector?.Invoke(new BM25SimilarityDescriptor()));

    /// <summary>
    /// A similarity with Bayesian smoothing using Dirichlet priors.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public SimilaritiesDescriptor LMDirichlet(string name, Func<LMDirichletSimilarityDescriptor, ILMDirichletSimilarity> selector) =>
        Assign(name, selector?.Invoke(new LMDirichletSimilarityDescriptor()));

    /// <summary>
    /// A similarity that attempts to capture important patterns in the text,
    /// while leaving out noise.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public SimilaritiesDescriptor LMJelinek(string name, Func<LMJelinekMercerSimilarityDescriptor, ILMJelinekMercerSimilarity> selector) =>
        Assign(name, selector?.Invoke(new LMJelinekMercerSimilarityDescriptor()));

    /// <summary>
    /// Similarity that implements the divergence from independence model
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public SimilaritiesDescriptor DFI(string name, Func<DFISimilarityDescriptor, IDFISimilarity> selector) =>
        Assign(name, selector?.Invoke(new DFISimilarityDescriptor()));

    /// <summary>
    /// Implements the divergence from randomness (DFR) framework introduced in Gianni Amati and Cornelis Joost Van Rijsbergen.
    /// 2002.
    /// Probabilistic models of information retrieval based on measuring the divergence from randomness. ACM Trans. Inf. Syst.
    /// 20, 4 (October
    /// 2002), 357-389.
    /// The DFR scoring formula is composed of three separate components: the basic model, the aftereffect and an additional
    /// normalization
    /// component,
    /// represented by the classes BasicModel, AfterEffect and Normalization, respectively.The names of these classes were
    /// chosen to match the
    /// names of their counterparts in the Terrier IR engine.
    /// </summary>
    public SimilaritiesDescriptor DFR(string name, Func<DFRSimilarityDescriptor, IDFRSimilarity> selector) =>
        Assign(name, selector?.Invoke(new DFRSimilarityDescriptor()));

    /// <summary>
    /// Information based model similarity.
    /// The algorithm is based on the concept that the information content in any symbolic distribution sequence
    /// is primarily determined by the repetitive usage of its basic elements.
    /// For written texts this challenge would correspond to comparing the writing styles of different authors.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public SimilaritiesDescriptor IB(string name, Func<IBSimilarityDescriptor, IIBSimilarity> selector) =>
        Assign(name, selector?.Invoke(new IBSimilarityDescriptor()));

    /// <summary>
    /// A custom similarity
    /// </summary>
    public SimilaritiesDescriptor Custom(string name, string type, Func<CustomSimilarityDescriptor, IPromise<ICustomSimilarity>> selector) =>
        Assign(name, selector?.Invoke(new CustomSimilarityDescriptor().Type(type))?.Value);

    /// <summary>
    /// A similarity that allows a script to be used in order to specify how scores should be computed.
    /// </summary>
    public SimilaritiesDescriptor Scripted(string name, Func<ScriptedSimilarityDescriptor, IScriptedSimilarity> selector) =>
        Assign(name, selector?.Invoke(new ScriptedSimilarityDescriptor()));
}
