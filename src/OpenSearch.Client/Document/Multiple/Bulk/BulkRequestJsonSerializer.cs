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

using System.IO;
using System.Text;
using System.Text.Json;
using OpenSearch.Net;

namespace OpenSearch.Client
{
	/// <summary>
	/// Writes the newline-delimited (NDJSON) <c>_bulk</c> body for the <c>System.Text.Json</c> serializer
	/// (#388), replacing the vendored <c>BulkRequestFormatter</c>. Each operation contributes an action
	/// line <c>{ "&lt;op&gt;": { …metadata… } }</c> followed, unless it is a delete, by its body line
	/// (the document via the source serializer, or — for updates / raw documents — the request/response
	/// serializer). NDJSON cannot go through a single-root <c>Utf8JsonWriter</c>, so each line is
	/// serialized independently and separated by <c>\n</c>.
	/// </summary>
	internal static class BulkRequestJsonSerializer
	{
		private const byte Newline = (byte)'\n';

		public static void Write(IBulkRequest value, Stream stream, IOpenSearchSerializer builtInSerializer)
		{
			if (value?.Operations == null)
				return;

			var settings = BuiltInSerializerState.GetConnectionSettings(builtInSerializer);
			var inferrer = settings.Inferrer;
			var requestResponseOptions = BuiltInSerializerState.GetOptions(builtInSerializer);
			var sourceOptions = BuiltInSerializerState.GetOptions(settings.SourceSerializer);

			for (var index = 0; index < value.Operations.Count; index++)
			{
				var op = value.Operations[index];
				op.Index ??= value.Index ?? op.ClrType;
				if (op.Index.Equals(value.Index)) op.Index = null;
				op.Id = op.GetIdForOperation(inferrer);
				var routing = op.GetRoutingForOperation(inferrer);
				// The vendored formatter omits routing that resolves to null (unlike _id, which is written
				// even when null); collapse to null so the WhenWritingNull policy drops the member.
				op.Routing = ResolvesToNull(routing, inferrer) ? null : routing;

				// Action line: { "<operation>": {metadata} }. The operation token is a fixed identifier
				// (index/create/update/delete) requiring no escaping.
				var metadata = JsonSerializer.SerializeToUtf8Bytes(op, op.GetType(), requestResponseOptions);
				stream.WriteByte((byte)'{');
				stream.WriteByte((byte)'"');
				var operation = Encoding.UTF8.GetBytes(op.Operation);
				stream.Write(operation, 0, operation.Length);
				stream.WriteByte((byte)'"');
				stream.WriteByte((byte)':');
				stream.Write(metadata, 0, metadata.Length);
				stream.WriteByte((byte)'}');
				stream.WriteByte(Newline);

				var body = op.GetBody();
				if (body == null)
					continue;

				// Updates and already-serialized documents use the request/response serializer; everything
				// else (index/create) is a document written through the source serializer.
				var useRequestResponse = op.Operation == "update" || body is ILazyDocument;
				if (!useRequestResponse && sourceOptions == null)
				{
					// The configured source serializer is a custom (non-STJ) serializer with no STJ options;
					// serialize the document through it directly so its wire contract is honored.
					settings.SourceSerializer.Serialize(body, stream, SerializationFormatting.None);
				}
				else
				{
					var bodyOptions = useRequestResponse ? requestResponseOptions : sourceOptions;
					var bodyBytes = JsonSerializer.SerializeToUtf8Bytes(body, body.GetType(), bodyOptions);
					stream.Write(bodyBytes, 0, bodyBytes.Length);
				}
				stream.WriteByte(Newline);
			}
		}

		private static bool ResolvesToNull(Routing routing, Inferrer inferrer)
		{
			if (routing == null) return true;
			if (routing.Document != null) return inferrer.Routing(routing.Document.GetType(), routing.Document) == null;
			if (routing.DocumentGetter != null)
			{
				var document = routing.DocumentGetter();
				return document == null || inferrer.Routing(document.GetType(), document) == null;
			}
			if (routing.LongValue != null) return false;
			return routing.StringValue == null;
		}
	}
}
