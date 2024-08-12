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
using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[DataContract]
public class IngestStats
{
    /// <summary>
    /// The total number of document ingested during the lifetime of this node
    /// </summary>
    [DataMember(Name = "count")]
    public long Count { get; set; }

    /// <summary>
    /// The total number of documents currently being ingested.
    /// </summary>
    [DataMember(Name = "current")]
    public long Current { get; set; }

    /// <summary>
    /// The total number ingest preprocessing operations failed during the lifetime of this node
    /// </summary>
    [DataMember(Name = "failed")]
    public long Failed { get; set; }

    /// <summary>
    /// The total time spent on ingest preprocessing documents during the lifetime of this node
    /// </summary>
    [DataMember(Name = "time_in_millis")]
    public long TimeInMilliseconds { get; set; }

    [DataMember(Name = "processors")]
    public IReadOnlyCollection<KeyedProcessorStats> Processors { get; internal set; } =
        EmptyReadOnly<KeyedProcessorStats>.Collection;
}

[JsonFormatter(typeof(KeyedProcessorStatsFormatter))]
public class KeyedProcessorStats
{
    /// <summary> The type of the processor </summary>
    public string Type { get; set; }

    /// <summary>The statistics for this processor</summary>
    public ProcessStats Statistics { get; set; }
}

internal class KeyedProcessorStatsFormatter : IJsonFormatter<KeyedProcessorStats>
{
    public KeyedProcessorStats Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.GetCurrentJsonToken() != JsonToken.BeginObject)
            return null;

        var count = 0;
        var stats = new KeyedProcessorStats();
        while (reader.ReadIsInObject(ref count))
        {
            stats.Type = reader.ReadPropertyName();
            stats.Statistics = formatterResolver.GetFormatter<ProcessStats>()
                .Deserialize(ref reader, formatterResolver);
        }

        return stats;
    }

    public void Serialize(ref JsonWriter writer, KeyedProcessorStats value, IJsonFormatterResolver formatterResolver)
    {
        if (value?.Type == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteBeginObject();
        writer.WritePropertyName(value.Type);
        formatterResolver.GetFormatter<ProcessStats>().Serialize(ref writer, value.Statistics, formatterResolver);
        writer.WriteEndObject();
    }
}

public class ProcessorStats
{
    /// <summary> The total number of document ingested during the lifetime of this node </summary>
    [DataMember(Name = "count")]
    public long Count { get; internal set; }

    /// <summary> The total number of documents currently being ingested. </summary>
    [DataMember(Name = "current")]
    public long Current { get; internal set; }

    /// <summary> The total number ingest preprocessing operations failed during the lifetime of this node </summary>
    [DataMember(Name = "failed")]
    public long Failed { get; internal set; }

    /// <summary> The total time spent on ingest preprocessing documents during the lifetime of this node </summary>
    [DataMember(Name = "time_in_millis")]
    public long TimeInMilliseconds { get; internal set; }
}
