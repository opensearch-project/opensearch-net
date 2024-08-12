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

namespace OpenSearch.Client;

[DataContract]
public class SnapshotStatusResponse : ResponseBase
{
    [DataMember(Name = "snapshots")]
    public IReadOnlyCollection<SnapshotStatus> Snapshots { get; internal set; } = EmptyReadOnly<SnapshotStatus>.Collection;
}

public class SnapshotStatus
{
    [DataMember(Name = "include_global_state")]
    public bool? IncludeGlobalState { get; internal set; }

    [DataMember(Name = "indices")]
    public IReadOnlyDictionary<string, SnapshotIndexStats> Indices { get; internal set; } = EmptyReadOnly<string, SnapshotIndexStats>.Dictionary;

    [DataMember(Name = "repository")]
    public string Repository { get; internal set; }

    [DataMember(Name = "shards_stats")]
    public SnapshotShardsStats ShardsStats { get; internal set; }

    [DataMember(Name = "snapshot")]
    public string Snapshot { get; internal set; }

    [DataMember(Name = "state")]
    public string State { get; internal set; }

    [DataMember(Name = "stats")]
    public SnapshotStats Stats { get; internal set; }

    [DataMember(Name = "uuid")]
    public string UUID { get; internal set; }
}

public class SnapshotIndexStats
{
    [DataMember(Name = "shards")]
    public IReadOnlyDictionary<string, SnapshotShardsStats> Shards { get; internal set; } = EmptyReadOnly<string, SnapshotShardsStats>.Dictionary;

    [DataMember(Name = "shards_stats")]
    public SnapshotShardsStats ShardsStats { get; internal set; }

    [DataMember(Name = "stats")]
    public SnapshotStats Stats { get; internal set; }
}

public class SnapshotIndexShardStats
{
    [DataMember(Name = "node")]
    public string Node { get; internal set; }

    [DataMember(Name = "stage")]
    public string Stage { get; internal set; }

    [DataMember(Name = "stats")]
    public SnapshotStats Stats { get; internal set; }
}

public class SnapshotShardsStats
{
    [DataMember(Name = "done")]
    public long Done { get; internal set; }

    [DataMember(Name = "failed")]
    public long Failed { get; internal set; }

    [DataMember(Name = "finalizing")]
    public long Finalizing { get; internal set; }

    [DataMember(Name = "initializing")]
    public long Initializing { get; internal set; }

    [DataMember(Name = "started")]
    public long Started { get; internal set; }

    [DataMember(Name = "total")]
    public long Total { get; internal set; }
}

public class SnapshotStats
{
    [DataMember(Name = "incremental")]
    public FileCountSnapshotStats Incremental { get; internal set; }

    [DataMember(Name = "total")]
    public FileCountSnapshotStats Total { get; internal set; }

    [DataMember(Name = "start_time_in_millis")]
    public long StartTimeInMilliseconds { get; internal set; }

    [DataMember(Name = "time_in_millis")]
    public long TimeInMilliseconds { get; internal set; }
}

public class FileCountSnapshotStats
{
    [DataMember(Name = "file_count")]
    public int FileCount { get; internal set; }

    [DataMember(Name = "size_in_bytes")]
    public long SizeInBytes { get; internal set; }
}
