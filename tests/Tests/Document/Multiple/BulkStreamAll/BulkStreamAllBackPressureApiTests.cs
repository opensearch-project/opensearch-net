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
	public class BulkStreamAllBackPressureApiTests : BulkStreamAllApiTestsBase
	{
		public BulkStreamAllBackPressureApiTests(IntrusiveOperationCluster cluster) : base(cluster) { }

		[I]
		public void BackPressureThrottlesIngestion()
		{
			var index = CreateIndexName();

			var size = 100;
			var numberOfDocuments = 1000;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);
			var seenPages = 0;

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(2)
				.BackPressure(2, 2) // tight backpressure
				.Size(size)
				.Index(index)
			);

			var observer = observableBulk.Wait(TimeSpan.FromSeconds(60), b =>
			{
				Interlocked.Increment(ref seenPages);
			});

			// All documents should eventually be processed despite backpressure
			seenPages.Should().Be(10); // 1000 / 100
			observer.TotalNumberOfFailedBuffers.Should().Be(0);
		}

		[I]
		public void BackPressureWithAffinityCompletes()
		{
			var index = CreateIndexName();

			var size = 50;
			var numberOfDocuments = 500;
			var distinctKeys = 5;
			var documents = CreateDocumentsWithAffinityKeys(numberOfDocuments, distinctKeys);
			var seenPages = 0;

			var observableBulk = Client.BulkStreamAll(documents, f => f
				.MaxDegreeOfParallelism(4)
				.BackPressure(4, 2)
				.Size(size)
				.Index(index)
				.DocumentAffinityKey(doc => doc.Name)
			);

			var observer = observableBulk.Wait(TimeSpan.FromSeconds(60), b =>
			{
				Interlocked.Increment(ref seenPages);
			});

			// All documents should be processed
			seenPages.Should().BeGreaterThan(0);
			observer.TotalNumberOfFailedBuffers.Should().Be(0);
		}
	}
}
