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
using OpenSearch.Net.Extensions;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Internal;


namespace OpenSearch.Client;

internal class SimilarityFormatter : IJsonFormatter<ISimilarity>
{
    private static readonly AutomataDictionary Similarities = new AutomataDictionary
    {
        { "BM25", 0 },
        { "LMDirichlet", 1 },
        { "DFR", 2 },
        { "DFI", 3 },
        { "IB", 4 },
        { "LMJelinekMercer", 5 },
        { "scripted", 6 }
    };

    private static readonly byte[] Type = JsonWriter.GetEncodedPropertyNameWithoutQuotation("type");

    public ISimilarity Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var arraySegment = reader.ReadNextBlockSegment();
        var segmentReader = new JsonReader(arraySegment.Array, arraySegment.Offset);

        var count = 0;
        ArraySegment<byte> type = default;
        while (segmentReader.ReadIsInObject(ref count))
        {
            var propertyName = segmentReader.ReadPropertyNameSegmentRaw();

            if (propertyName.EqualsBytes(Type))
            {
                type = segmentReader.ReadStringSegmentUnsafe();
                break;
            }

            segmentReader.ReadNextBlock();
        }

        segmentReader = new JsonReader(arraySegment.Array, arraySegment.Offset);

        if (Similarities.TryGetValue(type, out var value))
        {
            switch (value)
            {
                case 0:
                    return Deserialize<BM25Similarity>(ref segmentReader, formatterResolver);
                case 1:
                    return Deserialize<LMDirichletSimilarity>(ref segmentReader, formatterResolver);
                case 2:
                    return Deserialize<DFRSimilarity>(ref segmentReader, formatterResolver);
                case 3:
                    return Deserialize<DFISimilarity>(ref segmentReader, formatterResolver);
                case 4:
                    return Deserialize<IBSimilarity>(ref segmentReader, formatterResolver);
                case 5:
                    return Deserialize<LMJelinekMercerSimilarity>(ref segmentReader, formatterResolver);
                case 6:
                    return Deserialize<ScriptedSimilarity>(ref segmentReader, formatterResolver);
            }
        }

        var formatter = formatterResolver.GetFormatter<Dictionary<string, object>>();
        var dict = formatter.Deserialize(ref segmentReader, formatterResolver);
        return new CustomSimilarity(dict);
    }

    public void Serialize(ref JsonWriter writer, ISimilarity value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        switch (value.Type)
        {
            case "BM25":
                Serialize<IBM25Similarity>(ref writer, value, formatterResolver);
                break;
            case "LMDirichlet":
                Serialize<ILMDirichletSimilarity>(ref writer, value, formatterResolver);
                break;
            case "DFR":
                Serialize<IDFRSimilarity>(ref writer, value, formatterResolver);
                break;
            case "DFI":
                Serialize<IDFISimilarity>(ref writer, value, formatterResolver);
                break;
            case "IB":
                Serialize<IIBSimilarity>(ref writer, value, formatterResolver);
                break;
            case "LMJelinekMercer":
                Serialize<ILMJelinekMercerSimilarity>(ref writer, value, formatterResolver);
                break;
            case "scripted":
                Serialize<IScriptedSimilarity>(ref writer, value, formatterResolver);
                break;
            default:
                Serialize<ICustomSimilarity>(ref writer, value, formatterResolver);
                break;
        }
    }

    private static void Serialize<TSimilarity>(ref JsonWriter writer, ISimilarity value, IJsonFormatterResolver formatterResolver)
        where TSimilarity : class, ISimilarity
    {
        var formatter = formatterResolver.GetFormatter<TSimilarity>();
        formatter.Serialize(ref writer, value as TSimilarity, formatterResolver);
    }

    private static TSimilarity Deserialize<TSimilarity>(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        where TSimilarity : ISimilarity
    {
        var formatter = formatterResolver.GetFormatter<TSimilarity>();
        return formatter.Deserialize(ref reader, formatterResolver);
    }
}
