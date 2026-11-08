/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OpenSearch.Client;

[DataContract]
public class GetAllPitsResponse : ResponseBase
{
	[DataMember(Name = "pits")]
	public IReadOnlyCollection<PitDetail> Pits { get; internal set; }
}

[DataContract]
public class PitDetail
{
	[DataMember(Name = "pit_id")]
	public string PitId { get; internal set; }

	[DataMember(Name = "creation_time")]
	public long CreationTime { get; internal set; }

	[DataMember(Name = "keep_alive")]
	public long KeepAlive { get; internal set; }
}
