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

internal class AggregateDictionaryFormatter : IJsonFormatter<AggregateDictionary>
{
    private static readonly AggregateFormatter Formatter = new AggregateFormatter();

    public AggregateDictionary Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var dictionary = new Dictionary<string, IAggregate>();
        if (reader.GetCurrentJsonToken() != JsonToken.BeginObject)
        {
            reader.ReadNextBlock();
            return new AggregateDictionary(dictionary);
        }

        var count = 0;
        while (reader.ReadIsInObject(ref count))
        {
            var typedProperty = reader.ReadPropertyName();

            if (typedProperty.IsNullOrEmpty())
            {
                reader.ReadNextBlock();
                continue;
            }

            var tokens = AggregateDictionary.TypedKeyTokens(typedProperty);
            if (tokens.Length == 1)
                ParseAggregate(ref reader, formatterResolver, tokens[0], dictionary);
            else
                ReadAggregate(ref reader, formatterResolver, tokens, dictionary);
        }

        return new AggregateDictionary(dictionary);
    }

    public void Serialize(ref JsonWriter writer, AggregateDictionary value, IJsonFormatterResolver formatterResolver) =>
        throw new NotSupportedException();

    private static void ReadAggregate(ref JsonReader reader, IJsonFormatterResolver formatterResolver, string[] tokens,
        Dictionary<string, IAggregate> dictionary
    )
    {
        var name = tokens[1];
        var type = tokens[0];
        switch (type)
        {
            case "geo_centroid":
                ReadAggregate<GeoCentroidAggregate>(ref reader, formatterResolver, name, dictionary);
                break;
            case "geo_line":
                ReadAggregate<GeoLineAggregate>(ref reader, formatterResolver, name, dictionary);
                break;
            default:
                //still fall back to heuristics based parsed in case we do not know the key
                ParseAggregate(ref reader, formatterResolver, name, dictionary);
                break;
        }
    }

    private static void ReadAggregate<TAggregate>(ref JsonReader reader, IJsonFormatterResolver formatterResolver,
        string name,
        Dictionary<string, IAggregate> dictionary
    )
        where TAggregate : IAggregate
    {
        var aggregate = formatterResolver.GetFormatter<TAggregate>().Deserialize(ref reader, formatterResolver);
        dictionary.Add(name, aggregate);
    }

    private static void ParseAggregate(ref JsonReader reader, IJsonFormatterResolver formatterResolver, string name,
        Dictionary<string, IAggregate> dictionary
    )
    {
        var aggregate = Formatter.Deserialize(ref reader, formatterResolver);
        dictionary.Add(name, aggregate);
    }
}
