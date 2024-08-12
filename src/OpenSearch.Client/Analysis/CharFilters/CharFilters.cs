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

[JsonFormatter(typeof(VerbatimDictionaryKeysFormatter<CharFilters, ICharFilters, string, ICharFilter>))]
public interface ICharFilters : IIsADictionary<string, ICharFilter> { }

public class CharFilters : IsADictionaryBase<string, ICharFilter>, ICharFilters
{
    public CharFilters() { }

    public CharFilters(IDictionary<string, ICharFilter> container) : base(container) { }

    public CharFilters(Dictionary<string, ICharFilter> container) : base(container) { }

    public void Add(string name, ICharFilter analyzer) => BackingDictionary.Add(name, analyzer);
}

public class CharFiltersDescriptor : IsADictionaryDescriptorBase<CharFiltersDescriptor, ICharFilters, string, ICharFilter>
{
    public CharFiltersDescriptor() : base(new CharFilters()) { }

    public CharFiltersDescriptor UserDefined(string name, ICharFilter analyzer) => Assign(name, analyzer);

    /// <summary>
    /// The pattern_replace char filter allows the use of a regex to manipulate the characters in a string before analysis.
    /// </summary>
    public CharFiltersDescriptor PatternReplace(string name, Func<PatternReplaceCharFilterDescriptor, IPatternReplaceCharFilter> selector) =>
        Assign(name, selector?.Invoke(new PatternReplaceCharFilterDescriptor()));

    /// <summary>
    /// A char filter of type html_strip stripping out HTML elements from an analyzed text.
    /// </summary>
    public CharFiltersDescriptor HtmlStrip(string name, Func<HtmlStripCharFilterDescriptor, IHtmlStripCharFilter> selector = null) =>
        Assign(name, selector.InvokeOrDefault(new HtmlStripCharFilterDescriptor()));

    /// <summary>
    /// A char filter of type mapping replacing characters of an analyzed text with given mapping.
    /// </summary>
    public CharFiltersDescriptor Mapping(string name, Func<MappingCharFilterDescriptor, IMappingCharFilter> selector) =>
        Assign(name, selector?.Invoke(new MappingCharFilterDescriptor()));

    /// <summary>
    /// The kuromoji_iteration_mark normalizes Japanese horizontal iteration marks (odoriji) to their expanded form.
    /// Part of the `analysis-kuromoji` plugin:
    ///
    /// </summary>
    public CharFiltersDescriptor KuromojiIterationMark(string name,
        Func<KuromojiIterationMarkCharFilterDescriptor, IKuromojiIterationMarkCharFilter> selector = null
    ) =>
        Assign(name, selector?.InvokeOrDefault(new KuromojiIterationMarkCharFilterDescriptor()));

    /// <summary>
    /// Normalizes as defined here: http://userguide.icu-project.org/transforms/normalization
    /// Part of the `analysis-icu` plugin:
    /// </summary>
    public CharFiltersDescriptor IcuNormalization(string name, Func<IcuNormalizationCharFilterDescriptor, IIcuNormalizationCharFilter> selector
    ) =>
        Assign(name, selector?.Invoke(new IcuNormalizationCharFilterDescriptor()));
}
