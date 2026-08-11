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
	public class BulkStreamAllObserver : CoordinatedRequestObserverBase<BulkStreamAllResponse>
	{
		private long _totalNumberOfFailedBuffers;
		private long _totalNumberOfRetries;
		private long _totalDocumentsProcessed;

		public BulkStreamAllObserver(
			Action<BulkStreamAllResponse> onNext = null,
			Action<Exception> onError = null,
			Action onCompleted = null
		)
			: base(onNext, onError, onCompleted) { }

		public long TotalNumberOfFailedBuffers => _totalNumberOfFailedBuffers;

		public long TotalNumberOfRetries => _totalNumberOfRetries;

		public long TotalDocumentsProcessed => _totalDocumentsProcessed;

		internal void IncrementTotalNumberOfRetries() => Interlocked.Increment(ref _totalNumberOfRetries);

		internal void IncrementTotalNumberOfFailedBuffers() => Interlocked.Increment(ref _totalNumberOfFailedBuffers);

		internal void AddDocumentsProcessed(long count) => Interlocked.Add(ref _totalDocumentsProcessed, count);
	}
}
