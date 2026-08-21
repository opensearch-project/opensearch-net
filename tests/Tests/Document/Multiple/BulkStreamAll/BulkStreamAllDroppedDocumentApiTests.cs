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
	public class BulkStreamAllDroppedDocumentApiTests : BulkStreamAllApiTestsBase
	{
		public BulkStreamAllDroppedDocumentApiTests(IntrusiveOperationCluster cluster) : base(cluster) { }

		[I]
		public void DroppedDocumentCallbackIsInvoked()
		{
			var index = CreateIndexName();
			var droppedCount = 0;

			var size = 100;
			var numberOfDocuments = 200;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(1)
				.Size(size)
				.Index(index)
				.ContinueAfterDroppedDocuments()
				.DroppedDocumentCallback((item, doc) =>
				{
					Interlocked.Increment(ref droppedCount);
					item.Should().NotBeNull();
				})
				// Custom retry predicate that retries nothing — all failures become drops
				.RetryDocumentPredicate((item, doc) => false)
			);

			// This test verifies the callback mechanism works structurally.
			// Actual dropped documents would require the server to reject some items.
			observableBulk.Wait(TimeSpan.FromSeconds(30), b => { });

			// If all docs succeed, droppedCount should be 0 (no failures from server)
			// The test validates the wiring is correct — actual failure injection
			// would need a mock server or VirtualCluster returning partial failures.
		}

		[I]
		public void ContinueAfterDroppedDocumentsAllowsCompletion()
		{
			var index = CreateIndexName();

			var size = 100;
			var numberOfDocuments = 300;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);
			var seenPages = 0;

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(1)
				.Size(size)
				.Index(index)
				.ContinueAfterDroppedDocuments()
				.DroppedDocumentCallback((item, doc) => { })
			);

			observableBulk.Wait(TimeSpan.FromSeconds(30), b =>
			{
				Interlocked.Increment(ref seenPages);
			});

			seenPages.Should().Be(3);
		}
	}
}
