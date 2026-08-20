/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

using System;
using OpenSearch.Net;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	internal class BulkStreamRequestFormatter : IJsonFormatter<IBulkStreamRequest>
	{
		private const byte Newline = (byte)'\n';

		private static SourceWriteFormatter<object> SourceWriter { get; } = new SourceWriteFormatter<object>();

		public IBulkStreamRequest Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver) =>
			throw new NotSupportedException();

		public void Serialize(ref JsonWriter writer, IBulkStreamRequest value, IJsonFormatterResolver formatterResolver)
		{
			if (value?.Operations == null)
				return;

			var settings = formatterResolver.GetConnectionSettings();
			var inferrer = settings.Inferrer;
			var formatter = formatterResolver.GetFormatter<object>();

			for (var index = 0; index < value.Operations.Count; index++)
			{
				var op = value.Operations[index];
				op.Index ??= value.Index ?? op.ClrType;
				if (op.Index.Equals(value.Index)) op.Index = null;
				op.Id = op.GetIdForOperation(inferrer);
				op.Routing = op.GetRoutingForOperation(inferrer);

				writer.WriteBeginObject();
				writer.WritePropertyName(op.Operation);

				formatter.Serialize(ref writer, op, formatterResolver);
				writer.WriteEndObject();
				writer.WriteRaw(Newline);

				var body = op.GetBody();
				if (body == null)
					continue;

				if (op.Operation == "update" || body is ILazyDocument)
				{
					var requestResponseSerializer = settings.RequestResponseSerializer;
					requestResponseSerializer.SerializeUsingWriter(ref writer, body, settings, SerializationFormatting.None);
				}
				else
					SourceWriter.Serialize(ref writer, body, formatterResolver);
				writer.WriteRaw(Newline);
			}
		}
	}
}
