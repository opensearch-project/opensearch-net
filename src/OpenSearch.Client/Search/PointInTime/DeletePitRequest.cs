/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace OpenSearch.Client;

[MapsApi("delete_pit")]
[ReadAs(typeof(DeletePitRequest))]
public partial interface IDeletePitRequest
{
    [DataMember(Name = "pit_id")]
    IEnumerable<string> PitId { get; set; }
}

public partial class DeletePitRequest
{
    public DeletePitRequest(IEnumerable<string> pitId) : this(pitId?.ToArray()) { }

    public DeletePitRequest(params string[] pitId) => PitId = pitId;

    public IEnumerable<string> PitId { get; set; }
}

public partial class DeletePitDescriptor
{
    IEnumerable<string> IDeletePitRequest.PitId { get; set; }

    public DeletePitDescriptor PitId(IEnumerable<string> pitId) =>
        Assign(pitId?.ToArray(), (r, v) => r.PitId = v);

    public DeletePitDescriptor PitId(params string[] pitId) =>
        Assign(pitId, (r, v) => r.PitId = v);
}
