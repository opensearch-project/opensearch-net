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
using OpenSearch.Net.Utf8Json.Internal;
using OpenSearch.Net.Utf8Json.Resolvers;


namespace OpenSearch.Client;

internal class SourceFilterFormatter : IJsonFormatter<ISourceFilter>
{
    private static readonly AutomataDictionary Fields = new AutomataDictionary
    {
        { "includes", 0 },
        { "excludes", 1 }
    };

    public ISourceFilter Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();
        if (token == JsonToken.Null)
        {
            reader.ReadNext();
            return null;
        }

        var filter = new SourceFilter();
        switch (token)
        {
            case JsonToken.String:
                filter.Includes = new[] { reader.ReadString() };
                break;
            case JsonToken.BeginArray:
                var include = formatterResolver.GetFormatter<string[]>()
                    .Deserialize(ref reader, formatterResolver);
                filter.Includes = include;
                break;
            default:
                var count = 0;
                while (reader.ReadIsInObject(ref count))
                {
                    var propertyName = reader.ReadPropertyNameSegmentRaw();
                    if (Fields.TryGetValue(propertyName, out var value))
                    {
                        var includeExclude = formatterResolver.GetFormatter<Fields>()
                            .Deserialize(ref reader, formatterResolver);

                        switch (value)
                        {
                            case 0:
                                filter.Includes = includeExclude;
                                break;
                            case 1:
                                filter.Excludes = includeExclude;
                                break;
                        }
                    }
                    else
                        reader.ReadNextBlock();
                }
                break;
        }

        return filter;
    }

    public void Serialize(ref JsonWriter writer, ISourceFilter value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        DynamicObjectResolver.ExcludeNullCamelCase.GetFormatter<ISourceFilter>()
            .Serialize(ref writer, value, formatterResolver);
    }
}
