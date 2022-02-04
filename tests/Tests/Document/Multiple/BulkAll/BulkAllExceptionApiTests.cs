/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Threading;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using OpenSearch.Net.VirtualizedCluster;
using FluentAssertions;
using Osc;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Core.Xunit;
using Tests.Domain.Extensions;

namespace Tests.Document.Multiple.BulkAll
{
	public class BulkAllExceptionApiTests : BulkAllApiTestsBase
	{
		public BulkAllExceptionApiTests(IntrusiveOperationCluster cluster) : base(cluster) { }

		[I] public void WaitBulkAllThrowsAndIsCaught()
		{
			var index = CreateIndexName();

			var size = 1000;
			var pages = 10;
			var seenPages = 0;
			var numberOfDocuments = size * pages;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);

			var tokenSource = new CancellationTokenSource();
			var observableBulk = Client.BulkAll(documents, f => f
					.MaxDegreeOfParallelism(4)
					.BackOffTime(TimeSpan.FromSeconds(10))
					.BackOffRetries(2)
					.Size(size)
					.RefreshOnCompleted()
					.Index(index)
					.BufferToBulk((r, buffer) => r.IndexMany(buffer))
				, tokenSource.Token);

			try
			{
				observableBulk.Wait(TimeSpan.FromSeconds(30), b =>
				{
					if (seenPages == 8) throw new Exception("boom");

					Interlocked.Increment(ref seenPages);
				});
			}
			catch (Exception e)
			{
				seenPages.Should().Be(8);
				e.Message.Should().Be("boom");
			}
		}
	}


	[SkipOnCi] //TODO fails on canary windows only, need to come back to this one
	[SkipAttribute("Test fails after upgrading to .NET Core 3.0 on .NET 4.6.1 - only sees 1 request. Needs investigation")]
	public class BulkAllBadRetriesApiTests : BulkAllApiTestsBase
	{
		public BulkAllBadRetriesApiTests(IntrusiveOperationCluster cluster) : base(cluster) { }

		[U] public void Completes()
		{
			var cluster = VirtualClusterWith.Nodes(2)
				.ClientCalls(c => c.FailAlways())
				.StaticConnectionPool()
				.AllDefaults();

			var settings = new ConnectionSettings(cluster.ConnectionPool, cluster.Connection).ApplyDomainSettings();
			var client = new OpenSearchClient(settings);

			var index = CreateIndexName();

			var size = 1000;
			var pages = 10;
			var seenPages = 0;
			var numberOfDocuments = size * pages;
			var documents = CreateLazyStreamOfDocuments(numberOfDocuments);
			var requests = 0;

			Exception ex = null;
			var tokenSource = new CancellationTokenSource();
			var observableBulk = client.BulkAll(documents, f => f
					.MaxDegreeOfParallelism(1)
					.BulkResponseCallback(r => Interlocked.Increment(ref requests))
					.BackOffTime(TimeSpan.FromMilliseconds(1))
					.BackOffRetries(2)
					.Size(size)
					.RefreshOnCompleted()
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

			clientException.Message.Should()
				.StartWith("BulkAll halted after");

			requests.Should().Be(3);
			// OnNext only called for successful batches.
			seenPages.Should().Be(0);
		}
	}
}
