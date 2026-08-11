/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;

namespace OpenSearch.Client
{
	/// <summary>
	/// Implemented by generated types whose OpenAPI schema declares
	/// <c>additionalProperties: true</c>. The <see cref="AdditionalProperties"/>
	/// dictionary is serialized as top-level sibling keys alongside the typed
	/// members, and any unrecognized key during deserialization is stored here.
	/// </summary>
	public interface IHasAdditionalProperties
	{
		IDictionary<string, object> AdditionalProperties { get; set; }
	}
}
