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
	public class BulkStreamAllObserverApiTests : BulkStreamAllApiTestsBase
	{
		public BulkStreamAllObserverApiTests(IntrusiveOperationCluster cluster) : base(cluster) { }

		[I]
		public void ObserverTracksDocumentsProcessed()
		{
			var index = CreateIndexName();

			var size = 100;
			var numberOfDocuments = 500;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(2)
				.Size(size)
				.Index(index)
			);

			var observer = observableBulk.Wait(TimeSpan.FromSeconds(30), b =>
			{
				// Each batch should have items
				b.Items.Count.Should().BeGreaterThan(0);
			});

			observer.TotalNumberOfFailedBuffers.Should().Be(0);
			observer.TotalNumberOfRetries.Should().Be(0);
		}

		[I]
		public void WaitThrowsOnObserverException()
		{
			var index = CreateIndexName();

			var size = 100;
			var numberOfDocuments = 500;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);
			var seenPages = 0;

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(1)
				.Size(size)
				.Index(index)
			);

			Exception caughtException = null;
			try
			{
				observableBulk.Wait(TimeSpan.FromSeconds(30), b =>
				{
					if (seenPages == 3) throw new InvalidOperationException("test-explosion");
					Interlocked.Increment(ref seenPages);
				});
			}
			catch (Exception ex)
			{
				caughtException = ex;
			}

			seenPages.Should().Be(3);
			caughtException.Should().NotBeNull();
			caughtException.Message.Should().Be("test-explosion");
		}

		[I]
		public void ObserverCompletedCallbackIsFired()
		{
			var index = CreateIndexName();
			var completed = false;

			var size = 100;
			var numberOfDocuments = 200;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);

			var handle = new ManualResetEvent(false);
			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(1)
				.Size(size)
				.Index(index)
			);

			var observer = new BulkStreamAllObserver(
				onNext: b => { },
				onError: e => handle.Set(),
				onCompleted: () =>
				{
					completed = true;
					handle.Set();
				}
			);

			observableBulk.Subscribe(observer);
			handle.WaitOne(TimeSpan.FromSeconds(30));

			completed.Should().BeTrue();
		}
	}
}
