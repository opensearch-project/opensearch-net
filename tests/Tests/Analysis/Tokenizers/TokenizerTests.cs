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

namespace Tests.Analysis.Tokenizers;

using FuncTokenizer = Func<string, TokenizersDescriptor, IPromise<ITokenizers>>;

public static class TokenizerTests
{
    public class EdgeNGramTests : TokenizerAssertionBase<EdgeNGramTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.EdgeNGram(n, e => e
            .MaxGram(2)
            .MinGram(1)
            .TokenChars(TokenChar.Digit, TokenChar.Letter)
        );

        public override ITokenizer Initializer => new EdgeNGramTokenizer
        {
            MaxGram = 2,
            MinGram = 1,
            TokenChars = new[] { TokenChar.Digit, TokenChar.Letter }
        };

        public override object Json => new
        {
            min_gram = 1,
            max_gram = 2,
            token_chars = new[] { "digit", "letter" },
            type = "edge_ngram"
        };

        public override string Name => "endgen";
    }

    public class EdgeNGramCustomTokenCharsTests : TokenizerAssertionBase<EdgeNGramCustomTokenCharsTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.EdgeNGram(n, e => e
            .MaxGram(2)
            .MinGram(1)
            .TokenChars(TokenChar.Custom)
            .CustomTokenChars("+-_")
        );

        public override ITokenizer Initializer => new EdgeNGramTokenizer
        {
            MaxGram = 2,
            MinGram = 1,
            TokenChars = new[] { TokenChar.Custom },
            CustomTokenChars = "+-_"
        };

        public override object Json => new
        {
            min_gram = 1,
            max_gram = 2,
            token_chars = new[] { "custom" },
            custom_token_chars = "+-_",
            type = "edge_ngram"
        };

        public override string Name => "endgen_custom";
    }

    public class NGramTests : TokenizerAssertionBase<NGramTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.NGram(n, e => e
            .MaxGram(2)
            .MinGram(1)
            .TokenChars(TokenChar.Digit, TokenChar.Letter)
        );

        public override ITokenizer Initializer => new NGramTokenizer
        {
            MaxGram = 2,
            MinGram = 1,
            TokenChars = new[] { TokenChar.Digit, TokenChar.Letter }
        };

        public override object Json => new
        {
            min_gram = 1,
            max_gram = 2,
            token_chars = new[] { "digit", "letter" },
            type = "ngram"
        };

        public override string Name => "ng";
    }

    public class NGramCustomTokenCharsTests : TokenizerAssertionBase<NGramCustomTokenCharsTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.NGram(n, e => e
            .MaxGram(2)
            .MinGram(1)
            .TokenChars(TokenChar.Custom)
            .CustomTokenChars("+-_")
        );

        public override ITokenizer Initializer => new NGramTokenizer
        {
            MaxGram = 2,
            MinGram = 1,
            TokenChars = new[] { TokenChar.Custom },
            CustomTokenChars = "+-_"
        };

        public override object Json => new
        {
            min_gram = 1,
            max_gram = 2,
            token_chars = new[] { "custom" },
            custom_token_chars = "+-_",
            type = "ngram"
        };

        public override string Name => "ngram_custom";
    }

    public class PathHierarchyTests : TokenizerAssertionBase<PathHierarchyTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.PathHierarchy(n, e => e
            .BufferSize(2048)
            .Delimiter('|')
            .Replacement('-')
            .Reverse()
            .Skip(1)
        );

        public override ITokenizer Initializer => new PathHierarchyTokenizer
        {
            BufferSize = 2048,
            Delimiter = '|',
            Replacement = '-',
            Reverse = true,
            Skip = 1
        };

        public override object Json => new
        {
            delimiter = "|",
            replacement = "-",
            buffer_size = 2048,
            reverse = true,
            skip = 1,
            type = "path_hierarchy"
        };

        public override string Name => "path";
    }

    public class IcuTests : TokenizerAssertionBase<IcuTests>
    {
        private const string RuleFiles = "Latn:icu-files/KeywordTokenizer.rbbi";

        public override FuncTokenizer Fluent => (n, t) => t.Icu(n, e => e
            .RuleFiles(RuleFiles)
        );

        public override ITokenizer Initializer => new IcuTokenizer
        {
            RuleFiles = RuleFiles,
        };

        public override object Json => new
        {
            rule_files = RuleFiles,
            type = "icu_tokenizer"
        };

        public override string Name => "icu";
    }

    public class KuromojiTests : TokenizerAssertionBase<KuromojiTests>
    {
        private const string Example = "/箱根山-箱根/成田空港-成田/";
        private const string Inline = "東京スカイツリー,東京 スカイツリー,トウキョウ スカイツリー,カスタム名詞";

        public override FuncTokenizer Fluent => (n, t) => t.Kuromoji(n, e => e
            .Mode(KuromojiTokenizationMode.Extended)
            .DiscardPunctuation()
            .NBestExamples(Example)
            .NBestCost(1000)
            .UserDictionaryRules(Inline)
        );

        public override ITokenizer Initializer => new KuromojiTokenizer
        {
            Mode = KuromojiTokenizationMode.Extended,
            DiscardPunctuation = true,
            NBestExamples = Example,
            NBestCost = 1000,
            UserDictionaryRules = new[] { Inline }
        };

        public override object Json => new
        {
            discard_punctuation = true,
            mode = "extended",
            nbest_cost = 1000,
            nbest_examples = Example,
            type = "kuromoji_tokenizer",
            user_dictionary_rules = new[] { Inline }
        };

        public override string Name => "kuro";
    }

    public class KuromojiDiscardCompoundTokenTests : TokenizerAssertionBase<KuromojiDiscardCompoundTokenTests>
    {
        private const string Example = "/箱根山-箱根/成田空港-成田/";
        private const string Inline = "東京スカイツリー,東京 スカイツリー,トウキョウ スカイツリー,カスタム名詞";

        public override FuncTokenizer Fluent => (n, t) => t
            .Kuromoji(n, e => e
                .Mode(KuromojiTokenizationMode.Search)
                .DiscardCompoundToken()
            );

        public override ITokenizer Initializer => new KuromojiTokenizer
        {
            Mode = KuromojiTokenizationMode.Search,
            DiscardCompoundToken = true,
        };

        public override object Json => new
        {
            discard_compound_token = true,
            mode = "search",
            type = "kuromoji_tokenizer",
        };

        public override string Name => "kuro_discard_compound_token";
    }

    public class UaxTests : TokenizerAssertionBase<UaxTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.UaxEmailUrl(n, e => e
            .MaxTokenLength(12)
        );

        public override ITokenizer Initializer => new UaxEmailUrlTokenizer { MaxTokenLength = 12 };

        public override object Json => new
        {
            max_token_length = 12,
            type = "uax_url_email"
        };

        public override string Name => "uax";
    }

    public class PatternTests : TokenizerAssertionBase<PatternTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.Pattern(n, e => e
            .Flags("CASE_INSENSITIVE")
            .Group(1)
            .Pattern(@"\W+")
        );

        public override ITokenizer Initializer => new PatternTokenizer
        {
            Flags = "CASE_INSENSITIVE",
            Group = 1,
            Pattern = @"\W+"
        };

        public override object Json => new
        {
            pattern = @"\W+",
            flags = "CASE_INSENSITIVE",
            group = 1,
            type = "pattern"
        };

        public override string Name => "pat";
    }

    public class WhitespaceTests : TokenizerAssertionBase<WhitespaceTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.Whitespace(n);
        public override ITokenizer Initializer => new WhitespaceTokenizer();

        public override object Json => new { type = "whitespace" };
        public override string Name => "ws";
    }

    public class StandardTests : TokenizerAssertionBase<StandardTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.Standard(n);
        public override ITokenizer Initializer => new StandardTokenizer();

        public override object Json => new { type = "standard" };
        public override string Name => "stan";
    }

    public class NoriTests : TokenizerAssertionBase<NoriTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.Nori(n, e => e
            .DecompoundMode(NoriDecompoundMode.Mixed)
        );

        public override ITokenizer Initializer => new NoriTokenizer
        {
            DecompoundMode = NoriDecompoundMode.Mixed
        };

        public override object Json => new { type = "nori_tokenizer", decompound_mode = "mixed" };
        public override string Name => "nori";
    }

    public class NoriWithUserDictionaryTests : TokenizerAssertionBase<NoriWithUserDictionaryTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.Nori(n, e => e
            .DecompoundMode(NoriDecompoundMode.Mixed)
            .UserDictionaryRules("c++", "C샤프", "세종", "세종시 세종 시")
        );

        public override ITokenizer Initializer => new NoriTokenizer
        {
            DecompoundMode = NoriDecompoundMode.Mixed,
            UserDictionaryRules = new[] { "c++", "C샤프", "세종", "세종시 세종 시" }
        };

        public override object Json => new
        {
            type = "nori_tokenizer",
            decompound_mode = "mixed",
            user_dictionary_rules = new[] { "c++", "C샤프", "세종", "세종시 세종 시" }
        };
        public override string Name => "nori_userdictionary";
    }

    public class CharGroupTests : TokenizerAssertionBase<CharGroupTests>
    {
        private readonly string[] _chars = { "whitespace", "-", "\n" };

        public override FuncTokenizer Fluent => (n, t) => t.CharGroup(n, e => e
            .TokenizeOnCharacters(_chars)
        );

        public override ITokenizer Initializer => new CharGroupTokenizer
        {
            TokenizeOnCharacters = _chars
        };

        public override object Json => new
        {
            tokenize_on_chars = _chars,
            type = "char_group"
        };

        public override string Name => "char_group";
    }

    public class CharGroupMaxTokenLengthTests : TokenizerAssertionBase<CharGroupMaxTokenLengthTests>
    {
        private readonly string[] _chars = { "whitespace", "-", "\n" };

        public override FuncTokenizer Fluent => (n, t) => t.CharGroup(n, e => e
            .TokenizeOnCharacters(_chars)
            .MaxTokenLength(255)
        );

        public override ITokenizer Initializer => new CharGroupTokenizer
        {
            TokenizeOnCharacters = _chars,
            MaxTokenLength = 255
        };

        public override object Json => new
        {
            tokenize_on_chars = _chars,
            type = "char_group",
            max_token_length = 255
        };

        public override string Name => "char_group_max_token_length";
    }

    public class DiscardPunctuationTests : TokenizerAssertionBase<DiscardPunctuationTests>
    {
        public override FuncTokenizer Fluent => (n, t) => t.Nori(n, e => e
            .DiscardPunctuation()
        );

        public override ITokenizer Initializer => new NoriTokenizer
        {
            DiscardPunctuation = true
        };

        public override object Json => new { type = "nori_tokenizer", discard_punctuation = true };
        public override string Name => "nori-discard";
    }
}
