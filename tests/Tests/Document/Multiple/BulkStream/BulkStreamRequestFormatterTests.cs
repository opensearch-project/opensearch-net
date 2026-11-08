/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using OpenSearch.Client;
using Tests.Domain.Extensions;

namespace Tests.Document.Multiple.BulkStream
{
	// The _bulk/stream request body is newline-delimited JSON: an action/metadata line per operation, followed by a
	// source line for operations that carry a document. Runs under both engines to guard the Utf8Json formatter AND the
	// System.Text.Json BulkStreamRequestConverter (without which the body serializes to {} under UseSystemTextJson()).
	public class BulkStreamRequestFormatterTests
	{
		private class Doc
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		[U] public void IndexManyFramingUtf8Json() => AssertIndexManyFraming(useSystemTextJson: false);

		[U] public void IndexManyFramingSystemTextJson() => AssertIndexManyFraming(useSystemTextJson: true);

		private static void AssertIndexManyFraming(bool useSystemTextJson)
		{
			const int documents = 3;
			var connection = new RecordingConnection();
			var client = CreateClient(connection, useSystemTextJson);

			client.BulkStream(s => s
				.Index("bulkstream-framing")
				.IndexMany(Enumerable.Range(0, documents).Select(i => new Doc { Id = i, Name = $"doc-{i}" }))
			);

			var lines = NonEmptyLines(connection.LastRequestBody);
			lines.Should().HaveCount(documents * 2, "each indexed document is an action line plus a source line");

			for (var i = 0; i < documents; i++)
			{
				JObject.Parse(lines[i * 2]).Should().ContainKey("index", "every action line names its operation");
				JObject.Parse(lines[i * 2 + 1]).Value<string>("name").Should().Be($"doc-{i}", "the source line follows its action line, in order");
			}
		}

		[U] public void DeleteHasNoSourceLineUtf8Json() => AssertDeleteHasNoSourceLine(useSystemTextJson: false);

		[U] public void DeleteHasNoSourceLineSystemTextJson() => AssertDeleteHasNoSourceLine(useSystemTextJson: true);

		private static void AssertDeleteHasNoSourceLine(bool useSystemTextJson)
		{
			var connection = new RecordingConnection();
			var client = CreateClient(connection, useSystemTextJson);

			client.BulkStream(s => s
				.Index("bulkstream-framing")
				.Index<Doc>(o => o.Document(new Doc { Id = 0, Name = "doc-0" }))
				.Delete<Doc>(o => o.Id(1))
			);

			var lines = NonEmptyLines(connection.LastRequestBody);
			lines.Should().HaveCount(3); // index => action + source; delete => action only
			JObject.Parse(lines[0]).Should().ContainKey("index");
			JObject.Parse(lines[1]).Value<string>("name").Should().Be("doc-0");
			JObject.Parse(lines[2]).Should().ContainKey("delete", "a delete carries no document, so no source line follows it");
		}

		private static string[] NonEmptyLines(string body) => body.Split('\n').Where(l => l.Trim().Length > 0).ToArray();

		private static IOpenSearchClient CreateClient(RecordingConnection connection, bool useSystemTextJson)
		{
			var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection)
				.ApplyDomainSettings()
				.UseSystemTextJson(useSystemTextJson);
			return new OpenSearchClient(settings);
		}

		private sealed class RecordingConnection : InMemoryConnection
		{
			private static readonly byte[] EmptyStreamResponse = Encoding.UTF8.GetBytes("{\"took\":0,\"errors\":false,\"items\":[]}");

			public RecordingConnection() : base(EmptyStreamResponse, 200, null, RequestData.MimeType) { }

			public string LastRequestBody { get; private set; } = string.Empty;

			public override async Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
			{
				LastRequestBody = await ReadBodyAsync(requestData, cancellationToken).ConfigureAwait(false);
				return await base.RequestAsync<TResponse>(requestData, cancellationToken).ConfigureAwait(false);
			}

			public override TResponse Request<TResponse>(RequestData requestData)
			{
				LastRequestBody = ReadBodyAsync(requestData, CancellationToken.None).GetAwaiter().GetResult();
				return base.Request<TResponse>(requestData);
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
		}
	}
}
