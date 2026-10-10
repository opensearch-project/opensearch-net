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
	// Deterministic unit coverage for BulkAll's DocumentId (opensearch-net#353). Uses an in-memory connection
	// that records the raw _bulk body so we can assert the per-document _id the action line carried.
	public class BulkAllDocumentIdApiTests
	{
		private class SmallObject
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		[U]
		public void DocumentIdDerivesTheIdFromTheDocument()
		{
			var connection = new RecordingBulkConnection();

			var run = RunBulkAll(connection, documents: 3, size: 3,
				configure: f => f.DocumentId(d => d.Name));

			run.Error.Should().BeNull();
			connection.ActionIds.Should().Equal(new[] { "name-0", "name-1", "name-2" },
				"each document's _id must come from the DocumentId function, not from id inference");
		}

		[U]
		public void BufferToBulkTakesPrecedenceOverDocumentId()
		{
			var connection = new RecordingBulkConnection();

			// BufferToBulk is the complete-control escape hatch: when set, DocumentId is ignored and the caller
			// owns the operation. Here BufferToBulk sets its own id, so that id — not DocumentId's — must win.
			var run = RunBulkAll(connection, documents: 2, size: 2,
				configure: f => f
					.DocumentId(d => d.Name)
					.BufferToBulk((b, buffer) => b.IndexMany(buffer, (op, doc) => op.Id($"buffer-{doc.Id}"))));

			run.Error.Should().BeNull();
			connection.ActionIds.Should().Equal(new[] { "buffer-0", "buffer-1" },
				"BufferToBulk owns the operation and DocumentId must be ignored when it is set");
		}

		[U]
		public void NoDocumentIdFallsBackToInferredIds()
		{
			var connection = new RecordingBulkConnection();

			// SmallObject.Id is the inferred id property, so without DocumentId the _id must be the inferred value.
			var run = RunBulkAll(connection, documents: 2, size: 2, configure: null);

			run.Error.Should().BeNull();
			connection.ActionIds.Should().Equal(new[] { "0", "1" },
				"with no DocumentId and no BufferToBulk, the default path must infer the id");
		}

		private readonly struct RunResult
		{
			public RunResult(Exception error)
			{
				Error = error;
			}

			public Exception Error { get; }
		}

		private static RunResult RunBulkAll(
			IConnection connection, int documents, int size, Action<BulkAllDescriptor<SmallObject>> configure)
		{
			var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection)
				.ApplyDomainSettings();
			var client = new OpenSearchClient(settings);

			var docs = Enumerable.Range(0, documents).Select(i => new SmallObject { Id = i, Name = $"name-{i}" });
			Exception error = null;
			var handle = new ManualResetEventSlim(false);

			var observer = new BulkAllObserver(
				onError: e => { error = e; handle.Set(); },
				onCompleted: () => handle.Set());

			var observable = client.BulkAll(docs, f =>
			{
				f.MaxDegreeOfParallelism(1)
					.Size(size)
					.Index("bulkall-idselector")
					.BackOffRetries(1);
				configure?.Invoke(f);
				return f;
			});

			observable.Subscribe(observer);
			handle.Wait(TimeSpan.FromSeconds(30)).Should().BeTrue("the run must finish within the timeout");

			return new RunResult(error);
		}

		// In-memory connection that always returns a success _bulk response matching the documents it was sent, and
		// records the _id carried on each bulk action line (in request order) so tests can assert what the id selector
		// produced on the wire.
		private sealed class RecordingBulkConnection : InMemoryConnection
		{
			// Matches the _id on a bulk index action line: {"index":{"_index":"...","_id":"<id>"}}
			private static readonly Regex ActionIdPattern = new Regex("\"_id\"\\s*:\\s*\"([^\"]*)\"", RegexOptions.Compiled);

			private readonly object _gate = new object();
			private readonly List<string> _actionIds = new List<string>();

			public RecordingBulkConnection()
				: base(Array.Empty<byte>(), 200, null, RequestData.MimeType) { }

			public IReadOnlyList<string> ActionIds { get { lock (_gate) return _actionIds.ToArray(); } }

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
				var ids = ParseActionIds(body);
				lock (_gate) _actionIds.AddRange(ids);
				return BuildBulkResponse(ids);
			}

			private static IReadOnlyList<string> ParseActionIds(string body)
			{
				var ids = new List<string>();
				foreach (Match match in ActionIdPattern.Matches(body))
					ids.Add(match.Groups[1].Value);
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

			private static byte[] BuildBulkResponse(IReadOnlyList<string> ids)
			{
				var sb = new StringBuilder();
				sb.Append("{\"took\":1,\"errors\":false,\"items\":[");
				for (var i = 0; i < ids.Count; i++)
				{
					if (i > 0) sb.Append(',');
					sb.Append("{\"index\":{")
						.Append("\"_index\":\"bulkall-idselector\",")
						.Append("\"_id\":\"").Append(ids[i]).Append("\",")
						.Append("\"_version\":1,")
						.Append("\"status\":201,")
						.Append("\"result\":\"created\"")
						.Append("}}");
				}
				sb.Append("]}");
				return Encoding.UTF8.GetBytes(sb.ToString());
			}
		}
	}
}
