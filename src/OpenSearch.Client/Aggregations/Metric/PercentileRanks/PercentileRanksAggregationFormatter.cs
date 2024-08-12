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
using System.Linq;
using OpenSearch.Net.Utf8Json;
using OpenSearch.Net.Utf8Json.Internal;


namespace OpenSearch.Client;

internal class PercentileRanksAggregationFormatter : IJsonFormatter<IPercentileRanksAggregation>
{
    private static readonly AutomataDictionary AutomataDictionary = new AutomataDictionary
    {
        { "hdr", 0 },
        { "tdigest", 1 },
        { "field", 2 },
        { "script", 3 },
        { "missing", 4 },
        { "meta", 5 },
        { "values", 6 },
        { "keyed", 7 },
        { "format", 8 }
    };

    public IPercentileRanksAggregation Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.GetCurrentJsonToken() != JsonToken.BeginObject)
        {
            reader.ReadNextBlock();
            return null;
        }

        var count = 0;
        var percentiles = new PercentileRanksAggregation();

        while (reader.ReadIsInObject(ref count))
        {
            var propertyName = reader.ReadPropertyNameSegmentRaw();
            if (AutomataDictionary.TryGetValue(propertyName, out var value))
            {
                switch (value)
                {
                    case 0:
                        percentiles.Method = formatterResolver.GetFormatter<HDRHistogramMethod>()
                            .Deserialize(ref reader, formatterResolver);
                        break;
                    case 1:
                        percentiles.Method = formatterResolver.GetFormatter<TDigestMethod>()
                            .Deserialize(ref reader, formatterResolver);
                        break;
                    case 2:
                        percentiles.Field = reader.ReadString();
                        break;
                    case 3:
                        percentiles.Script = formatterResolver.GetFormatter<IScript>()
                            .Deserialize(ref reader, formatterResolver);
                        break;
                    case 4:
                        percentiles.Missing = reader.ReadDouble();
                        break;
                    case 5:
                        percentiles.Meta = formatterResolver.GetFormatter<IDictionary<string, object>>()
                            .Deserialize(ref reader, formatterResolver);
                        break;
                    case 6:
                        percentiles.Values = formatterResolver.GetFormatter<IEnumerable<double>>()
                            .Deserialize(ref reader, formatterResolver);
                        break;
                    case 7:
                        percentiles.Keyed = reader.ReadBoolean();
                        break;
                    case 8:
                        percentiles.Format = reader.ReadString();
                        break;
                }
            }
        }

        return percentiles;
    }

    public void Serialize(ref JsonWriter writer, IPercentileRanksAggregation value, IJsonFormatterResolver formatterResolver)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteBeginObject();
        var propertyWritten = false;

        if (value.Meta != null && value.Meta.Any())
        {
            writer.WritePropertyName("meta");
            var formatter = formatterResolver.GetFormatter<IDictionary<string, object>>();
            formatter.Serialize(ref writer, value.Meta, formatterResolver);
            propertyWritten = true;
        }

        if (value.Field != null)
        {
            if (propertyWritten)
                writer.WriteValueSeparator();

            var settings = formatterResolver.GetConnectionSettings();
            writer.WritePropertyName("field");
            writer.WriteString(settings.Inferrer.Field(value.Field));
            propertyWritten = true;
        }

        if (value.Script != null)
        {
            if (propertyWritten)
                writer.WriteValueSeparator();

            writer.WritePropertyName("script");
            var formatter = formatterResolver.GetFormatter<IScript>();
            formatter.Serialize(ref writer, value.Script, formatterResolver);
            propertyWritten = true;
        }

        if (value.Method != null)
        {
            if (propertyWritten)
                writer.WriteValueSeparator();

            switch (value.Method)
            {
                case ITDigestMethod tdigest:
                    {
                        writer.WritePropertyName("tdigest");
                        writer.WriteBeginObject();
                        if (tdigest.Compression.HasValue)
                        {
                            writer.WritePropertyName("compression");
                            writer.WriteDouble(tdigest.Compression.Value);
                        }
                        writer.WriteEndObject();
                        break;
                    }
                case IHDRHistogramMethod hdr:
                    {
                        writer.WritePropertyName("hdr");
                        writer.WriteBeginObject();
                        if (hdr.NumberOfSignificantValueDigits.HasValue)
                        {
                            writer.WritePropertyName("number_of_significant_value_digits");
                            writer.WriteInt32(hdr.NumberOfSignificantValueDigits.Value);
                        }
                        writer.WriteEndObject();
                        break;
                    }
            }

            propertyWritten = true;
        }

        if (value.Missing.HasValue)
        {
            if (propertyWritten)
                writer.WriteValueSeparator();

            writer.WritePropertyName("missing");
            writer.WriteDouble(value.Missing.Value);
            propertyWritten = true;
        }

        if (value.Values != null && value.Values.Any())
        {
            if (propertyWritten)
                writer.WriteValueSeparator();

            writer.WritePropertyName("values");
            var formatter = formatterResolver.GetFormatter<IEnumerable<double>>();
            formatter.Serialize(ref writer, value.Values, formatterResolver);
            propertyWritten = true;
        }

        if (value.Keyed.HasValue)
        {
            if (propertyWritten)
                writer.WriteValueSeparator();

            writer.WritePropertyName("keyed");
            writer.WriteBoolean(value.Keyed.Value);
            propertyWritten = true;
        }

        if (!string.IsNullOrEmpty(value.Format))
        {
            if (propertyWritten)
                writer.WriteValueSeparator();

            writer.WritePropertyName("format");
            writer.WriteString(value.Format);
        }

        writer.WriteEndObject();
    }
}
