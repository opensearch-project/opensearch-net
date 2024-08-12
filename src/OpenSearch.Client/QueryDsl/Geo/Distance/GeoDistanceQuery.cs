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

[JsonFormatter(typeof(GeoDistanceQueryFormatter))]
public interface IGeoDistanceQuery : IFieldNameQuery
{
    Distance Distance { get; set; }

    GeoDistanceType? DistanceType { get; set; }

    GeoLocation Location { get; set; }

    GeoValidationMethod? ValidationMethod { get; set; }
}

public class GeoDistanceQuery : FieldNameQueryBase, IGeoDistanceQuery
{
    public Distance Distance { get; set; }
    public GeoDistanceType? DistanceType { get; set; }
    public GeoLocation Location { get; set; }
    public GeoValidationMethod? ValidationMethod { get; set; }
    protected override bool Conditionless => IsConditionless(this);

    internal override void InternalWrapInContainer(IQueryContainer c) => c.GeoDistance = this;

    internal static bool IsConditionless(IGeoDistanceQuery q) =>
        q.Location == null || q.Distance == null || q.Field.IsConditionless();
}

public class GeoDistanceQueryDescriptor<T>
    : FieldNameQueryDescriptorBase<GeoDistanceQueryDescriptor<T>, IGeoDistanceQuery, T>
        , IGeoDistanceQuery where T : class
{
    protected override bool Conditionless => GeoDistanceQuery.IsConditionless(this);
    Distance IGeoDistanceQuery.Distance { get; set; }
    GeoDistanceType? IGeoDistanceQuery.DistanceType { get; set; }
    GeoLocation IGeoDistanceQuery.Location { get; set; }
    GeoValidationMethod? IGeoDistanceQuery.ValidationMethod { get; set; }

    public GeoDistanceQueryDescriptor<T> Location(GeoLocation location) => Assign(location, (a, v) => a.Location = v);

    public GeoDistanceQueryDescriptor<T> Location(double lat, double lon) => Assign(new GeoLocation(lat, lon), (a, v) => a.Location = v);

    public GeoDistanceQueryDescriptor<T> Distance(Distance distance) => Assign(distance, (a, v) => a.Distance = v);

    public GeoDistanceQueryDescriptor<T> Distance(double distance, DistanceUnit unit) => Assign(new Distance(distance, unit), (a, v) => a.Distance = v);

    public GeoDistanceQueryDescriptor<T> DistanceType(GeoDistanceType? type) => Assign(type, (a, v) => a.DistanceType = v);

    public GeoDistanceQueryDescriptor<T> ValidationMethod(GeoValidationMethod? validation) => Assign(validation, (a, v) => a.ValidationMethod = v);
}

internal class GeoDistanceQueryFormatter : IJsonFormatter<IGeoDistanceQuery>
{
    private static readonly AutomataDictionary Fields = new AutomataDictionary
    {
        { "_name", 0 },
        { "boost", 1 },
        { "validation_method", 2 },
        { "distance", 3 },
        { "distance_type", 4 }
    };

    public IGeoDistanceQuery Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.GetCurrentJsonToken() != JsonToken.BeginObject)
            return null;

        var query = new GeoDistanceQuery();
        var count = 0;
        while (reader.ReadIsInObject(ref count))
        {
            var property = reader.ReadPropertyNameSegmentRaw();
            if (Fields.TryGetValue(property, out var value))
            {
                switch (value)
                {
                    case 0:
                        query.Name = reader.ReadString();
                        break;
                    case 1:
                        query.Boost = reader.ReadDouble();
                        break;
                    case 2:
                        query.ValidationMethod = formatterResolver.GetFormatter<GeoValidationMethod>()
                            .Deserialize(ref reader, formatterResolver);
                        break;
                    case 3:
                        query.Distance = formatterResolver.GetFormatter<Distance>()
                            .Deserialize(ref reader, formatterResolver);
                        break;
                    case 4:
                        query.DistanceType = formatterResolver.GetFormatter<GeoDistanceType>()
                            .Deserialize(ref reader, formatterResolver);
                        break;
                }
            }
            else
            {
                query.Field = property.Utf8String();
                query.Location = formatterResolver.GetFormatter<GeoLocation>()
                    .Deserialize(ref reader, formatterResolver);
            }
        }

        return query;
    }

    public void Serialize(ref JsonWriter writer, IGeoDistanceQuery value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var written = false;

        writer.WriteBeginObject();

        if (!value.Name.IsNullOrEmpty())
        {
            writer.WritePropertyName("_name");
            writer.WriteString(value.Name);
            written = true;
        }

        if (value.Boost != null)
        {
            if (written)
                writer.WriteValueSeparator();

            writer.WritePropertyName("boost");
            writer.WriteDouble(value.Boost.Value);
            written = true;
        }

        if (value.ValidationMethod != null)
        {
            if (written)
                writer.WriteValueSeparator();

            writer.WritePropertyName("validation_method");
            formatterResolver.GetFormatter<GeoValidationMethod>()
                .Serialize(ref writer, value.ValidationMethod.Value, formatterResolver);
            written = true;
        }

        if (value.Distance != null)
        {
            if (written)
                writer.WriteValueSeparator();

            writer.WritePropertyName("distance");
            formatterResolver.GetFormatter<Distance>()
                .Serialize(ref writer, value.Distance, formatterResolver);
            written = true;
        }

        if (value.DistanceType != null)
        {
            if (written)
                writer.WriteValueSeparator();

            writer.WritePropertyName("distance_type");
            formatterResolver.GetFormatter<GeoDistanceType>()
                .Serialize(ref writer, value.DistanceType.Value, formatterResolver);
            written = true;
        }

        if (written)
            writer.WriteValueSeparator();

        var settings = formatterResolver.GetConnectionSettings();
        writer.WritePropertyName(settings.Inferrer.Field(value.Field));
        formatterResolver.GetFormatter<GeoLocation>()
            .Serialize(ref writer, value.Location, formatterResolver);

        writer.WriteEndObject();
    }
}
