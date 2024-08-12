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
using OpenSearch.Net.Extensions;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Internal;
using OpenSearch.Net.Utf8Json.Resolvers;


namespace OpenSearch.Client;

internal class TokenizerFormatter : IJsonFormatter<ITokenizer>
{
    private static byte[] TypeField = JsonWriter.GetEncodedPropertyNameWithoutQuotation("type");

    private static readonly AutomataDictionary TokenizerTypes = new AutomataDictionary
    {
        { "char_group", 0 },
        { "edgengram", 1 },
        { "edge_ngram", 1 },
        { "ngram", 2 },
        { "path_hierarchy", 3 },
        { "pattern", 4 },
        { "standard", 5 },
        { "uax_url_email", 6 },
        { "whitespace", 7 },
        { "kuromoji_tokenizer", 8 },
        { "icu_tokenizer", 9 },
        { "nori_tokenizer", 10 },
    };

    public ITokenizer Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var arraySegment = reader.ReadNextBlockSegment();
        var segmentReader = new JsonReader(arraySegment.Array, arraySegment.Offset);
        var count = 0;
        ArraySegment<byte> tokenizerType = default;
        while (segmentReader.ReadIsInObject(ref count))
        {
            var propertyName = segmentReader.ReadPropertyNameSegmentRaw();
            if (propertyName.EqualsBytes(TypeField))
            {
                tokenizerType = segmentReader.ReadStringSegmentUnsafe();
                break;
            }

            segmentReader.ReadNextBlock();
        }

        if (tokenizerType == default)
            return null;

        segmentReader = new JsonReader(arraySegment.Array, arraySegment.Offset);
        if (TokenizerTypes.TryGetValue(tokenizerType, out var value))
        {
            switch (value)
            {
                case 0: return Deserialize<CharGroupTokenizer>(ref segmentReader, formatterResolver);
                case 1: return Deserialize<EdgeNGramTokenizer>(ref segmentReader, formatterResolver);
                case 2: return Deserialize<NGramTokenizer>(ref segmentReader, formatterResolver);
                case 3: return Deserialize<PathHierarchyTokenizer>(ref segmentReader, formatterResolver);
                case 4: return Deserialize<PatternTokenizer>(ref segmentReader, formatterResolver);
                case 5: return Deserialize<StandardTokenizer>(ref segmentReader, formatterResolver);
                case 6: return Deserialize<UaxEmailUrlTokenizer>(ref segmentReader, formatterResolver);
                case 7: return Deserialize<WhitespaceTokenizer>(ref segmentReader, formatterResolver);
                case 8: return Deserialize<KuromojiTokenizer>(ref segmentReader, formatterResolver);
                case 9: return Deserialize<IcuTokenizer>(ref segmentReader, formatterResolver);
                case 10: return Deserialize<NoriTokenizer>(ref segmentReader, formatterResolver);
                default: return null;
            }
        }

        return null;
    }

    public void Serialize(ref JsonWriter writer, ITokenizer value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        switch (value.Type)
        {
            case "char_group":
                Serialize<ICharGroupTokenizer>(ref writer, value, formatterResolver);
                break;
            case "edge_ngram":
                Serialize<IEdgeNGramTokenizer>(ref writer, value, formatterResolver);
                break;
            case "ngram":
                Serialize<INGramTokenizer>(ref writer, value, formatterResolver);
                break;
            case "path_hierarchy":
                Serialize<IPathHierarchyTokenizer>(ref writer, value, formatterResolver);
                break;
            case "pattern":
                Serialize<IPatternTokenizer>(ref writer, value, formatterResolver);
                break;
            case "standard":
                Serialize<IStandardTokenizer>(ref writer, value, formatterResolver);
                break;
            case "uax_url_email":
                Serialize<IUaxEmailUrlTokenizer>(ref writer, value, formatterResolver);
                break;
            case "whitespace":
                Serialize<IWhitespaceTokenizer>(ref writer, value, formatterResolver);
                break;
            case "kuromoji_tokenizer":
                Serialize<IKuromojiTokenizer>(ref writer, value, formatterResolver);
                break;
            case "icu_tokenizer":
                Serialize<IIcuTokenizer>(ref writer, value, formatterResolver);
                break;
            case "nori_tokenizer":
                Serialize<INoriTokenizer>(ref writer, value, formatterResolver);
                break;
            default:
                // serialize user defined tokenizer
                var formatter = formatterResolver.GetFormatter<object>();
                formatter.Serialize(ref writer, value, formatterResolver);
                break;
        }
    }

    private static TTokenizer Deserialize<TTokenizer>(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        where TTokenizer : ITokenizer
    {
        var formatter = formatterResolver.GetFormatter<TTokenizer>();
        return formatter.Deserialize(ref reader, formatterResolver);
    }

    private static void Serialize<TTokenizer>(ref JsonWriter writer, ITokenizer value, IJsonFormatterResolver formatterResolver)
        where TTokenizer : class, ITokenizer
    {
        var formatter = formatterResolver.GetFormatter<TTokenizer>();
        formatter.Serialize(ref writer, value as TTokenizer, formatterResolver);
    }
}
