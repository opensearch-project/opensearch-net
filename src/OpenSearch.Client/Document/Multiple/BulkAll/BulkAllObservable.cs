/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	public class BulkAllObservable<T> : IDisposable, IObservable<BulkAllResponse> where T : class
	{
		// How many batches a single affinity worker may have accepted but not yet completed (one dispatching plus the
		// rest queued) before the producer must wait on that worker. Bounds memory while giving the producer enough
		// runway to keep feeding the other workers when one worker is slow or retrying.
		private const int AffinityWorkerQueueDepthDefault = 2;

		private readonly int _backOffRetries;
		private readonly TimeSpan _backOffTime;
		private readonly TimeSpan? _retryBaseDelay;
		private readonly TimeSpan? _retryMaxDelay;
		private readonly int _bulkSize;
		private readonly IOpenSearchClient _client;

		private readonly CancellationToken _compositeCancelToken;
		private readonly CancellationTokenSource _compositeCancelTokenSource;
		private readonly Action<BulkResponseItemBase, T> _droppedDocumentCallBack;
		private readonly int _maxDegreeOfParallelism;
		private readonly IBulkAllRequest<T> _partitionedBulkRequest;
		private readonly Func<BulkResponseItemBase, T, bool> _retryPredicate;
		private readonly Func<T, string> _affinityKeySelector;
		private Action _incrementFailed = () => { };
		private Action _incrementRetries = () => { };
		private Action<long> _addDocumentsProcessed = _ => { };
		private readonly Action<BulkResponse> _bulkResponseCallback;

		public BulkAllObservable(
			IOpenSearchClient client,
			IBulkAllRequest<T> partitionedBulkRequest,
			CancellationToken cancellationToken = default
		)
		{
			_client = client;
			_partitionedBulkRequest = partitionedBulkRequest;
			_backOffRetries = _partitionedBulkRequest.BackOffRetries.GetValueOrDefault(CoordinatedRequestDefaults.BulkAllBackOffRetriesDefault);
			_backOffTime = _partitionedBulkRequest?.BackOffTime?.ToTimeSpan() ?? CoordinatedRequestDefaults.BulkAllBackOffTimeDefault;
			_retryBaseDelay = _partitionedBulkRequest.RetryBaseDelay;
			_retryMaxDelay = _partitionedBulkRequest.RetryMaxDelay;
			// Clamp to at least 1: a Size <= 0 makes PartitionHelper loop forever on empty batches, and a
			// MaxDegreeOfParallelism <= 0 divides by zero when routing / round-robining.
			_bulkSize = Math.Max(1, _partitionedBulkRequest.Size ?? CoordinatedRequestDefaults.BulkAllSizeDefault);
			_retryPredicate = _partitionedBulkRequest.RetryDocumentPredicate ?? RetryBulkActionPredicate;
			_droppedDocumentCallBack = _partitionedBulkRequest.DroppedDocumentCallback ?? DroppedDocumentCallbackDefault;
			_bulkResponseCallback = _partitionedBulkRequest.BulkResponseCallback;
			_affinityKeySelector = _partitionedBulkRequest.DocumentAffinityKey;

			_maxDegreeOfParallelism = Math.Max(1,
				_partitionedBulkRequest.MaxDegreeOfParallelism ?? CoordinatedRequestDefaults.BulkAllMaxDegreeOfParallelismDefault);
			_compositeCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			_compositeCancelToken = _compositeCancelTokenSource.Token;
		}

		public void Dispose()
		{
			_compositeCancelTokenSource?.Cancel();
			_compositeCancelTokenSource?.Dispose();
		}

		public IDisposable Subscribe(IObserver<BulkAllResponse> observer)
		{
			observer.ThrowIfNull(nameof(observer));
			BulkAll(observer);
			return this;
		}

		public IDisposable Subscribe(BulkAllObserver observer)
		{
			_incrementFailed = observer.IncrementTotalNumberOfFailedBuffers;
			_incrementRetries = observer.IncrementTotalNumberOfRetries;
			_addDocumentsProcessed = observer.AddDocumentsProcessed;
			return Subscribe((IObserver<BulkAllResponse>)observer);
		}

		private void BulkAll(IObserver<BulkAllResponse> observer)
		{
			if (_affinityKeySelector != null)
			{
#pragma warning disable 4014
				RunAffinityAsync(observer);
#pragma warning restore 4014
				return;
			}

			var documents = _partitionedBulkRequest.Documents;
			var partitioned = new PartitionHelper<T>(documents, _bulkSize);
#pragma warning disable 4014
			partitioned.ForEachAsync(
#pragma warning restore 4014
				(buffer, page) => BulkAsync(buffer, page, 0, (int)(page % _maxDegreeOfParallelism)),
				(buffer, response) =>
				{
					_addDocumentsProcessed(buffer.Count);
					observer.OnNext(response);
				},
				ex => OnCompleted(ex, observer),
				_maxDegreeOfParallelism
			);
		}

		// Affinity dispatch: hash each document's key to a worker so operations sharing a key always land on the same
		// worker, and give every worker its own bounded queue drained by a dedicated consumer. Each consumer dispatches
		// its batches strictly FIFO with at most one in flight, preserving on-the-wire ordering for a key. The producer
		// only ever waits on the one worker whose queue is full, so a slow or backing-off batch holds up only its own
		// key while the remaining workers keep draining.
		private async Task RunAffinityAsync(IObserver<BulkAllResponse> observer)
		{
			var observerLock = new object();
			Exception dispatchException = null;

			async Task DispatchAsync(IList<T> batch, long page, int workerIndex)
			{
				try
				{
					var result = await BulkAsync(batch, page, 0, workerIndex).ConfigureAwait(false);
					if (result != null)
					{
						_addDocumentsProcessed(batch.Count);
						lock (observerLock)
							observer.OnNext(result);
					}
				}
				catch (Exception ex)
				{
					// Record the first failure and cancel so the producer and any in-flight batches stop promptly.
					Interlocked.CompareExchange(ref dispatchException, ex, null);
					try { _compositeCancelTokenSource.Cancel(); }
					catch (ObjectDisposedException) { /* already disposed; nothing left to cancel */ }
					throw;
				}
			}

			try
			{
				await RunWithAffinityAsync(DispatchAsync).ConfigureAwait(false);
				OnCompleted(null, observer);
			}
			catch (Exception ex)
			{
				OnCompleted(dispatchException ?? ex, observer);
			}
		}

		private async Task RunWithAffinityAsync(Func<IList<T>, long, int, Task> dispatch)
		{
			var buffers = new List<T>[_maxDegreeOfParallelism];
			var pageCounters = new long[_maxDegreeOfParallelism];
			var queues = new AffinityWorkerQueue[_maxDegreeOfParallelism];
			var consumers = new Task[_maxDegreeOfParallelism];

			for (var i = 0; i < _maxDegreeOfParallelism; i++)
			{
				buffers[i] = new List<T>(_bulkSize);
				queues[i] = new AffinityWorkerQueue(AffinityWorkerQueueDepthDefault);
				var workerIndex = i;
				consumers[i] = ConsumeWorkerAsync(queues[workerIndex], dispatch, workerIndex);
			}

			try
			{
				try
				{
					foreach (var document in _partitionedBulkRequest.Documents)
					{
						_compositeCancelToken.ThrowIfCancellationRequested();

						var key = _affinityKeySelector(document);
						var workerIndex = (int)((uint)GetStableHashCode(key) % (uint)_maxDegreeOfParallelism);
						buffers[workerIndex].Add(document);

						if (buffers[workerIndex].Count < _bulkSize) continue;

						var batch = new List<T>(buffers[workerIndex]);
						buffers[workerIndex].Clear();
						await queues[workerIndex].EnqueueAsync(batch, pageCounters[workerIndex]++, _compositeCancelToken).ConfigureAwait(false);
					}

					// Flush any partially-filled buffers, then signal each worker that no more batches are coming.
					for (var i = 0; i < _maxDegreeOfParallelism; i++)
					{
						if (buffers[i].Count > 0)
							await queues[i].EnqueueAsync(buffers[i], pageCounters[i]++, _compositeCancelToken).ConfigureAwait(false);
						queues[i].Complete();
					}

					await Task.WhenAll(consumers).ConfigureAwait(false);
				}
				catch
				{
					// Unblock any worker still waiting for input, then observe every consumer so a faulted batch never
					// surfaces as an unobserved task exception.
					for (var i = 0; i < _maxDegreeOfParallelism; i++)
						queues[i].Complete();
					try { await Task.WhenAll(consumers).ConfigureAwait(false); }
					catch { /* the originating failure is surfaced by the caller */ }
					throw;
				}
			}
			finally
			{
				for (var i = 0; i < _maxDegreeOfParallelism; i++)
					queues[i].Dispose();
			}
		}

		// Drains one worker's queue, dispatching each batch to completion before taking the next so that at most one
		// batch per worker is ever in flight and batches complete in the order they were enqueued.
		private async Task ConsumeWorkerAsync(AffinityWorkerQueue queue, Func<IList<T>, long, int, Task> dispatch, int workerIndex)
		{
			while (true)
			{
				var next = await queue.DequeueAsync(_compositeCancelToken).ConfigureAwait(false);
				if (next == null) break; // queue completed and drained

				// Do NOT acquire from BackPressure here. ProducerConsumerBackPressure is directional: in a Reindex the
				// scroll producer acquires a slot per page and the bulk side repays via Release() per completed batch
				// (BulkAsync / ThrowOnBadBulk). A consumer-side WaitAsync would add a second acquirer to that same pool
				// while a batch still repays at most backPressureFactor slots, so any config where a batch spans more
				// pages than the factor repays (a legal Reindex setup, e.g. searchSize=10, Size=1000, factor<100) would
				// drain the pool to zero and deadlock the reindex — the scroll blocked in WaitAsync, this worker holding
				// a ready batch, and nothing in flight to ever Release. Per-worker concurrency is already bounded to one
				// in-flight batch by AffinityWorkerQueue, matching the round-robin path which likewise only Releases.
				try
				{
					await dispatch(next.Value.Batch, next.Value.Page, workerIndex).ConfigureAwait(false);
				}
				finally
				{
					// Free the worker's slot only once the batch is fully done (including retries), so a slow or
					// backing-off batch counts against this worker's depth and eventually backpressures the producer
					// for this key alone — never for the other workers.
					queue.ReleaseSlot();
				}
			}
		}

		private void OnCompleted(Exception exception, IObserver<BulkAllResponse> observer)
		{
			if (exception != null)
				observer.OnError(exception);
			else
			{
				try
				{
					RefreshOnCompleted();
					observer.OnCompleted();
				}
				catch (Exception e)
				{
					observer.OnError(e);
				}
			}
		}

		private void RefreshOnCompleted()
		{
			if (!_partitionedBulkRequest.RefreshOnCompleted) return;

			var indices = _partitionedBulkRequest.RefreshIndices ?? _partitionedBulkRequest.Index;
			if (indices == null) return;

			var refresh = _client.Indices.Refresh(indices, r => r.RequestConfiguration(rc =>
			{
				switch (_partitionedBulkRequest)
				{
					case IHelperCallable helperCallable when helperCallable.ParentMetaData is object:
						rc.RequestMetaData(helperCallable.ParentMetaData);
						break;
					default:
						rc.RequestMetaData(RequestMetaDataFactory.BulkHelperRequestMetaData());
						break;
				}

				return rc;
			}));
			if (!refresh.IsValid) throw Throw($"Refreshing after all documents have indexed failed", refresh.ApiCall);
		}

		private async Task<BulkAllResponse> BulkAsync(IList<T> buffer, long page, int backOffRetries, int workerIndex)
		{
			_compositeCancelToken.ThrowIfCancellationRequested();

			var request = _partitionedBulkRequest;
			var (apiCall, items) = await SendBatchAsync(buffer).ConfigureAwait(false);

			_compositeCancelToken.ThrowIfCancellationRequested();

			if (!apiCall.Success)
				return await HandleBulkRequest(buffer, page, backOffRetries, workerIndex, apiCall).ConfigureAwait(false);

			var retryableDocuments = new List<T>();
			var droppedDocuments = new List<Tuple<BulkResponseItemBase, T>>();

			foreach (var documentWithResponse in items.Zip(buffer, Tuple.Create))
			{
				if (documentWithResponse.Item1.IsValid) continue;

				if (_retryPredicate(documentWithResponse.Item1, documentWithResponse.Item2))
					retryableDocuments.Add(documentWithResponse.Item2);
				else
					droppedDocuments.Add(documentWithResponse);
			}

			HandleDroppedDocuments(droppedDocuments, apiCall);

			if (retryableDocuments.Count > 0 && backOffRetries < _backOffRetries)
				return await RetryDocuments(page, ++backOffRetries, retryableDocuments, workerIndex).ConfigureAwait(false);

			if (retryableDocuments.Count > 0)
				throw ThrowOnBadBulk(apiCall, $"Bulk indexing failed and after retrying {backOffRetries} times");

			request.BackPressure?.Release();

			return new BulkAllResponse { Retries = backOffRetries, Page = page, WorkerIndex = workerIndex, Items = items };
		}

		// Sends a single buffer to _bulk and projects the response onto the common (ApiCall, Items) shape the
		// orchestrator works with.
		private async Task<(IApiCallDetails ApiCall, IReadOnlyCollection<BulkResponseItemBase> Items)> SendBatchAsync(IList<T> buffer)
		{
			var request = _partitionedBulkRequest;

			var parentMetaData = (_partitionedBulkRequest as IHelperCallable)?.ParentMetaData;
			var metaData = parentMetaData ?? RequestMetaDataFactory.BulkHelperRequestMetaData();

			var response = await _client.BulkAsync(s =>
				{
					s.Index(request.Index);
					s.Timeout(request.Timeout);
					if (request.BufferToBulk != null) request.BufferToBulk(s, buffer);
					else s.IndexMany(buffer);
					if (!string.IsNullOrEmpty(request.Pipeline)) s.Pipeline(request.Pipeline);
					if (request.Routing != null) s.Routing(request.Routing);
					if (request.WaitForActiveShards.HasValue) s.WaitForActiveShards(request.WaitForActiveShards.ToString());
					s.RequestConfiguration(rc => rc.RequestMetaData(metaData));
					return s;
				}, _compositeCancelToken)
				.ConfigureAwait(false);

			_bulkResponseCallback?.Invoke(response);
			return (response.ApiCall, response.Items);
		}

		private void HandleDroppedDocuments(List<Tuple<BulkResponseItemBase, T>> droppedDocuments, IApiCallDetails apiCall)
		{
			if (droppedDocuments.Count <= 0) return;

			foreach (var dropped in droppedDocuments) _droppedDocumentCallBack(dropped.Item1, dropped.Item2);
			if (!_partitionedBulkRequest.ContinueAfterDroppedDocuments)
				throw ThrowOnBadBulk(apiCall, $"{nameof(BulkAll)} halted after receiving failures that can not be retried from _bulk");
		}

		private async Task<BulkAllResponse> HandleBulkRequest(IList<T> buffer, long page, int backOffRetries, int workerIndex, IApiCallDetails apiCall)
		{
			var clientException = apiCall.OriginalException as OpenSearchClientException;
			var failureReason = clientException?.FailureReason;
			var reason = failureReason?.GetStringValue() ?? nameof(PipelineFailure.BadRequest);
			switch (failureReason)
			{
				case PipelineFailure.MaxRetriesReached:
					if (apiCall.AuditTrail.Last().Event == AuditEvent.FailedOverAllNodes)
						throw ThrowOnBadBulk(apiCall, $"{nameof(BulkAll)} halted after attempted bulk failed over all the active nodes");

					ThrowOnExhaustedRetries();
					return await RetryDocuments(page, ++backOffRetries, buffer, workerIndex).ConfigureAwait(false);
				case PipelineFailure.CouldNotStartSniffOnStartup:
				case PipelineFailure.BadAuthentication:
				case PipelineFailure.NoNodesAttempted:
				case PipelineFailure.SniffFailure:
				case PipelineFailure.Unexpected:
					throw ThrowOnBadBulk(apiCall, $"{nameof(BulkAll)} halted after {nameof(PipelineFailure)}.{reason} from _bulk");
				case PipelineFailure.BadResponse:
				case PipelineFailure.PingFailure:
				case PipelineFailure.MaxTimeoutReached:
				case PipelineFailure.BadRequest:
				default:
					ThrowOnExhaustedRetries();
					return await RetryDocuments(page, ++backOffRetries, buffer, workerIndex).ConfigureAwait(false);
			}

			void ThrowOnExhaustedRetries()
			{
				if (backOffRetries < _backOffRetries) return;

				throw ThrowOnBadBulk(apiCall,
					$"{nameof(BulkAll)} halted after {nameof(PipelineFailure)}.{reason} from _bulk and exhausting retries ({backOffRetries})"
				);
			}
		}

		private async Task<BulkAllResponse> RetryDocuments(long page, int backOffRetries, IList<T> retryDocuments, int workerIndex)
		{
			_incrementRetries();
			await Task.Delay(ComputeRetryDelay(backOffRetries), _compositeCancelToken).ConfigureAwait(false);
			return await BulkAsync(retryDocuments, page, backOffRetries, workerIndex).ConfigureAwait(false);
		}

		// When RetryBaseDelay is set, use exponential backoff with jitter (capped by RetryMaxDelay); otherwise preserve
		// the historical fixed BackOffTime delay. backOffRetries is the 1-based attempt number for this retry.
		private TimeSpan ComputeRetryDelay(int backOffRetries)
		{
			if (_retryBaseDelay == null) return _backOffTime;

			return RetryStrategy.ComputeDelay(backOffRetries - 1, _retryBaseDelay.Value, _retryMaxDelay ?? RetryStrategy.DefaultMaxDelay);
		}

		private Exception ThrowOnBadBulk(IApiCallDetails apiCall, string message)
		{
			_incrementFailed();
			_partitionedBulkRequest.BackPressure?.Release();
			return Throw(message, apiCall);
		}

		private static OpenSearchClientException Throw(string message, IApiCallDetails details) =>
			new OpenSearchClientException(PipelineFailure.BadResponse, message, details);


		private static bool RetryBulkActionPredicate(BulkResponseItemBase bulkResponseItem, T d) => bulkResponseItem.Status == 429;

		private static void DroppedDocumentCallbackDefault(BulkResponseItemBase bulkResponseItem, T d) { }

		/// <summary>
		/// A stable hash code implementation that is consistent across different .NET runtimes.
		/// (string.GetHashCode() is randomized in .NET Core).
		/// </summary>
		private static int GetStableHashCode(string str)
		{
			if (str == null) return 0;

			unchecked
			{
				var hash1 = 5381;
				var hash2 = hash1;

				for (var i = 0; i < str.Length && str[i] != '\0'; i += 2)
				{
					hash1 = ((hash1 << 5) + hash1) ^ str[i];
					if (i == str.Length - 1)
						break;
					hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
				}

				return hash1 + hash2 * 1566083941;
			}
		}

		// A bounded single-consumer FIFO queue of pending batches for one affinity worker. EnqueueAsync blocks the
		// producer only when this worker already holds `capacity` unfinished batches (queued plus in flight);
		// DequeueAsync yields batches in order and returns null once the queue has been completed and fully drained.
		// ReleaseSlot frees a slot when a batch finishes.
		private sealed class AffinityWorkerQueue : IDisposable
		{
			private readonly Queue<(IList<T> Batch, long Page)> _items = new Queue<(IList<T>, long)>();
			private readonly SemaphoreSlim _itemAvailable = new SemaphoreSlim(0);
			private readonly SemaphoreSlim _freeCapacity;
			private readonly object _gate = new object();
			private bool _completed;

			public AffinityWorkerQueue(int capacity) => _freeCapacity = new SemaphoreSlim(capacity, capacity);

			public async Task EnqueueAsync(IList<T> batch, long page, CancellationToken cancellationToken)
			{
				await _freeCapacity.WaitAsync(cancellationToken).ConfigureAwait(false);
				lock (_gate)
					_items.Enqueue((batch, page));
				_itemAvailable.Release();
			}

			public async Task<(IList<T> Batch, long Page)?> DequeueAsync(CancellationToken cancellationToken)
			{
				await _itemAvailable.WaitAsync(cancellationToken).ConfigureAwait(false);
				lock (_gate)
				{
					if (_items.Count == 0)
						return null; // woken by Complete() with nothing left to hand out
					return _items.Dequeue();
				}
			}

			// Returns a slot to the producer once a dequeued batch has finished processing.
			public void ReleaseSlot() => _freeCapacity.Release();

			public void Complete()
			{
				lock (_gate)
				{
					if (_completed) return;
					_completed = true;
				}
				_itemAvailable.Release(); // wake the consumer so it can observe completion once drained
			}

			public void Dispose()
			{
				_itemAvailable.Dispose();
				_freeCapacity.Dispose();
			}
		}
	}
}
