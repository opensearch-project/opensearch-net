/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Threading;

namespace OpenSearch.Client
{
	public static class BulkStreamAllExtensions
	{
		/// <summary>
		/// Subscribes to the <see cref="BulkStreamAllObservable{T}"/> and blocks until completion or timeout.
		/// </summary>
		/// <param name="observable">The bulk stream all observable to wait on.</param>
		/// <param name="maximumRunTime">The maximum time to wait for the operation to complete.</param>
		/// <param name="onNext">An action called for each successful batch.</param>
		/// <returns>The observer containing summary statistics.</returns>
		public static BulkStreamAllObserver Wait<T>(
			this BulkStreamAllObservable<T> observable,
			TimeSpan maximumRunTime,
			Action<BulkStreamAllResponse> onNext
		) where T : class
		{
			observable.ThrowIfNull(nameof(observable));
			maximumRunTime.ThrowIfNull(nameof(maximumRunTime));

			Exception exception = null;
			var handle = new ManualResetEvent(false);
			var observer = new BulkStreamAllObserver(
				onNext,
				e =>
				{
					exception = e;
					handle.Set();
				},
				() => handle.Set()
			);

			observable.Subscribe(observer);
			handle.WaitOne(maximumRunTime);

			if (exception != null) throw exception;

			return observer;
		}
	}
}
