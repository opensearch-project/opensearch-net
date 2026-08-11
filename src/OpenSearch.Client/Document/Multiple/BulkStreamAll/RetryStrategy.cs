/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;

namespace OpenSearch.Client
{
	internal static class RetryStrategy
	{
		private static readonly Random Jitter = new Random();

		public static readonly TimeSpan DefaultBaseDelay = TimeSpan.FromSeconds(1);
		public static readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(30);

		/// <summary>
		/// Computes the backoff delay for a given retry attempt using exponential backoff with jitter.
		/// Delay = min(maxDelay, baseDelay * 2^attempt * jitter)
		/// where jitter is in [0.75, 1.25].
		/// </summary>
		public static TimeSpan ComputeDelay(int attempt, TimeSpan baseDelay, TimeSpan maxDelay)
		{
			// Prevent overflow: cap the exponent
			var exponent = Math.Min(attempt, 30);
			var exponentialMs = baseDelay.TotalMilliseconds * (1L << exponent);

			// Apply jitter: ±25%
			double jitterFactor;
			lock (Jitter)
			{
				jitterFactor = 0.75 + Jitter.NextDouble() * 0.5; // [0.75, 1.25]
			}

			var delayMs = exponentialMs * jitterFactor;
			var cappedMs = Math.Min(delayMs, maxDelay.TotalMilliseconds);

			return TimeSpan.FromMilliseconds(Math.Max(cappedMs, 0));
		}
	}
}
