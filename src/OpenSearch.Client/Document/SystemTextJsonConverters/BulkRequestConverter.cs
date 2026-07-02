/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Net;

namespace OpenSearch.Client.Document.SystemTextJsonConverters;

/// <summary>
/// Converts <see cref="IBulkRequest"/> to NDJSON format.
/// Bulk requests use newline-delimited JSON: each operation consists of an action/metadata line
/// followed by an optional source document line.
/// </summary>
internal sealed class BulkRequestConverter : JsonConverter<IBulkRequest>
{
	private readonly IConnectionSettingsValues _settings;

	public BulkRequestConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override IBulkRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		throw new NotSupportedException("Deserialization of IBulkRequest is not supported. The server response uses a different format.");

	public override void Write(Utf8JsonWriter writer, IBulkRequest value, JsonSerializerOptions options)
	{
		if (value?.Operations == null || value.Operations.Count == 0)
			return;

		// Bulk requests implement NdJsonSerializable pattern.
		// Each operation writes an action line followed by an optional document body line.
		// The output is NDJSON (newline-delimited JSON), not a single JSON document.
		// This converter is intended to produce the individual JSON fragments;
		// the actual NDJSON assembly with newline separators is handled by the transport layer.

		var inferrer = _settings.Inferrer;

		for (var i = 0; i < value.Operations.Count; i++)
		{
			var op = value.Operations[i];

			// Resolve index - fall back to request-level index
			op.Index ??= value.Index ?? op.ClrType;
			if (op.Index != null && op.Index.Equals(value.Index))
				op.Index = null;

			op.Id = op.GetIdForOperation(inferrer);
			op.Routing = op.GetRoutingForOperation(inferrer);

			// Write the action/metadata line: {"index": {"_index": "...", "_id": "..."}}
			writer.WriteStartObject();
			writer.WritePropertyName(op.Operation);
			writer.WriteStartObject();

			if (op.Index != null)
			{
				var indexName = _settings.Inferrer.IndexName(op.Index);
				if (!string.IsNullOrEmpty(indexName))
				{
					writer.WritePropertyName("_index");
					writer.WriteStringValue(indexName);
				}
			}

			if (op.Id != null)
			{
				var idValue = ((IUrlParameter)op.Id).GetString(_settings);
				if (!string.IsNullOrEmpty(idValue))
				{
					writer.WritePropertyName("_id");
					writer.WriteStringValue(idValue);
				}
			}

			if (op.Routing != null)
			{
				var routingValue = ((IUrlParameter)op.Routing).GetString(_settings);
				if (!string.IsNullOrEmpty(routingValue))
				{
					writer.WritePropertyName("routing");
					writer.WriteStringValue(routingValue);
				}
			}

			if (op.Version.HasValue)
			{
				writer.WritePropertyName("version");
				writer.WriteNumberValue(op.Version.Value);
			}

			if (op.VersionType.HasValue)
			{
				writer.WritePropertyName("version_type");
				writer.WriteStringValue(op.VersionType.Value.ToString().ToLowerInvariant());
			}

			if (op.RetriesOnConflict.HasValue)
			{
				writer.WritePropertyName("retry_on_conflict");
				writer.WriteNumberValue(op.RetriesOnConflict.Value);
			}

			writer.WriteEndObject(); // end metadata object
			writer.WriteEndObject(); // end action line

			// Write the source document line (if applicable - delete has no body)
			var body = op.GetBody();
			if (body != null)
			{
				JsonSerializer.Serialize(writer, body, body.GetType(), options);
			}
		}
	}
}
