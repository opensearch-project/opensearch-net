/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Linq;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;
#nullable enable

using OpenSearch.Net;

namespace OpenSearch.Client.Document.SystemTextJsonConverters;

/// <summary>
/// Converts <see cref="IMultiGetRequest"/> to the multi-get request body format.
/// Produces: {"docs": [{"_index": "...", "_id": "...", "_source": [...]}, ...]}
/// or the flattened form: {"ids": ["1", "2", ...]} when all operations can be flattened.
/// </summary>
internal sealed class MultiGetRequestConverter : JsonConverter<IMultiGetRequest>
{
	private readonly IConnectionSettingsValues _settings;

	public MultiGetRequestConverter(IConnectionSettingsValues settings) => _settings = settings;

	public override IMultiGetRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		throw new NotSupportedException("Deserialization of IMultiGetRequest is not supported.");

	public override void Write(Utf8JsonWriter writer, IMultiGetRequest value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		if (value?.Documents == null || !value.Documents.Any())
		{
			writer.WriteEndObject();
			return;
		}

		var docs = value.Documents.ToList();

		// If an index is specified at the request level, remove it from individual docs that match
		if (value.Index != null)
		{
			var resolvedIndex = value.Index.GetString(_settings);
			foreach (var doc in docs)
			{
				if (doc.Index == null) continue;
				var docIndex = doc.Index.GetString(_settings);
				if (string.Equals(resolvedIndex, docIndex))
					doc.Index = null;
			}
		}

		var flatten = docs.All(p => p.CanBeFlattened);

		if (flatten)
		{
			writer.WritePropertyName("ids");
			writer.WriteStartArray();
			foreach (var doc in docs)
			{
				var idValue = doc.Id != null ? ((IUrlParameter)doc.Id).GetString(_settings) : null;
				writer.WriteStringValue(idValue);
			}
			writer.WriteEndArray();
		}
		else
		{
			writer.WritePropertyName("docs");
			writer.WriteStartArray();
			foreach (var doc in docs)
			{
				WriteMultiGetOperation(writer, doc, options);
			}
			writer.WriteEndArray();
		}

		writer.WriteEndObject();
	}

	private void WriteMultiGetOperation(Utf8JsonWriter writer, IMultiGetOperation op, JsonSerializerOptions options)
	{
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
			writer.WritePropertyName("routing");
			writer.WriteStringValue(op.Routing);
		}

		if (op.Source != null)
		{
			writer.WritePropertyName("_source");
			WriteSourceUnion(writer, op.Source, options);
		}

		if (op.StoredFields != null)
		{
			writer.WritePropertyName("stored_fields");
			JsonSerializer.Serialize(writer, op.StoredFields, options);
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

		writer.WriteEndObject();
	}

	private void WriteSourceUnion(Utf8JsonWriter writer, Union<bool, ISourceFilter> source, JsonSerializerOptions options)
	{
		switch (source.Tag)
		{
			case 0:
				writer.WriteBooleanValue(source.Item1);
				break;
			case 1:
				if (source.Item2 != null)
				{
					var sourceFilterConverter = new SourceFilterConverter(_settings);
					sourceFilterConverter.Write(writer, source.Item2, options);
				}
				else
				{
					writer.WriteNullValue();
				}
				break;
			default:
				writer.WriteNullValue();
				break;
		}
	}
}
