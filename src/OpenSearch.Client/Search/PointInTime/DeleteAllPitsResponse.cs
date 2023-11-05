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
public class DeleteAllPitsResponse : ResponseBase
{
	[DataMember(Name = "pits")]
	public IReadOnlyCollection<DeletedPit> Pits { get; internal set; }
}
