/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Threading;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;

namespace Tests.Document.Multiple.BulkStreamAll
{
	public class BulkStreamAllBasicApiTests : BulkStreamAllApiTestsBase
	{
		public BulkStreamAllBasicApiTests(IntrusiveOperationCluster cluster) : base(cluster) { }

		[I]
		public void BulkStreamAllIndexesAllDocuments()
		{
			var index = CreateIndexName();

			var size = 500;
			var pages = 10;
			var seenPages = 0;
			var numberOfDocuments = size * pages;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);

			var tokenSource = new CancellationTokenSource();
			var observableBulk = Client.BulkStreamAll(documents, f => f
					.MaxDegreeOfParallelism(4)
					.Size(size)
					.RefreshOnCompleted()
					.Index(index)
				, tokenSource.Token);

			var observer = observableBulk.Wait(TimeSpan.FromSeconds(60), b =>
			{
				Interlocked.Increment(ref seenPages);
				b.Items.Should().NotBeNull();
				b.Page.Should().BeGreaterThanOrEqualTo(0);
			});

			seenPages.Should().Be(pages);
			observer.TotalNumberOfFailedBuffers.Should().Be(0);
			observer.TotalNumberOfRetries.Should().Be(0);
		}

		[I]
		public void BulkStreamAllReportsCorrectPageNumbers()
		{
			var index = CreateIndexName();

			var size = 100;
			var numberOfDocuments = 350;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);
			var maxPageSeen = -1L;

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(1) // single worker for predictable page ordering
				.Size(size)
				.Index(index)
			);

			observableBulk.Wait(TimeSpan.FromSeconds(30), b =>
			{
				var page = b.Page;
				if (page > maxPageSeen)
					Interlocked.Exchange(ref maxPageSeen, page);
			});

			// 350 docs / 100 per batch = 4 batches (pages 0, 1, 2, 3)
			maxPageSeen.Should().BeGreaterThanOrEqualTo(3);
		}

		[I]
		public void BulkStreamAllWithCustomBufferToBulk()
		{
			var index = CreateIndexName();
			var bufferToBulkCalled = 0;

			var size = 100;
			var numberOfDocuments = 200;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(1)
				.Size(size)
				.Index(index)
				.BufferToBulk((descriptor, buffer) =>
				{
					Interlocked.Increment(ref bufferToBulkCalled);
					descriptor.IndexMany(buffer);
				})
			);

			observableBulk.Wait(TimeSpan.FromSeconds(30), b => { });

			bufferToBulkCalled.Should().Be(2); // 200 / 100 = 2 batches
		}

		[I]
		public void BulkStreamAllWithBulkResponseCallback()
		{
			var index = CreateIndexName();
			var callbackCount = 0;

			var size = 100;
			var numberOfDocuments = 300;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(1)
				.Size(size)
				.Index(index)
				.BulkResponseCallback(r =>
				{
					Interlocked.Increment(ref callbackCount);
					r.Should().NotBeNull();
				})
			);

			observableBulk.Wait(TimeSpan.FromSeconds(30), b => { });

			callbackCount.Should().Be(3); // 300 / 100 = 3 batches
		}
	}
}
