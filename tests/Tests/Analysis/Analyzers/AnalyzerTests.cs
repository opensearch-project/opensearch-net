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
using OpenSearch.Client;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;

namespace Tests.Analysis.Analyzers;

using FuncTokenizer = Func<string, AnalyzersDescriptor, IPromise<IAnalyzers>>;

public class AnalyzerTests
{
    public class KeywordTests : AnalyzerAssertionBase<KeywordTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an.Keyword("myKeyword");

        public override IAnalyzer Initializer =>
            new KeywordAnalyzer();

        public override object Json => new
        {
            type = "keyword"
        };

        public override string Name => "myKeyword";
    }

    public class CustomTests : AnalyzerAssertionBase<CustomTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an
            .Custom("myCustom", a => a
                .Filters("lowercase", "asciifolding")
                .CharFilters("html_strip")
                .Tokenizer("standard")
            );

        public override IAnalyzer Initializer => new CustomAnalyzer
        {
            CharFilter = new[] { "html_strip" },
            Tokenizer = "standard",
            Filter = new[] { "lowercase", "asciifolding" }
        };

        public override object Json => new
        {
            type = "custom",
            tokenizer = "standard",
            filter = new[] { "lowercase", "asciifolding" },
            char_filter = new[] { "html_strip" }
        };

        public override string Name => "myCustom";
    }

    public class PatternTests : AnalyzerAssertionBase<PatternTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an.Pattern(n, a => a.Pattern(@"\w"));

        public override IAnalyzer Initializer => new PatternAnalyzer { Pattern = @"\w" };

        public override object Json => new { type = "pattern", pattern = "\\w" };
        public override string Name => "myPattern ";
    }

    public class SimpleTests : AnalyzerAssertionBase<SimpleTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an.Simple("mySimple");

        public override IAnalyzer Initializer => new SimpleAnalyzer();
        public override object Json => new { type = "simple" };
        public override string Name => "mySimple";
    }

    public class LanguageTests : AnalyzerAssertionBase<LanguageTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an
            .Language("myLanguage", a => a.Language(Language.Dutch));

        public override IAnalyzer Initializer => new LanguageAnalyzer { Language = Language.Dutch };

        public override object Json => new { type = "dutch" };
        public override string Name => "myLanguage";
    }

    public class SnowballTests : AnalyzerAssertionBase<SnowballTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an
            .Snowball("mySnow", a => a.Language(SnowballLanguage.Dutch));

        public override IAnalyzer Initializer => new SnowballAnalyzer { Language = SnowballLanguage.Dutch };

        public override object Json => new
        {
            type = "snowball",
            language = "Dutch"
        };

        public override string Name => "mySnow";
    }

    public class StandardTests : AnalyzerAssertionBase<StandardTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an
            .Standard("myStandard", a => a.MaxTokenLength(2));

        public override IAnalyzer Initializer => new StandardAnalyzer { MaxTokenLength = 2 };

        public override object Json => new
        {
            type = "standard",
            max_token_length = 2
        };

        public override string Name => "myStandard";
    }

    public class StopTests : AnalyzerAssertionBase<StopTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an
            .Stop("myStop", a => a.StopwordsPath("analysis/stopwords.txt"));

        public override IAnalyzer Initializer => new StopAnalyzer { StopwordsPath = "analysis/stopwords.txt" };

        public override object Json => new
        {
            type = "stop",
            stopwords_path = "analysis/stopwords.txt"
        };

        public override string Name => "myStop";
    }

    public class WhitespaceTests : AnalyzerAssertionBase<WhitespaceTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an.Whitespace(n);

        public override IAnalyzer Initializer => new WhitespaceAnalyzer();
        public override object Json => new { type = "whitespace" };
        public override string Name => "myWhiteSpace";
    }

    public class FingerprintTests : AnalyzerAssertionBase<FingerprintTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an
            .Fingerprint("myFingerprint", a => a
                .PreserveOriginal()
                .Separator(",")
                .MaxOutputSize(100)
                .StopWords("a", "he", "the")
            );

        public override IAnalyzer Initializer =>
            new FingerprintAnalyzer
            {
                PreserveOriginal = true,
                Separator = ",",
                MaxOutputSize = 100,
                StopWords = new[] { "a", "he", "the" }
            };

        public override object Json => new
        {
            type = "fingerprint",
            preserve_original = true,
            separator = ",",
            max_output_size = 100,
            stopwords = new[] { "a", "he", "the" }
        };

        public override string Name => "myFingerprint";
    }


    public class KuromojiTests : AnalyzerAssertionBase<KuromojiTests>
    {
        public override FuncTokenizer Fluent => (n, an) => an
            .Kuromoji("kuro", a => a
                .Mode(KuromojiTokenizationMode.Search)
            );

        public override IAnalyzer Initializer =>
            new KuromojiAnalyzer
            {
                Mode = KuromojiTokenizationMode.Search
            };

        public override object Json => new
        {
            type = "kuromoji",
            mode = "search"
        };

        public override string Name => "kuro";
    }

    public class NoriTests : AnalyzerAssertionBase<NoriTests>
    {
        private readonly string[] _stopTags = { "NR", "SP" };

        public override FuncTokenizer Fluent => (n, t) => t.Nori(n, e => e
            .StopTags(_stopTags)
            .DecompoundMode(NoriDecompoundMode.Mixed)
        );

        public override IAnalyzer Initializer => new NoriAnalyzer
        {
            StopTags = _stopTags,
            DecompoundMode = NoriDecompoundMode.Mixed
        };

        public override object Json => new
        {
            type = "nori",
            decompound_mode = "mixed",
            stoptags = _stopTags
        };

        public override string Name => "nori";
    }

    public class IcuTests : AnalyzerAssertionBase<IcuTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.Icu(n, e => e
            .Method(IcuNormalizationType.Canonical)
            .Mode(IcuNormalizationMode.Decompose)
        );

        public override IAnalyzer Initializer => new IcuAnalyzer
        {
            Method = IcuNormalizationType.Canonical,
            Mode = IcuNormalizationMode.Decompose
        };

        public override object Json => new
        {
            type = "icu_analyzer",
            method = "nfc",
            mode = "decompose"
        };

        public override string Name => "icu_analyzer";
    }
}
