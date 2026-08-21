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
	public class BulkStreamAllDescriptor<T> : DescriptorBase<BulkStreamAllDescriptor<T>, IBulkStreamAllRequest<T>>, IBulkStreamAllRequest<T>, IHelperCallable
		where T : class
	{
		private readonly IEnumerable<T> _documents;

		public BulkStreamAllDescriptor(IEnumerable<T> documents)
		{
			_documents = documents;
			((IBulkStreamAllRequest<T>)this).Index = typeof(T);
		}

		int? IBulkStreamAllRequest<T>.MaxRetries { get; set; }
		TimeSpan? IBulkStreamAllRequest<T>.RetryBaseDelay { get; set; }
		TimeSpan? IBulkStreamAllRequest<T>.RetryMaxDelay { get; set; }
		ProducerConsumerBackPressure IBulkStreamAllRequest<T>.BackPressure { get; set; }
		Action<BulkStreamDescriptor, IList<T>> IBulkStreamAllRequest<T>.BufferToBulk { get; set; }
		bool IBulkStreamAllRequest<T>.ContinueAfterDroppedDocuments { get; set; }
		IEnumerable<T> IBulkStreamAllRequest<T>.Documents => _documents;
		Action<BulkResponseItemBase, T> IBulkStreamAllRequest<T>.DroppedDocumentCallback { get; set; }
		IndexName IBulkStreamAllRequest<T>.Index { get; set; }
		int? IBulkStreamAllRequest<T>.MaxDegreeOfParallelism { get; set; }
		string IBulkStreamAllRequest<T>.Pipeline { get; set; }
		Indices IBulkStreamAllRequest<T>.RefreshIndices { get; set; }
		bool IBulkStreamAllRequest<T>.RefreshOnCompleted { get; set; }
		Func<BulkResponseItemBase, T, bool> IBulkStreamAllRequest<T>.RetryDocumentPredicate { get; set; }
		Routing IBulkStreamAllRequest<T>.Routing { get; set; }
		int? IBulkStreamAllRequest<T>.Size { get; set; }
		Time IBulkStreamAllRequest<T>.Timeout { get; set; }
		int? IBulkStreamAllRequest<T>.WaitForActiveShards { get; set; }
		Action<BulkStreamResponse> IBulkStreamAllRequest<T>.BulkResponseCallback { get; set; }
		Func<T, string> IBulkStreamAllRequest<T>.DocumentAffinityKey { get; set; }
		TimeSpan? IBulkStreamAllRequest<T>.FlushInterval { get; set; }
		RequestMetaData IHelperCallable.ParentMetaData { get; set; }

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.MaxDegreeOfParallelism" />
		public BulkStreamAllDescriptor<T> MaxDegreeOfParallelism(int? parallelism) =>
			Assign(parallelism, (a, v) => a.MaxDegreeOfParallelism = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.Size" />
		public BulkStreamAllDescriptor<T> Size(int? size) => Assign(size, (a, v) => a.Size = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.MaxRetries" />
		public BulkStreamAllDescriptor<T> MaxRetries(int? retries) =>
			Assign(retries, (a, v) => a.MaxRetries = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.RetryBaseDelay" />
		public BulkStreamAllDescriptor<T> RetryBaseDelay(TimeSpan? delay) =>
			Assign(delay, (a, v) => a.RetryBaseDelay = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.RetryMaxDelay" />
		public BulkStreamAllDescriptor<T> RetryMaxDelay(TimeSpan? delay) =>
			Assign(delay, (a, v) => a.RetryMaxDelay = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.Index" />
		public BulkStreamAllDescriptor<T> Index(IndexName index) => Assign(index, (a, v) => a.Index = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.Index" />
		public BulkStreamAllDescriptor<T> Index<TOther>() where TOther : class => Assign(typeof(TOther), (a, v) => a.Index = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.RefreshOnCompleted" />
		public BulkStreamAllDescriptor<T> RefreshOnCompleted(bool refresh = true) => Assign(refresh, (a, v) => a.RefreshOnCompleted = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.RefreshIndices" />
		public BulkStreamAllDescriptor<T> RefreshIndices(Indices indicesToRefresh) => Assign(indicesToRefresh, (a, v) => a.RefreshIndices = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.Routing" />
		public BulkStreamAllDescriptor<T> Routing(Routing routing) => Assign(routing, (a, v) => a.Routing = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.Timeout" />
		public BulkStreamAllDescriptor<T> Timeout(Time timeout) => Assign(timeout, (a, v) => a.Timeout = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.Pipeline" />
		public BulkStreamAllDescriptor<T> Pipeline(string pipeline) => Assign(pipeline, (a, v) => a.Pipeline = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.BufferToBulk" />
		public BulkStreamAllDescriptor<T> BufferToBulk(Action<BulkStreamDescriptor, IList<T>> modifier) =>
			Assign(modifier, (a, v) => a.BufferToBulk = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.RetryDocumentPredicate" />
		public BulkStreamAllDescriptor<T> RetryDocumentPredicate(Func<BulkResponseItemBase, T, bool> predicate) =>
			Assign(predicate, (a, v) => a.RetryDocumentPredicate = v);

		/// <summary>
		/// Simple back pressure implementation that makes sure the minimum max concurrency between producer and consumer
		/// is not amplified by the greedier of the two by more than a given back pressure factor.
		/// </summary>
		/// <param name="maxConcurrency">The minimum maximum concurrency which would be the bottleneck of the producer consumer pipeline.</param>
		/// <param name="backPressureFactor">The maximum amplification back pressure of the greedier part of the producer consumer pipeline.</param>
		public BulkStreamAllDescriptor<T> BackPressure(int maxConcurrency, int? backPressureFactor = null) =>
			Assign(new ProducerConsumerBackPressure(backPressureFactor, maxConcurrency), (a, v) => a.BackPressure = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.ContinueAfterDroppedDocuments" />
		public BulkStreamAllDescriptor<T> ContinueAfterDroppedDocuments(bool proceed = true) =>
			Assign(proceed, (a, v) => a.ContinueAfterDroppedDocuments = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.DroppedDocumentCallback" />
		public BulkStreamAllDescriptor<T> DroppedDocumentCallback(Action<BulkResponseItemBase, T> callback) =>
			Assign(callback, (a, v) => a.DroppedDocumentCallback = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.BulkResponseCallback" />
		public BulkStreamAllDescriptor<T> BulkResponseCallback(Action<BulkStreamResponse> callback) =>
			Assign(callback, (a, v) => a.BulkResponseCallback = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.DocumentAffinityKey" />
		public BulkStreamAllDescriptor<T> DocumentAffinityKey(Func<T, string> keySelector) =>
			Assign(keySelector, (a, v) => a.DocumentAffinityKey = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.FlushInterval" />
		public BulkStreamAllDescriptor<T> FlushInterval(TimeSpan? interval) =>
			Assign(interval, (a, v) => a.FlushInterval = v);

		/// <inheritdoc cref="IBulkStreamAllRequest{T}.WaitForActiveShards" />
		public BulkStreamAllDescriptor<T> WaitForActiveShards(int? shards) =>
			Assign(shards, (a, v) => a.WaitForActiveShards = v);
	}
}
