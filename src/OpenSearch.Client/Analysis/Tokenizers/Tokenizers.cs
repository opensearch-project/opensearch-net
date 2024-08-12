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

[JsonFormatter(typeof(VerbatimDictionaryKeysFormatter<Tokenizers, ITokenizers, string, ITokenizer>))]
public interface ITokenizers : IIsADictionary<string, ITokenizer> { }

public class Tokenizers : IsADictionaryBase<string, ITokenizer>, ITokenizers
{
    public Tokenizers() { }

    public Tokenizers(IDictionary<string, ITokenizer> container) : base(container) { }

    public Tokenizers(Dictionary<string, ITokenizer> container) : base(container) { }

    public void Add(string name, ITokenizer analyzer) => BackingDictionary.Add(name, analyzer);
}

public class TokenizersDescriptor : IsADictionaryDescriptorBase<TokenizersDescriptor, ITokenizers, string, ITokenizer>
{
    public TokenizersDescriptor() : base(new Tokenizers()) { }

    public TokenizersDescriptor UserDefined(string name, ITokenizer analyzer) => Assign(name, analyzer);

    /// <summary>
    /// A tokenizer of type edgeNGram.
    /// </summary>
    public TokenizersDescriptor EdgeNGram(string name, Func<EdgeNGramTokenizerDescriptor, IEdgeNGramTokenizer> selector) =>
        Assign(name, selector?.Invoke(new EdgeNGramTokenizerDescriptor()));

    /// <summary>
    /// A tokenizer of type nGram.
    /// </summary>
    public TokenizersDescriptor NGram(string name, Func<NGramTokenizerDescriptor, INGramTokenizer> selector) =>
        Assign(name, selector?.Invoke(new NGramTokenizerDescriptor()));

    /// <summary>
    /// A tokenizer of type keyword that emits the entire input as a single input.
    /// </summary>
    public TokenizersDescriptor Keyword(string name, Func<KeywordTokenizerDescriptor, IKeywordTokenizer> selector) =>
        Assign(name, selector?.Invoke(new KeywordTokenizerDescriptor()));

    /// <summary>
    /// A tokenizer of type letter that divides text at non-letters. That’s to say, it defines tokens as maximal strings of
    /// adjacent letters.
    /// <para>
    /// Note, this does a decent job for most European languages, but does a terrible job for some Asian languages, where words
    /// are not
    /// separated by spaces.
    /// </para>
    /// </summary>
    public TokenizersDescriptor Letter(string name, Func<LetterTokenizerDescriptor, ILetterTokenizer> selector) =>
        Assign(name, selector?.Invoke(new LetterTokenizerDescriptor()));

    /// <summary>
    /// A tokenizer of type lowercase that performs the function of Letter Tokenizer and Lower Case Token Filter together.
    /// <para>It divides text at non-letters and converts them to lower case. </para>
    /// <para>While it is functionally equivalent to the combination of Letter Tokenizer and Lower Case Token Filter, </para>
    /// <para>there is a performance advantage to doing the two tasks at once, hence this (redundant) implementation.</para>
    /// </summary>
    public TokenizersDescriptor Lowercase(string name, Func<LowercaseTokenizerDescriptor, ILowercaseTokenizer> selector) =>
        Assign(name, selector?.Invoke(new LowercaseTokenizerDescriptor()));

    /// <summary>
    ///  The path_hierarchy tokenizer takes something like this:
    /// <para>/something/something/else</para>
    /// <para>And produces tokens:</para>
    /// <para></para>
    /// <para>/something</para>
    /// <para>/something/something</para>
    /// <para>/something/something/else</para>
    /// </summary>
    public TokenizersDescriptor PathHierarchy(string name, Func<PathHierarchyTokenizerDescriptor, IPathHierarchyTokenizer> selector) =>
        Assign(name, selector?.Invoke(new PathHierarchyTokenizerDescriptor()));

    /// <summary>
    /// A tokenizer of type pattern that can flexibly separate text into terms via a regular expression.
    /// </summary>
    public TokenizersDescriptor Pattern(string name, Func<PatternTokenizerDescriptor, IPatternTokenizer> selector) =>
        Assign(name, selector?.Invoke(new PatternTokenizerDescriptor()));

    /// <summary>
    /// A tokenizer of type standard providing grammar based tokenizer that is a good tokenizer for most European language
    /// documents.
    /// <para>The tokenizer implements the Unicode Text Segmentation algorithm, as specified in Unicode Standard Annex #29.</para>
    /// </summary>
    public TokenizersDescriptor Standard(string name, Func<StandardTokenizerDescriptor, IStandardTokenizer> selector = null) =>
        Assign(name, selector.InvokeOrDefault(new StandardTokenizerDescriptor()));

    /// <summary>
    /// A tokenizer of type uax_url_email which works exactly like the standard tokenizer, but tokenizes emails and urls as
    /// single tokens
    /// </summary>
    public TokenizersDescriptor UaxEmailUrl(string name, Func<UaxEmailUrlTokenizerDescriptor, IUaxEmailUrlTokenizer> selector) =>
        Assign(name, selector?.Invoke(new UaxEmailUrlTokenizerDescriptor()));

    /// <summary>
    /// A tokenizer of type whitespace that divides text at whitespace.
    /// </summary>
    public TokenizersDescriptor Whitespace(string name, Func<WhitespaceTokenizerDescriptor, IWhitespaceTokenizer> selector = null) =>
        Assign(name, selector.InvokeOrDefault(new WhitespaceTokenizerDescriptor()));

    /// <summary>
    /// A tokenizer of type pattern that can flexibly separate text into terms via a regular expression.
    /// Part of the `analysis-kuromoji` plugin:
    ///
    /// </summary>
    public TokenizersDescriptor Kuromoji(string name, Func<KuromojiTokenizerDescriptor, IKuromojiTokenizer> selector) =>
        Assign(name, selector?.Invoke(new KuromojiTokenizerDescriptor()));

    /// <summary>
    /// Tokenizes text into words on word boundaries, as defined in UAX #29: Unicode Text Segmentation. It behaves much
    /// like the standard tokenizer, but adds better support for some Asian languages by using a dictionary-based approach
    /// to identify words in Thai, Lao, Chinese, Japanese, and Korean, and using custom rules to break Myanmar and Khmer
    /// text into syllables.
    /// Part of the `analysis-icu` plugin:
    /// </summary>
    public TokenizersDescriptor Icu(string name, Func<IcuTokenizerDescriptor, IIcuTokenizer> selector) =>
        Assign(name, selector?.Invoke(new IcuTokenizerDescriptor()));

    /// <inheritdoc cref="INoriTokenizer" />
    public TokenizersDescriptor Nori(string name, Func<NoriTokenizerDescriptor, INoriTokenizer> selector) =>
        Assign(name, selector?.Invoke(new NoriTokenizerDescriptor()));

    /// <inheritdoc cref="ICharGroupTokenizer.TokenizeOnCharacters" />
    /// >
    public TokenizersDescriptor CharGroup(string name, Func<CharGroupTokenizerDescriptor, ICharGroupTokenizer> selector) =>
        Assign(name, selector?.Invoke(new CharGroupTokenizerDescriptor()));
}
