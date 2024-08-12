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

using System.Runtime.Serialization;

namespace OpenSearch.Client;

[DataContract]
public class ClusterIndicesStats
{
    [DataMember(Name = "completion")]
    public CompletionStats Completion { get; internal set; }

    [DataMember(Name = "count")]
    public long Count { get; internal set; }

    [DataMember(Name = "docs")]
    public DocStats Documents { get; internal set; }

    [DataMember(Name = "fielddata")]
    public FielddataStats Fielddata { get; internal set; }

    [DataMember(Name = "query_cache")]
    public QueryCacheStats QueryCache { get; internal set; }

    [DataMember(Name = "segments")]
    public SegmentsStats Segments { get; internal set; }

    [DataMember(Name = "shards")]
    public ClusterIndicesShardsStats Shards { get; internal set; }

    [DataMember(Name = "store")]
    public StoreStats Store { get; internal set; }
}

[DataContract]
public class ClusterIndicesShardsStats
{
    [DataMember(Name = "index")]
    public ClusterIndicesShardsIndexStats Index { get; internal set; }

    [DataMember(Name = "primaries")]
    public double Primaries { get; internal set; }

    [DataMember(Name = "replication")]
    public double Replication { get; internal set; }

    [DataMember(Name = "total")]
    public double Total { get; internal set; }
}

[DataContract]
public class ClusterIndicesShardsIndexStats
{
    [DataMember(Name = "primaries")]
    public ClusterShardMetrics Primaries { get; internal set; }

    [DataMember(Name = "replication")]
    public ClusterShardMetrics Replication { get; internal set; }

    [DataMember(Name = "shards")]
    public ClusterShardMetrics Shards { get; internal set; }
}

[DataContract]
public class ClusterShardMetrics
{
    [DataMember(Name = "avg")]
    public double Avg { get; internal set; }

    [DataMember(Name = "max")]
    public double Max { get; internal set; }

    [DataMember(Name = "min")]
    public double Min { get; internal set; }
}
