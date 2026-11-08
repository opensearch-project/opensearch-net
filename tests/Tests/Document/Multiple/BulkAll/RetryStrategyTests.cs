/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using FluentAssertions;
using OpenSearch.Client;

namespace Tests.Document.Multiple.BulkAll
{
	// Exponential-backoff-with-jitter delay policy used by BulkAll when RetryBaseDelay is set.
	public class RetryStrategyTests
	{
		[U]
		public void ComputesExponentialBackoff()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var maxDelay = TimeSpan.FromSeconds(5);

			RetryStrategy.ComputeDelay(0, baseDelay, maxDelay).TotalMilliseconds.Should().BeInRange(75, 125);   // 100 * 2^0 * [0.75,1.25]
			RetryStrategy.ComputeDelay(1, baseDelay, maxDelay).TotalMilliseconds.Should().BeInRange(150, 250);  // 200
			RetryStrategy.ComputeDelay(2, baseDelay, maxDelay).TotalMilliseconds.Should().BeInRange(300, 500);  // 400
			RetryStrategy.ComputeDelay(3, baseDelay, maxDelay).TotalMilliseconds.Should().BeInRange(600, 1000); // 800
		}

		[U]
		public void RespectsMaxDelay()
		{
			var delay = RetryStrategy.ComputeDelay(10, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
			delay.TotalSeconds.Should().BeLessThanOrEqualTo(2.0);
		}

		[U]
		public void HandlesLargeAttemptNumbersWithoutOverflow()
		{
			var delay = RetryStrategy.ComputeDelay(50, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(30));
			delay.TotalSeconds.Should().BeLessThanOrEqualTo(30.0);
			delay.TotalMilliseconds.Should().BeGreaterThan(0);
		}
	}
}
