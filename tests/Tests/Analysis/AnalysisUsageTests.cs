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
using System.Linq;
using System.Reflection;
using FluentAssertions;
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using Tests.Analysis.Analyzers;
using Tests.Analysis.CharFilters;
using Tests.Analysis.Normalizers;
using Tests.Analysis.TokenFilters;
using Tests.Analysis.Tokenizers;
using Tests.Core.Client;

namespace Tests.Analysis;

public class AnalysisUsageTestsTests
{
    [U]
    public static void CollectionsShouldNotBeEmpty()
    {
        var analyzers = AnalysisUsageTests.AnalyzersInitializer.Analysis.Analyzers;
        var charFilters = AnalysisUsageTests.CharFiltersInitializer.Analysis.CharFilters;
        var tokenizers = AnalysisUsageTests.TokenizersInitializer.Analysis.Tokenizers;
        var tokenFilters = AnalysisUsageTests.TokenFiltersInitializer.Analysis.TokenFilters;

        analyzers.Should().NotBeNull().And.NotBeEmpty();
        charFilters.Should().NotBeNull().And.NotBeEmpty();
        tokenizers.Should().NotBeNull().And.NotBeEmpty();
        tokenFilters.Should().NotBeNull().And.NotBeEmpty();
    }
}

public static class AnalysisUsageTests
{
    public static IndexSettings AnalyzersFluent =>
        Fluent<AnalyzersDescriptor, IAnalyzerAssertion, IAnalyzers>(i => i.Fluent, (a, v) => a.Analyzers = v.Value);

    public static IndexSettings AnalyzersInitializer =>
        Init<OpenSearch.Client.Analyzers, IAnalyzerAssertion, IAnalyzer>(i => i.Initializer, (a, v) => a.Analyzers = v);

    public static IndexSettings CharFiltersFluent =>
        Fluent<CharFiltersDescriptor, ICharFilterAssertion, ICharFilters>(i => i.Fluent, (a, v) => a.CharFilters = v.Value);

    public static IndexSettings CharFiltersInitializer =>
        Init<OpenSearch.Client.CharFilters, ICharFilterAssertion, ICharFilter>(i => i.Initializer, (a, v) => a.CharFilters = v);

    public static IndexSettings NormalizersFluent =>
        Fluent<NormalizersDescriptor, INormalizerAssertion, INormalizers>(i => i.Fluent, (a, v) => a.Normalizers = v.Value);

    public static IndexSettings NormalizersInitializer =>
        Init<OpenSearch.Client.Normalizers, INormalizerAssertion, INormalizer>(i => i.Initializer, (a, v) => a.Normalizers = v);

    public static IndexSettings TokenFiltersFluent =>
        Fluent<TokenFiltersDescriptor, ITokenFilterAssertion, ITokenFilters>(i => i.Fluent, (a, v) => a.TokenFilters = v.Value);

    public static IndexSettings TokenFiltersInitializer =>
        Init<OpenSearch.Client.TokenFilters, ITokenFilterAssertion, ITokenFilter>(i => i.Initializer, (a, v) => a.TokenFilters = v);

    public static IndexSettings TokenizersFluent =>
        Fluent<TokenizersDescriptor, ITokenizerAssertion, ITokenizers>(i => i.Fluent, (a, v) => a.Tokenizers = v.Value);

    public static IndexSettings TokenizersInitializer =>
        Init<OpenSearch.Client.Tokenizers, ITokenizerAssertion, ITokenizer>(i => i.Initializer, (a, v) => a.Tokenizers = v);

    private static IndexSettings Fluent<TContainer, TAssertion, TValue>(Func<TAssertion, Func<string, TContainer, IPromise<TValue>>> fluent,
        Action<OpenSearch.Client.Analysis, IPromise<TValue>> set
    )
        where TAssertion : IAnalysisAssertion
        where TContainer : IPromise<TValue>, new()
        where TValue : class => Wrap(an => set(an, Apply<TContainer, TAssertion>((t, a) => fluent(a)(a.Name, t))));

    private static IndexSettings Init<TContainer, TAssertion, TInitializer>(Func<TAssertion, TInitializer> value,
        Action<OpenSearch.Client.Analysis, TContainer> set
    )
        where TAssertion : IAnalysisAssertion
        where TContainer : IDictionary<string, TInitializer>, new() =>
        Wrap(an => set(an, Apply<TContainer, TAssertion>((t, a) => t[a.Name] = value(a))));

    private static TContainer Apply<TContainer, TAssertion>(Action<TContainer, TAssertion> act)
        where TAssertion : IAnalysisAssertion
        where TContainer : new() => All<TAssertion>()
        .Aggregate(new TContainer(), (t, a) =>
        {
            act(t, a);
            return t;
        }, t => t);

    private static IndexSettings Wrap(Action<OpenSearch.Client.Analysis> set)
    {
        var a = new OpenSearch.Client.Analysis();
        var s = new IndexSettings { Analysis = a };
        set(a);
        return s;
    }

    private static List<TAssertion> All<TAssertion>()
        where TAssertion : IAnalysisAssertion
    {
        var assertions = typeof(TokenizerTests).GetNestedTypes()
            .Union(typeof(TokenFilterTests).GetNestedTypes())
            .Union(typeof(NormalizerTests).GetNestedTypes())
            .Union(typeof(AnalyzerTests).GetNestedTypes())
            .Union(typeof(CharFilterTests).GetNestedTypes())
            .ToList();

        var nestedTypes = assertions
            .Where(t => typeof(TAssertion).IsAssignableFrom(t) && t.IsClass)
            .ToList();

        var types = nestedTypes
            .Select(t => new
            {
                t,
                a = t.GetCustomAttributes(typeof(SkipVersionAttribute)).FirstOrDefault() as SkipVersionAttribute
            })
            .Where(@t1 => @t1.a == null || !@t1.a.Ranges.Any(r => r.IsSatisfied(TestClient.Configuration.OpenSearchVersion)))
            .Select(@t1 => (TAssertion)Activator.CreateInstance(@t1.t));
        return types.ToList();
    }
}
