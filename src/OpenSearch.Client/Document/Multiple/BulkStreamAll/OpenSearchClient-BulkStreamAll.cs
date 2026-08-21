/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenSearch.Client
{
	public partial interface IOpenSearchClient
	{
		/// <summary>
		/// BulkStreamAll is a generic helper that will partition any lazy stream of documents and send them to OpenSearch
		/// as concurrent bulk stream requests via the <c>_bulk/stream</c> endpoint.
		/// Features automatic batching, retry with exponential backoff, backpressure handling,
		/// document-ID affinity routing, and progress reporting.
		/// </summary>
		/// <param name="documents">The lazy stream of documents.</param>
		/// <param name="selector">A descriptor to configure the bulk stream all operation.</param>
		/// <param name="cancellationToken">A cancellation token to stop the operation.</param>
		BulkStreamAllObservable<T> BulkStreamAll<T>(
			IEnumerable<T> documents,
			Func<BulkStreamAllDescriptor<T>, IBulkStreamAllRequest<T>> selector,
			CancellationToken cancellationToken = default
		) where T : class;

		/// <summary>
		/// BulkStreamAll is a generic helper that will partition any lazy stream of documents and send them to OpenSearch
		/// as concurrent bulk stream requests via the <c>_bulk/stream</c> endpoint.
		/// </summary>
		BulkStreamAllObservable<T> BulkStreamAll<T>(
			IBulkStreamAllRequest<T> request,
			CancellationToken cancellationToken = default
		) where T : class;
	}

	public partial class OpenSearchClient
	{
		/// <inheritdoc />
		public BulkStreamAllObservable<T> BulkStreamAll<T>(
			IEnumerable<T> documents,
			Func<BulkStreamAllDescriptor<T>, IBulkStreamAllRequest<T>> selector,
			CancellationToken cancellationToken = default
		) where T : class =>
			BulkStreamAll(selector.InvokeOrDefault(new BulkStreamAllDescriptor<T>(documents)), cancellationToken);

		/// <inheritdoc />
		public BulkStreamAllObservable<T> BulkStreamAll<T>(
			IBulkStreamAllRequest<T> request,
			CancellationToken cancellationToken = default
		) where T : class =>
			new BulkStreamAllObservable<T>(this, request, cancellationToken);
	}
}
