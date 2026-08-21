/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	public interface IBulkStreamAllRequest<T> where T : class
	{
		/// <summary>In case of a retryable failure, how many times should we automatically retry before failing.</summary>
		int? MaxRetries { get; set; }

		/// <summary>Base delay for exponential backoff retry (default 1s).</summary>
		TimeSpan? RetryBaseDelay { get; set; }

		/// <summary>Maximum delay cap for exponential backoff (default 30s).</summary>
		TimeSpan? RetryMaxDelay { get; set; }

		/// <summary>
		/// Simple back pressure implementation that makes sure the minimum max concurrency between producer and consumer
		/// is not amplified by the greedier of the two by more than a given back pressure factor.
		/// When set, each bulk request will call <see cref="ProducerConsumerBackPressure.Release" />.
		/// </summary>
		ProducerConsumerBackPressure BackPressure { get; set; }

		/// <summary>
		/// By default, <see cref="BulkStreamAllObservable{T}" /> calls <see cref="BulkStreamDescriptor.IndexMany{T}" /> on the buffer.
		/// There might be cases where you'd like more control over the bulk operation. By setting this callback, you are in complete control
		/// of describing how the buffer should be translated to a bulk operation.
		/// </summary>
		Action<BulkStreamDescriptor, IList<T>> BufferToBulk { get; set; }

		/// <summary>
		/// Halt the bulk all request if any of the documents returned is a failure that can not be retried.
		/// When true, will feed dropped documents to <see cref="DroppedDocumentCallback" />.
		/// </summary>
		bool ContinueAfterDroppedDocuments { get; set; }

		/// <summary>
		/// The documents to send to OpenSearch, ideally lazily evaluated by using <see langword="yield" /> return.
		/// </summary>
		IEnumerable<T> Documents { get; }

		/// <summary>
		/// If a bulk operation fails because it receives documents it can not retry they will be fed to this callback.
		/// </summary>
		Action<BulkResponseItemBase, T> DroppedDocumentCallback { get; set; }

		/// <summary>The index to use for items that don't specify one.</summary>
		IndexName Index { get; set; }

		/// <summary>The maximum number of bulk operations we want to have in flight at a time.</summary>
		int? MaxDegreeOfParallelism { get; set; }

		/// <summary>The pipeline id to preprocess all the incoming documents with.</summary>
		string Pipeline { get; set; }

		/// <summary>The indices you wish to refresh after the bulk all completes, defaults to <see cref="Index" />.</summary>
		Indices RefreshIndices { get; set; }

		/// <summary>Refresh the index after performing ALL the bulk operations (NOTE this is an additional request).</summary>
		bool RefreshOnCompleted { get; set; }

		/// <summary>
		/// A predicate to control which documents should be retried.
		/// Defaults to failed bulk items with a HTTP 429 (Too Many Requests) response status code.
		/// </summary>
		Func<BulkResponseItemBase, T, bool> RetryDocumentPredicate { get; set; }

		/// <summary>Specific per bulk operation routing value.</summary>
		Routing Routing { get; set; }

		/// <summary>The number of documents to send per bulk request.</summary>
		int? Size { get; set; }

		/// <summary>Explicit per operation timeout.</summary>
		Time Timeout { get; set; }

		/// <summary>
		/// Sets the number of shard copies that must be active before proceeding with the bulk operation.
		/// </summary>
		int? WaitForActiveShards { get; set; }

		/// <summary>
		/// Be notified every time a bulk response returns, this includes retries.
		/// </summary>
		Action<BulkStreamResponse> BulkResponseCallback { get; set; }

		/// <summary>
		/// Function to extract a routing key from a document for worker affinity.
		/// Documents with the same key always go to the same worker, preserving order.
		/// Null = round-robin (no ordering guarantee).
		/// </summary>
		Func<T, string> DocumentAffinityKey { get; set; }

		/// <summary>
		/// Time-based flush interval. When set, each worker will flush its buffer at least this often
		/// even if the batch size has not been reached. Null = no timer-based flush.
		/// </summary>
		TimeSpan? FlushInterval { get; set; }
	}

	public class BulkStreamAllRequest<T> : IBulkStreamAllRequest<T>, IHelperCallable
		where T : class
	{
		public BulkStreamAllRequest(IEnumerable<T> documents)
		{
			Documents = documents;
			Index = typeof(T);
		}

		/// <inheritdoc />
		public int? MaxRetries { get; set; }

		/// <inheritdoc />
		public TimeSpan? RetryBaseDelay { get; set; }

		/// <inheritdoc />
		public TimeSpan? RetryMaxDelay { get; set; }

		/// <inheritdoc />
		public ProducerConsumerBackPressure BackPressure { get; set; }

		/// <inheritdoc />
		public Action<BulkStreamDescriptor, IList<T>> BufferToBulk { get; set; }

		/// <inheritdoc />
		public bool ContinueAfterDroppedDocuments { get; set; }

		/// <inheritdoc />
		public IEnumerable<T> Documents { get; }

		/// <inheritdoc />
		public Action<BulkResponseItemBase, T> DroppedDocumentCallback { get; set; }

		/// <inheritdoc />
		public IndexName Index { get; set; }

		/// <inheritdoc />
		public int? MaxDegreeOfParallelism { get; set; }

		/// <inheritdoc />
		public string Pipeline { get; set; }

		/// <inheritdoc />
		public Indices RefreshIndices { get; set; }

		/// <inheritdoc />
		public bool RefreshOnCompleted { get; set; }

		/// <inheritdoc />
		public Func<BulkResponseItemBase, T, bool> RetryDocumentPredicate { get; set; }

		/// <inheritdoc />
		public Routing Routing { get; set; }

		/// <inheritdoc />
		public int? Size { get; set; }

		/// <inheritdoc />
		public Time Timeout { get; set; }

		/// <inheritdoc />
		public int? WaitForActiveShards { get; set; }

		/// <inheritdoc />
		public Action<BulkStreamResponse> BulkResponseCallback { get; set; }

		/// <inheritdoc />
		public Func<T, string> DocumentAffinityKey { get; set; }

		/// <inheritdoc />
		public TimeSpan? FlushInterval { get; set; }

		internal RequestMetaData ParentMetaData { get; set; }

		RequestMetaData IHelperCallable.ParentMetaData { get => ParentMetaData; set => ParentMetaData = value; }
	}
}
