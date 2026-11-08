/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client;

[DataContract]
public class DeletePitResponse : ResponseBase
{
	[DataMember(Name = "pits")]
	public IReadOnlyCollection<DeletedPit> Pits { get; internal set; } = EmptyReadOnly<DeletedPit>.Collection;
}

[DataContract]
public class DeletedPit
{
	[DataMember(Name = "pit_id")]
	public string PitId { get; set; }

	[DataMember(Name = "successful")]
	public bool Successful { get; set; }
}
