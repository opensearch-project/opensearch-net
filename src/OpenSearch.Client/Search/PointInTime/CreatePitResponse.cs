/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Runtime.Serialization;

namespace OpenSearch.Client;

[DataContract]
public class CreatePitResponse : ResponseBase
{
    [DataMember(Name = "pit_id")]
    public string PitId { get; internal set; }

    [DataMember(Name = "_shards")]
    public ShardStatistics Shards { get; internal set; }

    [DataMember(Name = "creation_time")]
    public long CreationTime { get; internal set; }
}
