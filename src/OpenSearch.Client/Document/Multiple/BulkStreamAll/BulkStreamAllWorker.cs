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
	internal class BulkStreamAllWorker<T> where T : class
	{
		private readonly IOpenSearchClient _client;
		private readonly IBulkStreamAllRequest<T> _request;
		private readonly int _workerIndex;
		private readonly int _bulkSize;
		private readonly int _maxRetries;
		private readonly TimeSpan _retryBaseDelay;
		private readonly TimeSpan _retryMaxDelay;
		private readonly Func<BulkResponseItemBase, T, bool> _retryPredicate;
		private readonly Action<BulkResponseItemBase, T> _droppedDocumentCallBack;
		private readonly Action<BulkStreamResponse> _bulkResponseCallback;
		private readonly CancellationToken _cancellationToken;

		private readonly List<T> _buffer;
		private long _pageCounter;

		private Action _incrementFailed;
		private Action _incrementRetries;

		public BulkStreamAllWorker(
			IOpenSearchClient client,
			IBulkStreamAllRequest<T> request,
			int workerIndex,
			CancellationToken cancellationToken)
		{
			_client = client;
			_request = request;
			_workerIndex = workerIndex;
			_cancellationToken = cancellationToken;

			_bulkSize = request.Size ?? BulkStreamAllDefaults.SizeDefault;
			_maxRetries = request.MaxRetries ?? BulkStreamAllDefaults.MaxRetriesDefault;
			_retryBaseDelay = request.RetryBaseDelay ?? RetryStrategy.DefaultBaseDelay;
			_retryMaxDelay = request.RetryMaxDelay ?? RetryStrategy.DefaultMaxDelay;
			_retryPredicate = request.RetryDocumentPredicate ?? DefaultRetryPredicate;
			_droppedDocumentCallBack = request.DroppedDocumentCallback ?? DefaultDroppedCallback;
			_bulkResponseCallback = request.BulkResponseCallback;

			_buffer = new List<T>(_bulkSize);
			_incrementFailed = () => { };
			_incrementRetries = () => { };
		}

		public void SetObserver(Action incrementFailed, Action incrementRetries)
		{
			_incrementFailed = incrementFailed;
			_incrementRetries = incrementRetries;
		}

		/// <summary>
		/// Add a document to this worker's buffer. If the buffer reaches the bulk size, flushes the batch.
		/// </summary>
		public async Task<BulkStreamAllResponse> AddAsync(T document)
		{
			_buffer.Add(document);

			if (_buffer.Count >= _bulkSize)
				return await FlushBufferAsync().ConfigureAwait(false);

			return null;
		}

		/// <summary>
		/// Flush whatever is currently in the buffer, regardless of size.
		/// </summary>
		public async Task<BulkStreamAllResponse> FlushBufferAsync()
		{
			if (_buffer.Count == 0)
				return null;

			var batch = new List<T>(_buffer);
			_buffer.Clear();

			var page = Interlocked.Increment(ref _pageCounter) - 1;
			return await BulkAsync(batch, page, 0).ConfigureAwait(false);
		}

		private async Task<BulkStreamAllResponse> BulkAsync(IList<T> buffer, long page, int attempt)
		{
			_cancellationToken.ThrowIfCancellationRequested();

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
			}, _cancellationToken).ConfigureAwait(false);

			_cancellationToken.ThrowIfCancellationRequested();
			_bulkResponseCallback?.Invoke(response);

			if (!response.ApiCall.Success)
				return await HandleBulkFailure(buffer, page, attempt, response).ConfigureAwait(false);

			var retryableDocuments = new List<T>();
			var droppedDocuments = new List<Tuple<BulkResponseItemBase, T>>();

			foreach (var documentWithResponse in response.Items.Zip(buffer, Tuple.Create))
			{
				if (documentWithResponse.Item1.IsValid) continue;

				if (_retryPredicate(documentWithResponse.Item1, documentWithResponse.Item2))
					retryableDocuments.Add(documentWithResponse.Item2);
				else
					droppedDocuments.Add(documentWithResponse);
			}

			HandleDroppedDocuments(droppedDocuments, response);

			if (retryableDocuments.Count > 0 && attempt < _maxRetries)
				return await RetryDocuments(page, attempt + 1, retryableDocuments).ConfigureAwait(false);

			if (retryableDocuments.Count > 0)
				throw ThrowOnBadBulk(response, $"Bulk indexing failed after retrying {attempt} times");

			_request.BackPressure?.Release();

			return new BulkStreamAllResponse
			{
				Page = page,
				WorkerIndex = _workerIndex,
				Retries = attempt,
				Items = response.Items,
				Took = response.Took
			};
		}

		private async Task<BulkStreamAllResponse> HandleBulkFailure(IList<T> buffer, long page, int attempt, BulkStreamResponse response)
		{
			var clientException = response.ApiCall.OriginalException as OpenSearchClientException;
			var failureReason = clientException?.FailureReason;
			var reason = failureReason?.GetStringValue() ?? nameof(PipelineFailure.BadRequest);

			switch (failureReason)
			{
				case PipelineFailure.MaxRetriesReached:
					if (response.ApiCall.AuditTrail.Last().Event == AuditEvent.FailedOverAllNodes)
						throw ThrowOnBadBulk(response, $"{nameof(BulkStreamAll)} halted after attempted bulk failed over all the active nodes");

					ThrowOnExhaustedRetries();
					return await RetryDocuments(page, attempt + 1, buffer).ConfigureAwait(false);
				case PipelineFailure.CouldNotStartSniffOnStartup:
				case PipelineFailure.BadAuthentication:
				case PipelineFailure.NoNodesAttempted:
				case PipelineFailure.SniffFailure:
				case PipelineFailure.Unexpected:
					throw ThrowOnBadBulk(response, $"{nameof(BulkStreamAll)} halted after {nameof(PipelineFailure)}.{reason} from _bulk/stream");
				case PipelineFailure.BadResponse:
				case PipelineFailure.PingFailure:
				case PipelineFailure.MaxTimeoutReached:
				case PipelineFailure.BadRequest:
				default:
					ThrowOnExhaustedRetries();
					return await RetryDocuments(page, attempt + 1, buffer).ConfigureAwait(false);
			}

			void ThrowOnExhaustedRetries()
			{
				if (attempt >= _maxRetries)
					throw ThrowOnBadBulk(response,
						$"{nameof(BulkStreamAll)} halted after {nameof(PipelineFailure)}.{reason} from _bulk/stream and exhausting retries ({attempt})");
			}
		}

		private void HandleDroppedDocuments(List<Tuple<BulkResponseItemBase, T>> droppedDocuments, BulkStreamResponse response)
		{
			if (droppedDocuments.Count <= 0) return;

			foreach (var dropped in droppedDocuments)
				_droppedDocumentCallBack(dropped.Item1, dropped.Item2);

			if (!_request.ContinueAfterDroppedDocuments)
				throw ThrowOnBadBulk(response, $"{nameof(BulkStreamAll)} halted after receiving failures that can not be retried from _bulk/stream");
		}

		private async Task<BulkStreamAllResponse> RetryDocuments(long page, int attempt, IList<T> retryDocuments)
		{
			_incrementRetries();
			var delay = RetryStrategy.ComputeDelay(attempt - 1, _retryBaseDelay, _retryMaxDelay);
			await Task.Delay(delay, _cancellationToken).ConfigureAwait(false);
			return await BulkAsync(retryDocuments, page, attempt).ConfigureAwait(false);
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

		// Placeholder class name for exception messages
		private static class BulkStreamAll { }
	}
}
