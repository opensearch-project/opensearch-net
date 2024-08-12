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

using OpenSearch.Net.Extensions;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Internal;


namespace OpenSearch.Client;

internal class FuzzinessInterfaceFormatter : IJsonFormatter<IFuzziness>
{
    public void Serialize(ref JsonWriter writer, IFuzziness value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        if (value.Auto)
        {
            if (!value.Low.HasValue || !value.High.HasValue)
                writer.WriteString("AUTO");
            else
                writer.WriteString($"AUTO:{value.Low},{value.High}");
        }
        else if (value.EditDistance.HasValue)
            writer.WriteInt32(value.EditDistance.Value);
        else if (value.Ratio.HasValue)
            writer.WriteDouble(value.Ratio.Value);
        else
            writer.WriteNull();
    }

    public IFuzziness Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var formatter = formatterResolver.GetFormatter<Fuzziness>();
        return formatter.Deserialize(ref reader, formatterResolver);
    }
}

internal class FuzzinessFormatter : IJsonFormatter<Fuzziness>
{
    private static readonly byte[] AutoBytes = JsonWriter.GetEncodedPropertyNameWithoutQuotation("AUTO");

    public Fuzziness Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();

        switch (token)
        {
            case JsonToken.String:
                {
                    var rawAuto = reader.ReadStringSegmentUnsafe();
                    if (rawAuto.EqualsBytes(AutoBytes))
                        return Fuzziness.Auto;

                    var colonIndex = -1;
                    var commaIndex = -1;
                    for (var i = AutoBytes.Length; i < rawAuto.Count; i++)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        if (rawAuto.Array[rawAuto.Offset + i] == (byte)':')
                            colonIndex = rawAuto.Offset + i;
                        else if (rawAuto.Array[rawAuto.Offset + i] == (byte)',')
                        {
                            commaIndex = rawAuto.Offset + i;
                            break;
                        }
                    }

                    var low = NumberConverter.ReadInt32(rawAuto.Array, colonIndex + 1, out _);
                    var high = NumberConverter.ReadInt32(rawAuto.Array, commaIndex + 1, out _);
                    return Fuzziness.AutoLength(low, high);
                }
            case JsonToken.Number:
                {
                    var value = reader.ReadNumberSegment();

                    if (value.IsDouble())
                    {
                        var ratio = NumberConverter.ReadDouble(value.Array, value.Offset, out _);
                        return Fuzziness.Ratio(ratio);
                    }
                    else
                    {
                        var editDistance = NumberConverter.ReadInt32(value.Array, value.Offset, out _);
                        return Fuzziness.EditDistance(editDistance);
                    }
                }
            default:
                reader.ReadNextBlock();
                return null;
        }
    }

    public void Serialize(ref JsonWriter writer, Fuzziness value, IJsonFormatterResolver formatterResolver)
    {
        var formatter = formatterResolver.GetFormatter<IFuzziness>();
        formatter.Serialize(ref writer, value, formatterResolver);
    }
}
