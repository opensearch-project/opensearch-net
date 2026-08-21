/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	internal static class BulkStreamAllDefaults
	{
		public const int MaxRetriesDefault = 3;
		public const int MaxDegreeOfParallelismDefault = 4;
		public const int SizeDefault = 1000;
	}

	public class BulkStreamAllObservable<T> : IDisposable, IObservable<BulkStreamAllResponse> where T : class
	{
		private readonly int _bulkSize;
		private readonly IOpenSearchClient _client;
		private readonly IBulkStreamAllRequest<T> _request;
		private readonly int _maxDegreeOfParallelism;
		private readonly Func<T, string> _affinityKeySelector;

		private readonly CancellationToken _compositeCancelToken;
		private readonly CancellationTokenSource _compositeCancelTokenSource;

		private Action _incrementFailed = () => { };
		private Action _incrementRetries = () => { };

		public BulkStreamAllObservable(
			IOpenSearchClient client,
			IBulkStreamAllRequest<T> request,
			CancellationToken cancellationToken = default
		)
		{
			_client = client;
			_request = request;
			_bulkSize = request.Size ?? BulkStreamAllDefaults.SizeDefault;
			_maxDegreeOfParallelism = request.MaxDegreeOfParallelism ?? BulkStreamAllDefaults.MaxDegreeOfParallelismDefault;
			_affinityKeySelector = request.DocumentAffinityKey;
			_compositeCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			_compositeCancelToken = _compositeCancelTokenSource.Token;
		}

		public void Dispose()
		{
			_compositeCancelTokenSource?.Cancel();
			_compositeCancelTokenSource?.Dispose();
		}

		public IDisposable Subscribe(IObserver<BulkStreamAllResponse> observer)
		{
			observer.ThrowIfNull(nameof(observer));
			BulkStreamAll(observer);
			return this;
		}

		public IDisposable Subscribe(BulkStreamAllObserver observer)
		{
			_incrementFailed = observer.IncrementTotalNumberOfFailedBuffers;
			_incrementRetries = observer.IncrementTotalNumberOfRetries;
			return Subscribe((IObserver<BulkStreamAllResponse>)observer);
		}

		private void BulkStreamAll(IObserver<BulkStreamAllResponse> observer)
		{
			var documents = _request.Documents;
			var partitioned = new PartitionHelper<T>(documents, _bulkSize);

			if (_affinityKeySelector != null)
			{
#pragma warning disable 4014
				RunWithAffinityAsync(observer);
#pragma warning restore 4014
			}
			else
			{
#pragma warning disable 4014
				partitioned.ForEachAsync(
#pragma warning restore 4014
					(buffer, page) => BulkAsync(buffer, page, 0, workerIndex: (int)(page % _maxDegreeOfParallelism)),
					(buffer, response) => observer.OnNext(response),
					ex => OnCompleted(ex, observer),
					_maxDegreeOfParallelism
				);
			}
		}

		private async Task RunWithAffinityAsync(IObserver<BulkStreamAllResponse> observer)
		{
			try
			{
				// Create per-worker buffers
				var workers = new List<T>[_maxDegreeOfParallelism];
				for (var i = 0; i < _maxDegreeOfParallelism; i++)
					workers[i] = new List<T>(_bulkSize);

				var pageCounters = new long[_maxDegreeOfParallelism];
				var pendingTasks = new List<Task<BulkStreamAllResponse>>();

				foreach (var document in _request.Documents)
				{
					_compositeCancelToken.ThrowIfCancellationRequested();

					// Hash-based routing
					var key = _affinityKeySelector(document);
					var workerIndex = (int)((uint)GetStableHashCode(key) % (uint)_maxDegreeOfParallelism);
					workers[workerIndex].Add(document);

					// Flush worker buffer when it reaches bulk size
					if (workers[workerIndex].Count >= _bulkSize)
					{
						var batch = new List<T>(workers[workerIndex]);
						workers[workerIndex].Clear();
						var page = pageCounters[workerIndex]++;
						var wi = workerIndex;

						// Apply backpressure
						if (_request.BackPressure != null)
							await _request.BackPressure.WaitAsync(_compositeCancelToken).ConfigureAwait(false);

						pendingTasks.Add(BulkAsync(batch, page, 0, wi));

						// Limit in-flight tasks
						if (pendingTasks.Count >= _maxDegreeOfParallelism)
						{
							var completed = await Task.WhenAny(pendingTasks).ConfigureAwait(false);
							pendingTasks.Remove(completed);
							var result = await completed.ConfigureAwait(false);
							if (result != null) observer.OnNext(result);
						}
					}
				}

				// Flush remaining buffers
				for (var i = 0; i < _maxDegreeOfParallelism; i++)
				{
					if (workers[i].Count > 0)
					{
						var batch = workers[i];
						var page = pageCounters[i]++;

						if (_request.BackPressure != null)
							await _request.BackPressure.WaitAsync(_compositeCancelToken).ConfigureAwait(false);

						pendingTasks.Add(BulkAsync(batch, page, 0, i));
					}
				}

				// Await all remaining
				while (pendingTasks.Count > 0)
				{
					var completed = await Task.WhenAny(pendingTasks).ConfigureAwait(false);
					pendingTasks.Remove(completed);
					var result = await completed.ConfigureAwait(false);
					if (result != null) observer.OnNext(result);
				}

				OnCompleted(null, observer);
			}
			catch (Exception ex)
			{
				OnCompleted(ex, observer);
			}
		}

		private void OnCompleted(Exception exception, IObserver<BulkStreamAllResponse> observer)
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
			if (!_request.RefreshOnCompleted) return;

			var indices = _request.RefreshIndices ?? _request.Index;
			if (indices == null) return;

			var refresh = _client.Indices.Refresh(indices, r => r.RequestConfiguration(rc =>
			{
				switch (_request)
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

			if (!refresh.IsValid)
				throw Throw("Refreshing after all documents have indexed failed", refresh.ApiCall);
		}

		private async Task<BulkStreamAllResponse> BulkAsync(IList<T> buffer, long page, int attempt, int workerIndex)
		{
			_compositeCancelToken.ThrowIfCancellationRequested();

			var response = await _client.BulkStreamAsync(s =>
			{
				s.Index(_request.Index);
				s.Timeout(_request.Timeout);

				if (_request.BufferToBulk != null)
					_request.BufferToBulk(s, buffer);
				else
					s.IndexMany(buffer);

				if (!string.IsNullOrEmpty(_request.Pipeline)) s.Pipeline(_request.Pipeline);
				if (_request.Routing != null) s.Routing(_request.Routing);
				if (_request.WaitForActiveShards.HasValue) s.WaitForActiveShards(_request.WaitForActiveShards.ToString());

				switch (_request)
				{
					case IHelperCallable helperCallable when helperCallable.ParentMetaData is object:
						s.RequestConfiguration(rc => rc.RequestMetaData(helperCallable.ParentMetaData));
						break;
					default:
						s.RequestConfiguration(rc => rc.RequestMetaData(RequestMetaDataFactory.BulkHelperRequestMetaData()));
						break;
				}

				return s;
			}, _compositeCancelToken).ConfigureAwait(false);

			_compositeCancelToken.ThrowIfCancellationRequested();
			_request.BulkResponseCallback?.Invoke(response);

			if (!response.ApiCall.Success)
				return await HandleBulkFailure(buffer, page, attempt, workerIndex, response).ConfigureAwait(false);

			var retryableDocuments = new List<T>();
			var droppedDocuments = new List<Tuple<BulkResponseItemBase, T>>();
			var retryPredicate = _request.RetryDocumentPredicate ?? DefaultRetryPredicate;
			var droppedCallback = _request.DroppedDocumentCallback ?? DefaultDroppedCallback;

			foreach (var documentWithResponse in response.Items.Zip(buffer, Tuple.Create))
			{
				if (documentWithResponse.Item1.IsValid) continue;

				if (retryPredicate(documentWithResponse.Item1, documentWithResponse.Item2))
					retryableDocuments.Add(documentWithResponse.Item2);
				else
					droppedDocuments.Add(documentWithResponse);
			}

			HandleDroppedDocuments(droppedDocuments, droppedCallback, response);

			var maxRetries = _request.MaxRetries ?? BulkStreamAllDefaults.MaxRetriesDefault;

			if (retryableDocuments.Count > 0 && attempt < maxRetries)
				return await RetryDocuments(page, attempt + 1, retryableDocuments, workerIndex).ConfigureAwait(false);

			if (retryableDocuments.Count > 0)
				throw ThrowOnBadBulk(response, $"Bulk indexing failed after retrying {attempt} times");

			_request.BackPressure?.Release();

			return new BulkStreamAllResponse
			{
				Page = page,
				WorkerIndex = workerIndex,
				Retries = attempt,
				Items = response.Items,
				Took = response.Took
			};
		}

		private async Task<BulkStreamAllResponse> HandleBulkFailure(
			IList<T> buffer, long page, int attempt, int workerIndex, BulkStreamResponse response)
		{
			var maxRetries = _request.MaxRetries ?? BulkStreamAllDefaults.MaxRetriesDefault;
			var clientException = response.ApiCall.OriginalException as OpenSearchClientException;
			var failureReason = clientException?.FailureReason;
			var reason = failureReason?.GetStringValue() ?? nameof(PipelineFailure.BadRequest);

			switch (failureReason)
			{
				case PipelineFailure.MaxRetriesReached:
					if (response.ApiCall.AuditTrail.Last().Event == AuditEvent.FailedOverAllNodes)
						throw ThrowOnBadBulk(response, $"{nameof(BulkStreamAllObservable<T>)} halted after attempted bulk failed over all the active nodes");

					ThrowOnExhaustedRetries();
					return await RetryDocuments(page, attempt + 1, buffer, workerIndex).ConfigureAwait(false);
				case PipelineFailure.CouldNotStartSniffOnStartup:
				case PipelineFailure.BadAuthentication:
				case PipelineFailure.NoNodesAttempted:
				case PipelineFailure.SniffFailure:
				case PipelineFailure.Unexpected:
					throw ThrowOnBadBulk(response,
						$"{nameof(BulkStreamAllObservable<T>)} halted after {nameof(PipelineFailure)}.{reason} from _bulk/stream");
				case PipelineFailure.BadResponse:
				case PipelineFailure.PingFailure:
				case PipelineFailure.MaxTimeoutReached:
				case PipelineFailure.BadRequest:
				default:
					ThrowOnExhaustedRetries();
					return await RetryDocuments(page, attempt + 1, buffer, workerIndex).ConfigureAwait(false);
			}

			void ThrowOnExhaustedRetries()
			{
				if (attempt >= maxRetries)
					throw ThrowOnBadBulk(response,
						$"{nameof(BulkStreamAllObservable<T>)} halted after {nameof(PipelineFailure)}.{reason} from _bulk/stream and exhausting retries ({attempt})");
			}
		}

		private void HandleDroppedDocuments(
			List<Tuple<BulkResponseItemBase, T>> droppedDocuments,
			Action<BulkResponseItemBase, T> droppedCallback,
			BulkStreamResponse response)
		{
			if (droppedDocuments.Count <= 0) return;

			foreach (var dropped in droppedDocuments)
				droppedCallback(dropped.Item1, dropped.Item2);

			if (!_request.ContinueAfterDroppedDocuments)
				throw ThrowOnBadBulk(response, $"{nameof(BulkStreamAllObservable<T>)} halted after receiving failures that can not be retried from _bulk/stream");
		}

		private async Task<BulkStreamAllResponse> RetryDocuments(long page, int attempt, IList<T> retryDocuments, int workerIndex)
		{
			_incrementRetries();
			var baseDelay = _request.RetryBaseDelay ?? RetryStrategy.DefaultBaseDelay;
			var maxDelay = _request.RetryMaxDelay ?? RetryStrategy.DefaultMaxDelay;
			var delay = RetryStrategy.ComputeDelay(attempt - 1, baseDelay, maxDelay);
			await Task.Delay(delay, _compositeCancelToken).ConfigureAwait(false);
			return await BulkAsync(retryDocuments, page, attempt, workerIndex).ConfigureAwait(false);
		}

		private Exception ThrowOnBadBulk(IOpenSearchResponse response, string message)
		{
			_incrementFailed();
			_request.BackPressure?.Release();
			return Throw(message, response.ApiCall);
		}

		private static OpenSearchClientException Throw(string message, IApiCallDetails details) =>
			new OpenSearchClientException(PipelineFailure.BadResponse, message, details);

		private static bool DefaultRetryPredicate(BulkResponseItemBase bulkResponseItem, T d) => bulkResponseItem.Status == 429;

		private static void DefaultDroppedCallback(BulkResponseItemBase bulkResponseItem, T d) { }

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
	}
}
