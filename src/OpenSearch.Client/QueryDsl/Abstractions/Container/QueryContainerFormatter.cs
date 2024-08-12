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

using System.Collections.Generic;
using System.Text;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Resolvers;


namespace OpenSearch.Client;

internal class QueryContainerFormatter : IJsonFormatter<QueryContainer>
{
    private static readonly IJsonFormatter<QueryContainer> QueryFormatter =
        DynamicObjectResolver.AllowPrivateExcludeNullCamelCase.GetFormatter<QueryContainer>();

    public QueryContainer Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        switch (reader.GetCurrentJsonToken())
        {
            case JsonToken.BeginObject:
                return QueryFormatter.Deserialize(ref reader, formatterResolver);
            case JsonToken.String:
                var jsonString = reader.ReadStringSegmentUnsafe();
                var jsonStringReader = new JsonReader(jsonString.Array, jsonString.Offset);
                return QueryFormatter.Deserialize(ref jsonStringReader, formatterResolver);
            default:
                reader.ReadNextBlock();
                return null;
        }
    }

    public void Serialize(ref JsonWriter writer, QueryContainer value, IJsonFormatterResolver formatterResolver)
    {
        var queryFormatter = formatterResolver.GetFormatter<IQueryContainer>();
        queryFormatter.Serialize(ref writer, value, formatterResolver);
    }
}

internal class QueryContainerInterfaceFormatter : IJsonFormatter<IQueryContainer>
{
    public IQueryContainer Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var queryFormatter = formatterResolver.GetFormatter<QueryContainer>();
        return queryFormatter.Deserialize(ref reader, formatterResolver);
    }

    public void Serialize(ref JsonWriter writer, IQueryContainer value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var rawQuery = value.RawQuery;
        if ((!rawQuery?.Raw.IsNullOrEmpty() ?? false) && rawQuery.IsWritable)
        {
            writer.WriteRaw(Encoding.UTF8.GetBytes(rawQuery.Raw));
            return;
        }

        var queryFormatter = DynamicObjectResolver.ExcludeNullCamelCase.GetFormatter<IQueryContainer>();
        queryFormatter.Serialize(ref writer, value, formatterResolver);
    }
}

internal class QueryContainerCollectionFormatter : IJsonFormatter<IEnumerable<QueryContainer>>
{
    private static readonly QueryContainerFormatter QueryContainerFormatter =
        new QueryContainerFormatter();

    private static readonly QueryContainerInterfaceFormatter QueryContainerInterfaceFormatter =
        new QueryContainerInterfaceFormatter();

    public IEnumerable<QueryContainer> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();
        switch (token)
        {
            case JsonToken.BeginArray:
                {
                    var count = 0;
                    var queryContainers = new List<QueryContainer>();
                    while (reader.ReadIsInArray(ref count))
                        queryContainers.Add(QueryContainerFormatter.Deserialize(ref reader, formatterResolver));

                    return queryContainers;
                }
            case JsonToken.BeginObject:
                {
                    var queryContainers = new List<QueryContainer>
                {
                    QueryContainerFormatter.Deserialize(ref reader, formatterResolver)
                };

                    return queryContainers;
                }
            default:
                reader.ReadNextBlock();
                return null;
        }
    }

    public void Serialize(ref JsonWriter writer, IEnumerable<QueryContainer> value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
            writer.WriteNull();
        else
        {
            writer.WriteBeginArray();
            var e = value.GetEnumerator();
            try
            {
                var written = false;
                while (e.MoveNext())
                {
                    if (e.Current != null && e.Current.IsWritable)
                    {
                        if (written)
                            writer.WriteValueSeparator();

                        QueryContainerInterfaceFormatter.Serialize(ref writer, e.Current, formatterResolver);
                        written = true;
                    }
                }
            }
            finally
            {
                e.Dispose();
            }

            writer.WriteEndArray();
        }
    }
}
