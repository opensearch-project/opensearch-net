/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.Document.Multiple.BulkStreamAll
{
	public abstract class BulkStreamAllApiTestsBase : IClusterFixture<IntrusiveOperationCluster>
	{
		protected BulkStreamAllApiTestsBase(IntrusiveOperationCluster cluster) => Client = cluster.Client;

		protected IOpenSearchClient Client { get; }

		protected static string CreateIndexName() => $"bulkstreamall-{Guid.NewGuid().ToString("N").Substring(8)}";

		protected IEnumerable<SmallObject> CreateLazyStreamOfDocuments(int count)
		{
			for (var i = 0; i < count; i++)
				yield return new SmallObject { Id = i, Name = $"doc-{i}" };
		}

		protected IEnumerable<SmallObject> CreateDocumentsWithAffinityKeys(int count, int distinctKeys)
		{
			for (var i = 0; i < count; i++)
				yield return new SmallObject { Id = i, Name = $"key-{i % distinctKeys}" };
		}

		protected static void OnError(ref Exception ex, Exception e, EventWaitHandle handle)
		{
			ex = e;
			handle.Set();
			throw e;
		}

		protected class SmallObject
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}
	}
}
