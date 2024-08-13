/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

namespace OpenSearch.Client;

[MapsApi("indices.stats")]
[ReadAs(typeof(IndicesStatsRequest))]
public partial interface IIndicesStatsRequest
{
}

public partial class IndicesStatsRequest
{
}

public partial class IndicesStatsDescriptor
{
}
