/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// The <c>_bulk/stream</c> endpoint responds with newline-delimited JSON: one
	/// <c>{ "took", "errors", "items" }</c> object per server-side batch (batches are flushed by <c>batch_size</c>,
	/// which defaults to 1, or by <c>batch_interval</c>), so a request for N documents can come back as many objects.
	/// The default single-object deserializer would read only the first object and drop every batch after it, so
	/// callers would see the outcome of only the first document — retry, dropped-document routing and per-batch
	/// reporting would never fire for the rest.
	///
	/// This builder splits the streamed body into its individual objects and deserializes each one with the
	/// configured serializer (so it works under both the Utf8Json and System.Text.Json engines — neither can read
	/// multiple top-level JSON values in a single pass), then aggregates them into one <see cref="BulkStreamResponse"/>:
	/// <c>items</c> concatenated in wire order, <c>errors</c> OR-ed and <c>took</c> summed. Because every object sits on
	/// its own line and JSON escapes any newline inside a string, splitting on the newline byte is safe.
	/// </summary>
	internal class BulkStreamResponseBuilder : CustomResponseBuilderBase
	{
		public static BulkStreamResponseBuilder Instance { get; } = new BulkStreamResponseBuilder();

		public override object DeserializeResponse(IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream)
		{
			// On a transport-level failure the body is not the streamed format (it may be an error envelope or not
			// JSON at all); leave it to the pipeline, which surfaces the failure via ApiCall.
			if (!response.Success)
				return new BulkStreamResponse();

			var aggregate = new BulkStreamResponse();
			var items = new List<BulkResponseItemBase>();

			foreach (var segment in ReadObjectSegments(stream))
			{
				var chunk = builtInSerializer.Deserialize<BulkStreamResponse>(segment);
				MergeChunk(aggregate, items, chunk);
			}

			aggregate.Items = items;
			return aggregate;
		}

		public override async Task<object> DeserializeResponseAsync(
			IOpenSearchSerializer builtInSerializer, IApiCallDetails response, Stream stream, CancellationToken ctx = default)
		{
			if (!response.Success)
				return new BulkStreamResponse();

			var aggregate = new BulkStreamResponse();
			var items = new List<BulkResponseItemBase>();

			foreach (var segment in await ReadObjectSegmentsAsync(stream, ctx).ConfigureAwait(false))
			{
				var chunk = await builtInSerializer.DeserializeAsync<BulkStreamResponse>(segment, ctx).ConfigureAwait(false);
				MergeChunk(aggregate, items, chunk);
			}

			aggregate.Items = items;
			return aggregate;
		}

		private static void MergeChunk(BulkStreamResponse aggregate, List<BulkResponseItemBase> items, BulkStreamResponse chunk)
		{
			if (chunk == null) return;

			if (chunk.Items != null && chunk.Items.Count > 0)
				items.AddRange(chunk.Items);

			aggregate.Errors |= chunk.Errors;
			aggregate.Took += chunk.Took;
		}

		private static IEnumerable<Stream> ReadObjectSegments(Stream stream)
		{
			using (var reader = new StreamReader(stream, Encoding.UTF8))
				return ToSegments(reader.ReadToEnd());
		}

		private static async Task<IEnumerable<Stream>> ReadObjectSegmentsAsync(Stream stream, CancellationToken ctx)
		{
			using (var reader = new StreamReader(stream, Encoding.UTF8))
				return ToSegments(await reader.ReadToEndAsync().ConfigureAwait(false));
		}

		// Splits the newline-delimited body into one memory stream per non-blank object.
		private static IEnumerable<Stream> ToSegments(string body)
		{
			var segments = new List<Stream>();
			if (string.IsNullOrEmpty(body)) return segments;

			foreach (var line in body.Split('\n'))
			{
				var trimmed = line.Trim();
				if (trimmed.Length == 0) continue;
				segments.Add(new MemoryStream(Encoding.UTF8.GetBytes(trimmed)));
			}

			return segments;
		}
	}
}
