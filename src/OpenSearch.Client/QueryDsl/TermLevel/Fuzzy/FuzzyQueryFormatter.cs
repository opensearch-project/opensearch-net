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


namespace OpenSearch.Client;

internal class FuzzyQueryFormatter : FieldNameQueryFormatter<FuzzyQuery, IFuzzyQuery>
{
    private static readonly AutomataDictionary Fields = new AutomataDictionary
    {
        { "value", 0 },
        { "fuzziness", 1 },
        { "prefix_length", 2 },
        { "max_expansions", 3 },
        { "transpositions", 4 },
        { "rewrite", 5 },
        { "_name", 6 },
        { "boost", 7 },
    };

    public override void SerializeInternal(ref JsonWriter writer, IFuzzyQuery value, IJsonFormatterResolver formatterResolver)
    {
        switch (value)
        {
            case IFuzzyStringQuery fuzzyStringQuery:
                {
                    var formatter = formatterResolver.GetFormatter<IFuzzyStringQuery>();
                    formatter.Serialize(ref writer, fuzzyStringQuery, formatterResolver);
                    break;
                }
            case IFuzzyDateQuery fuzzyDateQuery:
                {
                    var formatter = formatterResolver.GetFormatter<IFuzzyDateQuery>();
                    formatter.Serialize(ref writer, fuzzyDateQuery, formatterResolver);
                    break;
                }
            case IFuzzyNumericQuery fuzzyNumericQuery:
                {
                    var formatter = formatterResolver.GetFormatter<IFuzzyNumericQuery>();
                    formatter.Serialize(ref writer, fuzzyNumericQuery, formatterResolver);
                    break;
                }
            default:
                base.SerializeInternal(ref writer, value, formatterResolver);
                break;
        }
    }

    public override IFuzzyQuery Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.GetCurrentJsonToken() == JsonToken.Null)
        {
            reader.ReadNext();
            return null;
        }

        var count = 0;
        IFuzzyQuery query = null;
        string name = null;
        double? boost = null;
        MultiTermQueryRewrite multiTermQueryRewrite = null;
        int? prefixLength = null;
        int? maxExpansions = null;
        bool? transpositions = null;

        while (reader.ReadIsInObject(ref count))
        {
            var field = reader.ReadPropertyName();
            // ReSharper disable once TooWideLocalVariableScope
            ArraySegment<byte> fuzzinessSegment = default;
            var valueCount = 0;
            while (reader.ReadIsInObject(ref valueCount))
            {
                var property = reader.ReadPropertyNameSegmentRaw();
                if (Fields.TryGetValue(property, out var value))
                {
                    switch (value)
                    {
                        case 0:
                            {
                                var token = reader.GetCurrentJsonToken();
                                switch (token)
                                {
                                    case JsonToken.String:
                                        var valueSegment = reader.ReadStringSegmentUnsafe();
                                        if (valueSegment.IsDateTime(formatterResolver, out var dateTime))
                                            query = new FuzzyDateQuery
                                            {
                                                Field = field,
                                                Value = dateTime
                                            };
                                        else
                                            query = new FuzzyQuery
                                            {
                                                Field = field,
                                                Value = valueSegment.Utf8String()
                                            };
                                        break;
                                    case JsonToken.Number:
                                        query = new FuzzyNumericQuery
                                        {
                                            Field = field,
                                            Value = reader.ReadDouble()
                                        };
                                        break;
                                }

                                if (fuzzinessSegment != default)
                                {
                                    var fuzzinessReader = new JsonReader(fuzzinessSegment.Array, fuzzinessSegment.Offset);
                                    SetFuzziness(ref fuzzinessReader, query, formatterResolver);
                                }
                                break;
                            }
                        case 1:
                            {
                                if (query != null)
                                    SetFuzziness(ref reader, query, formatterResolver);
                                else
                                    fuzzinessSegment = reader.ReadNextBlockSegment();
                                break;
                            }
                        case 2:
                            prefixLength = reader.ReadInt32();
                            break;
                        case 3:
                            maxExpansions = reader.ReadInt32();
                            break;
                        case 4:
                            transpositions = reader.ReadBoolean();
                            break;
                        case 5:
                            var rewriteFormatter = formatterResolver.GetFormatter<MultiTermQueryRewrite>();
                            multiTermQueryRewrite = rewriteFormatter.Deserialize(ref reader, formatterResolver);
                            break;
                        case 6:
                            name = reader.ReadString();
                            break;
                        case 7:
                            boost = reader.ReadDouble();
                            break;
                    }
                }
            }
        }

        query.PrefixLength = prefixLength;
        query.MaxExpansions = maxExpansions;
        query.Transpositions = transpositions;
        query.Rewrite = multiTermQueryRewrite;
        query.Name = name;
        query.Boost = boost;
        return query;
    }

    private static void SetFuzziness(ref JsonReader reader, IFuzzyQuery query, IJsonFormatterResolver formatterResolver)
    {
        switch (query)
        {
            case FuzzyDateQuery fuzzyDateQuery:
                fuzzyDateQuery.Fuzziness = formatterResolver.GetFormatter<Time>()
                    .Deserialize(ref reader, formatterResolver);
                break;
            case FuzzyNumericQuery fuzzyNumericQuery:
                fuzzyNumericQuery.Fuzziness = reader.ReadDouble();
                break;
            case FuzzyQuery fuzzyQuery:
                fuzzyQuery.Fuzziness = formatterResolver.GetFormatter<Fuzziness>()
                    .Deserialize(ref reader, formatterResolver);
                break;
        }
    }
}
