/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>BulkStreamRequestFormatter</c>.
	///
	/// A <c>_bulk/stream</c> request body is <em>newline-delimited JSON</em> (ndjson), not a single JSON document: for
	/// each operation an action/metadata object (<c>{ "&lt;op&gt;": { … } }</c>) is written, followed by a raw
	/// <c>'\n'</c>, and — unless the operation has no body (e.g. <c>delete</c>) — a source/body object followed by
	/// another raw <c>'\n'</c>.
	///
	/// A <see cref="Utf8JsonWriter"/> forbids emitting more than one JSON value at the document root and cannot emit a
	/// bare newline between values, so the whole ndjson payload is built into a buffer (using nested writers that
	/// inherit the serializer's encoder/indentation) and emitted verbatim with a single
	/// <see cref="Utf8JsonWriter.WriteRawValue(System.ReadOnlySpan{byte}, bool)"/> call
	/// (<c>skipInputValidation: true</c>). Mirrors <see cref="BulkRequestConverter"/> for the <c>_bulk</c> request;
	/// without it a bulk stream request serializes to <c>{}</c> under System.Text.Json (its <c>Operations</c> are
	/// <c>[IgnoreDataMember]</c>), sending no documents.
	/// </summary>
	internal class BulkStreamRequestConverter : SettingsAwareConverter<IBulkStreamRequest>
	{
		private const byte Newline = (byte)'\n';

		public BulkStreamRequestConverter(IConnectionSettingsValues settings) : base(settings) { }

		// The legacy Deserialize threw NotSupportedException — a bulk stream request body is never read back.
		public override IBulkStreamRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, IBulkStreamRequest value, JsonSerializerOptions options)
		{
			if (value?.Operations == null)
				return;

			var inferrer = Settings.Inferrer;
			var writerOptions = new JsonWriterOptions { Encoder = options.Encoder, Indented = options.WriteIndented };

			using var ms = new MemoryStream();

			for (var index = 0; index < value.Operations.Count; index++)
			{
				var op = value.Operations[index];
				op.Index ??= value.Index ?? op.ClrType;
				if (op.Index.Equals(value.Index)) op.Index = null;
				op.Id = op.GetIdForOperation(inferrer);
				op.Routing = op.GetRoutingForOperation(inferrer);

				// Action/metadata line: { "<operation>": <op> }
				using (var mw = new Utf8JsonWriter(ms, writerOptions))
				{
					mw.WriteStartObject();
					mw.WritePropertyName(op.Operation);
					// Serialize with the runtime type so all [DataMember] members resolve (serializing as `object`
					// would emit an empty object under System.Text.Json).
					JsonSerializer.Serialize(mw, op, op.GetType(), options);
					mw.WriteEndObject();
				}
				ms.WriteByte(Newline);

				var body = op.GetBody();
				if (body == null)
					continue;

				// Per-op body. Mirror the legacy SourceWriteFormatter distinction: an OpenSearch.Client type (e.g. the
				// update body wrapper) is written through the request options so its registered converters apply, while
				// a plain user document goes through the configured SourceSerializer so a custom source serializer
				// governs its shape.
				using (var bw = new Utf8JsonWriter(ms, writerOptions))
				{
					if (body.GetType().IsOpenSearchClientType())
						JsonSerializer.Serialize(bw, body, body.GetType(), options);
					else
						ProxyRequestDocumentWriter.Write(bw, body, Settings, options);
				}
				ms.WriteByte(Newline);
			}

			if (ms.Length == 0)
				return;

			// skipInputValidation: the buffer is newline-delimited JSON — multiple root values separated by raw '\n' —
			// which is deliberately NOT a single valid JSON document, so validation must be bypassed to emit it verbatim.
			writer.WriteRawValue(ms.ToArray(), skipInputValidation: true);
		}
	}
}
