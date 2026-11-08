/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Domain.Extensions;

namespace Tests.Document.Multiple.BulkAll
{
	// Deterministic unit coverage for BulkAllObservable's retry / dropped-document partition logic and the affinity
	// routing added when folding the streaming helper into BulkAll. Driven by a programmable in-memory connection that
	// returns a per-request _bulk response and records exactly which documents each request carried.
	public class BulkAllRetryDropApiTests
	{
		private class SmallObject
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		[U]
		public void PartialFailureRetriesOnlyTheFailingDocument()
		{
			const int failingId = 2;
			var connection = new ProgrammableBulkConnection((ordinal, ids) =>
				ids.Select(id => ordinal == 0 && id == failingId ? 429 : 201).ToArray());

			var run = RunBulkAll(connection, documents: 5, size: 5, maxDegreeOfParallelism: 1, backOffRetries: 3);

			run.Error.Should().BeNull();
			run.Observer.TotalNumberOfRetries.Should().Be(1, "one partial failure means exactly one retry");
			connection.RequestCount.Should().Be(2, "the initial batch plus a single retry");
			connection.RequestedDocIds[1].Should().Equal(new[] { failingId }, "only the failing document may be re-sent");
		}

		[U]
		public void PersistentFailureExhaustsRetriesThenThrows()
		{
			const int backOffRetries = 2;
			var connection = new ProgrammableBulkConnection((ordinal, ids) => ids.Select(id => id == 2 ? 429 : 201).ToArray());

			var run = RunBulkAll(connection, documents: 5, size: 5, maxDegreeOfParallelism: 1, backOffRetries: backOffRetries);

			run.Error.Should().BeOfType<OpenSearchClientException>();
			run.Error.Message.Should().Contain("after retrying");
			connection.RequestCount.Should().Be(backOffRetries + 1, "initial attempt plus BackOffRetries retries");
			run.Observer.TotalNumberOfFailedBuffers.Should().Be(1);
		}

		[U]
		public void ContinueAfterDroppedDocumentsFalseHaltsAndDispatchesNoFurtherBatches()
		{
			var connection = new ProgrammableBulkConnection((ordinal, ids) => ids.Select(id => id == 0 ? 400 : 201).ToArray());

			var run = RunBulkAll(connection, documents: 4, size: 2, maxDegreeOfParallelism: 1, backOffRetries: 3,
				configure: f => f.ContinueAfterDroppedDocuments(false));

			run.Error.Should().NotBeNull();
			run.Error.Message.Should().Contain("halted");
			connection.RequestBodies.Should().NotContain(body => body.Contains("\"doc-2\""),
				"the second batch must never be dispatched once the first halts on a dropped document");
		}

		[U]
		public void RetryPreservesAffinityOrdering()
		{
			var connection = new ProgrammableBulkConnection((ordinal, ids) =>
				ids.Select(id => ordinal == 0 && id == 0 ? 429 : 201).ToArray());

			var run = RunBulkAll(connection, documents: 4, size: 2, maxDegreeOfParallelism: 4, backOffRetries: 3,
				affinityKey: "same-key");

			run.Error.Should().BeNull();
			run.Observer.TotalNumberOfRetries.Should().Be(1);
			run.Pages.Should().Equal(new long[] { 0, 1 },
				"batches sharing an affinity key must complete in dispatch order even when an earlier batch retries");
		}

		[U]
		public void CustomRetryPredicateRetriesAnOtherwiseNonRetryableStatus()
		{
			const int failingId = 1;
			var connection = new ProgrammableBulkConnection((ordinal, ids) =>
				ids.Select(id => ordinal == 0 && id == failingId ? 400 : 201).ToArray());

			var run = RunBulkAll(connection, documents: 4, size: 4, maxDegreeOfParallelism: 1, backOffRetries: 3,
				configure: f => f.RetryDocumentPredicate((item, doc) => item.Status == 400));

			run.Error.Should().BeNull();
			run.Observer.TotalNumberOfRetries.Should().Be(1, "the custom predicate makes the 400 retryable");
			connection.RequestedDocIds[1].Should().Equal(new[] { failingId });
		}

		[U]
		public void CustomRetryPredicateSuppressesRetryForOtherwiseRetryableStatus()
		{
			const int failingId = 1;
			var droppedIds = new List<int>();
			var connection = new ProgrammableBulkConnection((ordinal, ids) => ids.Select(id => id == failingId ? 429 : 201).ToArray());

			var run = RunBulkAll(connection, documents: 4, size: 4, maxDegreeOfParallelism: 1, backOffRetries: 3,
				configure: f => f
					.RetryDocumentPredicate((item, doc) => false)
					.ContinueAfterDroppedDocuments()
					.DroppedDocumentCallback((item, doc) => droppedIds.Add(doc.Id)));

			run.Error.Should().BeNull();
			run.Observer.TotalNumberOfRetries.Should().Be(0, "the predicate suppresses all retries");
			connection.RequestCount.Should().Be(1, "a suppressed retry must not re-send the batch");
			droppedIds.Should().ContainSingle().Which.Should().Be(failingId);
		}

		[U]
		public void DroppedDocumentCallbackReceivesTheFailingItemAndDocument()
		{
			const int failingId = 3;
			BulkResponseItemBase droppedItem = null;
			SmallObject droppedDocument = null;
			var connection = new ProgrammableBulkConnection((ordinal, ids) => ids.Select(id => id == failingId ? 400 : 201).ToArray());

			var run = RunBulkAll(connection, documents: 5, size: 5, maxDegreeOfParallelism: 1, backOffRetries: 3,
				configure: f => f
					.ContinueAfterDroppedDocuments()
					.DroppedDocumentCallback((item, doc) => { droppedItem = item; droppedDocument = doc; }));

			run.Error.Should().BeNull();
			droppedDocument.Should().NotBeNull();
			droppedDocument.Id.Should().Be(failingId, "the callback must receive the document that failed");
			droppedItem.Status.Should().Be(400, "the callback must receive that document's response item");
		}

		[U]
		public void ContinueAfterDroppedDocumentsTrueAllowsSubsequentBatches()
		{
			var connection = new ProgrammableBulkConnection((ordinal, ids) => ids.Select(id => id == 0 ? 400 : 201).ToArray());

			var run = RunBulkAll(connection, documents: 4, size: 2, maxDegreeOfParallelism: 1, backOffRetries: 3,
				configure: f => f.ContinueAfterDroppedDocuments());

			run.Error.Should().BeNull();
			run.Pages.Should().Contain(new long[] { 0, 1 }, "both batches must complete despite the dropped document");
			connection.RequestBodies.Should().Contain(body => body.Contains("\"doc-2\""), "the second batch must still be dispatched");
		}

		[U]
		public void BulkResponseCallbackIsInvokedForEachResponse()
		{
			var callbackCount = 0;
			var connection = new ProgrammableBulkConnection((ordinal, ids) => ids.Select(_ => 201).ToArray());

			var run = RunBulkAll(connection, documents: 6, size: 2, maxDegreeOfParallelism: 1, backOffRetries: 3,
				configure: f => f.BulkResponseCallback(_ => Interlocked.Increment(ref callbackCount)));

			run.Error.Should().BeNull();
			callbackCount.Should().Be(3, "6 documents at size 2 => 3 successful bulk requests");
		}

		[U]
		public void TracksTotalDocumentsProcessed()
		{
			const int documents = 6;
			var connection = new ProgrammableBulkConnection((ordinal, ids) => ids.Select(_ => 201).ToArray());

			var run = RunBulkAll(connection, documents, size: 2, maxDegreeOfParallelism: 1, backOffRetries: 3);

			run.Error.Should().BeNull();
			run.Observer.TotalDocumentsProcessed.Should().Be(documents);
		}

		[U]
		public void EmptyDocumentStreamCompletesWithoutRequests()
		{
			var connection = new ProgrammableBulkConnection((ordinal, ids) => ids.Select(_ => 201).ToArray());

			var run = RunBulkAll(connection, documents: 0, size: 10, maxDegreeOfParallelism: 4, backOffRetries: 3,
				affinityKey: "k");

			run.Error.Should().BeNull();
			run.Pages.Should().BeEmpty();
			connection.RequestCount.Should().Be(0, "an empty stream must not issue any request");
		}

		[U]
		public void AffinityDoesNotDeadlockWhenBackPressurePoolIsDrained()
		{
			// Regression: ProducerConsumerBackPressure is directional — in a Reindex the scroll producer acquires a slot
			// per page and the bulk consumer repays via Release(). The affinity consumer must therefore never acquire
			// from that pool. Here we mimic a scroll that has buffered ahead and drained the pool to zero, then run an
			// affinity BulkAll wired to that same pool (exactly how ReindexObservable wires it). A consumer-side WaitAsync
			// would block before the batch is ever dispatched — so nothing is in flight to ever Release — and the run
			// would hang forever. With the fix the consumer dispatches without acquiring and the run completes.
			var backPressure = new ProducerConsumerBackPressure(backPressureFactor: 1, maxConcurrency: 1);
			backPressure.WaitAsync().GetAwaiter().GetResult(); // drain the single slot: pool now empty, nothing in flight

			var connection = new ProgrammableBulkConnection((ordinal, ids) => ids.Select(_ => 201).ToArray());

			var run = RunBulkAll(connection, documents: 4, size: 2, maxDegreeOfParallelism: 2, backOffRetries: 3,
				affinityKey: "same-key", backPressure: backPressure);

			run.Error.Should().BeNull();
			run.Pages.Should().NotBeEmpty("the affinity path must make progress even when the back-pressure pool is empty");
			connection.RequestCount.Should().BeGreaterThan(0, "batches must dispatch without acquiring a back-pressure slot");
		}

		private readonly struct RunResult
		{
			public RunResult(BulkAllObserver observer, Exception error, List<long> pages)
			{
				Observer = observer;
				Error = error;
				Pages = pages;
			}

			public BulkAllObserver Observer { get; }
			public Exception Error { get; }
			public List<long> Pages { get; }
		}

		private static RunResult RunBulkAll(
			IConnection connection, int documents, int size, int maxDegreeOfParallelism, int backOffRetries,
			string affinityKey = null, Action<BulkAllDescriptor<SmallObject>> configure = null,
			ProducerConsumerBackPressure backPressure = null)
		{
			var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection)
				.ApplyDomainSettings();
			var client = new OpenSearchClient(settings);

			var docs = Enumerable.Range(0, documents).Select(i => new SmallObject { Id = i, Name = $"doc-{i}" });
			var pages = new List<long>();
			Exception error = null;
			var handle = new ManualResetEventSlim(false);

			var observer = new BulkAllObserver(
				onNext: r => { lock (pages) pages.Add(r.Page); },
				onError: e => { error = e; handle.Set(); },
				onCompleted: () => handle.Set());

			var observable = client.BulkAll(docs, f =>
			{
				f.MaxDegreeOfParallelism(maxDegreeOfParallelism)
					.Size(size)
					.Index("bulkall-retrydrop")
					.BackOffRetries(backOffRetries)
					.RetryBaseDelay(TimeSpan.FromMilliseconds(1))
					.RetryMaxDelay(TimeSpan.FromMilliseconds(5))
					.BufferToBulk((r, buffer) => r.IndexMany(buffer));
				if (affinityKey != null) f.DocumentAffinityKey(_ => affinityKey);
				if (backPressure != null) ((IBulkAllRequest<SmallObject>)f).BackPressure = backPressure;
				configure?.Invoke(f);
				return f;
			});

			observable.Subscribe(observer);
			handle.Wait(TimeSpan.FromSeconds(30)).Should().BeTrue("the run must finish within the timeout");

			return new RunResult(observer, error, pages);
		}

		// In-memory connection that maps each _bulk request to per-document statuses via a resolver
		// (ordinal, docIdsInRequestOrder) => status[], returns a single _bulk response object, and records the
		// documents seen on each request so tests can assert exactly what was (re-)sent.
		private sealed class ProgrammableBulkConnection : InMemoryConnection
		{
			private static readonly Regex DocIdPattern = new Regex("doc-(\\d+)", RegexOptions.Compiled);

			private readonly Func<int, IReadOnlyList<int>, int[]> _statusResolver;
			private readonly object _gate = new object();
			private readonly List<string> _requestBodies = new List<string>();
			private readonly List<IReadOnlyList<int>> _requestedDocIds = new List<IReadOnlyList<int>>();

			public ProgrammableBulkConnection(Func<int, IReadOnlyList<int>, int[]> statusResolver)
				: base(Array.Empty<byte>(), 200, null, RequestData.MimeType) =>
				_statusResolver = statusResolver;

			public IReadOnlyList<string> RequestBodies { get { lock (_gate) return _requestBodies.ToArray(); } }
			public IReadOnlyList<IReadOnlyList<int>> RequestedDocIds { get { lock (_gate) return _requestedDocIds.ToArray(); } }
			public int RequestCount { get { lock (_gate) return _requestBodies.Count; } }

			public override async Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
			{
				var body = await ReadBodyAsync(requestData, cancellationToken).ConfigureAwait(false);
				return await ReturnConnectionStatusAsync<TResponse>(requestData, cancellationToken, Handle(body), 200).ConfigureAwait(false);
			}

			public override TResponse Request<TResponse>(RequestData requestData)
			{
				var body = ReadBodyAsync(requestData, CancellationToken.None).GetAwaiter().GetResult();
				return ReturnConnectionStatus<TResponse>(requestData, Handle(body), 200);
			}

			private byte[] Handle(string body)
			{
				var ids = ParseDocIds(body);
				int ordinal;
				lock (_gate)
				{
					ordinal = _requestBodies.Count;
					_requestBodies.Add(body);
					_requestedDocIds.Add(ids);
				}

				return BuildBulkResponse(ids, _statusResolver(ordinal, ids));
			}

			private static IReadOnlyList<int> ParseDocIds(string body)
			{
				var ids = new List<int>();
				foreach (Match match in DocIdPattern.Matches(body))
					ids.Add(int.Parse(match.Groups[1].Value));
				return ids;
			}

			private static async Task<string> ReadBodyAsync(RequestData requestData, CancellationToken cancellationToken)
			{
				if (requestData.PostData == null) return string.Empty;

				using (var stream = requestData.MemoryStreamFactory.Create())
				{
					await requestData.PostData.WriteAsync(stream, requestData.ConnectionSettings, cancellationToken).ConfigureAwait(false);
					return Encoding.UTF8.GetString(stream.ToArray());
				}
			}

			// A single _bulk response object: { took, errors, items: [ { index: { ... } }, ... ] }.
			private static byte[] BuildBulkResponse(IReadOnlyList<int> ids, int[] statuses)
			{
				var anyFailed = statuses.Any(s => s < 200 || s >= 300);
				var sb = new StringBuilder();
				sb.Append("{\"took\":1,\"errors\":").Append(anyFailed ? "true" : "false").Append(",\"items\":[");
				for (var i = 0; i < ids.Count; i++)
				{
					var status = i < statuses.Length ? statuses[i] : 201;
					var failed = status < 200 || status >= 300;
					if (i > 0) sb.Append(',');
					sb.Append("{\"index\":{")
						.Append("\"_index\":\"bulkall-retrydrop\",")
						.Append("\"_id\":\"").Append(ids[i]).Append("\",")
						.Append("\"_version\":1,")
						.Append("\"status\":").Append(status)
						.Append(failed
							? ",\"error\":{\"type\":\"test_failure\",\"reason\":\"injected failure\"}"
							: ",\"result\":\"created\"")
						.Append("}}");
				}
				sb.Append("]}");
				return Encoding.UTF8.GetBytes(sb.ToString());
			}
		}
	}
}
