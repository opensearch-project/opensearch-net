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
using OpenSearch.Net.Utf8Json.Internal;

namespace OpenSearch.Client;

internal class TermsIncludeFormatter : IJsonFormatter<TermsInclude>
{
    private static readonly AutomataDictionary AutomataDictionary = new AutomataDictionary
    {
        { "partition", 0 },
        { "num_partitions", 1 }
    };

    public TermsInclude Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        var token = reader.GetCurrentJsonToken();
        if (token == JsonToken.Null)
        {
            reader.ReadNext();
            return null;
        }

        TermsInclude termsInclude;
        switch (token)
        {
            case JsonToken.BeginArray:
                var formatter = formatterResolver.GetFormatter<IEnumerable<string>>();
                termsInclude = new TermsInclude(formatter.Deserialize(ref reader, formatterResolver));
                break;
            case JsonToken.BeginObject:
                long partition = 0;
                long numberOfPartitions = 0;
                var count = 0;
                while (reader.ReadIsInObject(ref count))
                {
                    var propertyName = reader.ReadPropertyNameSegmentRaw();
                    if (AutomataDictionary.TryGetValue(propertyName, out var value))
                    {
                        switch (value)
                        {
                            case 0:
                                partition = reader.ReadInt64();
                                break;
                            case 1:
                                numberOfPartitions = reader.ReadInt64();
                                break;
                        }
                    }
                }

                termsInclude = new TermsInclude(partition, numberOfPartitions);
                break;
            case JsonToken.String:
                termsInclude = new TermsInclude(reader.ReadString());
                break;
            default:
                throw new Exception($"Unexpected token {token} when deserializing {nameof(TermsInclude)}");
        }

        return termsInclude;
    }

    public void Serialize(ref JsonWriter writer, TermsInclude value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
            writer.WriteNull();
        else if (value.Values != null)
        {
            var formatter = formatterResolver.GetFormatter<IEnumerable<string>>();
            formatter.Serialize(ref writer, value.Values, formatterResolver);
        }
        else if (value.Partition.HasValue && value.NumberOfPartitions.HasValue)
        {
            writer.WriteBeginObject();
            writer.WritePropertyName("partition");
            writer.WriteInt64(value.Partition.Value);
            writer.WriteValueSeparator();
            writer.WritePropertyName("num_partitions");
            writer.WriteInt64(value.NumberOfPartitions.Value);
            writer.WriteEndObject();
        }
        else
            writer.WriteString(value.Pattern);
    }
}
