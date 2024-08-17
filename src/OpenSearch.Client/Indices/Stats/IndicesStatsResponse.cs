/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[DataContract]
public class IndicesStatsResponse : ResponseBase
{
    [DataMember(Name = "_all")]
    public AllIndicesStats All { get; internal set; }

    [DataMember(Name = "_shards")]
    public ShardStatistics Shards { get; internal set; }

    [DataMember(Name = "indices")]
    public IndicesStatsDictionary Indices { get; internal set; }
}

[JsonFormatter(typeof(Converter))]
public class IndicesStatsDictionary : ResolvableDictionaryProxy<IndexName, IndicesStats>
{
    private IndicesStatsDictionary(IConnectionConfigurationValues s, IReadOnlyDictionary<IndexName, IndicesStats> d)
        : base(s, d) { }

    private class Converter : ResolvableDictionaryFormatterBase<IndicesStatsDictionary, IndexName, IndicesStats>
    {
        protected override IndicesStatsDictionary Create(IConnectionSettingsValues s, Dictionary<IndexName, IndicesStats> d) => new(s, d);
    }
}

[DataContract]
public class AllIndicesStats
{
    [DataMember(Name = "primaries")]
    public IndexStats Primaries { get; internal set; }

    [DataMember(Name = "total")]
    public IndexStats Total { get; internal set; }
}

[DataContract]
public class IndicesStats
{
    [DataMember(Name = "uuid")]
    public string Uuid { get; internal set; }

    [DataMember(Name = "primaries")]
    public IndexStats Primaries { get; internal set; }

    [DataMember(Name = "total")]
    public IndexStats Total { get; internal set; }

    [DataMember(Name = "shards")]
    public IReadOnlyDictionary<string, IReadOnlyCollection<IndexShardStats>> Shards { get; internal set; }
}

[DataContract]
public abstract class IndexStatsBase
{
    [DataMember(Name = "docs")]
    public DocStats Documents { get; internal set; }

    [DataMember(Name = "store")]
    public StoreStats Store { get; internal set; }

    [DataMember(Name = "indexing")]
    public IndexingStats Indexing { get; internal set; }

    [DataMember(Name = "get")]
    public GetStats Get { get; internal set; }

    [DataMember(Name = "search")]
    public SearchStats Search { get; internal set; }

    [DataMember(Name = "merges")]
    public MergesStats Merges { get; internal set; }

    [DataMember(Name = "refresh")]
    public RefreshStats Refresh { get; internal set; }

    [DataMember(Name = "flush")]
    public FlushStats Flush { get; internal set; }

    [DataMember(Name = "warmer")]
    public WarmerStats Warmer { get; internal set; }

    [DataMember(Name = "query_cache")]
    public QueryCacheStats QueryCache { get; internal set; }

    [DataMember(Name = "fielddata")]
    public FielddataStats Fielddata { get; internal set; }

    [DataMember(Name = "completion")]
    public CompletionStats Completion { get; internal set; }

    [DataMember(Name = "segments")]
    public SegmentsStats Segments { get; internal set; }

    [DataMember(Name = "translog")]
    public TranslogStats Translog { get; internal set; }

    [DataMember(Name = "request_cache")]
    public RequestCacheStats RequestCache { get; internal set; }

    [DataMember(Name = "recovery")]
    public RecoveryStats Recovery { get; internal set; }
}

[DataContract]
public class IndexStats : IndexStatsBase
{

}

[DataContract]
public class IndexShardStats : IndexStatsBase
{
    [DataMember(Name = "routing")]
    public ShardRouting Routing { get; internal set; }

    [DataMember(Name = "commit")]
    public ShardCommitStats Commit { get; internal set; }

    [DataMember(Name = "seq_no")]
    public ShardSequenceNumberStats SequenceNumber { get; internal set; }

    [DataMember(Name = "retention_leases")]
    public ShardRetentionLeasesStats RetentionLeases { get; internal set; }

    [DataMember(Name = "shard_path")]
    public ShardPath ShardPath { get; internal set; }
}

[DataContract]
public class ShardRouting
{
    [DataMember(Name = "state")]
    public ShardRoutingState State { get; internal set; }

    [DataMember(Name = "primary")]
    public bool Primary { get; internal set; }

    [DataMember(Name = "node")]
    public string Node { get; internal set; }

    [DataMember(Name = "relocating_node")]
    public string RelocatingNode { get; internal set; }
}

[StringEnum]
public enum ShardRoutingState
{
    [EnumMember(Value = "INITIALIZING")]
    Initializing,

    [EnumMember(Value = "RELOCATING")]
    Relocating,

    [EnumMember(Value = "STARTED")]
    Started,

    [EnumMember(Value = "UNASSIGNED")]
    Unassigned
}

[DataContract]
public class ShardCommitStats
{
    [DataMember(Name = "id")]
    public string Id { get; internal set; }

    [DataMember(Name = "generation")]
    public long Generation { get; internal set; }

    [DataMember(Name = "num_docs")]
    public int NumDocs { get; internal set; }

    [DataMember(Name = "user_data")]
    public IReadOnlyDictionary<string, string> UserData { get; internal set; }
}

[DataContract]
public class ShardSequenceNumberStats
{
    [DataMember(Name = "max_seq_no")]
    public long MaxSequenceNumber { get; internal set; }

    [DataMember(Name = "local_checkpoint")]
    public long LocalCheckpoint { get; internal set; }

    [DataMember(Name = "global_checkpoint")]
    public long GlobalCheckpoint { get; internal set; }
}

[DataContract]
public class ShardRetentionLeasesStats
{
    [DataMember(Name = "primary_term")]
    public long PrimaryTerm { get; internal set; }

    [DataMember(Name = "version")]
    public long Version { get; internal set; }

    [DataMember(Name = "leases")]
    public IReadOnlyCollection<ShardRetentionLease> Leases { get; internal set; }
}

[DataContract]
public class ShardRetentionLease
{
    [DataMember(Name = "id")]
    public string Id { get; internal set; }

    [DataMember(Name = "retaining_seq_no")]
    public long RetainingSequenceNumber { get; internal set; }

    [DataMember(Name = "timestamp")]
    public long Timestamp { get; internal set; }

    [DataMember(Name = "source")]
    public string Source { get; internal set; }
}

[DataContract]
public class ShardPath
{
    [DataMember(Name = "state_path")]
    public string StatePath { get; internal set; }

    [DataMember(Name = "data_path")]
    public string DataPath { get; internal set; }

    [DataMember(Name = "is_custom_data_path")]
    public bool IsCustomDataPath { get; internal set; }
}
