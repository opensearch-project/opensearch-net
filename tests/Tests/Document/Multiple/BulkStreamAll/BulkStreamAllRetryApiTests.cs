/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Threading;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using OpenSearch.Net.VirtualizedCluster;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain.Extensions;

namespace Tests.Document.Multiple.BulkStreamAll
{
	public class BulkStreamAllRetryApiTests : BulkStreamAllApiTestsBase
	{
		public BulkStreamAllRetryApiTests(IntrusiveOperationCluster cluster) : base(cluster) { }

		[U]
		public void RetriesExhaustedThrowsException()
		{
			var cluster = VirtualClusterWith.Nodes(2)
				.ClientCalls(c => c.FailAlways())
				.StaticConnectionPool()
				.AllDefaults();

			var settings = new ConnectionSettings(cluster.ConnectionPool, cluster.Connection).ApplyDomainSettings();
			var client = new OpenSearchClient(settings);

			var index = CreateIndexName();
			var size = 100;
			var numberOfDocuments = 100;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);
			var seenPages = 0;
			var requests = 0;

			Exception ex = null;
			var tokenSource = new CancellationTokenSource();
			var observableBulk = client.BulkStreamAll(documents, f => f
					.MaxDegreeOfParallelism(1)
					.BulkResponseCallback(r => Interlocked.Increment(ref requests))
					.MaxRetries(2)
					.RetryBaseDelay(TimeSpan.FromMilliseconds(1))
					.RetryMaxDelay(TimeSpan.FromMilliseconds(10))
					.Size(size)
					.Index(index)
					.BufferToBulk((r, buffer) => r.IndexMany(buffer))
				, tokenSource.Token);

			try
			{
				observableBulk.Wait(TimeSpan.FromSeconds(30), b =>
				{
					Interlocked.Increment(ref seenPages);
				});
			}
			catch (Exception e)
			{
				ex = e;
			}

			ex.Should().NotBeNull();
			var clientException = ex.Should().BeOfType<OpenSearchClientException>().Subject;
			clientException.Message.Should().Contain("halted");

			// Initial attempt + 2 retries = 3 total requests
			requests.Should().Be(3);
			// OnNext only called for successful batches
			seenPages.Should().Be(0);
		}

		[U]
		public void RetryStrategyComputesExponentialBackoff()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var maxDelay = TimeSpan.FromSeconds(5);

			// Attempt 0: ~100ms * 1 = ~100ms
			var delay0 = RetryStrategy.ComputeDelay(0, baseDelay, maxDelay);
			delay0.TotalMilliseconds.Should().BeInRange(75, 125); // 100 * [0.75, 1.25]

			// Attempt 1: ~100ms * 2 = ~200ms
			var delay1 = RetryStrategy.ComputeDelay(1, baseDelay, maxDelay);
			delay1.TotalMilliseconds.Should().BeInRange(150, 250); // 200 * [0.75, 1.25]

			// Attempt 2: ~100ms * 4 = ~400ms
			var delay2 = RetryStrategy.ComputeDelay(2, baseDelay, maxDelay);
			delay2.TotalMilliseconds.Should().BeInRange(300, 500); // 400 * [0.75, 1.25]

			// Attempt 3: ~100ms * 8 = ~800ms
			var delay3 = RetryStrategy.ComputeDelay(3, baseDelay, maxDelay);
			delay3.TotalMilliseconds.Should().BeInRange(600, 1000); // 800 * [0.75, 1.25]
		}

		[U]
		public void RetryStrategyRespectsMaxDelay()
		{
			var baseDelay = TimeSpan.FromSeconds(1);
			var maxDelay = TimeSpan.FromSeconds(2);

			// Attempt 10: would be 1024s without cap
			var delay = RetryStrategy.ComputeDelay(10, baseDelay, maxDelay);
			delay.TotalSeconds.Should().BeLessOrEqualTo(2.0);
		}

		[U]
		public void RetryStrategyHandlesLargeAttemptNumbers()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var maxDelay = TimeSpan.FromSeconds(30);

			// Should not overflow or throw for large attempt numbers
			var delay = RetryStrategy.ComputeDelay(50, baseDelay, maxDelay);
			delay.TotalSeconds.Should().BeLessOrEqualTo(30.0);
			delay.TotalMilliseconds.Should().BeGreaterThan(0);
		}
	}
}
