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
using Tests.Core.Xunit;

namespace Tests.Document.Multiple.BulkStreamAll
{
	public class BulkStreamAllCancellationApiTests : BulkStreamAllApiTestsBase
	{
		public BulkStreamAllCancellationApiTests(IntrusiveOperationCluster cluster) : base(cluster) { }

		[I] [SkipOnCi]
		public void CancellationTokenStopsBulkStreamAll()
		{
			var index = CreateIndexName();
			var handle = new ManualResetEvent(false);

			var size = 100;
			var pages = 1000;
			var seenPages = 0;
			var numberOfDocuments = size * pages;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);

			var tokenSource = new CancellationTokenSource();
			var observableBulk = Client.BulkStreamAll(documents, f => f
					.MaxDegreeOfParallelism(4)
					.MaxRetries(2)
					.RetryBaseDelay(TimeSpan.FromSeconds(10))
					.Size(size)
					.Index(index)
				, tokenSource.Token);

			Exception ex = null;
			var observer = new BulkStreamAllObserver(
				onError: e => OnError(ref ex, e, handle),
				onNext: b => Interlocked.Increment(ref seenPages)
			);

			observableBulk.Subscribe(observer);

			// Wait a bit to see some progress
			handle.WaitOne(TimeSpan.FromSeconds(3));
			tokenSource.Cancel();
			// Give in-flight requests a chance to cancel
			handle.WaitOne(TimeSpan.FromSeconds(3));

			if (ex != null && !(ex is OperationCanceledException)) throw ex;

			seenPages.Should().BeLessThan(pages).And.BeGreaterThan(0);
			observer.TotalNumberOfFailedBuffers.Should().Be(0);
		}

		[I] [SkipOnCi]
		public void DisposingObservableCancelsBulkStreamAll()
		{
			var index = CreateIndexName();
			var handle = new ManualResetEvent(false);

			var size = 100;
			var pages = 1000;
			var seenPages = 0;
			var numberOfDocuments = size * pages;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(4)
				.MaxRetries(2)
				.RetryBaseDelay(TimeSpan.FromSeconds(10))
				.Size(size)
				.Index(index)
			);

			Exception ex = null;
			var observer = new BulkStreamAllObserver(
				onError: e => OnError(ref ex, e, handle),
				onCompleted: () => handle.Set(),
				onNext: b => Interlocked.Increment(ref seenPages)
			);

			observableBulk.Subscribe(observer);

			// Wait a bit for some progress
			handle.WaitOne(TimeSpan.FromSeconds(3));
			observableBulk.Dispose();
			// Give in-flight requests a chance to cancel
			handle.WaitOne(TimeSpan.FromSeconds(3));

			if (ex != null && !(ex is OperationCanceledException)) throw ex;

			seenPages.Should().BeLessThan(pages).And.BeGreaterThan(0);
		}
	}
}
