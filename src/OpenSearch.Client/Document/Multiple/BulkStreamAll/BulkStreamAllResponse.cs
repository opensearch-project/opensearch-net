/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Runtime.Serialization;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// Response from a single bulk batch within the BulkStreamAll operation.
	/// </summary>
	[DataContract]
	public class BulkStreamAllResponse
	{
		/// <summary>This is the Nth batch (0-based).</summary>
		public long Page { get; internal set; }

		/// <summary>Which worker processed this batch.</summary>
		public int WorkerIndex { get; internal set; }

		/// <summary>The number of back off retries that were needed to store this batch.</summary>
		public int Retries { get; internal set; }

		/// <summary>The items returned from the bulk response.</summary>
		public IReadOnlyCollection<BulkResponseItemBase> Items { get; internal set; } = EmptyReadOnly<BulkResponseItemBase>.Collection;

		/// <summary>Server-side time in milliseconds.</summary>
		public long Took { get; internal set; }
	}
}
