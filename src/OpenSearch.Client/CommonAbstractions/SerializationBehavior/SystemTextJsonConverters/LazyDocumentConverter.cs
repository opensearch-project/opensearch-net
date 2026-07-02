/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/

#nullable enable

using System;
#nullable enable

using System.Buffers;
#nullable enable

using System.Text.Json;
#nullable enable

using System.Text.Json.Serialization;

namespace OpenSearch.Client.SystemTextJsonConverters;

internal sealed class LazyDocumentConverter : JsonConverter<LazyDocument>
{
	public override LazyDocument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
			return null;

		using var document = JsonDocument.ParseValue(ref reader);
		var bytes = JsonDocumentToBytes(document);
		return new LazyDocument(bytes);
	}

	public override void Write(Utf8JsonWriter writer, LazyDocument value, JsonSerializerOptions options)
	{
		if (value?.Bytes == null)
		{
			writer.WriteNullValue();
			return;
		}

		// Write the raw JSON bytes directly
		var reader = new Utf8JsonReader(value.Bytes);
		if (reader.Read())
		{
			using var doc = JsonDocument.ParseValue(ref reader);
			doc.RootElement.WriteTo(writer);
		}
		else
		{
			writer.WriteNullValue();
		}
	}

	private static byte[] JsonDocumentToBytes(JsonDocument document)
	{
		var buffer = new ArrayBufferWriter<byte>();
		using var jsonWriter = new Utf8JsonWriter(buffer);
		document.RootElement.WriteTo(jsonWriter);
		jsonWriter.Flush();
		return buffer.WrittenSpan.ToArray();
	}
}

internal sealed class LazyDocumentInterfaceConverter : JsonConverter<ILazyDocument>
{
	private readonly LazyDocumentConverter _inner = new();

	public override ILazyDocument? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
		_inner.Read(ref reader, typeof(LazyDocument), options);

	public override void Write(Utf8JsonWriter writer, ILazyDocument value, JsonSerializerOptions options)
	{
		if (value is LazyDocument lazyDocument)
		{
			_inner.Write(writer, lazyDocument, options);
		}
		else
		{
			writer.WriteNullValue();
		}
	}
}
