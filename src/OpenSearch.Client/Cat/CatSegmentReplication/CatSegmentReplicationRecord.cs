/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Runtime.Serialization;

namespace OpenSearch.Client;

[DataContract]
public class CatSegmentReplicationRecord : ICatRecord
{
	[DataMember(Name = "shardId")]
	public string ShardId { get; set; }

	[DataMember(Name = "target_node")]
	public string TargetNode { get; set; }

	[DataMember(Name = "target_host")]
	public string TargetHost { get; set; }

	[DataMember(Name = "checkpoints_behind")]
	public string CheckpointsBehind { get; set; }

	[DataMember(Name = "bytes_behind")]
	public string BytesBehind { get; set; }

	[DataMember(Name = "current_lag")]
	public string CurrentLag { get; set; }

	[DataMember(Name = "last_completed_lag")]
	public string LastCompletedLag { get; set; }

	[DataMember(Name = "rejected_requests")]
	public string RejectedRequests { get; set; }

	[DataMember(Name = "stage")]
	public string Stage { get; set; }

	[DataMember(Name = "time")]
	public string Time { get; set; }

	[DataMember(Name = "files_fetched")]
	public string FilesFetched { get; set; }

	[DataMember(Name = "files_percent")]
	public string FilesPercent { get; set; }

	[DataMember(Name = "bytes_fetched")]
	public string BytesFetched { get; set; }

	[DataMember(Name = "bytes_percent")]
	public string BytesPercent { get; set; }

	[DataMember(Name = "start_time")]
	public string StartTime { get; set; }

	[DataMember(Name = "stop_time")]
	public string StopTime { get; set; }

	[DataMember(Name = "files")]
	public string Files { get; set; }

	[DataMember(Name = "files_total")]
	public string FilesTotal { get; set; }

	[DataMember(Name = "bytes")]
	public string Bytes { get; set; }

	[DataMember(Name = "bytes_total")]
	public string BytesTotal { get; set; }

	[DataMember(Name = "replicating_stage_time_taken")]
	public string ReplicatingStageTimeTaken { get; set; }

	[DataMember(Name = "get_checkpoint_info_stage_time_taken")]
	public string GetCheckpointInfoStageTimeTaken { get; set; }

	[DataMember(Name = "file_diff_stage_time_taken")]
	public string FileDiffStageTimeTaken { get; set; }

	[DataMember(Name = "get_files_stage_time_taken")]
	public string GetFilesStageTimeTaken { get; set; }

	[DataMember(Name = "finalize_replication_stage_time_taken")]
	public string FinalizeReplicationStageTimeTaken { get; set; }
}
