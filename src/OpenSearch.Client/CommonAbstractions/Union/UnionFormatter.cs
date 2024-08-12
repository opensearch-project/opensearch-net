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
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

internal class UnionFormatter<TFirst, TSecond> : IJsonFormatter<Union<TFirst, TSecond>>
{
    private readonly bool _attemptTSecondIfTFirstIsNull;

    public UnionFormatter() => _attemptTSecondIfTFirstIsNull = false;

    public UnionFormatter(bool attemptTSecondIfTFirstIsNull) => _attemptTSecondIfTFirstIsNull = attemptTSecondIfTFirstIsNull;

    public Union<TFirst, TSecond> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var segment = reader.ReadNextBlockSegment();
        if (TryRead(ref segment, formatterResolver, out TFirst first))
        {
            if (first == null && _attemptTSecondIfTFirstIsNull)
            {
                if (TryRead(ref segment, formatterResolver, out TSecond second))
                    return second;
            }
            else
            {
                return first;
            }
        }
        else if (TryRead(ref segment, formatterResolver, out TSecond second))
            return second;

        return null;
    }

    public void Serialize(ref JsonWriter writer, Union<TFirst, TSecond> value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        switch (value.Tag)
        {
            case 0:
                {
                    var formatter = formatterResolver.GetFormatter<TFirst>();
                    formatter.Serialize(ref writer, value.Item1, formatterResolver);
                    break;
                }
            case 1:
                {
                    var formatter = formatterResolver.GetFormatter<TSecond>();
                    formatter.Serialize(ref writer, value.Item2, formatterResolver);
                    break;
                }
            default:
                throw new Exception($"Unrecognized tag value: {value.Tag}");
        }
    }

    public bool TryRead<T>(ref ArraySegment<byte> segment, IJsonFormatterResolver formatterResolver, out T v)
    {
        var segmentReader = new JsonReader(segment.Array, segment.Offset);
        try
        {
            var formatter = formatterResolver.GetFormatter<T>();
            v = formatter.Deserialize(ref segmentReader, formatterResolver);
            return true;
        }
        catch
        {
            v = default;
            return false;
        }
    }
}
