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

[JsonFormatter(typeof(UnionListFormatter<AnalyzeCharFilters, string, ICharFilter>))]
public class AnalyzeCharFilters : List<Union<string, ICharFilter>>
{
    public AnalyzeCharFilters() { }

    public AnalyzeCharFilters(List<Union<string, ICharFilter>> tokenFilters)
    {
        if (tokenFilters == null) return;

        foreach (var v in tokenFilters) this.AddIfNotNull(v);
    }

    public AnalyzeCharFilters(string[] tokenFilters)
    {
        if (tokenFilters == null) return;

        foreach (var v in tokenFilters) this.AddIfNotNull(v);
    }

    public void Add(ICharFilter filter) => Add(new Union<string, ICharFilter>(filter));

    public void Add(string filter) => Add(new Union<string, ICharFilter>(filter));

    public static implicit operator AnalyzeCharFilters(CharFilterBase tokenFilter) =>
        tokenFilter == null ? null : new AnalyzeCharFilters { tokenFilter };

    public static implicit operator AnalyzeCharFilters(string tokenFilter) => tokenFilter == null ? null : new AnalyzeCharFilters { tokenFilter };

    public static implicit operator AnalyzeCharFilters(string[] tokenFilters) =>
        tokenFilters == null ? null : new AnalyzeCharFilters(tokenFilters);
}

public class AnalyzeCharFiltersDescriptor : DescriptorPromiseBase<AnalyzeCharFiltersDescriptor, AnalyzeCharFilters>
{
    public AnalyzeCharFiltersDescriptor() : base(new AnalyzeCharFilters()) { }

    /// <summary>
    /// A reference to a token filter that is part of the mapping
    /// </summary>
    public AnalyzeCharFiltersDescriptor Name(string tokenFilter) => Assign(tokenFilter, (a, v) => a.AddIfNotNull(v));

    private AnalyzeCharFiltersDescriptor AssignIfNotNull(ICharFilter filter) =>
        Assign(filter, (a, v) => { if (v != null) a.Add(v); });

    /// <summary>
    /// The pattern_replace char filter allows the use of a regex to manipulate the characters in a string before analysis.
    /// </summary>
    public AnalyzeCharFiltersDescriptor PatternReplace(Func<PatternReplaceCharFilterDescriptor, IPatternReplaceCharFilter> selector) =>
        AssignIfNotNull(selector?.Invoke(new PatternReplaceCharFilterDescriptor()));

    /// <summary>
    /// A char filter of type html_strip stripping out HTML elements from an analyzed text.
    /// </summary>
    public AnalyzeCharFiltersDescriptor HtmlStrip(Func<HtmlStripCharFilterDescriptor, IHtmlStripCharFilter> selector = null) =>
        AssignIfNotNull(selector.InvokeOrDefault(new HtmlStripCharFilterDescriptor()));

    /// <summary>
    /// A char filter of type mapping replacing characters of an analyzed text with given mapping.
    /// </summary>
    public AnalyzeCharFiltersDescriptor Mapping(Func<MappingCharFilterDescriptor, IMappingCharFilter> selector) =>
        AssignIfNotNull(selector?.Invoke(new MappingCharFilterDescriptor()));

    /// <summary>
    /// The kuromoji_iteration_mark normalizes Japanese horizontal iteration marks (odoriji) to their expanded form.
    /// Part of the `analysis-kuromoji` plugin:
    ///
    /// </summary>
    public AnalyzeCharFiltersDescriptor KuromojiIterationMark(
        Func<KuromojiIterationMarkCharFilterDescriptor, IKuromojiIterationMarkCharFilter> selector = null
    ) =>
        AssignIfNotNull(selector?.InvokeOrDefault(new KuromojiIterationMarkCharFilterDescriptor()));

    /// <summary>
    /// Normalizes as defined here: http://userguide.icu-project.org/transforms/normalization
    /// Part of the `analysis-icu` plugin:
    /// </summary>
    public AnalyzeCharFiltersDescriptor IcuNormalization(Func<IcuNormalizationCharFilterDescriptor, IIcuNormalizationCharFilter> selector) =>
        AssignIfNotNull(selector?.Invoke(new IcuNormalizationCharFilterDescriptor()));
}
