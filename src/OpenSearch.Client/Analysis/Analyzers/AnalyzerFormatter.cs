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

using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

internal class AnalyzerFormatter : IJsonFormatter<IAnalyzer>
{
    public IAnalyzer Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var arraySegment = reader.ReadNextBlockSegment();
        var segmentReader = new JsonReader(arraySegment.Array, arraySegment.Offset);
        var count = 0;
        string analyzerType = null;
        var tokenizerPresent = false;
        while (segmentReader.ReadIsInObject(ref count))
        {
            var propertyName = segmentReader.ReadPropertyName();
            switch (propertyName)
            {
                case "type":
                    analyzerType = segmentReader.ReadString();
                    break;
                case "tokenizer":
                    segmentReader.ReadNext();
                    tokenizerPresent = true;
                    break;
                default:
                    segmentReader.ReadNextBlock();
                    break;
            }
        }

        segmentReader = new JsonReader(arraySegment.Array, arraySegment.Offset);

        switch (analyzerType)
        {
            case "stop":
                return Deserialize<StopAnalyzer>(ref segmentReader, formatterResolver);
            case "standard":
                return Deserialize<StandardAnalyzer>(ref segmentReader, formatterResolver);
            case "snowball":
                return Deserialize<SnowballAnalyzer>(ref segmentReader, formatterResolver);
            case "pattern":
                return Deserialize<PatternAnalyzer>(ref segmentReader, formatterResolver);
            case "keyword":
                return Deserialize<KeywordAnalyzer>(ref segmentReader, formatterResolver);
            case "whitespace":
                return Deserialize<WhitespaceAnalyzer>(ref segmentReader, formatterResolver);
            case "simple":
                return Deserialize<SimpleAnalyzer>(ref segmentReader, formatterResolver);
            case "fingerprint":
                return Deserialize<FingerprintAnalyzer>(ref segmentReader, formatterResolver);
            case "kuromoji":
                return Deserialize<KuromojiAnalyzer>(ref segmentReader, formatterResolver);
            case "nori":
                return Deserialize<NoriAnalyzer>(ref segmentReader, formatterResolver);
            case "icu_analyzer":
                return Deserialize<IcuAnalyzer>(ref segmentReader, formatterResolver);
            default:
                if (tokenizerPresent)
                    return Deserialize<CustomAnalyzer>(ref segmentReader, formatterResolver);

                return Deserialize<LanguageAnalyzer>(ref segmentReader, formatterResolver);
        }
    }

    public void Serialize(ref JsonWriter writer, IAnalyzer value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        switch (value.Type)
        {
            case "stop":
                Serialize<IStopAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "standard":
                Serialize<IStandardAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "snowball":
                Serialize<ISnowballAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "pattern":
                Serialize<IPatternAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "keyword":
                Serialize<IKeywordAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "whitespace":
                Serialize<IWhitespaceAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "simple":
                Serialize<ISimpleAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "fingerprint":
                Serialize<IFingerprintAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "kuromoji":
                Serialize<IKuromojiAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "nori":
                Serialize<INoriAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "icu_analyzer":
                Serialize<IIcuAnalyzer>(ref writer, value, formatterResolver);
                break;
            case "custom":
                Serialize<ICustomAnalyzer>(ref writer, value, formatterResolver);
                break;
            default:
                Serialize<ILanguageAnalyzer>(ref writer, value, formatterResolver);
                break;
        }
    }

    private static void Serialize<TAnalyzer>(ref JsonWriter writer, IAnalyzer value, IJsonFormatterResolver formatterResolver)
        where TAnalyzer : class, IAnalyzer
    {
        var formatter = formatterResolver.GetFormatter<TAnalyzer>();
        formatter.Serialize(ref writer, value as TAnalyzer, formatterResolver);
    }

    private static TAnalyzer Deserialize<TAnalyzer>(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        where TAnalyzer : IAnalyzer
    {
        var formatter = formatterResolver.GetFormatter<TAnalyzer>();
        return formatter.Deserialize(ref reader, formatterResolver);
    }
}
