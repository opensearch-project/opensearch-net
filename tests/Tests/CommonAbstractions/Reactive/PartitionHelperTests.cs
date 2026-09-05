/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.Linq;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;

namespace Tests.CommonAbstractions.Reactive
{
	// PartitionHelper splits a (possibly lazy) sequence into fixed-size batches; it backs the round-robin dispatch
	// path of the bulk helpers. It had no direct coverage, so pin its batching contract here.
	public class PartitionHelperTests
	{
		private static List<IList<int>> Partition(IEnumerable<int> items, int size) =>
			new PartitionHelper<int>(items, size).ToList();

		[U]
		public void SplitsIntoFullBatchesWithARemainder()
		{
			var batches = Partition(Enumerable.Range(0, 7), 3);

			batches.Should().HaveCount(3);
			batches[0].Should().Equal(0, 1, 2);
			batches[1].Should().Equal(3, 4, 5);
			batches[2].Should().Equal(6); // the trailing partial batch
		}

		[U]
		public void ExactMultipleProducesNoTrailingEmptyBatch()
		{
			var batches = Partition(Enumerable.Range(0, 6), 3);

			batches.Should().HaveCount(2);
			batches[1].Should().Equal(3, 4, 5);
			batches.Should().OnlyContain(b => b.Count == 3);
		}

		[U]
		public void PartitionSizeOfOnePlacesEachItemInItsOwnBatch()
		{
			var batches = Partition(Enumerable.Range(0, 4), 1);

			batches.Should().HaveCount(4);
			batches.SelectMany(b => b).Should().Equal(0, 1, 2, 3);
			batches.Should().OnlyContain(b => b.Count == 1);
		}

		[U]
		public void EmptySourceYieldsNoBatches()
		{
			var batches = Partition(Enumerable.Empty<int>(), 3);

			batches.Should().BeEmpty();
		}

		[U]
		public void FewerItemsThanPartitionSizeYieldsASinglePartialBatch()
		{
			var batches = Partition(Enumerable.Range(0, 2), 5);

			batches.Should().ContainSingle();
			batches[0].Should().Equal(0, 1);
		}

		[U]
		public void PreservesOrderAndAllItemsAcrossBatches()
		{
			var source = Enumerable.Range(0, 100).ToArray();

			var flattened = Partition(source, 7).SelectMany(b => b).ToArray();

			flattened.Should().Equal(source, "batching must neither drop, duplicate, nor reorder items");
		}

		[U]
		public void DoesNotEnumerateTheSourceUntilIterated()
		{
			var enumerated = false;

			IEnumerable<int> LazySource()
			{
				enumerated = true;
				yield return 1;
			}

			// Constructing the helper must not pull from the source; enumeration is deferred until iterated.
			var helper = new PartitionHelper<int>(LazySource(), 3);
			enumerated.Should().BeFalse();

			helper.ToList();
			enumerated.Should().BeTrue();
		}
	}
}
