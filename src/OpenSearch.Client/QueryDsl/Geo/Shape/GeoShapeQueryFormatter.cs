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

internal class GeoShapeQueryFieldNameFormatter : IJsonFormatter<IGeoShapeQuery>
{
    public IGeoShapeQuery Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver) =>
        throw new NotSupportedException();

    public void Serialize(ref JsonWriter writer, IGeoShapeQuery value, IJsonFormatterResolver formatterResolver)
    {
        var fieldName = value.Field;
        if (fieldName == null)
        {
            writer.WriteNull();
            return;
        }

        var settings = formatterResolver.GetConnectionSettings();
        var field = settings.Inferrer.Field(fieldName);

        if (field.IsNullOrEmpty())
        {
            writer.WriteNull();
            return;
        }

        writer.WriteBeginObject();
        var name = value.Name;
        var boost = value.Boost;
        var ignoreUnmapped = value.IgnoreUnmapped;

        if (!name.IsNullOrEmpty())
        {
            writer.WritePropertyName("_name");
            writer.WriteString(name);
            writer.WriteValueSeparator();
        }
        if (boost != null)
        {
            writer.WritePropertyName("boost");
            writer.WriteDouble(boost.Value);
            writer.WriteValueSeparator();
        }
        if (ignoreUnmapped != null)
        {
            writer.WritePropertyName("ignore_unmapped");
            writer.WriteBoolean(ignoreUnmapped.Value);
            writer.WriteValueSeparator();
        }

        writer.WritePropertyName(field);

        writer.WriteBeginObject();

        var written = false;

        if (value.Shape != null)
        {
            writer.WritePropertyName("shape");
            var shapeFormatter = formatterResolver.GetFormatter<IGeoShape>();
            shapeFormatter.Serialize(ref writer, value.Shape, formatterResolver);
            written = true;
        }
        else if (value.IndexedShape != null)
        {
            writer.WritePropertyName("indexed_shape");
            var fieldLookupFormatter = formatterResolver.GetFormatter<IFieldLookup>();
            fieldLookupFormatter.Serialize(ref writer, value.IndexedShape, formatterResolver);
            written = true;
        }

        if (value.Relation.HasValue)
        {
            if (written)
                writer.WriteValueSeparator();

            writer.WritePropertyName("relation");
            formatterResolver.GetFormatter<GeoShapeRelation>()
                .Serialize(ref writer, value.Relation.Value, formatterResolver);
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}

internal class GeoShapeQueryFormatter : IJsonFormatter<IGeoShapeQuery>
{
    private static readonly AutomataDictionary Fields = new AutomataDictionary
    {
        { "boost", 0 },
        { "_name", 1 },
        { "ignore_unmapped", 2 }
    };

    private static readonly AutomataDictionary ShapeFields = new AutomataDictionary
    {
        { "shape", 0 },
        { "indexed_shape", 1 },
        { "relation", 2 }
    };

    public IGeoShapeQuery Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.ReadIsNull())
            return null;

        var count = 0;
        string field = null;
        double? boost = null;
        string name = null;
        bool? ignoreUnmapped = null;
        IGeoShapeQuery query = null;
        GeoShapeRelation? relation = null;

        while (reader.ReadIsInObject(ref count))
        {
            var propertyName = reader.ReadPropertyNameSegmentRaw();
            if (Fields.TryGetValue(propertyName, out var value))
            {
                switch (value)
                {
                    case 0:
                        boost = reader.ReadDouble();
                        break;
                    case 1:
                        name = reader.ReadString();
                        break;
                    case 2:
                        ignoreUnmapped = reader.ReadBoolean();
                        break;
                }
            }
            else
            {
                field = propertyName.Utf8String();
                var shapeCount = 0;
                while (reader.ReadIsInObject(ref shapeCount))
                {
                    var shapeProperty = reader.ReadPropertyNameSegmentRaw();
                    if (ShapeFields.TryGetValue(shapeProperty, out var shapeValue))
                    {
                        switch (shapeValue)
                        {
                            case 0:
                                var shapeFormatter = formatterResolver.GetFormatter<IGeoShape>();
                                query = new GeoShapeQuery
                                {
                                    Shape = shapeFormatter.Deserialize(ref reader, formatterResolver)
                                };
                                break;
                            case 1:
                                var fieldLookupFormatter = formatterResolver.GetFormatter<FieldLookup>();
                                query = new GeoShapeQuery
                                {
                                    IndexedShape = fieldLookupFormatter.Deserialize(ref reader, formatterResolver)
                                };
                                break;
                            case 2:
                                relation = formatterResolver.GetFormatter<GeoShapeRelation>()
                                    .Deserialize(ref reader, formatterResolver);
                                break;
                        }
                    }
                }
            }
        }

        if (query == null)
            return null;

        query.Boost = boost;
        query.Name = name;
        query.Field = field;
        query.Relation = relation;
        query.IgnoreUnmapped = ignoreUnmapped;
        return query;
    }

    public void Serialize(ref JsonWriter writer, IGeoShapeQuery value, IJsonFormatterResolver formatterResolver) =>
        throw new NotSupportedException();
}
