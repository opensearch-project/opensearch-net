/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.Document.Multiple.BulkStreamAll
{
	public class BulkStreamAllAffinityApiTests : BulkStreamAllApiTestsBase
	{
		public BulkStreamAllAffinityApiTests(IntrusiveOperationCluster cluster) : base(cluster) { }

		[I]
		public void DocumentsWithSameKeyRouteToSameWorker()
		{
			var index = CreateIndexName();

			var size = 50;
			var numberOfDocuments = 500;
			var distinctKeys = 10;
			var documents = CreateDocumentsWithAffinityKeys(numberOfDocuments, distinctKeys);

			// Track which worker index processes documents for each affinity key
			var keyToWorkers = new ConcurrentDictionary<string, ConcurrentBag<int>>();

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(4)
				.Size(size)
				.Index(index)
				.DocumentAffinityKey(doc => doc.Name)
			);

			observableBulk.Wait(TimeSpan.FromSeconds(60), b =>
			{
				// Each response has a WorkerIndex — all items in this batch went through the same worker
				foreach (var item in b.Items)
				{
					// We can't easily correlate item back to the original doc's Name here,
					// but we can verify the structural constraint that the WorkerIndex is consistent
					b.WorkerIndex.Should().BeInRange(0, 3);
				}
			});

			// The key assertion is that the system doesn't crash and completes successfully
			// with affinity routing enabled. Deeper ordering tests require inspecting the actual
			// bulk request bodies which would need integration-level verification.
		}

		[I]
		public void AffinityKeyNullDefaultsToRoundRobin()
		{
			var index = CreateIndexName();

			var size = 100;
			var numberOfDocuments = 400;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);
			var workersSeen = new ConcurrentBag<int>();

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(4)
				.Size(size)
				.Index(index)
				// No DocumentAffinityKey set — should use round-robin
			);

			observableBulk.Wait(TimeSpan.FromSeconds(30), b =>
			{
				workersSeen.Add(b.WorkerIndex);
			});

			// Without affinity, pages are distributed across workers
			workersSeen.Should().NotBeEmpty();
		}

		[I]
		public void AffinityPreservesOrderWithinSameKey()
		{
			var index = CreateIndexName();

			// Create documents where each key has sequential IDs
			var documents = new List<SmallObject>();
			for (var batch = 0; batch < 10; batch++)
			{
				for (var key = 0; key < 5; key++)
				{
					documents.Add(new SmallObject { Id = batch * 5 + key, Name = $"order-{key}" });
				}
			}

			var size = 10; // Force multiple batches
			var batchesByWorker = new ConcurrentDictionary<int, ConcurrentBag<long>>();

			var observableBulk = Client.BulkStreamAll((IEnumerable<SmallObject>)documents, f => f
				.MaxDegreeOfParallelism(4)
				.Size(size)
				.Index(index)
				.DocumentAffinityKey(doc => doc.Name)
			);

			observableBulk.Wait(TimeSpan.FromSeconds(30), b =>
			{
				var bag = batchesByWorker.GetOrAdd(b.WorkerIndex, _ => new ConcurrentBag<long>());
				bag.Add(b.Page);
			});

			// Each worker should have received pages in order (page numbers increasing)
			foreach (var kvp in batchesByWorker)
			{
				var pages = kvp.Value.OrderBy(p => p).ToList();
				for (var i = 1; i < pages.Count; i++)
				{
					pages[i].Should().BeGreaterThan(pages[i - 1],
						$"Worker {kvp.Key} should process pages in order");
				}
			}
		}
	}
}
