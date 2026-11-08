/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client;

[MapsApi("cat.pit_segments")]
[ReadAs(typeof(CatPitSegmentsRequest))]
public partial interface ICatPitSegmentsRequest
{
	[DataMember(Name = "pit_id")]
	IEnumerable<string> PitId { get; set; }
}

public partial class CatPitSegmentsRequest
{
	[SerializationConstructor]
	public CatPitSegmentsRequest() { }

	public CatPitSegmentsRequest(IEnumerable<string> pitId) : this(pitId?.ToArray()) { }

	public CatPitSegmentsRequest(params string[] pitId) => PitId = pitId;

	public IEnumerable<string> PitId { get; set; }
}

public partial class CatPitSegmentsDescriptor
{
	IEnumerable<string> ICatPitSegmentsRequest.PitId { get; set; }

	public CatPitSegmentsDescriptor PitId(IEnumerable<string> pitId) =>
		Assign(pitId?.ToArray(), (r, v) => r.PitId = v);

	public CatPitSegmentsDescriptor PitId(params string[] pitId) =>
		Assign(pitId, (r, v) => r.PitId = v);
}
