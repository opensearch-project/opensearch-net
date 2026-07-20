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
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenSearch.Client
{
	/// <summary>
	/// System.Text.Json replacement for the legacy Utf8Json <c>BulkRequestFormatter</c>.
	///
	/// A bulk request body is <em>newline-delimited JSON</em> (ndjson), not a single JSON document: for each
	/// operation an action/metadata object (<c>{ "&lt;op&gt;": { … } }</c>) is written, followed by a raw
	/// <c>'\n'</c>, and — unless the operation has no body (e.g. <c>delete</c>) — a source/body object followed by
	/// another raw <c>'\n'</c>.
	///
	/// A <see cref="Utf8JsonWriter"/> forbids emitting more than one JSON value at the document root and cannot emit
	/// a bare newline between values, so we build the whole ndjson payload into a buffer (using nested writers that
	/// inherit the serializer's encoder/indentation) and emit it verbatim with a single
	/// <see cref="Utf8JsonWriter.WriteRawValue(System.ReadOnlySpan{byte}, bool)"/> call
	/// (<c>skipInputValidation: true</c>) — reproducing the legacy <c>writer.WriteRaw((byte)'\n')</c> bytes exactly.
	///
	/// Settings-aware: reproduces the legacy index/id/routing inference the old formatter obtained via
	/// <c>formatterResolver.GetConnectionSettings().Inferrer</c>.
	/// </summary>
	internal class BulkRequestConverter : SettingsAwareConverter<IBulkRequest>
	{
		private const byte Newline = (byte)'\n';

		public BulkRequestConverter(IConnectionSettingsValues settings) : base(settings) { }

		// The legacy Deserialize threw NotSupportedException — a bulk request body is never read back.
		public override IBulkRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
			throw new NotSupportedException();

		public override void Write(Utf8JsonWriter writer, IBulkRequest value, JsonSerializerOptions options)
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
