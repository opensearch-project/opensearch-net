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

internal class RelationNameFormatter : IJsonFormatter<RelationName>, IObjectPropertyNameFormatter<RelationName>
{
    public RelationName Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.GetCurrentJsonToken() == JsonToken.String)
        {
            RelationName relationName = reader.ReadString();
            return relationName;
        }

        reader.ReadNextBlock();
        return null;
    }

    public void Serialize(ref JsonWriter writer, RelationName value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var settings = formatterResolver.GetConnectionSettings();
        writer.WriteString(settings.Inferrer.RelationName(value));
    }

    public void SerializeToPropertyName(ref JsonWriter writer, RelationName value, IJsonFormatterResolver formatterResolver) =>
        Serialize(ref writer, value, formatterResolver);

    public RelationName DeserializeFromPropertyName(ref JsonReader reader, IJsonFormatterResolver formatterResolver) =>
        Deserialize(ref reader, formatterResolver);
}
